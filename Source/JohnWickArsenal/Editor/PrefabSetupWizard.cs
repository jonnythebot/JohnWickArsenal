using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Wizard to automatically set up weapon prefabs with correct components, hierarchy, and settings.
    /// Run after importing models to configure them for Blade & Sorcery Nomad.
    /// </summary>
    public class PrefabSetupWizard : EditorWindow
    {
        private Vector2 scrollPos;
        private GameObject selectedModel;
        private WeaponType weaponType;
        private string weaponId;
        private bool autoCreateHierarchy = true;
        private bool autoAddComponents = true;
        private bool autoSetupColliders = true;
        private bool autoSetupRigidbody = true;
        private bool assignToResources = true;

        private enum WeaponType
        {
            Pistol,
            Shotgun,
            Revolver,
            Rifle,
            Throwable_Knife,
            Throwable_Pencil
        }

        [MenuItem("John Wick Arsenal/Prefab Setup Wizard", priority = 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabSetupWizard>("JWA Prefab Wizard");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(10);
            DrawHeader();

            EditorGUILayout.Space(10);
            DrawModelSelection();

            EditorGUILayout.Space(10);
            DrawWeaponConfig();

            EditorGUILayout.Space(10);
            DrawOptions();

            EditorGUILayout.Space(10);
            DrawActions();

            EditorGUILayout.Space(10);
            DrawQuickActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("John Wick Arsenal - Prefab Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Configure imported models for B&S Nomad", EditorStyles.miniLabel);
        }

        private void DrawModelSelection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("1. Select Model", EditorStyles.boldLabel);

            selectedModel = (GameObject)EditorGUILayout.ObjectField(
                "Source Model (FBX/OBJ)",
                selectedModel,
                typeof(GameObject),
                false
            );

            if (selectedModel != null)
            {
                EditorGUILayout.HelpBox($"Selected: {selectedModel.name}\nPath: {AssetDatabase.GetAssetPath(selectedModel)}", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWeaponConfig()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("2. Weapon Configuration", EditorStyles.boldLabel);

            weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);

            // Auto-generate ID from model name if empty
            if (string.IsNullOrEmpty(weaponId) && selectedModel != null)
            {
                weaponId = GenerateWeaponId(selectedModel.name);
            }

            weaponId = EditorGUILayout.TextField("Weapon ID", weaponId);
            EditorGUILayout.LabelField("Format: jwa_<category>_<name>", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawOptions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3. Auto-Setup Options", EditorStyles.boldLabel);

            autoCreateHierarchy = EditorGUILayout.Toggle("Create Hierarchy", autoCreateHierarchy);
            autoAddComponents = EditorGUILayout.Toggle("Add Components", autoAddComponents);
            autoSetupColliders = EditorGUILayout.Toggle("Setup Colliders", autoSetupColliders);
            autoSetupRigidbody = EditorGUILayout.Toggle("Setup Rigidbody", autoSetupRigidbody);
            assignToResources = EditorGUILayout.Toggle("Save to Resources", assignToResources);

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("4. Actions", EditorStyles.boldLabel);

            GUI.enabled = selectedModel != null && !string.IsNullOrEmpty(weaponId);

            if (GUILayout.Button("🔧 Create Prefab", GUILayout.Height(40)))
            {
                CreatePrefab();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("📋 Copy Component List to Clipboard"))
            {
                CopyComponentList();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate All Prefabs"))
            {
                new PrefabValidator().ValidateAll();
            }
            if (GUILayout.Button("Open Resources Folder"))
            {
                OpenResourcesFolder();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Collider Presets"))
            {
                GenerateColliderPresets();
            }
            if (GUILayout.Button("Create Animation Template"))
            {
                CreateAnimationTemplate();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
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
                {
                    SetupHierarchy(instance);
                }

                // Add components
                if (autoAddComponents)
                {
                    AddComponents(instance);
                }

                // Setup colliders
                if (autoSetupColliders)
                {
                    SetupColliders(instance);
                }

                // Setup rigidbody
                if (autoSetupRigidbody)
                {
                    SetupRigidbody(instance);
                }

                // Save as prefab
                string prefabPath;
                if (assignToResources)
                {
                    string resourcesDir = "Assets/JohnWickArsenal/Resources";
                    string categoryDir = GetCategoryPath();
                    string fullDir = Path.Combine(resourcesDir, categoryDir);

                    if (!AssetDatabase.IsValidFolder(fullDir))
                    {
                        Directory.CreateDirectory(fullDir);
                        AssetDatabase.Refresh();
                    }

                    prefabPath = Path.Combine(fullDir, $"{weaponId}.prefab");
                }
                else
                {
                    prefabPath = $"Assets/JohnWickArsenal/Prefabs/{weaponId}.prefab";
                    string dir = Path.GetDirectoryName(prefabPath);
                    if (!AssetDatabase.IsValidFolder(dir))
                    {
                        Directory.CreateDirectory(dir);
                        AssetDatabase.Refresh();
                    }
                }

                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                AssetDatabase.Refresh();

                Debug.Log($"[JWA Wizard] ✅ Created prefab: {prefabPath}");

                // Ping in project window
                EditorGUIUtility.PingObject(prefab);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JWA Wizard] Failed to create prefab: {e.Message}\n{e.StackTrace}");
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
            CreateTransformIfMissing(root, "MuzzlePoint", GetMuzzlePosition());
            CreateTransformIfMissing(root, "EjectionPort", GetEjectionPortPosition());
            CreateTransformIfMissing(root, "Grip_L", GetLeftGripPosition());
            CreateTransformIfMissing(root, "Grip_R", GetRightGripPosition());

            if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun)
            {
                CreateTransformIfMissing(root, "Sight_Front", GetFrontSightPosition());
                CreateTransformIfMissing(root, "Sight_Rear", GetRearSightPosition());
            }

            if (weaponType == WeaponType.Throwable_Knife || weaponType == WeaponType.Throwable_Pencil)
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
            // Remove any existing Weapon/Item to avoid duplicates
            var existingWeapon = root.GetComponent<Weapon>();
            var existingItem = root.GetComponent<Item>();
            if (existingWeapon != null) DestroyImmediate(existingWeapon);
            if (existingItem != null) DestroyImmediate(existingItem);

            if (IsThrowable())
            {
                var item = root.AddComponent<Item>();
                var throwable = root.AddComponent<Throwable>();

                // Add custom behavior
                if (weaponType == WeaponType.Throwable_Knife)
                {
                    root.AddComponent<JohnWickArsenal.Behaviors.ThrowingKnifeBehavior>();
                }
                else if (weaponType == WeaponType.Throwable_Pencil)
                {
                    root.AddComponent<JohnWickArsenal.Behaviors.PencilBehavior>();
                }
            }
            else
            {
                var weapon = root.AddComponent<Weapon>();
                var gun = root.AddComponent<Gun>();
                var magazine = root.AddComponentInChildren<Magazine>();

                // Add custom behavior
                switch (weaponType)
                {
                    case WeaponType.Pistol:
                        root.AddComponent<JohnWickArsenal.Behaviors.JohnWickPistolBehavior>();
                        break;
                    case WeaponType.Shotgun:
                        root.AddComponent<JohnWickArsenal.Behaviors.JohnWickShotgunBehavior>();
                        break;
                    case WeaponType.Revolver:
                        root.AddComponent<JohnWickArsenal.Behaviors.JohnWickRevolverBehavior>();
                        break;
                    case WeaponType.Rifle:
                        root.AddComponent<JohnWickArsenal.Behaviors.JohnWickRifleBehavior>();
                        break;
                }
            }

            // Audio source for all
            if (root.GetComponent<AudioSource>() == null)
            {
                var audio = root.AddComponent<AudioSource>();
                audio.spatialBlend = 1f;
                audio.rolloffMode = AudioRolloffMode.Linear;
                audio.maxDistance = 50f;
            }
        }

        private void SetupColliders(GameObject root)
        {
            // Remove existing colliders
            var existingColliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in existingColliders)
            {
                if (c.gameObject != root) // Keep root collider if exists
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

        private void SetupRigidbody(GameObject root)
        {
            var rb = root.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = root.AddComponent<Rigidbody>();
            }

            rb.mass = GetWeaponMass();
            rb.drag = 0.1f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

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

        private Vector3 GetLeftGripPosition()
        {
            return new Vector3(-0.02f, -0.05f, 0.02f);
        }

        private Vector3 GetRightGripPosition()
        {
            return new Vector3(0.02f, -0.05f, 0.02f);
        }

        private Vector3 GetFrontSightPosition()
        {
            return new Vector3(0, 0.06f, 0.3f);
        }

        private Vector3 GetRearSightPosition()
        {
            return new Vector3(0, 0.06f, 0.1f);
        }

        private Vector3 GetTipPosition()
        {
            return new Vector3(0, 0, 0.15f);
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

        private float GetWeaponMass()
        {
            switch (weaponType)
            {
                case WeaponType.Pistol: return 0.8f;
                case WeaponType.Shotgun: return 3.5f;
                case WeaponType.Revolver: return 1.1f;
                case WeaponType.Rifle: return 2.3f;
                case WeaponType.Throwable_Knife: return 0.15f;
                case WeaponType.Throwable_Pencil: return 0.01f;
                default: return 1f;
            }
        }

        private string GetCategoryPath()
        {
            switch (weaponType)
            {
                case WeaponType.Pistol: return "Weapons/Pistols";
                case WeaponType.Shotgun: return "Weapons/Shotguns";
                case WeaponType.Revolver: return "Weapons/Revolvers";
                case WeaponType.Rifle: return "Weapons/Rifles";
                case WeaponType.Throwable_Knife:
                case WeaponType.Throwable_Pencil: return "Weapons/Throwables";
                default: return "Weapons";
            }
        }

        private string GenerateWeaponId(string modelName)
        {
            string clean = modelName
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace(" ", "")
                .ToLower();

            string prefix = GetCategoryPath().Replace("Weapons/", "").Replace("/", "_").ToLower();
            return $"jwa_{prefix}_{clean}";
        }

        private bool IsThrowable()
        {
            return weaponType == WeaponType.Throwable_Knife || weaponType == WeaponType.Throwable_Pencil;
        }

        private void CopyComponentList()
        {
            string list = GenerateComponentList();
            EditorGUIUtility.systemCopyBuffer = list;
            Debug.Log("[JWA Wizard] Component list copied to clipboard");
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
                sb.AppendLine(weaponType == WeaponType.Throwable_Knife
                    ? "JohnWickArsenal.Behaviors.ThrowingKnifeBehavior"
                    : "JohnWickArsenal.Behaviors.PencilBehavior");
            }
            else
            {
                sb.AppendLine("Weapon");
                sb.AppendLine("Gun");
                sb.AppendLine("Magazine (on Magazine_Model child)");

                switch (weaponType)
                {
                    case WeaponType.Pistol: sb.AppendLine("JohnWickArsenal.Behaviors.JohnWickPistolBehavior"); break;
                    case WeaponType.Shotgun: sb.AppendLine("JohnWickArsenal.Behaviors.JohnWickShotgunBehavior"); break;
                    case WeaponType.Revolver: sb.AppendLine("JohnWickArsenal.Behaviors.JohnWickRevolverBehavior"); break;
                    case WeaponType.Rifle: sb.AppendLine("JohnWickArsenal.Behaviors.JohnWickRifleBehavior"); break;
                }
            }

            sb.AppendLine("AudioSource (spatialBlend=1, maxDistance=50)");
            sb.AppendLine("Rigidbody (mass=" + GetWeaponMass() + ", interpolation=Interpolate)");
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

        private void OpenResourcesFolder()
        {
            string path = "Assets/JohnWickArsenal/Resources";
            if (!AssetDatabase.IsValidFolder(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
            EditorUtility.RevealInFinder(Path.GetFullPath(path));
        }

        private void GenerateColliderPresets()
        {
            string path = "Assets/JohnWickArsenal/Editor/ColliderPresets.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string content = @"
using UnityEngine;

namespace JohnWickArsenal.Editor
{
    public static class ColliderPresets
    {
        public static Vector3[] PistolHull = new Vector3[]
        {
            new Vector3(-0.02f, -0.08f, -0.05f),
            new Vector3(0.02f, -0.08f, -0.05f),
            new Vector3(-0.02f, 0.05f, -0.05f),
            new Vector3(0.02f, 0.05f, -0.05f),
            new Vector3(-0.02f, -0.08f, 0.15f),
            new Vector3(0.02f, -0.08f, 0.15f),
            new Vector3(-0.02f, 0.05f, 0.15f),
            new Vector3(0.02f, 0.05f, 0.15f),
        };

        public static Vector3[] RifleHull = new Vector3[]
        {
            new Vector3(-0.03f, -0.1f, -0.1f),
            new Vector3(0.03f, -0.1f, -0.1f),
            new Vector3(-0.03f, 0.08f, -0.1f),
            new Vector3(0.03f, 0.08f, -0.1f),
            new Vector3(-0.03f, -0.1f, 0.5f),
            new Vector3(0.03f, -0.1f, 0.5f),
            new Vector3(-0.03f, 0.08f, 0.5f),
            new Vector3(0.03f, 0.08f, 0.5f),
        };
    }
}
";
            File.WriteAllText(path, content);
            AssetDatabase.Refresh();
            Debug.Log("[JWA Wizard] Generated ColliderPresets.cs");
        }

        private void CreateAnimationTemplate()
        {
            string path = "Assets/JohnWickArsenal/Animations/AnimationTemplate.controller";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Create a basic animator controller template
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);

            // Add parameters
            controller.AddParameter("Fire", UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Reload", UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Draw", UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Holster", UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Inspect", UnityEditor.Animations.AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Empty", UnityEditor.Animations.AnimatorControllerParameterType.Bool);
            controller.AddParameter("Aiming", UnityEditor.Animations.AnimatorControllerParameterType.Bool);

            AssetDatabase.Refresh();
            Debug.Log("[JWA Wizard] Created AnimationTemplate.controller");
        }
    }
}