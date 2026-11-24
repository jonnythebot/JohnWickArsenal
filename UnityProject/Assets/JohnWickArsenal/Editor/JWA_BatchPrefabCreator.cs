using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Batch-mode compatible Auto Prefab Creator for John Wick Arsenal.
/// Does NOT inherit from EditorWindow - works in Unity batch mode.
/// </summary>
public static class JWA_BatchPrefabCreator
{
    private static string modelsPath = "Assets/JohnWickArsenal/Models";
    private static string resourcesPath = "Assets/JohnWickArsenal/Resources";
    private static int processedCount = 0;
    private static int errorCount = 0;
    private static List<string> logMessages = new List<string>();

    /// <summary>
    /// Main entry point for batch mode prefab creation.
    /// Called via: -executeMethod JWA_BatchPrefabCreator.RunFullPipeline
    /// </summary>
    public static void RunFullPipeline()
    {
        logMessages.Clear();
        processedCount = 0;
        errorCount = 0;

        Log("=== Starting Full Pipeline ===");
        Log($"Models Path: {modelsPath}");
        Log($"Resources Path: {resourcesPath}");

        try
        {
            ScanAndImportAll();
            CreateAllPrefabs();
            ValidateAllPrefabs();
            GenerateSpawnMenuJSON();

            Log("=== Full Pipeline Complete ===");
            Log($"Processed: {processedCount}, Errors: {errorCount}");

            if (errorCount > 0)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception e)
        {
            Log($"FATAL ERROR: {e.Message}", LogType.Error);
            Log(e.StackTrace, LogType.Error);
            EditorApplication.Exit(1);
        }
    }

    private static void ScanAndImportAll()
    {
        Log("Scanning for OBJ/GLB models...");

        if (!Directory.Exists(modelsPath))
        {
            Log($"Models path not found: {modelsPath}", LogType.Error);
            return;
        }

        var objFiles = Directory.GetFiles(modelsPath, "*.obj", SearchOption.AllDirectories);
        var glbFiles = Directory.GetFiles(modelsPath, "*.glb", SearchOption.AllDirectories);
        var allModelFiles = new List<string>();
        allModelFiles.AddRange(objFiles);
        allModelFiles.AddRange(glbFiles);

        Log($"Found {objFiles.Length} OBJ files, {glbFiles.Length} GLB files");

        foreach (var modelFile in allModelFiles)
        {
            try
            {
                string relativePath = modelFile.Replace(Application.dataPath, "Assets");
                Log($"Importing: {relativePath}");

                // Force reimport
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

                // Configure import settings
                ConfigureModelImportSettings(relativePath);

                processedCount++;
            }
            catch (Exception e)
            {
                Log($"Failed to import {modelFile}: {e.Message}", LogType.Error);
                errorCount++;
            }
        }

        AssetDatabase.Refresh();
        Log($"Import complete. Processed: {processedCount}, Errors: {errorCount}");
    }

    private static void ConfigureModelImportSettings(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null) return;

        // Configure for Quest/mobile
        importer.globalScale = 1.0f;
        importer.importMaterials = true;
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
        importer.importBlendShapeNormals = ModelImporterNormals.Import;
        importer.importVisibility = true;
        importer.importCameras = false;
        importer.importLights = false;

        // Optimize for mobile
        importer.meshCompression = ModelImporterMeshCompression.Medium;
        importer.optimizeMesh = true;
        importer.optimizeMeshPolygons = true;

        // Rig/Animation
        importer.animationType = ModelImporterAnimationType.None;
        importer.generateAnimations = ModelImporterGenerateAnimations.None;

