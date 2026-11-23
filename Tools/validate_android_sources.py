#!/usr/bin/env python3
"""Validate source files for the local Android build without starting Unity."""

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "UnityProject"
EDITOR_VERSION = "2022.3.20f1"
EDITOR_REVISION = "61c2feb0970d"


def load_json(path):
    try:
        value = json.loads(path.read_text(encoding="utf-8-sig"))
    except json.JSONDecodeError as error:
        raise SystemExit(f"{path.relative_to(ROOT)}:{error.lineno}: {error.msg}") from error
    if not isinstance(value, dict):
        raise SystemExit(f"{path.relative_to(ROOT)} must contain a JSON object")
    return value


def main():
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
    print(f"Source checks passed for Unity {EDITOR_VERSION}. Run the local build to compile and export.")


if __name__ == "__main__":
    main()
