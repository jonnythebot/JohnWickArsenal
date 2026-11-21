using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Automated prefab creator for John Wick Arsenal.
    /// Imports all generated OBJ models and creates properly configured prefabs.
    /// </summary>
    public class JWA_AutoPrefabCreator : EditorWindow
    {
        private Vector2 scrollPos;
        private bool autoImport = true;
        private bool autoCreatePrefabs = true;
        private bool autoSetupMaterials = true;
        private bool autoSetupColliders = true;
        private bool autoSetupRigidbodies = true;
        private bool autoRegisterInSpawnMenu = true;
        private string modelsPath = "Assets/JohnWickArsenal/Models";
        private string resourcesPath = "Assets/JohnWickArsenal/Resources";
        private int processedCount = 0;
        private int errorCount = 0;
        private List<string> logMessages = new List<string>();

        [MenuItem("John Wick Arsenal/Auto Prefab Creator", priority = 11)]
        public static void ShowWindow()
        {
            var window = GetWindow<JWA_AutoPrefabCreator>("JWA Auto Prefab Creator");
            window.minSize = new Vector2(500, 700);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(10);
            DrawHeader();

            EditorGUILayout.Space(10);
            DrawConfiguration();

            EditorGUILayout.Space(10);
            DrawActions();

            EditorGUILayout.Space(10);
            DrawLog();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("John Wick Arsenal - Auto Prefab Creator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Automatically imports OBJ models and creates B&S Nomad compatible prefabs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Models Path: {modelsPath}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Resources Path: {resourcesPath}", EditorStyles.miniLabel);
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            modelsPath = EditorGUILayout.TextField("Models Folder", modelsPath);
            resourcesPath = EditorGUILayout.TextField("Resources Output Folder", resourcesPath);

            EditorGUILayout.Space(5);
            autoImport = EditorGUILayout.Toggle("Auto Import Models", autoImport);
            autoCreatePrefabs = EditorGUILayout.Toggle("Auto Create Prefabs", autoCreatePrefabs);
            autoSetupMaterials = EditorGUILayout.Toggle("Auto Setup Materials", autoSetupMaterials);
            autoSetupColliders = EditorGUILayout.Toggle("Auto Setup Colliders", autoSetupColliders);
            autoSetupRigidbodies = EditorGUILayout.Toggle("Auto Setup Rigidbodies", autoSetupRigidbodies);
            autoRegisterInSpawnMenu = EditorGUILayout.Toggle("Auto Register in Spawn Menu", autoRegisterInSpawnMenu);

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            GUI.enabled = !EditorApplication.isCompiling;

            if (GUILayout.Button("🔍 Scan & Import All Models", GUILayout.Height(40)))
            {
                ScanAndImportAll();
            }

            if (GUILayout.Button("🔧 Create All Prefabs", GUILayout.Height(40)))
            {
                CreateAllPrefabs();
            }

            if (GUILayout.Button("🚀 Full Pipeline (Import + Prefabs + Validate)", GUILayout.Height(50)))
            {
                RunFullPipeline();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Log"))
            {
                logMessages.Clear();
            }
            if (GUILayout.Button("Open Resources Folder"))
            {
                OpenResourcesFolder();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate All Prefabs"))
            {
                ValidateAllPrefabs();
            }
            if (GUILayout.Button("Generate Spawn Menu JSON"))
            {
                GenerateSpawnMenuJSON();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawLog()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Log ({logMessages.Count} messages)", EditorStyles.boldLabel);

            var logStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                fontSize = 10,
                richText = true
            };

            string logText = string.Join("\n", logMessages);
            EditorGUILayout.TextArea(logText, logStyle, GUILayout.MinHeight(200), GUILayout.MaxHeight(300));

            EditorGUILayout.EndVertical();
        }

        private void Log(string message, LogType type = LogType.Log)
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
            Debug.Log($"[JWA AutoPrefab] {message}");
            Repaint();
        }

        private void ScanAndImportAll()
        {
            logMessages.Clear();
            processedCount = 0;
            errorCount = 0;

            if (!Directory.Exists(modelsPath))
            {
                Log($"Models path not found: {modelsPath}", LogType.Error);
                return;
            }

            Log("Scanning for OBJ models...");

            var objFiles = Directory.GetFiles(modelsPath, "*.obj", SearchOption.AllDirectories);
            Log($"Found {objFiles.Length} OBJ files");

            foreach (var objFile in objFiles)
            {
                try
                {
                    string relativePath = objFile.Replace(Application.dataPath, "Assets");
                    Log($"Importing: {relativePath}");

                    // Force reimport
                    AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

                    // Configure import settings
                    ConfigureModelImportSettings(relativePath);

                    processedCount++;
                }
                catch (Exception e)
                {
                    Log($"Failed to import {objFile}: {e.Message}", LogType.Error);
                    errorCount++;
                }
            }

            AssetDatabase.Refresh();
            Log($"Import complete. Processed: {processedCount}, Errors: {errorCount}");
        }

        private void ConfigureModelImportSettings(string assetPath)
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

        private void CreateAllPrefabs()
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
                    string objPath = $"Assets/JohnWickArsenal/Models/{modelPath}.obj";
                    if (!File.Exists(objPath))
                    {
                        Log($"Model not found: {objPath}", LogType.Warning);
                        continue;
                    }

                    CreatePrefabForWeapon(objPath, modelPath, category, weaponType, behaviorType);
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

        private void CreatePrefabForWeapon(string objPath, string modelPath, string category, string weaponType, string behaviorType)
        {
            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            string weaponId = GenerateWeaponId(modelName, category);

            // Load the model
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
            if (modelPrefab == null)
            {
                Log($"Could not load model: {objPath}", LogType.Error);
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
            if (autoSetupColliders)
                SetupColliders(instance, weaponType);

            // Setup rigidbody
            if (autoSetupRigidbodies)
                SetupRigidbody(instance, weaponType);

            // Setup materials
            if (autoSetupMaterials)
                SetupMaterials(instance);

            // Save as prefab
            string categoryDir = $"Weapons/{category}";
            string fullDir = Path.Combine(resourcesPath, categoryDir);
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            string prefabPath = Path.Combine(fullDir, $"{weaponId}.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            DestroyImmediate(instance);

            // Set Addressable address
            SetAddressableAddress(prefab, $"Weapons/{category}/{modelName}");

            Log($"Created prefab: {prefabPath}");
            EditorGUIUtility.PingObject(prefab);
        }

        private void CreateEquipmentPrefabs()
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

                string objPath = $"Assets/JohnWickArsenal/Models/Equipment/{modelName}.obj";
                if (!File.Exists(objPath)) continue;

                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
                if (modelPrefab == null) continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                instance.name = $"jwa_equipment_{modelName.ToLower()}";

                // Setup as equipment item
                SetupEquipmentHierarchy(instance, type);

                // Add components
                AddEquipmentComponents(instance, type);

                if (autoSetupColliders)
                    SetupEquipmentColliders(instance);

                if (autoSetupRigidbodies)
                    SetupEquipmentRigidbody(instance, type);

                if (autoSetupMaterials)
                    SetupMaterials(instance);

                string fullDir = Path.Combine(resourcesPath, $"Equipment/{category}");
                if (!Directory.Exists(fullDir))
                    Directory.CreateDirectory(fullDir);

                string prefabPath = Path.Combine(fullDir, $"{instance.name}.prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                SetAddressableAddress(prefab, $"Equipment/{category}/{modelName}");
                Log($"Created equipment prefab: {prefabPath}");
                processedCount++;
            }
        }

        private void CreateSuitPrefabs()
        {
            var suits = new[]
            {
                "ClassicBlackSuit",
                "BlackTacticalSuit",
                "FormalThreePieceSuit",
                "ArmoredCombatSuit",
                "EliteHighProtectionSuit"
            };

            foreach (string modelName in suits)
            {
                string objPath = $"Assets/JohnWickArsenal/Models/Suits/{modelName}.obj";
                if (!File.Exists(objPath)) continue;

                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
                if (modelPrefab == null) continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                instance.name = $"jwa_suit_{modelName.ToLower()}";

                // Setup as wearable suit
                SetupSuitHierarchy(instance);

                // Add components
                AddSuitComponents(instance);

                if (autoSetupColliders)
                    SetupEquipmentColliders(instance);

                if (autoSetupRigidbodies)
                {
                    var rb = instance.GetComponent<Rigidbody>();
                    if (rb == null) rb = instance.AddComponent<Rigidbody>();
                    rb.mass = 3f;
                    rb.useGravity = true;
                }

                if (autoSetupMaterials)
                    SetupMaterials(instance);

                string fullDir = Path.Combine(resourcesPath, "Equipment/Suits");
                if (!Directory.Exists(fullDir))
                    Directory.CreateDirectory(fullDir);

                string prefabPath = Path.Combine(fullDir, $"{instance.name}.prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                SetAddressableAddress(prefab, $"Equipment/Suits/{modelName}");
                Log($"Created suit prefab: {prefabPath}");
                processedCount++;
            }
        }

        private void SetupHierarchy(GameObject root, string weaponType)
        {
            // Ensure Model container
            Transform modelContainer = root.transform.Find("Model");
            if (modelContainer == null)
            {
                modelContainer = new GameObject("Model").transform;
                modelContainer.SetParent(root.transform);
                modelContainer.localPosition = Vector3.zero;
                modelContainer.localRotation = Quaternion.identity;
                modelContainer.localScale = Vector3.one;

                // Move all mesh renderers under Model
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    if (r.transform != root.transform && r.transform.parent == root.transform)
                    {
                        r.transform.SetParent(modelContainer);
                    }
                }
            }

            // Create required transforms based on weapon type
            CreateTransformIfMissing(root, "MuzzlePoint", GetMuzzlePosition(weaponType));
            CreateTransformIfMissing(root, "EjectionPort", GetEjectionPortPosition(weaponType));
            CreateTransformIfMissing(root, "Grip_L", GetLeftGripPosition(weaponType));
            CreateTransformIfMissing(root, "Grip_R", GetRightGripPosition(weaponType));

            if (weaponType == "Rifle" || weaponType == "Shotgun")
            {
                CreateTransformIfMissing(root, "Sight_Front", GetFrontSightPosition(weaponType));
                CreateTransformIfMissing(root, "Sight_Rear", GetRearSightPosition(weaponType));
            }

            if (weaponType == "Throwable_Knife" || weaponType == "Throwable_Pencil")
            {
                CreateTransformIfMissing(root, "Tip", GetTipPosition(weaponType));
            }
        }

        private void CreateTransformIfMissing(GameObject root, string name, Vector3 localPos)
        {
            if (root.transform.Find(name) == null)
            {
                var t = new GameObject(name).transform;
                t.SetParent(root.transform);
                t.localPosition = localPos;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
        }

        private void SetupEquipmentHierarchy(GameObject root, string type)
        {
            // Equipment root transform
            CreateTransformIfMissing(root, "AttachPoint", Vector3.zero);

            if (type == "Holster")
            {
                CreateTransformIfMissing(root, "WeaponSlot", new Vector3(0, -0.1f, 0.05f));
                CreateTransformIfMissing(root, "RetentionPoint", new Vector3(0, 0.05f, 0.02f));
            }
            else if (type == "Pouch")
            {
                CreateTransformIfMissing(root, "ItemSlot", new Vector3(0, 0.05f, 0.02f));
            }
        }

        private void SetupSuitHierarchy(GameObject root)
        {
            // Suit attachment points for equipment
            CreateTransformIfMissing(root, "ChestAttach", new Vector3(0, 0.15f, 0.1f));
            CreateTransformIfMissing(root, "BackAttach", new Vector3(0, 0.15f, -0.1f));
            CreateTransformIfMissing(root, "LeftHipAttach", new Vector3(-0.15f, -0.05f, 0));
            CreateTransformIfMissing(root, "RightHipAttach", new Vector3(0.15f, -0.05f, 0));
            CreateTransformIfMissing(root, "LeftThighAttach", new Vector3(-0.15f, -0.35f, 0));
            CreateTransformIfMissing(root, "RightThighAttach", new Vector3(0.15f, -0.35f, 0));
        }

        private void AddWeaponComponents(GameObject root, string weaponType, string behaviorType)
        {
            bool isThrowable = weaponType.StartsWith("Throwable");

            if (isThrowable)
            {
                // Item/Throwable components
                var item = root.GetComponent<Item>();
                if (item == null) item = root.AddComponent<Item>();

                var throwable = root.GetComponent<Throwable>();
                if (throwable == null) throwable = root.AddComponent<Throwable>();

                // Custom behavior
                Type behaviorClass = Type.GetType($"JohnWickArsenal.Behaviors.{behaviorType}");
                if (behaviorClass != null)
                {
                    var behavior = root.GetComponent(behaviorClass);
                    if (behavior == null) root.AddComponent(behaviorClass);
                }
            }
            else
            {
                // Weapon/Gun components
                var weapon = root.GetComponent<Weapon>();
                if (weapon == null) weapon = root.AddComponent<Weapon>();

                var gun = root.GetComponent<Gun>();
                if (gun == null) gun = root.AddComponent<Gun>();

                // Magazine (find magazine model child)
                Transform magTransform = root.transform.Find("Model/Magazine") ?? root.transform.Find("Magazine");
                if (magTransform != null)
                {
                    var magazine = magTransform.gameObject.GetComponent<Magazine>();
                    if (magazine == null) magTransform.gameObject.AddComponent<Magazine>();
                }

                // Custom behavior
                Type behaviorClass = Type.GetType($"JohnWickArsenal.Behaviors.{behaviorType}");
                if (behaviorClass != null)
                {
                    var behavior = root.GetComponent(behaviorClass);
                    if (behavior == null) root.AddComponent(behaviorClass);
                }
            }

            // AudioSource
            var audio = root.GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = root.AddComponent<AudioSource>();
                audio.spatialBlend = 1.0f;
                audio.maxDistance = 50f;
                audio.rolloffMode = AudioRolloffMode.Logarithmic;
                audio.dopplerLevel = 0.5f;
            }
        }

        private void AddEquipmentComponents(GameObject root, string type)
        {
            var item = root.GetComponent<Item>();
            if (item == null) item = root.AddComponent<Item>();

            // Equipment behavior would be added here
            // For now, just ensure it's an Item
        }

        private void AddSuitComponents(GameObject root)
        {
            var item = root.GetComponent<Item>();
            if (item == null) item = root.AddComponent<Item>();

            // Suit behavior would be added here
        }

        private void SetupColliders(GameObject root, string weaponType)
        {
            // Remove existing colliders except root
            var existingColliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in existingColliders)
            {
                if (c.gameObject != root)
                    DestroyImmediate(c);
            }

            // Add convex mesh collider to Model
            var model = root.transform.Find("Model");
            if (model != null)
            {
                var renderers = model.GetComponentsInChildren<MeshRenderer>();
                foreach (var r in renderers)
                {
                    var mf = r.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        var mc = r.gameObject.AddComponent<MeshCollider>();
                        mc.convex = true;
                        mc.cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
                    }
                }
            }

            // Root box collider for handling
            if (root.GetComponent<Collider>() == null)
            {
                var bc = root.AddComponent<BoxCollider>();
                bc.size = EstimateWeaponBounds(root);
            }
        }

        private void SetupEquipmentColliders(GameObject root)
        {
            var existingColliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in existingColliders)
            {
                if (c.gameObject != root)
                    DestroyImmediate(c);
            }

            // Root box collider
            if (root.GetComponent<Collider>() == null)
            {
                var bc = root.AddComponent<BoxCollider>();
                bc.size = EstimateWeaponBounds(root);
            }
        }

        private void SetupRigidbody(GameObject root, string weaponType)
        {
            var rb = root.GetComponent<Rigidbody>();
            if (rb == null) rb = root.AddComponent<Rigidbody>();

            float mass = GetWeaponMass(weaponType);
            rb.mass = mass;
            rb.drag = 0.1f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void SetupEquipmentRigidbody(GameObject root, string type)
        {
            var rb = root.GetComponent<Rigidbody>();
            if (rb == null) rb = root.AddComponent<Rigidbody>();

            float mass = type == "Holster" ? 0.3f : 0.5f;
            rb.mass = mass;
            rb.drag = 0.1f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void SetupMaterials(GameObject root)
        {
            // Materials are imported from MTL files
            // Ensure they use URP/Lit shader
            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null && mat.shader != null)
                    {
                        // Check if shader is compatible with URP
                        if (mat.shader.name.Contains("Standard") || mat.shader.name.Contains("Universal"))
                        {
                            // Good - already URP compatible
                        }
                        else
                        {
                            // Try to upgrade to URP/Lit
                            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                            if (urpLit != null)
                            {
                                mat.shader = urpLit;
                            }
                        }
                    }
                }
            }
        }

        private Vector3 GetMuzzlePosition(string weaponType)
        {
            switch (weaponType)
            {
                case "Pistol": return new Vector3(0, 0.02f, 0.12f);
                case "Shotgun": return new Vector3(0, 0.03f, 0.45f);
                case "Revolver": return new Vector3(0, 0.015f, 0.18f);
                case "Rifle": return new Vector3(0, 0.04f, 0.4f);
                default: return new Vector3(0, 0, 0.1f);
            }
        }

        private Vector3 GetEjectionPortPosition(string weaponType)
        {
            switch (weaponType)
            {
                case "Pistol": return new Vector3(0.03f, 0.05f, 0.05f);
                case "Shotgun": return new Vector3(0.05f, 0.06f, 0.1f);
                case "Revolver": return new Vector3(0.04f, 0, 0);
                case "Rifle": return new Vector3(0.04f, 0.06f, 0.1f);
                default: return new Vector3(0.03f, 0.03f, 0);
            }
        }

        private Vector3 GetLeftGripPosition(string weaponType)
        {
            return new Vector3(-0.02f, -0.05f, 0.02f);
        }

        private Vector3 GetRightGripPosition(string weaponType)
        {
            return new Vector3(0.02f, -0.05f, 0.02f);
        }

        private Vector3 GetFrontSightPosition(string weaponType)
        {
            return weaponType == "Shotgun" ? new Vector3(0, 0.06f, 0.5f) : new Vector3(0, 0.06f, 0.3f);
        }

        private Vector3 GetRearSightPosition(string weaponType)
        {
            return weaponType == "Shotgun" ? new Vector3(0, 0.06f, 0.15f) : new Vector3(0, 0.06f, 0.1f);
        }

        private Vector3 GetTipPosition(string weaponType)
        {
            return weaponType == "Throwable_Pencil" ? new Vector3(0, 0, 0.19f) : new Vector3(0, 0, 0.15f);
        }

        private Vector3 EstimateWeaponBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Vector3(0.1f, 0.15f, 0.3f);

            Bounds bounds = new Bounds();
            bool first = true;
            foreach (var r in renderers)
            {
                if (first) { bounds = r.bounds; first = false; }
                else bounds.Encapsulate(r.bounds);
            }
            return bounds.size;
        }

        private float GetWeaponMass(string weaponType)
        {
            switch (weaponType)
            {
                case "Pistol": return 0.8f;
                case "Shotgun": return 3.5f;
                case "Revolver": return 1.1f;
                case "Rifle": return 2.3f;
                case "Throwable_Knife": return 0.15f;
                case "Throwable_Pencil": return 0.01f;
                default: return 1f;
            }
        }

        private string GenerateWeaponId(string modelName, string category)
        {
            string clean = modelName
                .Replace(" ", "_")
                .Replace("-", "_")
                .ToLower();

            string prefix = category.ToLower().Replace(" ", "_");
            return $"jwa_{prefix}_{clean}";
        }

        private void SetAddressableAddress(GameObject prefab, string address)
        {
            // Mark as Addressable
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entry.address = address;
                entry.SetLabel("Weapon", true, true);
            }
        }

        private void ValidateAllPrefabs()
        {
            Log("Validating all prefabs...");
            int valid = 0, invalid = 0;

            var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { resourcesPath });
            foreach (var guid in prefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (ValidatePrefab(prefab))
                {
                    valid++;
                    Log($"✓ Valid: {path}");
                }
                else
                {
                    invalid++;
                    Log($"✗ Invalid: {path}", LogType.Warning);
                }
            }

            Log($"Validation complete. Valid: {valid}, Invalid: {invalid}");
        }

        private bool ValidatePrefab(GameObject prefab)
        {
            if (prefab == null) return false;

            // Check required components
            bool hasWeapon = prefab.GetComponent<Weapon>() != null;
            bool hasItem = prefab.GetComponent<Item>() != null;
            bool hasRigidbody = prefab.GetComponent<Rigidbody>() != null;
            bool hasCollider = prefab.GetComponent<Collider>() != null;

            // Check required transforms
            bool hasMuzzle = prefab.transform.Find("MuzzlePoint") != null;
            bool hasEjection = prefab.transform.Find("EjectionPort") != null;
            bool hasGripL = prefab.transform.Find("Grip_L") != null;
            bool hasGripR = prefab.transform.Find("Grip_R") != null;

            // Must have either Weapon or Item
            if (!hasWeapon && !hasItem) return false;

            return hasRigidbody && hasCollider && hasMuzzle && hasGripL && hasGripR;
        }

        private void GenerateSpawnMenuJSON()
        {
            Log("Generating spawn menu JSON...");

            var spawnMenu = new
            {
                category = "John Wick Arsenal",
                subcategories = new[]
                {
                    new { name = "Pistols", items = new[] { "jwa_pistols_hk_p30l", "jwa_pistols_glock_34", "jwa_pistols_sti_2011_combat_master", "jwa_pistols_tti_pit_viper" } },
                    new { name = "Shotguns", items = new[] { "jwa_shotguns_benelli_m4", "jwa_shotguns_keltec_ksg" } },
                    new { name = "Revolvers", items = new[] { "jwa_revolvers_remington_1875" } },
                    new { name = "Rifles", items = new[] { "jwa_rifles_tti_tr1_ultralight" } },
                    new { name = "Throwables", items = new[] { "jwa_throwables_throwing_knife", "jwa_throwables_pencil" } },
                    new { name = "Suits", items = new[] { "jwa_suit_classicblacksuit", "jwa_suit_blacktacticalsuit", "jwa_suit_formalthreepiecesuit", "jwa_suit_armoredcombatsuit", "jwa_suit_elitehighprotectionsuit" } },
                    new { name = "Holsters", items = new[] { "jwa_equipment_shoulderholster_double", "jwa_equipment_hipholster_tti_race", "jwa_equipment_thighholster_tactical", "jwa_equipment_ankleholster_concealed" } },
                    new { name = "Pouches", items = new[] { "jwa_equipment_chestrig_magcarrier", "jwa_equipment_beltpouches_4x", "jwa_equipment_benellim4_shellcarrier", "jwa_equipment_ksg_dualtubecarrier", "jwa_equipment_knifebandolier", "jwa_equipment_pencilcase" } }
                }
            };

            string json = JsonUtility.ToJson(spawnMenu, true);
            string path = "Assets/JohnWickArsenal/ModConfig/spawnmenu_generated.json";
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Log($"Generated: {path}");
        }

        private void OpenResourcesFolder()
        {
            string fullPath = Path.GetFullPath(resourcesPath);
            if (Directory.Exists(fullPath))
                EditorUtility.RevealInFinder(fullPath);
            else
                Log("Resources folder doesn't exist yet", LogType.Warning);
        }

        private void RunFullPipeline()
        {
            Log("=== Starting Full Pipeline ===");
            ScanAndImportAll();
            CreateAllPrefabs();
            ValidateAllPrefabs();
            GenerateSpawnMenuJSON();
            Log("=== Full Pipeline Complete ===");
        }

        /// <summary>
        /// Static entry point for command-line execution via Unity batch mode.
        /// </summary>
        public static void RunFullPipeline()
        {
            var window = CreateInstance<JWA_AutoPrefabCreator>();
            window.autoImport = true;
            window.autoCreatePrefabs = true;
            window.autoSetupMaterials = true;
            window.autoSetupColliders = true;
            window.autoSetupRigidbodies = true;
            window.autoRegisterInSpawnMenu = true;
            window.modelsPath = "Assets/JohnWickArsenal/Models";
            window.resourcesPath = "Assets/JohnWickArsenal/Resources";
            window.RunFullPipeline();
        }
    }
}