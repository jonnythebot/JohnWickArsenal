using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Automated prefab creation for John Wick Arsenal weapons.
/// Supports all weapon types with proper component setup, collider generation, and validation.
/// </summary>
public class JWA_PrefabSetupWizard : EditorWindow
{
    private const string MOD_NAME = "JohnWickArsenal";
    private const string RESOURCES_ROOT = "Assets/JohnWickArsenal/Resources";

    // State
    private Vector2 scrollPos;
    private GameObject selectedModel;
    private WeaponType weaponType = WeaponType.Pistol;
    private string weaponId = "";
    private string weaponName = "";
    private bool autoCreateHierarchy = true;
    private bool autoAddComponents = true;
    private bool autoSetupColliders = true;
    private bool autoSetupRigidbody = true;
    private bool saveToResources = true;
    private bool validateAfterCreate = true;

    // Weapon definitions
    private static readonly WeaponDefinition[] WeaponDefinitions = new[]
    {
        new WeaponDefinition { id = "jwa_hk_p30l", name = "H&K P30L", type = WeaponType.Pistol, caliber = "9x19mm", capacity = 15, mass = 0.8f },
        new WeaponDefinition { id = "jwa_glock_34", name = "Glock 34", type = WeaponType.Pistol, caliber = "9x19mm", capacity = 17, mass = 0.78f },
        new WeaponDefinition { id = "jwa_sti_2011_combat_master", name = "STI 2011 Combat Master", type = WeaponType.Pistol, caliber = ".40 S&W", capacity = 20, mass = 1.15f },
        new WeaponDefinition { id = "jwa_tti_pit_viper", name = "TTI Pit Viper", type = WeaponType.Pistol, caliber = "9mm Major", capacity = 21, mass = 1.1f },
        new WeaponDefinition { id = "jwa_benelli_m4", name = "Benelli M4", type = WeaponType.Shotgun, caliber = "12 Gauge", capacity = 7, mass = 3.78f },
        new WeaponDefinition { id = "jwa_keltec_ksg", name = "Kel-Tec KSG", type = WeaponType.Shotgun, caliber = "12 Gauge", capacity = 14, mass = 3.1f },
        new WeaponDefinition { id = "jwa_remington_1875", name = "Remington 1875", type = WeaponType.Revolver, caliber = ".45 Colt", capacity = 6, mass = 1.1f },
        new WeaponDefinition { id = "jwa_tti_tr1_ultralight", name = "TTI TR-1 Ultralight", type = WeaponType.Rifle, caliber = "5.56x45mm", capacity = 30, mass = 2.3f },
        new WeaponDefinition { id = "jwa_throwing_knife", name = "Throwing Knife", type = WeaponType.Throwable, caliber = "", capacity = 5, mass = 0.15f },
        new WeaponDefinition { id = "jwa_pencil", name = "Pencil", type = WeaponType.Throwable, caliber = "", capacity = 3, mass = 0.01f }
    };

    private enum WeaponType
    {
        Pistol,
        Shotgun,
        Revolver,
        Rifle,
        Throwable
    }

    private class WeaponDefinition
    {
        public string id;
        public string name;
        public WeaponType type;
        public string caliber;
        public int capacity;
        public float mass;
    }

    [MenuItem("John Wick Arsenal/Prefab Setup Wizard", priority = 10)]
    public static void ShowWindow()
    {
        var window = GetWindow<JWA_PrefabSetupWizard>("JWA Prefab Wizard");
        window.minSize = new Vector2(500, 800);
        window.Show();
    }

