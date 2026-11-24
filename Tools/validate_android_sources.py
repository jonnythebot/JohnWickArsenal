#!/usr/bin/env python3
"""Validate Android build sources and cosmetic assets without starting Unity."""

import argparse
import hashlib
import json
import math
from pathlib import Path
import struct
import subprocess
import sys
import zlib


ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "UnityProject"
MOD_ASSETS = PROJECT / "Assets/JohnWickArsenal"
MODELS = MOD_ASSETS / "Models"
BACKUPS = ROOT / "Backups/ProceduralModels"
EDITOR_VERSION = "2022.3.20f1"
EDITOR_REVISION = "61c2feb0970d"
PNG_HEADER = b"\x89PNG\r\n\x1a\n"
FBX_HEADER = b"Kaydara FBX Binary  \x00\x1a\x00"
GARMENTS = {
    "Suits/ClassicBlackSuit": ("Albedo", "Bump"),
    "Cosmetics/Garments/DinnerJacket/DinnerJacket": ("Albedo", "Normal"),
    "Cosmetics/Garments/DoubleBreastedSuit/DoubleBreastedSuit": ("Albedo", "Normal"),
}
CLASSIC_BACKUP = {
    ".obj": "068c5783db213675366aedcf8cebf48c64f3d1d67d069f044fde30bb0f9d830a",
    ".mtl": "1434ba49ebf467a7964217d8b60c1aadca14ef16ae48552641ffb2e14fb8064d",
}
BACKUP_GROUPS = {"Equipment": 10, "Pistols": 4, "Revolvers": 1, "Rifles": 1,
                 "Shotguns": 2, "Suits": 5, "Throwables": 2}


def fail(path, message):
    raise ValueError(f"{path.relative_to(ROOT)}: {message}")


def wavefront_lines(path):
    for line_number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        fields = line.partition("#")[0].split(maxsplit=1)
        if len(fields) == 2:
            yield line_number, fields[0], fields[1]


def resolve_asset(path, reference, root):
    reference = reference.replace("\\", "/")
    target = (path.parent / reference).resolve()
    if not target.is_relative_to(root) or Path(reference).is_absolute() or ":" in reference:
        fail(path, f"reference must stay within the asset tree: {reference}")
    if not target.is_file():
        fail(path, f"missing source dependency {reference}")
    return target


def verify_obj(path, root):
    sizes = {"v": 0, "vt": 0, "vn": 0}
    material_files = set()
    used_materials = set()
    faces = 0
    for line, command, argument in wavefront_lines(path):
        if command in sizes:
            size = 1 if command == "vt" else 3
            coordinates = argument.split()
            if len(coordinates) < size or not all(math.isfinite(float(value)) for value in coordinates):
                fail(path, f"line {line}: invalid {command} coordinates")
            sizes[command] += 1
        elif command == "f":
            face = argument.split()
            if len(face) < 3:
                fail(path, f"line {line}: face needs at least three vertices")
            vertices = set()
            for reference in face:
                indices = reference.split("/")
                if not 1 <= len(indices) <= 3 or not indices[0]:
                    fail(path, f"line {line}: invalid face index {reference}")
                for key, index in zip(sizes, indices):
                    if not index:
                        continue
                    value = int(index)
                    if not value or abs(value) > sizes[key]:
                        fail(path, f"line {line}: {key} index {value} is out of range")
                    if key == "v":
                        vertices.add(value if value > 0 else sizes[key] + value + 1)
            if len(vertices) < 3:
                fail(path, f"line {line}: face has fewer than three distinct vertices")
            faces += 1
        elif command == "mtllib":
            material_files.add(resolve_asset(path, argument, root))
        elif command == "usemtl":
            used_materials.add(argument)
    if not faces:
        fail(path, "no faces found")

    materials = set()
    textures = set()
    for material_file in material_files:
        for _, command, argument in wavefront_lines(material_file):
            if command == "newmtl":
                materials.add(argument)
            elif command.startswith("map_") or command.lower() in {"bump", "disp", "decal", "norm", "refl"}:
                textures.add(resolve_asset(material_file, argument, root))
    if used_materials - materials:
        fail(path, f"undefined materials: {', '.join(sorted(used_materials - materials))}")
    return material_files, textures


