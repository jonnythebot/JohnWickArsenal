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
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("John Wick Arsenal - Prefab Setup Wizard", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Automated weapon prefab creation with B&S Nomad integration", EditorStyles.miniLabel);
        EditorGUILayout.Space();
    }

    private void DrawModelSelection()
    {
        EditorGUILayout.LabelField("Model Selection", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        selectedModel = (GameObject)EditorGUILayout.ObjectField("Model (FBX/OBJ/GLB)", selectedModel, typeof(GameObject), false);
        if (GUILayout.Button("Auto-Find", GUILayout.Width(80)))
        {
            AutoFindModel();
        }
        EditorGUILayout.EndHorizontal();

        if (selectedModel != null)
        {
            EditorGUILayout.LabelField("Path: " + AssetDatabase.GetAssetPath(selectedModel), EditorStyles.miniLabel);
            EditorPrefs.SetString("JWA_LastModel", AssetDatabase.GetAssetPath(selectedModel));
        }
        EditorGUILayout.Space();
    }

    private void DrawWeaponSelection()
    {
        EditorGUILayout.LabelField("Weapon Configuration", EditorStyles.boldLabel);
        weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);

        var def = Array.Find(WeaponDefinitions, d => d.type == weaponType);
        if (def != null)
        {
            weaponId = EditorGUILayout.TextField("Weapon ID", def.id);
            weaponName = EditorGUILayout.TextField("Weapon Name", def.name);
        }
        else
        {
            weaponId = EditorGUILayout.TextField("Weapon ID", weaponId);
            weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);
        }
        EditorGUILayout.Space();
    }

    private void DrawOptions()
    {
        EditorGUILayout.LabelField("Prefab Options", EditorStyles.boldLabel);
        autoCreateHierarchy = EditorGUILayout.Toggle("Auto-Create Hierarchy", autoCreateHierarchy);
        autoAddComponents = EditorGUILayout.Toggle("Auto-Add B&S Components", autoAddComponents);
        autoSetupColliders = EditorGUILayout.Toggle("Auto-Setup Colliders", autoSetupColliders);
        autoSetupRigidbody = EditorGUILayout.Toggle("Auto-Setup Rigidbody", autoSetupRigidbody);
        saveToResources = EditorGUILayout.Toggle("Save to Resources", saveToResources);
        validateAfterCreate = EditorGUILayout.Toggle("Validate After Create", validateAfterCreate);
        EditorGUILayout.Space();
    }

    private void DrawActions()
    {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Prefab"))
        {
            CreatePrefab();
        }
        if (GUILayout.Button("Validate Selected"))
        {
            ValidatePrefab();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private void DrawBatchOperations()
    {
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
        if (GUILayout.Button("Create All Weapon Prefabs"))
        {
            CreateAllPrefabs();
        }
        EditorGUILayout.Space();
    }

    private void AutoFindModel()
    {
        var guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/JohnWickArsenal/Models" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj != null && obj.GetComponentsInChildren<MeshFilter>().Length > 0)
            {
                selectedModel = obj;
                break;
            }
        }
    }

    private void CreatePrefab()
    {
        if (selectedModel == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a model first.", "OK");
            return;
        }

        var prefabPath = saveToResources ? $"{RESOURCES_ROOT}/{weaponId}.prefab" : $"Assets/JohnWickArsenal/Prefabs/Weapons/{weaponId}.prefab";
        var directory = Path.GetDirectoryName(prefabPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            var parent = Path.GetDirectoryName(directory);
            var name = Path.GetFileName(directory);
            AssetDatabase.CreateFolder(parent, name);
        }

        var instance = PrefabUtility.InstantiatePrefab(selectedModel) as GameObject;
        instance.name = weaponId;

        if (autoCreateHierarchy)
        {
            CreateWeaponHierarchy(instance);
        }

        if (autoAddComponents)
        {
            AddWeaponComponents(instance);
        }

        if (autoSetupColliders)
        {
            SetupColliders(instance);
        }

        if (autoSetupRigidbody)
        {
            SetupRigidbody(instance);
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);

        EditorUtility.DisplayDialog("Success", $"Prefab created at {prefabPath}", "OK");
    }

    private void CreateWeaponHierarchy(GameObject root)
    {
        // Ensure standard hierarchy points exist
        EnsureChild(root, "MuzzlePoint");
        EnsureChild(root, "EjectionPort");
        EnsureChild(root, "Grip_L");
        EnsureChild(root, "Grip_R");
        EnsureChild(root, "Sight_Front");
        EnsureChild(root, "Sight_Rear");
        EnsureChild(root, "Tip");
    }

    private Transform EnsureChild(GameObject parent, string name)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing;
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        return child.transform;
    }

    private void AddWeaponComponents(GameObject root)
    {
#if !JWA_COSMETIC_ONLY
        // B&S Nomad integration - requires BladeAndSorcery assemblies
        var asm = Assembly.Load("BladeAndSorcery.Gameplay");
        if (asm == null)
        {
            Debug.LogWarning("[JWA_PrefabSetupWizard] BladeAndSorcery.Gameplay assembly not found. Skipping B&S component setup.");
            return;
        }

        var weaponType = asm.GetType("BladeAndSorcery.Gameplay.Weapon");
        var itemType = asm.GetType("BladeAndSorcery.Gameplay.Item");
        var gunType = asm.GetType("BladeAndSorcery.Gameplay.Gun");
        var magazineType = asm.GetType("BladeAndSorcery.Gameplay.Magazine");
        var throwableType = asm.GetType("BladeAndSorcery.Gameplay.Throwable");

        if (weaponType != null) root.AddComponent(weaponType);
        if (itemType != null) root.AddComponent(itemType);

        var def = Array.Find(WeaponDefinitions, d => d.type == weaponType);
        if (def != null)
        {
            switch (def.type)
            {
                case WeaponType.Pistol:
                case WeaponType.Rifle:
                    if (gunType != null) root.AddComponent(gunType);
                    break;
                case WeaponType.Shotgun:
                    if (gunType != null) root.AddComponent(gunType);
                    break;
                case WeaponType.Revolver:
                    if (gunType != null) root.AddComponent(gunType);
                    break;
                case WeaponType.Throwable:
                    if (throwableType != null) root.AddComponent(throwableType);
                    break;
            }
        }
#else
        Debug.Log("[JWA_PrefabSetupWizard] JWA_COSMETIC_ONLY defined - skipping B&S component setup.");
#endif
    }

    private void SetupColliders(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.GetComponent<MeshCollider>() == null)
            {
                var collider = root.AddComponent<MeshCollider>();
                collider.convex = true;
                break; // One convex collider for the whole weapon
            }
        }
    }

    private void SetupRigidbody(GameObject root)
    {
        if (root.GetComponent<Rigidbody>() == null)
        {
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void ValidatePrefab()
    {
        if (selectedModel == null)
        {
            EditorUtility.DisplayDialog("Error", "No model selected.", "OK");
            return;
        }

        var issues = new List<string>();

        if (selectedModel.GetComponentsInChildren<MeshFilter>().Length == 0)
            issues.Add("No MeshFilter found in model hierarchy.");

        if (selectedModel.GetComponentsInChildren<Renderer>().Length == 0)
            issues.Add("No Renderer found in model hierarchy.");

#if !JWA_COSMETIC_ONLY
        // B&S validation requires assemblies
        var asm = Assembly.Load("BladeAndSorcery.Gameplay");
        if (asm == null)
            issues.Add("BladeAndSorcery.Gameplay assembly not loaded - cannot validate B&S components.");
        else
        {
            var weaponType = asm.GetType("BladeAndSorcery.Gameplay.Weapon");
            var itemType = asm.GetType("BladeAndSorcery.Gameplay.Item");
            var gunType = asm.GetType("BladeAndSorcery.Gameplay.Gun");
            var magazineType = asm.GetType("BladeAndSorcery.Gameplay.Magazine");
            var throwableType = asm.GetType("BladeAndSorcery.Gameplay.Throwable");

            var weaponComp = selectedModel.GetComponent(weaponType);
            var itemComp = selectedModel.GetComponent(itemType);
            var gunComp = selectedModel.GetComponent(gunType);
            var magazineComp = selectedModel.GetComponent(magazineType);
            var throwableComp = selectedModel.GetComponent(throwableType);

            if (weaponComp == null) issues.Add("Missing Weapon component.");
            if (itemComp == null) issues.Add("Missing Item component.");
        }
#else
        issues.Add("JWA_COSMETIC_ONLY defined - B&S component validation skipped.");
#endif

        if (issues.Count == 0)
            EditorUtility.DisplayDialog("Validation", "No issues found.", "OK");
        else
            EditorUtility.DisplayDialog("Validation Issues", string.Join("\n", issues), "OK");
    }

    private void CreateAllPrefabs()
    {
        foreach (var def in WeaponDefinitions)
        {
            var guids = AssetDatabase.FindAssets($"t:GameObject {def.id}", new[] { "Assets/JohnWickArsenal/Models" });
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model != null)
                {
                    selectedModel = model;
                    weaponType = def.type;
                    weaponId = def.id;
                    weaponName = def.name;
                    CreatePrefab();
                }
            }
        }
    }
}
