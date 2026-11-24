#!/usr/bin/env python3
"""Export the briefcase prop using a local Warpfrog BasSDK Android project."""

import argparse
import base64
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile

from build_local import check_android_modules


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "UnityProject/Assets/JohnWickArsenal/Models/Cosmetics"
EDITOR = ROOT / "Tools/BasSDK/Editor/JwaCosmeticsExporter.cs"
SDK_URL = "https://dev.azure.com/Warpfrog/BasSDK/_git/BasSDK"
SDK_INPUTS = ("Assets", "Packages", "ProjectSettings", "BuildStaging/Catalogs")
UNITY_VERSION = "2021.3.38f1"
MOD_NAME = "JohnWickArsenalCosmetics"
ITEM_ID = "jwa_cosmetic_briefcase"
ADDRESS = "JWA.Cosmetics.Prop.Briefcase"
UNITY_METHOD = "JohnWickArsenal.BasSDK.Editor.JwaCosmeticsExporter.ExportBatch"


def read_json(path):
    result = json.loads(path.read_text(encoding="utf-8-sig"))
    if not isinstance(result, dict):
        raise ValueError(f"Expected a JSON object in {path}")
    return result


def sdk_version(sdk):
    if not all((sdk / path).is_dir() for path in SDK_INPUTS):
        raise ValueError(f"A complete, local Warpfrog BasSDK checkout is required. {SDK_URL}")

    versions = (sdk / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8-sig").splitlines()
    if f"m_EditorVersion: {UNITY_VERSION}" not in versions:
        raise ValueError(f"Use the Warpfrog 1.3 SDK project for Unity {UNITY_VERSION}.")

    for path in (
        "Assets/SDK/Scripts/Assemblies/Editor/AssetBundleBuilder.cs",
        "Assets/AddressableAssetsData/AddressableAssetSettings.asset",
        "Assets/SDK/Shadowood/LitMoss/LitMoss-ASshader.ASshader",
    ):
        asset = sdk / path
        with asset.open("rb") as data:
            if data.read(40).startswith(b"version https://git-lfs.github.com/"):
                raise ValueError(f"Download the Git LFS content required by the SDK: {asset}")

    game = (sdk / "Assets/SDK/Config/ThunderRoad/GameSettings/BladeAndSorcery.asset").read_text(encoding="utf-8-sig")
    if not re.search(r"^\s*versionMajor:\s*1\s*$", game, re.MULTILINE) or not re.search(
        r"^\s*versionMinor:\s*3\s*$", game, re.MULTILINE
    ):
        raise ValueError("Use the BasSDK 1.3 game settings.")
    match = re.search(r"^\s*minModVersion:\s*([\d.]+)\s*$", game, re.MULTILINE)
    if not match:
        raise ValueError("The SDK game settings are missing the manifest compatibility version.")
    return match[1]


def check_editor(editor):
    if not editor.is_file() or not os.access(editor, os.X_OK):
        raise ValueError(f"Unity executable does not exist or cannot be run: {editor}")
    check_android_modules(editor)

    info = subprocess.run([str(editor), "-version"], capture_output=True, text=True, timeout=30, check=False)
    if info.returncode or not re.search(rf"\b{re.escape(UNITY_VERSION)}\b", info.stdout + info.stderr):
        raise ValueError(f"Expected Unity {UNITY_VERSION}, got an unrecognized version from {editor}.")


def source_files():
    prop = SOURCE / "Props/Briefcase"
    files = {
        EDITOR: Path("Editor") / EDITOR.name,
        SOURCE / "ASSET_CREDITS.txt": Path("ASSET_CREDITS.txt"),
    }
    for path in ("Briefcase.fbx", "Briefcase.png", "Briefcase.fbm/Briefcase_Albedo.png", "Briefcase.fbm/Briefcase_Normal.png"):
        files[prop / path] = Path(path)

    for path in files:
        if not path.is_file() or not path.stat().st_size:
            raise ValueError(f"Missing source asset: {path}")
        if path.suffix == ".png":
            verify_signature(path, b"\x89PNG\r\n\x1a\n")
    verify_signature(prop / "Briefcase.fbx", b"Kaydara FBX Binary  \0")
    return files


def verify_signature(path, signature):
    with path.open("rb") as stream:
        if stream.read(len(signature)) != signature:
            raise ValueError(f"Unexpected file format: {path}")


def addressable_key(value):
    data = value.encode("ascii")
    return b"\0" + len(data).to_bytes(4, "little") + data


def verify_export(folder, game_version):
    manifest = read_json(folder / "manifest.json")
    if manifest.get("GameVersion") != game_version or manifest.get("Thumbnail") != "icon.png":
        raise ValueError("The SDK manifest has inconsistent compatibility or thumbnail metadata.")
    for name in ("Name", "Description", "Author", "ModVersion"):
        if not isinstance(manifest.get(name), str) or not manifest[name].strip():
            raise ValueError(f"The SDK manifest is missing {name}.")
    release = manifest["ModVersion"]
    if not re.fullmatch(r"\d+(?:\.\d+){1,3}", release):
        raise ValueError("Invalid SDK manifest ModVersion.")

    verify_signature(folder / "icon.png", b"\x89PNG\r\n\x1a\n")
    if not (folder / "ASSET_CREDITS.txt").read_text(encoding="utf-8-sig").strip():
        raise ValueError("Missing briefcase asset credits.")

    item = read_json(folder / f"Item_{ITEM_ID}.json")
    expected = ("ThunderRoad.ItemData, ThunderRoad", ITEM_ID, 4, "Prop", ADDRESS, [])
    actual = tuple(item.get(name) for name in ("$type", "id", "version", "type", "prefabAddress", "modules"))
    if actual != expected:
        raise ValueError("The exported briefcase is missing the expected props-only ItemData.")
    sprite = item.get("iconAddress", "")
    if not isinstance(sprite, str) or not re.fullmatch(re.escape(ADDRESS) + r"\.Icon(?:\[[^\[\]]+\])?", sprite):
        raise ValueError("The exported briefcase has an invalid sprite address.")

    catalogs = list(folder.glob("catalog_*.json"))
    if len(catalogs) != 1:
        raise ValueError("Expected the SDK to export one Addressables catalog.")
    catalog = read_json(catalogs[0])
    if not re.fullmatch(r"[0-9a-fA-F]{32}", catalogs[0].with_suffix(".hash").read_text(encoding="utf-8-sig").strip()):
        raise ValueError("The Addressables catalog is missing its hash.")

    encoded = catalog.get("m_KeyDataString")
    if not isinstance(encoded, str):
        raise ValueError("The Addressables catalog is missing the item and sprite keys.")
    keys = base64.b64decode(encoded, validate=True)
    if addressable_key(ADDRESS) not in keys or not any(
        addressable_key(key) in keys for key in (sprite, f"{ADDRESS}.Icon")
    ):
        raise ValueError("The exported catalog does not contain the item and sprite addresses.")

    internal = catalog.get("m_InternalIds")
    if not isinstance(internal, list) or not all(isinstance(value, str) for value in internal):
        raise ValueError("The Addressables catalog has invalid internal IDs.")
    paths = [PurePosixPath(value.replace("\\", "/")) for value in internal]
    referenced = {path.name for path in paths if path.suffix == ".bundle"}
    required = {path.name for path in paths if path.suffix == ".bundle" and MOD_NAME in path.parts}
    bundles = {path.name: path for path in folder.glob("*.bundle")}
    if not bundles or required.difference(bundles) or set(bundles).difference(referenced):
        raise ValueError("The Android asset bundles do not match the catalog's local bundle references.")
    for path in bundles.values():
        verify_signature(path, b"UnityFS\0")
    return release


def build(sdk, editor, output, game_version, files):
    output.mkdir(parents=True, exist_ok=True)
    destination = Path(tempfile.mkdtemp(prefix="nomad-cosmetics-", dir=output))
    log = destination / "unity-nomad.log"
    with tempfile.TemporaryDirectory(prefix="jwa-bassdk-") as workspace:
        project = Path(workspace) / "BasSDK"
        export = Path(workspace) / MOD_NAME
        print(f"Staging a temporary copy of the official SDK. Log: {log}", flush=True)
        for path in SDK_INPUTS:
            shutil.copytree(sdk / path, project / path)
        stage = project / "Assets/JwaCosmetics"
        stage.mkdir()
        for origin, path in files.items():
            target = stage / path
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(origin, target)

        command = [
            str(editor), "-batchmode", "-nographics", "-quit", "-projectPath", str(project),
            "-buildTarget", "Android", "-executeMethod", UNITY_METHOD, "-logFile", str(log),
        ]
        result = subprocess.run(command, cwd=project, env={**os.environ, "JWA_NOMAD_COSMETICS_OUTPUT": str(export)})
        if result.returncode:
            raise ValueError(f"Unity exited with {result.returncode}. See {log}")

        try:
            release = verify_export(export, game_version)
        except (OSError, ValueError) as error:
            raise ValueError(f"Android Nomad export failed validation. {error} See {log}") from error

        package = destination / f"{MOD_NAME}_v{release}.zip"
        temporary_zip = package.with_suffix(".zip.tmp")
        try:
            with zipfile.ZipFile(temporary_zip, "w", zipfile.ZIP_DEFLATED) as archive:
                for path in sorted(export.rglob("*")):
                    if path.is_file():
                        archive.write(path, f"{MOD_NAME}/{path.relative_to(export).as_posix()}")
            temporary_zip.replace(package)
        finally:
            temporary_zip.unlink(missing_ok=True)
    print(f"Nomad package: {package}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sdk", type=Path, default=os.environ.get("BASSDK_PROJECT"), help="local Warpfrog BasSDK project")
    parser.add_argument("--unity", type=Path, default=os.environ.get("BASSDK_UNITY_PATH"), help="Unity 2021.3.38f1 executable")
    parser.add_argument("--output", type=Path, default=ROOT / "BuildOutput", help="directory for the package and Unity log")
    parser.add_argument("--check", action="store_true", help="check source inputs and the local SDK/editor installation")
    args = parser.parse_args()
    if args.sdk is None or args.unity is None:
        parser.error("provide --sdk and --unity, or set BASSDK_PROJECT and BASSDK_UNITY_PATH")

    try:
        sdk = args.sdk.expanduser().resolve()
        editor = args.unity.expanduser().resolve()
        game_version = sdk_version(sdk)
        files = source_files()
        check_editor(editor)
        if args.check:
            print(f"Found the briefcase inputs and BasSDK for Unity {UNITY_VERSION}, manifest GameVersion {game_version}.")
        else:
            build(sdk, editor, args.output.expanduser().resolve(), game_version, files)
        return 0
    except (OSError, ValueError, subprocess.TimeoutExpired) as error:
        print(error, file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
