using System.IO;
using UnityEditor;
using UnityEngine;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Generates nonfunctional cosmetic display prefabs from GLB models.
    /// Run via: Tools > John Wick Arsenal > Generate Display Prefabs
    /// </summary>
    public class JWA_DisplayPrefabGenerator : EditorWindow
    {
        [MenuItem("Tools/John Wick Arsenal/Generate Display Prefabs")]
        public static void GenerateAllDisplayPrefabs()
        {
            string basePath = "Assets/JohnWickArsenal/Prefabs/DisplayModels";
            string modelsPath = "Assets/JohnWickArsenal/Models/DisplayModels";

            // Create directory structure
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/JohnWickArsenal/Prefabs", "DisplayModels");
            }

            int created = 0;

            // 1. TTI Pit Viper (already exists as DisplayPistol_TTI_PitViper)
            created += CreateDisplayPrefab(basePath, modelsPath, 
                "TTI_PitViper", "Models/Pistols/TTI_PitViper.glb",
                "DisplayPistol_TTI_PitViper", "Pistol");

            // 2. Glock 34
            created += CreateDisplayPrefab(basePath, modelsPath,
                "Glock_34", "Models/Pistols/Glock_34.glb",
                "DisplayPistol_Glock_34", "Pistol");

            // 3. AR-15 (from Vinish assault-rifle)
            created += CreateDisplayPrefab(basePath, modelsPath,
                "AR15_AssaultRifle", "Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb",
                "DisplayRifle_AR15", "Rifle");

            // 4. AWP Sniper Rifle (from Vinish sniper-rifle)
            created += CreateDisplayPrefab(basePath, modelsPath,
                "AWP_SniperRifle", "Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb",
                "DisplayRifle_AWP", "Rifle");

            // 5. MP5 (from Vinish submachine-gun)
            created += CreateDisplayPrefab(basePath, modelsPath,
                "MP5_SubmachineGun", "Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb",
                "DisplaySMG_MP5", "SMG");

            // 6. Remington 870 (from Vinish pump-shotgun)
            created += CreateDisplayPrefab(basePath, modelsPath,
                "Remington870_PumpShotgun", "Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb",
                "DisplayShotgun_Remington870", "Shotgun");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[JWA_DisplayPrefabGenerator] Created {created} display prefabs at {basePath}");
            EditorUtility.DisplayDialog("Complete", $"Generated {created} display prefabs!", "OK");
        }

        static int CreateDisplayPrefab(string basePath, string modelsPath, string modelName, string modelRelativePath, string prefabName, string weaponType)
        {
            string fullModelPath = $"{modelsPath}/{modelRelativePath}";
            string prefabPath = $"{basePath}/{prefabName}.prefab";

            // Check if model exists
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fullModelPath);
            if (modelAsset == null)
            {
                Debug.LogWarning($"[JWA_DisplayPrefabGenerator] Model not found: {fullModelPath}");
                return 0;
            }

            // Check if prefab already exists
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                Debug.Log($"[{prefabName}] Already exists, skipping");
                return 0;
            }

            // Instantiate model
            var instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[{prefabName}] Failed to instantiate model");
                return 0;
            }

            instance.name = prefabName;

            // Add display component
            var display = instance.AddComponent<JWA_DisplayModel>();
            display.modelName = modelName;
            display.weaponType = weaponType;
            display.isCosmeticOnly = true;

            // Create hand attachment point
            var handAttach = new GameObject("HandAttachPoint");
            handAttach.transform.SetParent(instance.transform);
            handAttach.transform.localPosition = Vector3.zero;
            handAttach.transform.localRotation = Quaternion.identity;
            display.handAttachPoint = handAttach.transform;

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            DestroyImmediate(instance);

            Debug.Log($"[{prefabName}] Created at {prefabPath}");
            return 1;
        }

        [MenuItem("Tools/John Wick Arsenal/Validate Display Models")]
        public static void ValidateDisplayModels()
        {
            string modelsPath = "Assets/JohnWickArsenal/Models/DisplayModels";
            var guids = AssetDatabase.FindAssets("t:GameObject", new[] { modelsPath });
            
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null)
                {
                    Debug.Log($"[Validate] Found: {path}");
                }
            }
            EditorUtility.DisplayDialog("Validation", $"Found {guids.Length} model assets in {modelsPath}", "OK");
        }
    }

    /// <summary>
    /// Component identifying a nonfunctional cosmetic display model.
    /// </summary>
    public class JWA_DisplayModel : MonoBehaviour
    {
        public string modelName;
        public string weaponType; // Pistol, Rifle, SMG, Shotgun, etc.
        public bool isCosmeticOnly = true;
        public Transform handAttachPoint; // Aligns with NPC/player hand bone

        private void Awake()
        {
            // Ensure cosmetic-only
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }
        }

        /// <summary>
        /// Attach this display model to a hand bone.
        /// </summary>
        public void AttachToHand(Transform handBone)
        {
            if (handAttachPoint == null || handBone == null)
            {
                Debug.LogError($"[{modelName}] Missing handAttachPoint or handBone");
                return;
            }

            transform.SetParent(handBone, false);
            transform.position = handBone.position;
            transform.rotation = handBone.rotation;
            transform.localPosition = -handAttachPoint.localPosition;
            transform.localRotation = Quaternion.Inverse(handAttachPoint.localRotation);
        }
    }
}