        // Save
        importer.SaveAndReimport();
    }

    private static void CreateAllPrefabs()
    {
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
            AssetDatabase.Refresh();
        }

        var categories = new Dictionary<string, (string category, string weaponType, string behaviorType)>
        {
            // Pistols
            { "Pistols/HK_P30L", ("Pistols", "Pistol", "JohnWickPistolBehavior") },
            { "Pistols/Glock_34", ("Pistols", "Pistol", "JohnWickPistolBehavior") },
            { "Pistols/STI_2011_CombatMaster", ("Pistols", "Pistol", "JohnWickPistolBehavior") },
            { "Pistols/TTI_PitViper", ("Pistols", "Pistol", "JohnWickPistolBehavior") },

            // Shotguns
            { "Shotguns/Benelli_M4", ("Shotguns", "Shotgun", "JohnWickShotgunBehavior") },
            { "Shotguns/KelTec_KSG", ("Shotguns", "Shotgun", "JohnWickShotgunBehavior") },

            // Revolvers
            { "Revolvers/Remington_1875", ("Revolvers", "Revolver", "JohnWickRevolverBehavior") },

            // Rifles
            { "Rifles/TTI_TR1_Ultralight", ("Rifles", "Rifle", "JohnWickRifleBehavior") },

            // Throwables
            { "Throwables/ThrowingKnife", ("Throwables", "Throwable_Knife", "ThrowingKnifeBehavior") },
            { "Throwables/Pencil", ("Throwables", "Throwable_Pencil", "PencilBehavior") },
        };

        processedCount = 0;
        errorCount = 0;

        foreach (var kvp in categories)
        {
            string modelPath = kvp.Key;
            string category = kvp.Value.category;
            string weaponType = kvp.Value.weaponType;
            string behaviorType = kvp.Value.behaviorType;

            try
            {
                string glbPath = $"Assets/JohnWickArsenal/Models/{modelPath}.glb";
                string objPath = $"Assets/JohnWickArsenal/Models/{modelPath}.obj";
                string modelPathFull = File.Exists(glbPath) ? glbPath : objPath;

                if (!File.Exists(modelPathFull))
                {
                    Log($"Model not found: {glbPath} or {objPath}", LogType.Warning);
                    continue;
                }

                CreatePrefabForWeapon(modelPathFull, modelPath, category, weaponType, behaviorType);
                processedCount++;
            }
            catch (Exception e)
            {
                Log($"Failed to create prefab for {modelPath}: {e.Message}", LogType.Error);
                errorCount++;
            }
        }

        // Create equipment prefabs
        CreateEquipmentPrefabs();

        // Create suit prefabs
        CreateSuitPrefabs();

        AssetDatabase.Refresh();
        Log($"Prefab creation complete. Processed: {processedCount}, Errors: {errorCount}");
    }

    private static void CreatePrefabForWeapon(string modelFilePath, string modelPath, string category, string weaponType, string behaviorType)
    {
        string modelName = Path.GetFileNameWithoutExtension(modelPath);
        string weaponId = GenerateWeaponId(modelName, category);

        // Load the model
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelFilePath);
        if (modelPrefab == null)
        {
            Log($"Could not load model: {modelFilePath}", LogType.Error);
            return;
        }

        // Instantiate
        GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
        instance.name = weaponId;

        // Setup hierarchy
        SetupHierarchy(instance, weaponType);

        // Add components
        AddWeaponComponents(instance, weaponType, behaviorType);

        // Setup colliders
        SetupColliders(instance, weaponType);

        // Setup rigidbody
        SetupRigidbody(instance, weaponType);

        // Setup materials
        SetupMaterials(instance);

        // Save as prefab
        string categoryDir = $"Weapons/{category}";
        string fullDir = Path.Combine(resourcesPath, categoryDir);
        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        string prefabPath = Path.Combine(fullDir, $"{weaponId}.prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        UnityEngine.Object.DestroyImmediate(instance);

        // Set Addressable address
        SetAddressableAddress(prefab, $"Weapons/{category}/{modelName}");

        Log($"Created prefab: {prefabPath}");
    }

    private static void CreateEquipmentPrefabs()
    {
        var equipment = new Dictionary<string, (string category, string type)>
        {
            { "ShoulderHolster_Double", ("Holsters", "Holster") },
            { "HipHolster_TTIRace", ("Holsters", "Holster") },
            { "ThighHolster_Tactical", ("Holsters", "Holster") },
            { "AnkleHolster_Concealed", ("Holsters", "Holster") },
            { "ChestRig_MagCarrier", ("Pouches", "Pouch") },
            { "BeltPouches_4x", ("Pouches", "Pouch") },
            { "BenelliM4_ShellCarrier", ("Pouches", "Pouch") },
            { "KSG_DualTubeCarrier", ("Pouches", "Pouch") },
            { "KnifeBandolier", ("Pouches", "Pouch") },
            { "PencilCase", ("Pouches", "Pouch") },
        };

        foreach (var kvp in equipment)
        {
            string modelName = kvp.Key;
            string category = kvp.Value.category;
            string type = kvp.Value.type;

            string glbPath = $"Assets/JohnWickArsenal/Models/Equipment/{modelName}.glb";
            string objPath = $"Assets/JohnWickArsenal/Models/Equipment/{modelName}.obj";
            string modelFilePath = File.Exists(glbPath) ? glbPath : objPath;

            if (!File.Exists(modelFilePath)) continue;

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelFilePath);
            if (modelPrefab == null) continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            instance.name = modelName;

            SetupHierarchy(instance, "Equipment");
            AddEquipmentComponents(instance, type);
            SetupColliders(instance, "Equipment");
            SetupRigidbody(instance, "Equipment");
            SetupMaterials(instance);

            string categoryDir = $"Equipment/{category}";
            string fullDir = Path.Combine(resourcesPath, categoryDir);
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            string prefabPath = Path.Combine(fullDir, $"{modelName}.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);

            SetAddressableAddress(prefab, $"Equipment/{category}/{modelName}");

            Log($"Created equipment prefab: {prefabPath}");
        }
    }

    private static void CreateSuitPrefabs()
    {
        var suits = new Dictionary<string, string>
        {
            { "ClassicBlackSuit", "Suits" },
            { "BlackTacticalSuit", "Suits" },
            { "FormalThreePieceSuit", "Suits" },
            { "ArmoredCombatSuit", "Suits" },
            { "EliteHighProtectionSuit", "Suits" },
        };

        foreach (var kvp in suits)
        {
            string modelName = kvp.Key;
            string category = kvp.Value;

            string glbPath = $"Assets/JohnWickArsenal/Models/Suits/{modelName}.glb";
            string objPath = $"Assets/JohnWickArsenal/Models/Suits/{modelName}.obj";
            string modelFilePath = File.Exists(glbPath) ? glbPath : objPath;

            if (!File.Exists(modelFilePath)) continue;

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelFilePath);
            if (modelPrefab == null) continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            instance.name = modelName;

            SetupHierarchy(instance, "Suit");
            AddSuitComponents(instance);
            SetupColliders(instance, "Suit");
            SetupRigidbody(instance, "Suit");
            SetupMaterials(instance);

            string categoryDir = $"Suits/{category}";
            string fullDir = Path.Combine(resourcesPath, categoryDir);
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            string prefabPath = Path.Combine(fullDir, $"{modelName}.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);

            SetAddressableAddress(prefab, $"Suits/{category}/{modelName}");

            Log($"Created suit prefab: {prefabPath}");
        }
    }

    private static void SetupHierarchy(GameObject root, string weaponType)
    {
        // Ensure required transforms exist
        string[] requiredTransforms;

        if (weaponType == "Throwable_Knife" || weaponType == "Throwable_Pencil")
        {
            requiredTransforms = new[] { "Tip" };
        }
        else if (weaponType == "Equipment" || weaponType == "Suit")
        {
            requiredTransforms = new string[0];
        }
        else
        {
            requiredTransforms = new[] { "MuzzlePoint", "EjectionPort", "Grip_L", "Grip_R", "Sight_Front", "Sight_Rear" };
        }

        foreach (var tName in requiredTransforms)
        {
            if (root.transform.Find(tName) == null)
            {
                var t = new GameObject(tName).transform;
                t.SetParent(root.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
        }

        // Position transforms based on weapon type
        PositionTransforms(root, weaponType);
    }

    private static void PositionTransforms(GameObject root, string weaponType)
    {
        var bounds = GetBounds(root);
        var center = bounds.center;
        var size = bounds.size;

        // MuzzlePoint - front of barrel
        var muzzle = root.transform.Find("MuzzlePoint");
        if (muzzle != null)
        {
            muzzle.localPosition = new Vector3(0, center.y, center.z + size.z * 0.6f);
        }

        // EjectionPort - right side
        var ejection = root.transform.Find("EjectionPort");
        if (ejection != null)
        {
            ejection.localPosition = new Vector3(center.x + size.x * 0.3f, center.y + size.y * 0.2f, center.z);
        }

        // Grips
        var gripL = root.transform.Find("Grip_L");
        if (gripL != null)
        {
            gripL.localPosition = new Vector3(center.x - size.x * 0.2f, center.y - size.y * 0.3f, center.z - size.z * 0.2f);
        }

        var gripR = root.transform.Find("Grip_R");
        if (gripR != null)
        {
            gripR.localPosition = new Vector3(center.x + size.x * 0.2f, center.y - size.y * 0.3f, center.z - size.z * 0.2f);
        }

        // Sights
        var sightFront = root.transform.Find("Sight_Front");
        if (sightFront != null)
        {
            sightFront.localPosition = new Vector3(0, center.y + size.y * 0.4f, center.z + size.z * 0.4f);
        }

        var sightRear = root.transform.Find("Sight_Rear");
        if (sightRear != null)
        {
            sightRear.localPosition = new Vector3(0, center.y + size.y * 0.4f, center.z - size.z * 0.3f);
        }

        // Tip (for throwables)
        var tip = root.transform.Find("Tip");
        if (tip != null)
        {
            tip.localPosition = new Vector3(0, center.y, center.z + size.z * 0.6f);
        }
    }

    private static Bounds GetBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one);

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        // Convert to local space
        bounds.center = obj.transform.InverseTransformPoint(bounds.center);
        return bounds;
    }

    private static void AddWeaponComponents(GameObject root, string weaponType, string behaviorType)
    {
        // Add the appropriate behavior script
        var behaviorAssembly = typeof(JohnWickPistolBehavior).Assembly;
        var behaviorClass = behaviorAssembly.GetType($"JohnWickArsenal.Behaviors.{behaviorType}");

        if (behaviorClass != null)
        {
            root.AddComponent(behaviorClass);
        }
        else
        {
            Log($"Behavior type not found: {behaviorType}", LogType.Warning);
        }

        // Add base Weapon/Gun/Throwable component via reflection
#if !JWA_COSMETIC_ONLY
        var bsAsm = typeof(Weapon).Assembly;
        if (bsAsm != null)
        {
            if (weaponType == "Throwable_Knife" || weaponType == "Throwable_Pencil")
            {
                var throwableType = bsAsm.GetType("BladeAndSorcery.Gameplay.Throwable");
                if (throwableType != null && root.GetComponent(throwableType) == null)
                    root.AddComponent(throwableType);
            }
            else
            {
                var gunType = bsAsm.GetType("BladeAndSorcery.Gameplay.Gun");
                if (gunType != null && root.GetComponent(gunType) == null)
                    root.AddComponent(gunType);
            }
        }
#else
        Debug.Log("[JWA_BatchPrefabCreator] JWA_COSMETIC_ONLY defined - skipping B&S component setup.");
#endif
    }

    private static void AddEquipmentComponents(GameObject root, string type)
    {
#if !JWA_COSMETIC_ONLY
        var bsAsm = typeof(Weapon).Assembly;
        if (bsAsm != null)
        {
            var itemType = bsAsm.GetType("BladeAndSorcery.Gameplay.Item");
            if (itemType != null && root.GetComponent(itemType) == null)
                root.AddComponent(itemType);
        }
#else
        Debug.Log("[JWA_BatchPrefabCreator] JWA_COSMETIC_ONLY defined - skipping B&S equipment components.");
#endif
    }

    private static void AddSuitComponents(GameObject root)
    {
#if !JWA_COSMETIC_ONLY
        var bsAsm = typeof(Weapon).Assembly;
        if (bsAsm != null)
        {
            var itemType = bsAsm.GetType("BladeAndSorcery.Gameplay.Item");
            if (itemType != null && root.GetComponent(itemType) == null)
                root.AddComponent(itemType);
        }
#else
        Debug.Log("[JWA_BatchPrefabCreator] JWA_COSMETIC_ONLY defined - skipping B&S suit components.");
#endif
    }

    private static void SetupColliders(GameObject root, string weaponType)
    {
        if (root.GetComponent<Collider>() == null)
        {
            // Add appropriate collider based on weapon type
            var bounds = GetBounds(root);
            var boxCollider = root.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size * 1.1f;
        }

        // Add colliders to children if needed
        var renderers = root.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r.GetComponent<Collider>() == null && r != root.GetComponent<Renderer>())
            {
                var bounds = r.bounds;
                var localCenter = root.transform.InverseTransformPoint(bounds.center);
                var localSize = bounds.size;

                var childCollider = r.gameObject.AddComponent<BoxCollider>();
                childCollider.center = localCenter;
                childCollider.size = localSize * 1.05f;
            }
        }
    }

    private static void SetupRigidbody(GameObject root, string weaponType)
    {
        if (root.GetComponent<Rigidbody>() == null)
        {
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = weaponType switch
            {
                "Pistol" => 1.0f,
                "Revolver" => 1.2f,
                "Shotgun" => 3.5f,
                "Rifle" => 4.0f,
                "Throwable_Knife" => 0.3f,
                "Throwable_Pencil" => 0.05f,
                "Equipment" => 0.5f,
                "Suit" => 2.0f,
                _ => 1.0f
            };
            rb.drag = 0.1f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    private static void SetupMaterials(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null && material.shader != null)
                {
                    // Ensure materials use URP/Lit shader for Quest compatibility
                    if (material.shader.name.Contains("Standard") && !material.shader.name.Contains("URP"))
                    {
                        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                        if (urpLit != null)
                        {
                            material.shader = urpLit;
                        }
                    }
                }
            }
        }
    }

    private static string GenerateWeaponId(string modelName, string category)
    {
        string prefix = category switch
        {
            "Pistols" => "jwa_pistol",
            "Shotguns" => "jwa_shotgun",
            "Revolvers" => "jwa_revolver",
            "Rifles" => "jwa_rifle",
            "Throwables" => "jwa_throwable",
            _ => "jwa"
        };
        return $"{prefix}_{modelName.ToLower().Replace(" ", "_")}";
    }

    private static void SetAddressableAddress(GameObject prefab, string address)
    {
        try
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entry.address = address;
                entry.SetLabel("weapon", true, true);
            }
        }
        catch (Exception e)
        {
            Log($"Failed to set addressable address: {e.Message}", LogType.Warning);
        }
    }

    private static void ValidateAllPrefabs()
    {
        Log("Validating all prefabs...");
        var validator = new PrefabValidator();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { resourcesPath });

        int valid = 0, invalid = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (validator.ValidatePrefab(path, Log))
                valid++;
            else
                invalid++;
        }

        Log($"Validation complete: {valid} valid, {invalid} invalid");
    }

    private static void GenerateSpawnMenuJSON()
    {
        Log("Generating spawn menu JSON...");
        // This would generate the spawnmenu.json for B&S
        // Implementation depends on B&S spawn menu format
        Log("Spawn menu generation skipped (implement per B&S format)");
    }

    private static void Log(string message, LogType type = LogType.Log)
    {
        string prefix = "";
        switch (type)
        {
            case LogType.Error: prefix = "❌ "; break;
            case LogType.Warning: prefix = "⚠️ "; break;
            case LogType.Log: prefix = "ℹ️ "; break;
        }
        string msg = $"{prefix}{DateTime.Now:HH:mm:ss} {message}";
        logMessages.Add(msg);
        Debug.Log($"[JWA BatchPrefabCreator] {message}");
    }

    // ========================================================================
    // PREFAB VALIDATOR (same as JWA_BatchBuilder)
    // ========================================================================

    private class PrefabValidator
    {
        public bool ValidatePrefab(string prefabPath, Action<string> logCallback)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                logCallback?.Invoke($"<color=red>❌ {Path.GetFileName(prefabPath)}: Could not load</color>");
                return false;
            }

            bool hasErrors = false;

            // Check for missing scripts
            var components = prefab.GetComponentsInChildren<Component>(true);
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    logCallback?.Invoke($"<color=red>❌ {prefab.name}: Missing script on {GetGameObjectPath(comp?.gameObject)}</color>");
                    hasErrors = true;
                }
            }

            // Check for required components using reflection
            var asm = typeof(Weapon).Assembly;
            if (asm != null)
            {
                var weaponType = asm.GetType("BladeAndSorcery.Gameplay.Weapon");
                var itemType = asm.GetType("BladeAndSorcery.Gameplay.Item");
                var gunType = asm.GetType("BladeAndSorcery.Gameplay.Gun");
                var magazineType = asm.GetType("BladeAndSorcery.Gameplay.Magazine");
                var throwableType = asm.GetType("BladeAndSorcery.Gameplay.Throwable");

                bool isThrowable = prefab.GetComponent(itemType) != null;

                if (isThrowable)
                {
                    if (prefab.GetComponent(itemType) == null) { logCallback?.Invoke($"⚠ {prefab.name}: Missing Item"); hasErrors = true; }
                    if (prefab.GetComponent(throwableType) == null) { logCallback?.Invoke($"⚠ {prefab.name}: Missing Throwable"); hasErrors = true; }
                }
                else
                {
                    if (prefab.GetComponent(weaponType) == null) { logCallback?.Invoke($"⚠ {prefab.name}: Missing Weapon"); hasErrors = true; }
                    if (prefab.GetComponent(gunType) == null) { logCallback?.Invoke($"⚠ {prefab.name}: Missing Gun"); hasErrors = true; }
                    if (prefab.GetComponentInChildren(magazineType) == null) { logCallback?.Invoke($"⚠ {prefab.name}: Missing Magazine"); hasErrors = true; }
                }
            }

            // Check transforms
            string[] required = { "MuzzlePoint", "EjectionPort", "Grip_L", "Grip_R" };
            foreach (var tName in required)
            {
                if (prefab.transform.Find(tName) == null)
                {
                    logCallback?.Invoke($"⚠ {prefab.name}: Missing transform {tName}");
                    hasErrors = true;
                }
            }

            // Check collider
            if (prefab.GetComponent<Collider>() == null)
            {
                logCallback?.Invoke($"⚠ {prefab.name}: Missing root collider");
                hasErrors = true;
            }

            // Check rigidbody
            if (prefab.GetComponent<Rigidbody>() == null)
            {
                logCallback?.Invoke($"⚠ {prefab.name}: Missing Rigidbody");
                hasErrors = true;
            }

            if (!hasErrors)
            {
                logCallback?.Invoke($"<color=green>✅ {prefab.name}: Valid</color>");
            }

            return !hasErrors;
        }

        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";
            string path = obj.name;
            var current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