def verify_png(path, limit=1024):
    data = path.read_bytes()
    if not data.startswith(PNG_HEADER):
        fail(path, "invalid PNG signature")
    offset = len(PNG_HEADER)
    chunks = []
    while offset + 12 <= len(data):
        size, kind = struct.unpack_from(">I4s", data, offset)
        start, end = offset + 8, offset + 8 + size
        if end + 4 > len(data):
            fail(path, "truncated PNG chunk")
        if zlib.crc32(data[offset + 4:end]) != struct.unpack_from(">I", data, end)[0]:
            fail(path, f"invalid PNG chunk checksum for {kind.decode('ascii', errors='replace')}")
        if not chunks:
            if kind != b"IHDR" or size != 13:
                fail(path, "missing PNG image header")
            width, height = struct.unpack_from(">II", data, start)
            if not 0 < width <= limit or not 0 < height <= limit:
                fail(path, f"PNG dimensions {width}x{height} exceed the {limit}-pixel budget")
        chunks.append(kind)
        offset = end + 4
        if kind == b"IEND":
            if size or offset != len(data) or b"IDAT" not in chunks:
                fail(path, "invalid PNG image ending")
            return
    fail(path, "missing PNG image ending")


def verify_fbx(path, root):
    data = path.read_bytes()
    if not data.startswith(FBX_HEADER) or len(data) < len(FBX_HEADER) + 4:
        fail(path, "invalid binary FBX header")
    version = struct.unpack_from("<I", data, len(FBX_HEADER))[0]
    header = struct.Struct("<QQQB" if version >= 7500 else "<IIIB")
    texture_refs = set()
    node_names = set()

    def scan_nodes(offset, end):
        while offset + header.size <= end:
            next_node, property_count, property_size, name_size = header.unpack_from(data, offset)
            if not next_node:
                return
            name_start = offset + header.size
            properties = name_start + name_size
            children = properties + property_size
            if not (0 < name_size and name_start < properties <= children <= next_node <= end):
                fail(path, "invalid FBX node boundaries")
            name = data[name_start:properties]
            node_names.add(name)
            if name == b"RelativeFilename" and property_count == 1:
                if property_size < 5 or data[properties] != ord("S"):
                    fail(path, "invalid FBX relative filename property")
                name_bytes = struct.unpack_from("<I", data, properties + 1)[0]
                if name_bytes != property_size - 5:
                    fail(path, "invalid FBX relative filename size")
                texture_refs.add(data[properties + 5:children].decode("utf-8"))
            scan_nodes(children, next_node)
            offset = next_node

    scan_nodes(len(FBX_HEADER) + 4, len(data))
    if not {b"Objects", b"Geometry", b"Vertices", b"PolygonVertexIndex", b"Connections"} <= node_names:
        fail(path, "missing FBX model data")
    return {resolve_asset(path, reference, root) for reference in texture_refs if reference}


