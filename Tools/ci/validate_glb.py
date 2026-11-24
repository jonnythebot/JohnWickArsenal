#!/usr/bin/env python3
import pygltflib
import trimesh
import os
import sys

models = {
    'UnityProject/Assets/JohnWickArsenal/Models/Pistols/TTI_PitViper.glb': {'max_tris': 20000, 'source': 'petroulacl'},
    'UnityProject/Assets/JohnWickArsenal/Models/Pistols/Glock_34.glb': {'max_tris': 20000, 'source': 'petroulacl'},
    'UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb': {'max_tris': 20000, 'source': 'vinish'},
    'UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb': {'max_tris': 20000, 'source': 'vinish'},
    'UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb': {'max_tris': 20000, 'source': 'vinish'},
    'UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb': {'max_tris': 20000, 'source': 'vinish'},
}

for path, spec in models.items():
    if not os.path.exists(path):
        print('MISSING: ' + path)
        sys.exit(1)
    gltf = pygltflib.GLTF2().load_binary(path)
    scene = trimesh.load(path, force='scene')
    tris = sum(len(g.faces) for g in scene.geometry.values() if hasattr(g, 'faces'))
    has_textures = len(gltf.images) > 0
    print('OK: ' + os.path.basename(path) + ': ' + str(tris) + ' tris, ' + str(len(gltf.meshes)) + ' meshes, ' + str(len(gltf.materials)) + ' mats, embedded tex: ' + str(has_textures))
    if tris > spec['max_tris']:
        print('  Exceeds ' + str(spec['max_tris']) + ' tris limit')

print('All GLB models validated')
