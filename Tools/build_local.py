#!/usr/bin/env python3
"""Build the Android cosmetic review bundle with a local, activated Unity editor."""

import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile


ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "UnityProject"
VERSION = "2022.3.20f1"
BUNDLE_NAME = "johnwickarsenal-cosmetics.bundle"
EDITOR_CLASS = "JohnWickArsenal.LocalTools.JwaLocalAndroidBuild"


def find_editor(override):
    if override:
        path = Path(override).expanduser().resolve()
        if not path.is_file() or not os.access(path, os.X_OK):
            raise ValueError(f"Unity executable does not exist or cannot be run: {path}")
        return path

    hubs = (
        Path.home() / "Unity/Hub/Editor",
        Path.home() / ".local/share/unity3d/Hub/Editor",
        Path("/Applications/Unity/Hub/Editor"),
        Path(os.environ.get("ProgramFiles", "C:/Program Files")) / "Unity/Hub/Editor",
    )
    for hub in hubs:
        for executable in ("Editor/Unity", "Editor/Unity.exe", "Unity.app/Contents/MacOS/Unity"):
            candidate = hub / VERSION / executable
            if candidate.is_file():
                return candidate.resolve()
    raise ValueError(f"Install Unity {VERSION} through Unity Hub or supply its executable with --unity.")


def check_installation(editor):
    project_version = (PROJECT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8")
    if f"m_EditorVersion: {VERSION}" not in project_version.splitlines():
        raise ValueError(f"UnityProject must declare Unity {VERSION}.")
    check_android_modules(editor)


def check_android_modules(editor):
    engine = next(
        (candidate for candidate in (
            editor.parent / "Data/PlaybackEngines/AndroidPlayer",
            editor.parent.parent / "PlaybackEngines/AndroidPlayer",
            editor.parent.parent.parent.parent / "PlaybackEngines/AndroidPlayer",
        ) if candidate.is_dir()),
        None,
    )
    if engine is None:
        raise ValueError(f"Install Android Build Support for the editor at {editor}.")
    missing = [name for name in ("SDK", "NDK", "OpenJDK") if not (engine / name).is_dir()]
    if missing:
        raise ValueError("Install the missing Unity Hub Android modules: " + ", ".join(missing))


def run_unity(editor, output, import_only):
    output.mkdir(parents=True, exist_ok=True)
    prefix = "cosmetic-imports-" if import_only else "cosmetics-android-"
    export = Path(tempfile.mkdtemp(prefix=prefix, dir=output))
    with tempfile.NamedTemporaryFile(prefix="unity-android-", suffix=".log", dir=output, delete=False) as log_file:
        log = Path(log_file.name)
    completed = False
    try:
        method = "ValidateBatch" if import_only else "ExportBatch"
        command = [
            str(editor), "-batchmode", "-nographics", "-quit",
            "-projectPath", str(PROJECT), "-buildTarget", "Android",
            "-executeMethod", f"{EDITOR_CLASS}.{method}", "-logFile", str(log),
        ]
        print(f"Running the local Unity editor. Log: {log}", flush=True)
        process = subprocess.run(command, cwd=ROOT, env={**os.environ, "JWA_COSMETICS_OUTPUT": str(export)})
        if process.returncode != 0:
            raise ValueError(f"Unity exited with {process.returncode}. See {log}")

        report = export / "cosmetic-imports.json"
        try:
            data = json.loads(report.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError) as error:
            raise ValueError(f"Unity did not write a readable cosmetic import report. See {log}") from error
        if (not isinstance(data, dict) or data.get("unityVersion") != VERSION or data.get("target") != "Android"
                or not isinstance(data.get("models"), list) or not data["models"]):
            raise ValueError(f"Unity import report does not identify validated Android cosmetic models. See {log}")

        if not import_only:
            bundle = export / BUNDLE_NAME
            if not bundle.is_file():
                raise ValueError(f"Unity did not create {BUNDLE_NAME}. See {log}")
            with bundle.open("rb") as bundle_data:
                if bundle_data.read(8) != b"UnityFS\0":
                    raise ValueError(f"Unity output has an invalid bundle header. See {log}")
            print(f"Android cosmetic review bundle: {bundle}")
        completed = True
        print(f"Unity cosmetic import report: {report}")
        print("Unity log:", log)
    finally:
        if not completed:
            shutil.rmtree(export)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--unity", default=os.environ.get("UNITY_EXECUTABLE"), help="path to Unity executable")
    parser.add_argument("--output", type=Path, default=ROOT / "BuildOutput", help="local bundle/log output folder")
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--check", action="store_true", help="check installed Android modules without launching Unity")
    mode.add_argument("--validate-imports", action="store_true", help="launch Unity to compile and check imports only")
    args = parser.parse_args()
    try:
        editor = find_editor(args.unity)
        check_installation(editor)
        if args.check:
            print(f"Found editor and Android modules at {editor}. Confirm license activation in Unity Hub.")
        else:
            run_unity(editor, args.output.resolve(), args.validate_imports)
        return 0
    except (OSError, ValueError) as error:
        print(error, file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
