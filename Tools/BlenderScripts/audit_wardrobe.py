"""Check wardrobe mesh skinning against a local BasSDK character reference.

Run from the repository root with Blender 4.x:
  blender --background --python-exit-code 1 --python Tools/BlenderScripts/audit_wardrobe.py -- \
    --reference "/path/to/BasSDK/Assets/SDK/Examples/Characters/Sources/HumanMale_Complete_Model.fbx" \
    "path/to/garment.fbx"
"""

import argparse
import json
import math
from pathlib import Path
import sys

import bpy


def import_asset(path):
    if not path.is_file():
        raise ValueError(f"File does not exist: {path}")
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if path.suffix.lower() == ".fbx":
        result = bpy.ops.import_scene.fbx(filepath=str(path), use_image_search=False)
    elif path.suffix.lower() == ".obj":
        result = bpy.ops.wm.obj_import(filepath=str(path), forward_axis="NEGATIVE_Z", up_axis="Y")
    else:
        raise ValueError(f"Expected an FBX or OBJ file: {path}")
    if "FINISHED" not in result:
        raise ValueError(f"Blender could not import {path}")
    bpy.context.view_layer.update()
    return list(bpy.context.scene.objects)


def armature_for(mesh):
    targets = {
        modifier.object for modifier in mesh.modifiers
        if modifier.type == "ARMATURE" and modifier.object is not None
    }
    return next(iter(targets)) if len(targets) == 1 else None


def bone_hierarchy(armature):
    return {bone.name: bone.parent.name if bone.parent else None for bone in armature.data.bones}


def bounds(mesh):
    points = [mesh.matrix_world @ vertex.co for vertex in mesh.data.vertices]
    return [
        [round(extreme(point[axis] for point in points), 6) for axis in range(3)]
        for extreme in (min, max)
    ]


def reference_rig(path):
    objects = import_asset(path)
    body = next((obj for obj in objects if obj.type == "MESH" and obj.name == "UnifiedMesh_LOD0"), None)
    rig = armature_for(body) if body is not None else None
    if rig is None or not rig.data.bones:
        raise ValueError("Reference must contain the weighted UnifiedMesh_LOD0 body from the SDK.")
    hierarchy = bone_hierarchy(rig)
    return hierarchy, {"path": str(path), "armature": rig.name, "bones": len(hierarchy), "bodyBounds": bounds(body)}


def inspect_mesh(mesh, reference):
    skin = armature_for(mesh)
    garment_rig = bone_hierarchy(skin) if skin is not None else {}
    group_names = {group.index: group.name for group in mesh.vertex_groups}
    joint_mismatches = set()
    unweighted = 0
    not_normalized = 0
    max_influences = 0

    for vertex in mesh.data.vertices:
        influences = [
            (group_names[group.group], group.weight) for group in vertex.groups
            if math.isfinite(group.weight) and group.weight > 1e-5
        ]
        valid_weights = []
        for name, weight in influences:
            if name not in reference or name not in garment_rig or garment_rig[name] != reference[name]:
                joint_mismatches.add(name)
                continue
            valid_weights.append(weight)
        if not valid_weights:
            unweighted += 1
        elif not math.isclose(sum(valid_weights), 1.0, abs_tol=0.01):
            not_normalized += 1
        max_influences = max(max_influences, len(valid_weights))

    mesh.data.calc_loop_triangles()
    return {
        "name": mesh.name,
        "vertices": len(mesh.data.vertices),
        "triangles": len(mesh.data.loop_triangles),
        "bounds": bounds(mesh),
        "armature": skin.name if skin is not None else None,
        "incompatibleWeightedBones": sorted(joint_mismatches),
        "verticesWithoutCompatibleWeights": unweighted,
        "verticesWithUnnormalizedWeights": not_normalized,
        "maxInfluences": max_influences,
        "skinningCheckPassed": skin is not None and not (joint_mismatches or unweighted or not_normalized),
    }


def audit_garment(path, reference):
    meshes = [
        inspect_mesh(obj, reference) for obj in import_asset(path)
        if obj.type == "MESH" and len(obj.data.vertices)
    ]
    return {
        "path": str(path),
        "meshes": sorted(meshes, key=lambda mesh: mesh["name"]),
        "skinningCheckPassed": bool(meshes) and all(mesh["skinningCheckPassed"] for mesh in meshes),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--reference", type=Path, required=True, help="Local stock BasSDK character FBX")
    parser.add_argument("garments", type=Path, nargs="+")
    arguments = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(arguments)

    try:
        reference, metadata = reference_rig(args.reference)
        garments = [audit_garment(path, reference) for path in args.garments]
    except (RuntimeError, ValueError) as error:
        print(f"Wardrobe audit failed: {error}", file=sys.stderr)
        return 1

    print(json.dumps({
        "reference": metadata,
        "garments": garments,
        "scope": "Skinning and directly weighted joint parents. Review scale, fit, joint poses and Manikin masks in the SDK.",
    }, indent=2))
    return 0 if all(garment["skinningCheckPassed"] for garment in garments) else 1


if __name__ == "__main__":
    sys.exit(main())