    private void OnEnable()
    {
        // Try to load last used model
        if (string.IsNullOrEmpty(weaponId))
        {
            var lastModel = EditorPrefs.GetString("JWA_LastModel", "");
            if (!string.IsNullOrEmpty(lastModel))
            {
                selectedModel = AssetDatabase.LoadAssetAtPath<GameObject>(lastModel);
            }
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        DrawModelSelection();
        DrawWeaponSelection();
        DrawOptions();
        DrawActions();
        DrawBatchOperations();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("John Wick Arsenal - Prefab Setup Wizard", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Create properly configured weapon prefabs for B&S Nomad", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(5);
    }

    private void DrawModelSelection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("1. Source Model", EditorStyles.boldLabel);

        var newModel = (GameObject)EditorGUILayout.ObjectField(
            "Model (FBX/OBJ/Prefab)",
            selectedModel,
            typeof(GameObject),
            false
        );

        if (newModel != selectedModel)
        {
            selectedModel = newModel;
            if (selectedModel != null)
            {
                EditorPrefs.SetString("JWA_LastModel", AssetDatabase.GetAssetPath(selectedModel));
                // Auto-detect weapon type from model name
                AutoDetectWeaponType();
            }
        }

        if (selectedModel != null)
        {
            var path = AssetDatabase.GetAssetPath(selectedModel);
            EditorGUILayout.HelpBox($"Selected: {selectedModel.name}\nPath: {path}\nType: {GetModelType(path)}", MessageType.Info);

            // Show model info
            var renderers = selectedModel.GetComponentsInChildren<Renderer>(true);
            var totalTris = 0;
            foreach (var r in renderers)
            {
                if (r is MeshRenderer mr && mr.GetComponent<MeshFilter>() != null)
                {
                    totalTris += mr.GetComponent<MeshFilter>().sharedMesh?.triangles.Length / 3 ?? 0;
                }
                else if (r is SkinnedMeshRenderer smr)
                {
                    totalTris += smr.sharedMesh?.triangles.Length / 3 ?? 0;
                }
            }

            var bounds = GetModelBounds(selectedModel);
            EditorGUILayout.LabelField($"Triangles: ~{totalTris:N0}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Bounds: {bounds.size}", EditorStyles.miniLabel);

            if (totalTris > 25000)
            {
                EditorGUILayout.HelpBox("⚠ High triangle count for Quest. Consider LODs or optimization.", MessageType.Warning);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawWeaponSelection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("2. Weapon Configuration", EditorStyles.boldLabel);

        // Weapon type dropdown
        var newType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);
        if (newType != weaponType)
        {
            weaponType = newType;
            UpdateWeaponIdFromType();
        }

        // Weapon definition selection
        var definitions = WeaponDefinitions.Where(d => d.type == weaponType).ToArray();
        if (definitions.Length > 0)
        {
            var names = definitions.Select(d => $"{d.name} ({d.id})").ToArray();
            var currentIndex = Array.FindIndex(definitions, d => d.id == weaponId);
            if (currentIndex < 0) currentIndex = 0;

            var newIndex = EditorGUILayout.Popup("Weapon Definition", currentIndex, names);
            if (newIndex != currentIndex || string.IsNullOrEmpty(weaponId))
            {
                var def = definitions[newIndex];
                weaponId = def.id;
                weaponName = def.name;
            }
        }

        weaponId = EditorGUILayout.TextField("Weapon ID", weaponId);
        weaponName = EditorGUILayout.TextField("Display Name", weaponName);

        EditorGUILayout.LabelField("Format: jwa_<category>_<name> (e.g., jwa_hk_p30l)", EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }

    private void DrawOptions()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("3. Auto-Setup Options", EditorStyles.boldLabel);

        autoCreateHierarchy = EditorGUILayout.ToggleLeft("Create standard hierarchy (Model, Transforms)", autoCreateHierarchy);
        autoAddComponents = EditorGUILayout.ToggleLeft("Add required components (Weapon/Gun/Item, Behaviors)", autoAddComponents);
        autoSetupColliders = EditorGUILayout.ToggleLeft("Generate colliders (convex mesh + box)", autoSetupColliders);
        autoSetupRigidbody = EditorGUILayout.ToggleLeft("Configure Rigidbody (mass, interpolation)", autoSetupRigidbody);
        saveToResources = EditorGUILayout.ToggleLeft("Save to Resources folder (for Addressables)", saveToResources);
        validateAfterCreate = EditorGUILayout.ToggleLeft("Validate prefab after creation", validateAfterCreate);

        EditorGUILayout.EndVertical();
    }

    private void DrawActions()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("4. Actions", EditorStyles.boldLabel);

        var canCreate = selectedModel != null && !string.IsNullOrEmpty(weaponId) && !string.IsNullOrEmpty(weaponName);
        GUI.enabled = canCreate && !EditorApplication.isCompiling;

        if (GUILayout.Button("🔧 Create Prefab", GUILayout.Height(45)))
        {
            CreatePrefab();
        }

        GUI.enabled = true;

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📋 Copy Component List"))
        {
            CopyComponentList();
        }
        if (GUILayout.Button("🔍 Validate Existing"))
        {
            ValidateExistingPrefabs();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawBatchOperations()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Create all 10 weapon prefabs at once:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("Requires models in Assets/JohnWickArsenal/Models/ with matching names", EditorStyles.miniLabel);

        if (GUILayout.Button("📦 Create All Weapon Prefabs (Batch)"))
        {
            CreateAllPrefabsBatch();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("🧹 Clean Invalid Prefabs"))
        {
            CleanInvalidPrefabs();
        }

        EditorGUILayout.EndVertical();
    }

    // ========================================================================
    // IMPLEMENTATION
    // ========================================================================

    private void AutoDetectWeaponType()
    {
        if (selectedModel == null) return;

        var name = selectedModel.name.ToLower();

        if (name.Contains("p30l") || name.Contains("glock") || name.Contains("sti") || name.Contains("pitviper") || name.Contains("pistol"))
            weaponType = WeaponType.Pistol;
        else if (name.Contains("benelli") || name.Contains("m4") || name.Contains("ksg") || name.Contains("shotgun"))
            weaponType = WeaponType.Shotgun;
        else if (name.Contains("remington") || name.Contains("1875") || name.Contains("revolver"))
            weaponType = WeaponType.Revolver;
        else if (name.Contains("tr1") || name.Contains("ultralight") || name.Contains("rifle") || name.Contains("ar15"))
            weaponType = WeaponType.Rifle;
        else if (name.Contains("knife") || name.Contains("pencil") || name.Contains("throw"))
            weaponType = WeaponType.Throwable;

        UpdateWeaponIdFromType();
    }

    private void UpdateWeaponIdFromType()
    {
        var def = WeaponDefinitions.FirstOrDefault(d => d.type == weaponType);
        if (def != null && (string.IsNullOrEmpty(weaponId) || weaponId == def.id))
        {
            weaponId = def.id;
            weaponName = def.name;
        }
    }

    private void CreatePrefab()
    {
        try
        {
            // Instantiate model
            var instance = PrefabUtility.InstantiatePrefab(selectedModel) as GameObject;
            instance.name = weaponId;

            // Setup hierarchy
            if (autoCreateHierarchy)
                SetupHierarchy(instance);

            // Add components
            if (autoAddComponents)
                AddComponents(instance);

            // Setup colliders
            if (autoSetupColliders)
                SetupColliders(instance);

            // Setup rigidbody
            if (autoSetupRigidbody)
                SetupRigidbody(instance);

            // Apply weapon-specific settings
            ApplyWeaponSettings(instance);

            // Save as prefab
            string prefabPath = SavePrefab(instance);
            DestroyImmediate(instance);

            AssetDatabase.Refresh();

            // Validate
            if (validateAfterCreate)
            {
                var result = ValidatePrefab(prefabPath);
                if (result.isValid)
                {
                    EditorUtility.DisplayDialog("Success", $"Prefab created and validated:\n{prefabPath}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", $"Prefab created but validation found issues:\n{prefabPath}\n\n{string.Join("\n", result.issues)}", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Success", $"Prefab created:\n{prefabPath}", "OK");
            }

            // Ping in project window
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorGUIUtility.PingObject(prefab);
        }
        catch (Exception e)
        {
            Debug.LogError($"[JWA Prefab Wizard] Failed to create prefab: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error", $"Failed to create prefab:\n{e.Message}", "OK");
        }
    }

    private void SetupHierarchy(GameObject root)
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

            // Move all renderers under Model
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r.transform != root.transform && r.transform.parent == root.transform)
                {
                    r.transform.SetParent(modelContainer);
                }
            }
        }

        // Create required transforms
        CreateTransformIfMissing(root, "MuzzlePoint", GetMuzzlePosition());
        CreateTransformIfMissing(root, "EjectionPort", GetEjectionPortPosition());
        CreateTransformIfMissing(root, "Grip_L", GetLeftGripPosition());
        CreateTransformIfMissing(root, "Grip_R", GetRightGripPosition());

        if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun)
        {
            CreateTransformIfMissing(root, "Sight_Front", GetFrontSightPosition());
            CreateTransformIfMissing(root, "Sight_Rear", GetRearSightPosition());
        }

