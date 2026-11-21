import bpy
import os
import sys

files_to_convert = [
    'kevlar_vest.blend',
    'secret_agent.blend',
    'clothing_character_kit.blend',
    'tactical_knife.blend',
    'mw3_trophy_system.blend'
]

for blend_file in files_to_convert:
    if os.path.exists(blend_file):
        print(f"Converting {blend_file}...")
        bpy.ops.wm.open_mainfile(filepath=blend_file)
        glb_file = blend_file.replace('.blend', '.glb')
        bpy.ops.export_scene.gltf(filepath=glb_file, export_format='GLB')
        print(f"  -> {glb_file}")
    else:
        print(f"  ! Not found: {blend_file}")

print("Done!")