def verify_assets():
    scoped_files = {MODELS / "Cosmetics/ASSET_CREDITS.txt"}
    for name, maps in GARMENTS.items():
        base = MODELS / name
        obj = base.with_suffix(".obj")
        materials, textures = verify_obj(obj, MODELS)
        expected = {base.with_name(f"{base.name}_{channel}.png") for channel in maps}
        if base.with_suffix(".mtl") not in materials or base.with_name(f"{base.name}_Albedo.png") not in textures:
            fail(obj, "missing the expected material and albedo binding")
        scoped_files.update({obj} | materials | textures | expected)

    briefcase = MODELS / "Cosmetics/Props/Briefcase/Briefcase.fbx"
    briefcase_maps = {briefcase.with_suffix(".fbm") / f"Briefcase_{channel}.png" for channel in ("Albedo", "Normal")}
    texture_refs = verify_fbx(briefcase, MODELS)
    if not briefcase_maps <= texture_refs:
        fail(briefcase, "missing albedo or normal texture binding")
    scoped_files.update({briefcase} | texture_refs)
    for directory in (MODELS / "Suits", MODELS / "Cosmetics", MOD_ASSETS / "Cosmetics"):
        for model in sorted(directory.rglob("*")):
            if model in scoped_files or not model.is_file():
                continue
            if model.suffix.lower() == ".obj":
                materials, textures = verify_obj(model, MOD_ASSETS)
                scoped_files.update({model} | materials | textures)
            elif model.suffix.lower() == ".fbx":
                scoped_files.update({model} | verify_fbx(model, MOD_ASSETS))
    for texture in sorted(path for path in scoped_files if path.suffix == ".png"):
        verify_png(texture)
    if not (MODELS / "Cosmetics/ASSET_CREDITS.txt").read_text(encoding="utf-8").strip():
        fail(MODELS / "Cosmetics/ASSET_CREDITS.txt", "empty asset attribution")

    for name, count in BACKUP_GROUPS.items():
        group = BACKUPS / name
        objs, mtls = set(group.glob("*.obj")), set(group.glob("*.mtl"))
        if len(objs) < count or {path.with_suffix(".mtl") for path in objs} != mtls:
            fail(group, f"expected at least {count} complete OBJ/MTL backup pairs")
        for obj in sorted(objs):
            materials, textures = verify_obj(obj, BACKUPS)
            if obj.with_suffix(".mtl") not in materials:
                fail(obj, "backup does not reference its matching MTL")
            scoped_files.update({obj} | materials | textures)
    for suffix, expected in CLASSIC_BACKUP.items():
        backup = BACKUPS / f"Suits/ClassicBlackSuit{suffix}"
        original_bytes = backup.read_bytes().replace(b"\r\n", b"\n")
        if hashlib.sha256(original_bytes).hexdigest() != expected:
            fail(backup, "does not match the original procedural ClassicBlackSuit source")
    return scoped_files


def load_json(path):
    try:
        value = json.loads(path.read_text(encoding="utf-8-sig"))
    except json.JSONDecodeError as error:
        raise SystemExit(f"{path.relative_to(ROOT)}:{error.lineno}: {error.msg}") from error
    if not isinstance(value, dict):
        raise SystemExit(f"{path.relative_to(ROOT)} must contain a JSON object")
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-tracked", action="store_true", help="also check that scoped source assets are in Git")
    args = parser.parse_args()

    version_file = PROJECT / "ProjectSettings/ProjectVersion.txt"
    version_lines = version_file.read_text(encoding="utf-8-sig").splitlines()
    required_lines = {
        f"m_EditorVersion: {EDITOR_VERSION}",
        f"m_EditorVersionWithRevision: {EDITOR_VERSION} ({EDITOR_REVISION})",
    }
    if not required_lines.issubset(version_lines):
        raise SystemExit(f"{version_file.relative_to(ROOT)} must pin Unity {EDITOR_VERSION} ({EDITOR_REVISION})")

    manifest = load_json(PROJECT / "Packages/manifest.json")
    dependencies = manifest.get("dependencies")
    if not isinstance(dependencies, dict) or not all(isinstance(version, str) for version in dependencies.values()):
        raise SystemExit("Unity package dependencies must map package names to version strings")
    lockfile = PROJECT / "Packages/packages-lock.json"
    if lockfile.exists():
        load_json(lockfile)
    load_json(ROOT / "mod.json")
    for path in sorted((ROOT / "ModConfig").glob("*.json")):
        load_json(path)
    for source in (ROOT / "Source", PROJECT / "Assets"):
        for path in sorted(source.rglob("*.asmdef")):
            load_json(path)

    build_script = ROOT / "Tools/build_local.py"
    compile(build_script.read_text(encoding="utf-8-sig"), str(build_script.relative_to(ROOT)), "exec")
    editor_export = PROJECT / "Assets/LocalTools/Editor/JwaLocalAndroidBuild.cs"
    if not editor_export.is_file():
        raise SystemExit(f"Local Android exporter missing: {editor_export.relative_to(ROOT)}")

    scoped_files = verify_assets()
    if args.check_tracked:
        tracked = subprocess.check_output(["git", "ls-files", "-z"], cwd=ROOT, text=True).split("\0")
        missing = {path.relative_to(ROOT).as_posix() for path in scoped_files} - set(tracked)
        if missing:
            raise ValueError("Untracked cosmetic sources or backups: " + ", ".join(sorted(missing)))
    print(f"Source and asset checks passed for Unity {EDITOR_VERSION}. Tested {len(scoped_files)} asset and backup files.")


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, subprocess.CalledProcessError) as error:
        sys.exit(str(error))
