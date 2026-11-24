#!/usr/bin/env python3
import os
import sys

required_dirs = [
    "UnityProject/Assets/JohnWickArsenal/Models",
    "UnityProject/Assets/JohnWickArsenal/Prefabs",
    "UnityProject/Assets/JohnWickArsenal/Editor",
    "UnityProject/Assets/Scripts/JohnWickArsenal",
    "UnityProject/Assets/LocalTools/Editor",
    "UnityProject/Assets/Scripts/JohnWickArsenal",
    "UnityProject/ProjectSettings",
    "UnityProject/Packages",
]

required_files = [
    "UnityProject/Packages/manifest.json",
    "UnityProject/ProjectSettings/ProjectVersion.txt",
    "UnityProject/Assets/JohnWickArsenal/Editor/JWA_DisplayPrefabGenerator.cs",
    "UnityProject/Assets/JohnWickArsenal/Editor/JohnWickArsenal.Cosmetic.Editor.asmdef",
    "UnityProject/Assets/JohnWickArsenal/Models/Pistols/TTI_PitViper.glb",
    "UnityProject/Assets/JohnWickArsenal/Models/Pistols/Glock_34.glb",
    "UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb",
    "UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb",
    "UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb",
    "UnityProject/Assets/JohnWickArsenal/Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb",
    "UnityProject/Assets/JohnWickArsenal/Models/Suits/ClassicBlackSuit.obj",
    "UnityProject/Assets/JohnWickArsenal/Models/Suits/ClassicBlackSuit.mtl",
    "UnityProject/Assets/JohnWickArsenal/Models/Suits/ClassicBlackSuit_Albedo.png",
    "UnityProject/Assets/JohnWickArsenal/Models/Suits/ClassicBlackSuit_Bump.png",
    "UnityProject/Assets/JohnWickArsenal/Prefabs/DisplayProps/DisplayPistol_TTI_PitViper.prefab",
    "UnityProject/Assets/JohnWickArsenal/Editor/JWA_DisplayPrefabGenerator.cs",
    "UnityProject/Assets/JohnWickArsenal/Editor/JohnWickArsenal.Cosmetic.Editor.asmdef",
]

for dir_path in required_dirs:
    if not os.path.isdir(dir_path):
        print('Missing directory: ' + dir_path)
        sys.exit(1)
    print('OK: ' + dir_path)

for file_path in required_files:
    if not os.path.isfile(file_path):
        print('Missing file: ' + file_path)
        sys.exit(1)
    print('OK: ' + file_path)

print('All required files and directories present')