        if (weaponType == WeaponType.Throwable)
        {
            CreateTransformIfMissing(root, "Tip", GetTipPosition());
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
        }
    }

    private void AddComponents(GameObject root)
    {
        // Remove existing conflicting components
        var existingWeapon = root.GetComponent<Component>(); // placeholder
        var weaponTypeName = typeof(Weapon).Name;
        var itemTypeName = typeof(Item).Name;

        // Use reflection to avoid direct dependency
        var assembly = Assembly.GetAssembly(typeof(Weapon));
        if (assembly != null)
        {
            var weaponType = assembly.GetType("BladeAndSorcery.Gameplay.Weapon");
            var itemType = assembly.GetType("BladeAndSorcery.Gameplay.Item");
            var gunType = assembly.GetType("BladeAndSorcery.Gameplay.Gun");
            var magazineType = assembly.GetType("BladeAndSorcery.Gameplay.Magazine");
            var throwableType = assembly.GetType("BladeAndSorcery.Gameplay.Throwable");

            if (weaponType != null)
            {
                var existing = root.GetComponent(weaponType);
                if (existing != null) DestroyImmediate(existing);
            }
            if (itemType != null)
            {
                var existing = root.GetComponent(itemType);
                if (existing != null) DestroyImmediate(existing);
            }

            if (IsThrowable())
            {
                if (itemType != null) root.AddComponent(itemType);
                if (throwableType != null) root.AddComponent(throwableType);

                // Add custom behavior
                AddCustomBehavior(root, IsPencil() ? "PencilBehavior" : "ThrowingKnifeBehavior");
            }
            else
            {
                if (weaponType != null) root.AddComponent(weaponType);
                if (gunType != null) root.AddComponent(gunType);
                if (magazineType != null)
                {
                    var magObj = root.transform.Find("Magazine_Model");
                    if (magObj != null) magObj.gameObject.AddComponent(magazineType);
                    else
                    {
                        // Create Magazine_Model if missing
                        var magModel = new GameObject("Magazine_Model");
                        magModel.transform.SetParent(root.transform);
                        magModel.transform.localPosition = GetMagazinePosition();
                        magModel.AddComponent(magazineType);
                    }
                }

                // Add custom behavior
                string behaviorName = GetBehaviorName();
                if (!string.IsNullOrEmpty(behaviorName))
                    AddCustomBehavior(root, behaviorName);
            }
        }

        // Audio source for all
        if (root.GetComponent<AudioSource>() == null)
        {
            var audio = root.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.maxDistance = 50f;
            audio.dopplerLevel = 0f;
        }
    }

    private void AddCustomBehavior(GameObject root, string behaviorName)
    {
        var asm = Assembly.GetExecutingAssembly();
        var behaviorType = asm.GetType($"JohnWickArsenal.Behaviors.{behaviorName}");
        if (behaviorType != null)
        {
            root.AddComponent(behaviorType);
        }
        else
        {
            Debug.LogWarning($"[JWA] Behavior not found: {behaviorName}");
        }
    }

    private void SetupColliders(GameObject root)
    {
        // Remove existing colliders from children (keep root)
        var existingColliders = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in existingColliders)
        {
            if (c.gameObject != root)
                DestroyImmediate(c);
        }

        // Add convex mesh colliders to Model renderers
        var model = root.transform.Find("Model");
        if (model != null)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
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
            bc.center = GetColliderCenter(root);
        }
    }

    private void SetupRigidbody(GameObject root)
    {
        var rb = root.GetComponent<Rigidbody>();
        if (rb == null)
            rb = root.AddComponent<Rigidbody>();

        rb.mass = GetWeaponMass();
        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.sleepThreshold = 0.005f;
    }

    private void ApplyWeaponSettings(GameObject root)
    {
        // Apply weapon-specific settings via reflection
        var def = WeaponDefinitions.FirstOrDefault(d => d.id == weaponId);
        if (def == null) return;

        // Gun settings
        var asm = Assembly.GetAssembly(typeof(Weapon));
        if (asm != null)
        {
            var gunType = asm.GetType("BladeAndSorcery.Gameplay.Gun");
            if (gunType != null)
            {
                var gun = root.GetComponent(gunType);
                if (gun != null)
                {
                    // Set fields via reflection
                    SetField(gun, "caliber", def.caliber);
                    SetField(gun, "magazineCapacity", def.capacity);
                }
            }
        }
    }

    private void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
            field.SetValue(obj, value);
    }

    private string SavePrefab(GameObject instance)
    {
        string prefabPath;
        if (saveToResources)
        {
            string categoryPath = GetCategoryPath();
            string fullDir = Path.Combine(RESOURCES_ROOT, categoryPath);

            if (!AssetDatabase.IsValidFolder(fullDir))
            {
                Directory.CreateDirectory(fullDir);
                AssetDatabase.Refresh();
            }

            prefabPath = Path.Combine(fullDir, $"{weaponId}.prefab");
        }
        else
        {
            string dir = $"Assets/JohnWickArsenal/Prefabs/{GetCategoryPath()}";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var parts = dir.Split('/');
                string current = "Assets";
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = Path.Combine(current, parts[i]);
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
            prefabPath = Path.Combine(dir, $"{weaponId}.prefab");
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        return prefabPath;
    }

    private ValidationResult ValidatePrefab(string prefabPath)
    {
        var result = new ValidationResult { isValid = true, issues = new List<string>() };
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            result.isValid = false;
            result.issues.Add("Prefab not found at path");
            return result;
        }

        // Check required components
        var asm = Assembly.GetAssembly(typeof(Weapon));
        if (asm != null)
        {
            var weaponType = asm.GetType("BladeAndSorcery.Gameplay.Weapon");
            var itemType = asm.GetType("BladeAndSorcery.Gameplay.Item");
            var gunType = asm.GetType("BladeAndSorcery.Gameplay.Gun");
            var magazineType = asm.GetType("BladeAndSorcery.Gameplay.Magazine");
            var throwableType = asm.GetType("BladeAndSorcery.Gameplay.Throwable");

            if (IsThrowable())
            {
                if (prefab.GetComponent(itemType) == null) result.issues.Add("Missing Item component");
                if (prefab.GetComponent(throwableType) == null) result.issues.Add("Missing Throwable component");
            }
            else
            {
                if (prefab.GetComponent(weaponType) == null) result.issues.Add("Missing Weapon component");
                if (prefab.GetComponent(gunType) == null) result.issues.Add("Missing Gun component");
                if (prefab.GetComponentInChildren(magazineType) == null) result.issues.Add("Missing Magazine component");
            }
        }

        // Check transforms
        string[] requiredTransforms = { "MuzzlePoint", "EjectionPort", "Grip_L", "Grip_R" };
        if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun)
            requiredTransforms = requiredTransforms.Concat(new[] { "Sight_Front", "Sight_Rear" }).ToArray();
        if (weaponType == WeaponType.Throwable)
            requiredTransforms = requiredTransforms.Concat(new[] { "Tip" }).ToArray();

        foreach (var tName in requiredTransforms)
        {
            if (prefab.transform.Find(tName) == null)
                result.issues.Add($"Missing transform: {tName}");
        }

        // Check colliders
        if (prefab.GetComponent<Collider>() == null)
            result.issues.Add("Missing root collider");

        // Check rigidbody
        if (prefab.GetComponent<Rigidbody>() == null)
            result.issues.Add("Missing Rigidbody");

        // Check custom behavior
        string behaviorName = IsThrowable() ? (IsPencil() ? "PencilBehavior" : "ThrowingKnifeBehavior") : GetBehaviorName();
        if (!string.IsNullOrEmpty(behaviorName))
        {
            var behaviorType = Assembly.GetExecutingAssembly().GetType($"JohnWickArsenal.Behaviors.{behaviorName}");
            if (behaviorType != null && prefab.GetComponent(behaviorType) == null)
                result.issues.Add($"Missing behavior: {behaviorName}");
        }

        if (result.issues.Count > 0)
            result.isValid = false;

        return result;
    }

    private class ValidationResult
    {
        public bool isValid;
        public List<string> issues;
    }

    private void CopyComponentList()
    {
        var list = GenerateComponentList();
        EditorGUIUtility.systemCopyBuffer = list;
        Debug.Log("[JWA Prefab Wizard] Component list copied to clipboard");
    }

    private string GenerateComponentList()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"// Components for {weaponId} ({weaponType})");
        sb.AppendLine("// Add these to the root GameObject:");

        if (IsThrowable())
        {
            sb.AppendLine("Item");
            sb.AppendLine("Throwable");
            sb.AppendLine(IsPencil() ? "JohnWickArsenal.Behaviors.PencilBehavior" : "JohnWickArsenal.Behaviors.ThrowingKnifeBehavior");
        }
        else
        {
            sb.AppendLine("Weapon");
            sb.AppendLine("Gun");
            sb.AppendLine("Magazine (on Magazine_Model child)");
            sb.AppendLine($"JohnWickArsenal.Behaviors.{GetBehaviorName()}");
        }

        sb.AppendLine("AudioSource (spatialBlend=1, maxDistance=50)");
        sb.AppendLine($"Rigidbody (mass={GetWeaponMass()}, interpolation=Interpolate, collisionDetection=ContinuousDynamic)");
        sb.AppendLine("Collider (BoxCollider on root, MeshCollider convex on Model)");

        sb.AppendLine("\n// Required Transforms:");
        sb.AppendLine("MuzzlePoint");
        sb.AppendLine("EjectionPort");
        sb.AppendLine("Grip_L");
        sb.AppendLine("Grip_R");

        if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun)
        {
            sb.AppendLine("Sight_Front");
            sb.AppendLine("Sight_Rear");
        }

        if (IsThrowable())
        {
            sb.AppendLine("Tip");
        }

        return sb.ToString();
    }

    private void ValidateExistingPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RESOURCES_ROOT });
        int valid = 0, invalid = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var result = ValidatePrefab(path);
            if (result.isValid) valid++; else { invalid++; Debug.LogWarning($"[JWA] Invalid prefab: {path}\n{string.Join("\n", result.issues)}"); }
        }

        EditorUtility.DisplayDialog("Validation Complete", $"Valid: {valid}\nInvalid: {invalid}", "OK");
    }

    private void CreateAllPrefabsBatch()
    {
        if (!EditorUtility.DisplayDialog("Batch Create",
            "This will attempt to create all 10 weapon prefabs.\n" +
            "Models must be in Assets/JohnWickArsenal/Models/ with matching names.\n\n" +
            "Continue?", "Yes", "Cancel"))
            return;

        int created = 0, failed = 0;

        foreach (var def in WeaponDefinitions)
        {
            // Try to find model
            string modelPath = FindModelForWeapon(def);
            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogWarning($"[JWA Batch] Model not found for {def.name}, skipping");
                failed++;
                continue;
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                failed++;
                continue;
            }

            // Create prefab
            try
            {
                selectedModel = model;
                weaponType = def.type;
                weaponId = def.id;
                weaponName = def.name;

                var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                instance.name = def.id;

                if (autoCreateHierarchy) SetupHierarchy(instance);
                if (autoAddComponents) AddComponents(instance);
                if (autoSetupColliders) SetupColliders(instance);
                if (autoSetupRigidbody) SetupRigidbody(instance);
                ApplyWeaponSettings(instance);

                string categoryPath = GetCategoryPathForType(def.type);
                string fullDir = Path.Combine(RESOURCES_ROOT, categoryPath);
                if (!AssetDatabase.IsValidFolder(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                    AssetDatabase.Refresh();
                }

                string prefabPath = Path.Combine(fullDir, $"{def.id}.prefab");
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                created++;
                Debug.Log($"[JWA Batch] Created: {prefabPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JWA Batch] Failed {def.name}: {e.Message}");
                failed++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Batch Complete", $"Created: {created}\nFailed: {failed}", "OK");
    }

    private string FindModelForWeapon(WeaponDefinition def)
    {
        // Search common locations
        string[] searchPaths = new[]
        {
            $"Assets/JohnWickArsenal/Models/{GetCategoryPathForType(def.type)}/{def.name}",
            $"Assets/JohnWickArsenal/Models/{GetCategoryPathForType(def.type)}/{def.id}",
            $"Assets/JohnWickArsenal/Models/{def.name}",
            $"Assets/JohnWickArsenal/Models/{def.id}"
        };

        foreach (var path in searchPaths)
        {
            var guids = AssetDatabase.FindAssets("", new[] { path });
            if (guids.Length > 0)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".obj") || assetPath.EndsWith(".prefab"))
                    return assetPath;
            }
        }

        // Try to find any model in the category folder
        var categoryPath = $"Assets/JohnWickArsenal/Models/{GetCategoryPathForType(def.type)}";
        if (AssetDatabase.IsValidFolder(categoryPath))
        {
            var guids = AssetDatabase.FindAssets("t:GameObject", new[] { categoryPath });
            foreach (var guid in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith(".fbx") || p.EndsWith(".obj"))
                    return p;
            }
        }

        return null;
    }

    private void CleanInvalidPrefabs()
    {
        if (!EditorUtility.DisplayDialog("Clean Invalid Prefabs",
            "This will delete prefabs that fail validation.\n" +
            "A backup will be created in Assets/JohnWickArsenal/Backups/\n\n" +
            "Continue?", "Yes", "Cancel"))
            return;

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RESOURCES_ROOT });
        int deleted = 0;

        string backupDir = "Assets/JohnWickArsenal/Backups";
        if (!AssetDatabase.IsValidFolder(backupDir))
        {
            AssetDatabase.CreateFolder("Assets/JohnWickArsenal", "Backups");
        }

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var result = ValidatePrefab(path);
            if (!result.isValid)
            {
                // Move to backup
                string backupPath = Path.Combine(backupDir, Path.GetFileName(path));
                AssetDatabase.MoveAsset(path, backupPath);
                deleted++;
                Debug.Log($"[JWA] Moved invalid prefab to backup: {path}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Clean Complete", $"Moved {deleted} invalid prefabs to backup folder", "OK");
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================

    private Vector3 GetMuzzlePosition()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol: return new Vector3(0, 0.02f, 0.12f);
            case WeaponType.Shotgun: return new Vector3(0, 0.03f, 0.45f);
            case WeaponType.Revolver: return new Vector3(0, 0.015f, 0.18f);
            case WeaponType.Rifle: return new Vector3(0, 0.04f, 0.4f);
            default: return new Vector3(0, 0, 0.1f);
        }
    }

    private Vector3 GetEjectionPortPosition()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol: return new Vector3(0.03f, 0.05f, 0.05f);
            case WeaponType.Shotgun: return new Vector3(0.05f, 0.06f, 0.1f);
            case WeaponType.Revolver: return new Vector3(0.04f, 0, 0);
            case WeaponType.Rifle: return new Vector3(0.04f, 0.06f, 0.1f);
            default: return new Vector3(0.03f, 0.03f, 0);
        }
    }

    private Vector3 GetLeftGripPosition() => new Vector3(-0.02f, -0.05f, 0.02f);
    private Vector3 GetRightGripPosition() => new Vector3(0.02f, -0.05f, 0.02f);
    private Vector3 GetFrontSightPosition() => new Vector3(0, 0.06f, 0.3f);
    private Vector3 GetRearSightPosition() => new Vector3(0, 0.06f, 0.1f);
    private Vector3 GetTipPosition() => new Vector3(0, 0, 0.15f);
    private Vector3 GetMagazinePosition()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol: return new Vector3(0, -0.08f, 0.02f);
            case WeaponType.Shotgun: return new Vector3(0, -0.1f, 0.15f);
            case WeaponType.Rifle: return new Vector3(0, -0.12f, 0.1f);
            default: return new Vector3(0, -0.05f, 0);
        }
    }

    private Bounds GetModelBounds(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one * 0.1f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private string GetModelType(string path)
    {
        if (path.EndsWith(".fbx")) return "FBX";
        if (path.EndsWith(".obj")) return "OBJ";
        if (path.EndsWith(".prefab")) return "Prefab";
        return "Unknown";
    }

    private Vector3 EstimateWeaponBounds(GameObject root)
    {
        var bounds = GetModelBounds(root);
        return bounds.size;
    }

    private Vector3 GetColliderCenter(GameObject root)
    {
        var bounds = GetModelBounds(root);
        return bounds.center - root.transform.position;
    }

    private float GetWeaponMass()
    {
        var def = WeaponDefinitions.FirstOrDefault(d => d.id == weaponId);
        if (def != null) return def.mass;

        switch (weaponType)
        {
            case WeaponType.Pistol: return 0.8f;
            case WeaponType.Shotgun: return 3.5f;
            case WeaponType.Revolver: return 1.1f;
            case WeaponType.Rifle: return 2.3f;
            case WeaponType.Throwable: return 0.15f;
            default: return 1f;
        }
    }

    private string GetBehaviorName()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol: return "JohnWickPistolBehavior";
            case WeaponType.Shotgun: return "JohnWickShotgunBehavior";
            case WeaponType.Revolver: return "JohnWickRevolverBehavior";
            case WeaponType.Rifle: return "JohnWickRifleBehavior";
            default: return "";
        }
    }

    private bool IsThrowable() => weaponType == WeaponType.Throwable;
    private bool IsPencil() => weaponId == "jwa_pencil";

    private string GetCategoryPath() => GetCategoryPathForType(weaponType);

    private string GetCategoryPathForType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Pistol: return "Weapons/Pistols";
            case WeaponType.Shotgun: return "Weapons/Shotguns";
            case WeaponType.Revolver: return "Weapons/Revolvers";
            case WeaponType.Rifle: return "Weapons/Rifles";
            case WeaponType.Throwable: return "Weapons/Throwables";
            default: return "Weapons";
        }
    }
}