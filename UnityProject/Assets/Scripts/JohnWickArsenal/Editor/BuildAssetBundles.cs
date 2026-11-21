using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.Build.Content;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Comprehensive AssetBundle builder for John Wick Arsenal mod.
    /// Builds Android-compatible bundles with LZ4 compression for Nomad.
    /// </summary>
    public class BuildAssetBundles : EditorWindow
    {
        // Configuration
        private const string MOD_NAME = "JohnWickArsenal";
        private const string BUNDLE_NAME = "johnwickarsenal";
        private const BuildTarget TARGET_PLATFORM = BuildTarget.Android;
        private const BuildCompression COMPRESSION = BuildCompression.LZ4;

        // Paths
        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string ModRoot => Path.Combine(ProjectRoot, "..", "JohnWickArsenal");
        private static string BundleOutputDir => Path.Combine(ProjectRoot, "AssetBundles", "Android");
        private static string ModBundleOutputDir => Path.Combine(ModRoot, "AssetBundles");
        private static string ResourcesPath => "Assets/JohnWickArsenal/Resources";

        // UI State
        private Vector2 scrollPos;
        private bool includeDebugInfo = true;
        private bool copyToModFolder = true;
        private bool validateAfterBuild = true;
        private BuildReport lastReport;

        [MenuItem("John Wick Arsenal/Build AssetBundle", priority = 1)]
        public static void BuildFromMenu()
        {
            BuildBundle();
        }

        [MenuItem("John Wick Arsenal/Open Builder Window", priority = 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildAssetBundles>("JWA Bundle Builder");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        [MenuItem("John Wick Arsenal/Validate Prefabs", priority = 3)]
        public static void ValidatePrefabs()
        {
            var validator = new PrefabValidator();
            validator.ValidateAll();
        }

        [MenuItem("John Wick Arsenal/Copy Bundle to Mod Folder", priority = 4)]
        public static void CopyBundleToMod()
        {
            CopyBundleToModFolder();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(10);
            DrawHeader();

            EditorGUILayout.Space(10);
            DrawConfiguration();

            EditorGUILayout.Space(10);
            DrawBuildActions();

            EditorGUILayout.Space(10);
            DrawLastBuildInfo();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("John Wick Arsenal - AssetBundle Builder", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Target: {TARGET_PLATFORM} | Compression: {COMPRESSION}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Bundle Name: {BUNDLE_NAME}.bundle", EditorStyles.miniLabel);
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

            includeDebugInfo = EditorGUILayout.Toggle("Include Debug Symbols", includeDebugInfo);
            copyToModFolder = EditorGUILayout.Toggle("Auto-copy to Mod Folder", copyToModFolder);
            validateAfterBuild = EditorGUILayout.Toggle("Validate After Build", validateAfterBuild);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Output Paths:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Build: {BundleOutputDir}", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField($"Mod: {ModBundleOutputDir}", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            GUI.enabled = !EditorApplication.isCompiling;

            if (GUILayout.Button("🔨 Build AssetBundle", GUILayout.Height(40)))
            {
                BuildBundle();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("✅ Validate Prefabs"))
            {
                ValidatePrefabs();
            }
            if (GUILayout.Button("📦 Copy to Mod Folder"))
            {
                CopyBundleToModFolder();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🧹 Clean Build Folder"))
            {
                CleanBuildFolder();
            }
            if (GUILayout.Button("📋 Open Mod Folder"))
            {
                OpenModFolder();
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }

        private void DrawLastBuildInfo()
        {
            if (lastReport != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Last Build Report", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Status: {lastReport.summary.result}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Time: {lastReport.summary.totalTime}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Size: {FormatBytes(lastReport.summary.totalSize)}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Assets: {lastReport.summary.totalAssets}", EditorStyles.miniLabel);

                if (lastReport.errors.Length > 0)
                {
                    EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
                    foreach (var err in lastReport.errors.Take(5))
                    {
                        EditorGUILayout.LabelField($"  • {err}", EditorStyles.wordWrappedMiniLabel);
                    }
                }

                if (GUILayout.Button("Clear Report"))
                {
                    lastReport = null;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public static BuildReport BuildBundle()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Ensure directories exist
                Directory.CreateDirectory(BundleOutputDir);
                Directory.CreateDirectory(ModBundleOutputDir);

                // Collect all prefabs in Resources
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ResourcesPath });

                if (prefabGuids.Length == 0)
                {
                    Debug.LogError("[JWA Builder] No prefabs found in Resources folder!");
                    return null;
                }

                var assetPaths = prefabGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();

                Debug.Log($"[JWA Builder] Found {assetPaths.Length} prefabs to bundle");

                // Validate each prefab
                var validator = new PrefabValidator();
                var validPaths = new List<string>();
                foreach (var path in assetPaths)
                {
                    if (validator.ValidatePrefab(path))
                    {
                        validPaths.Add(path);
                    }
                }

                if (validPaths.Count == 0)
                {
                    Debug.LogError("[JWA Builder] No valid prefabs after validation!");
                    return null;
                }

                // Build using BuildPipeline (legacy but compatible)
                var options = BuildAssetBundleOptions.None;
                if (includeDebugInfo) options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

                var manifest = BuildPipeline.BuildAssetBundles(
                    BundleOutputDir,
                    new[] { new AssetBundleBuild
                    {
                        assetBundleName = BUNDLE_NAME,
                        assetNames = validPaths.ToArray()
                    } },
                    options,
                    TARGET_PLATFORM
                );

                sw.Stop();

                if (manifest == null)
                {
                    Debug.LogError("[JWA Builder] Build failed - manifest is null");
                    return null;
                }

                var bundlePath = Path.Combine(BundleOutputDir, BUNDLE_NAME);
                var fileInfo = new FileInfo(bundlePath);

                var report = new BuildReport
                {
                    summary = new BuildSummary
                    {
                        result = BuildResult.Succeeded,
                        totalTime = sw.Elapsed,
                        totalSize = fileInfo.Exists ? fileInfo.Length : 0,
                        totalAssets = validPaths.Count,
                        outputPath = bundlePath
                    },
                    errors = Array.Empty<string>()
                };

                Debug.Log($"[JWA Builder] ✅ Build successful! Size: {FormatBytes(fileInfo.Length)}, Time: {sw.Elapsed}");

                // Copy to mod folder if enabled
                if (copyToModFolder)
                {
                    CopyBundleToModFolder();
                }

                // Validate if enabled
                if (validateAfterBuild)
                {
                    ValidateBuiltBundle(bundlePath);
                }

                // Update window if open
                var window = GetWindow<BuildAssetBundles>();
                if (window != null) window.lastReport = report;

                return report;
            }
            catch (Exception e)
            {
                sw.Stop();
                Debug.LogError($"[JWA Builder] Build failed: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private static void ValidateBuiltBundle(string bundlePath)
        {
            try
            {
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    Debug.LogError("[JWA Builder] Validation failed: Could not load bundle");
                    return;
                }

                var assetNames = bundle.GetAllAssetNames();
                Debug.Log($"[JWA Builder] Validation: Bundle contains {assetNames.Length} assets:");
                foreach (var name in assetNames)
                {
                    Debug.Log($"  • {name}");
                }

                bundle.Unload(false);
                Debug.Log("[JWA Builder] ✅ Bundle validation passed");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JWA Builder] Validation error: {e.Message}");
            }
        }

        public static void CopyBundleToModFolder()
        {
            string sourcePath = Path.Combine(BundleOutputDir, BUNDLE_NAME);
            string destPath = Path.Combine(ModBundleOutputDir, $"{BUNDLE_NAME}.bundle");

            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"[JWA Builder] Source bundle not found: {sourcePath}");
                return;
            }

            Directory.CreateDirectory(ModBundleOutputDir);
            File.Copy(sourcePath, destPath, true);

            var fileInfo = new FileInfo(destPath);
            Debug.Log($"[JWA Builder] ✅ Copied to mod folder: {destPath} ({FormatBytes(fileInfo.Length)})");
        }

        private static void CleanBuildFolder()
        {
            if (Directory.Exists(BundleOutputDir))
            {
                Directory.Delete(BundleOutputDir, true);
                Debug.Log("[JWA Builder] Cleaned build folder");
            }
        }

        private static void OpenModFolder()
        {
            if (Directory.Exists(ModRoot))
            {
                EditorUtility.RevealInFinder(ModRoot);
            }
            else
            {
                Debug.LogWarning($"[JWA Builder] Mod folder not found: {ModRoot}");
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int i = 0;
            double size = bytes;
            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }
            return $"{size:F1} {suffixes[i]}";
        }

        // Data structures
        private class BuildReport
        {
            public BuildSummary summary;
            public string[] errors;
        }

        private class BuildSummary
        {
            public BuildResult result;
            public TimeSpan totalTime;
            public long totalSize;
            public int totalAssets;
            public string outputPath;
        }
    }

    /// <summary>
    /// Validates weapon prefabs have all required components
    /// </summary>
    public class PrefabValidator
    {
        private readonly List<string> errors = new List<string>();
        private readonly List<string> warnings = new List<string>();

        public bool ValidatePrefab(string prefabPath)
        {
            errors.Clear();
            warnings.Clear();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                errors.Add($"Could not load prefab: {prefabPath}");
                return false;
            }

            // Check root components
            ValidateRootComponents(prefab);

            // Check child structure
            ValidateChildStructure(prefab);

            // Check for missing scripts
            CheckMissingScripts(prefab);

            // Log results
            if (errors.Count > 0)
            {
                Debug.LogError($"[JWA Validator] ❌ {prefab.name}: {errors.Count} errors");
                foreach (var e in errors) Debug.LogError($"  • {e}");
                return false;
            }

            if (warnings.Count > 0)
            {
                Debug.LogWarning($"[JWA Validator] ⚠️ {prefab.name}: {warnings.Count} warnings");
                foreach (var w in warnings) Debug.LogWarning($"  • {w}");
            }

            Debug.Log($"[JWA Validator] ✅ {prefab.name} passed validation");
            return true;
        }

        public void ValidateAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/JohnWickArsenal/Resources" });
            int passed = 0, failed = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ValidatePrefab(path)) passed++; else failed++;
            }

            Debug.Log($"[JWA Validator] Complete: {passed} passed, {failed} failed");
        }

        private void ValidateRootComponents(GameObject prefab)
        {
            // Must have either Weapon or Item component
            var weapon = prefab.GetComponent<Weapon>();
            var item = prefab.GetComponent<Item>();

            if (weapon == null && item == null)
            {
                errors.Add("Missing Weapon or Item component on root");
            }

            if (weapon != null)
            {
                ValidateWeapon(prefab, weapon);
            }
            else if (item != null)
            {
                ValidateItem(prefab, item);
            }

            // Required components
            if (prefab.GetComponent<Rigidbody>() == null)
                warnings.Add("Missing Rigidbody on root");

            if (prefab.GetComponent<Collider>() == null)
                warnings.Add("Missing Collider on root");

            // Custom behavior
            var behaviors = prefab.GetComponents<MonoBehaviour>().Where(mb =>
                mb.GetType().Namespace == "JohnWickArsenal.Behaviors").ToArray();

            if (behaviors.Length == 0)
                warnings.Add("No JohnWickArsenal behavior attached");
        }

        private void ValidateWeapon(GameObject prefab, Weapon weapon)
        {
            var gun = prefab.GetComponent<Gun>();
            if (gun == null) errors.Add("Weapon has no Gun component");

            var magazine = prefab.GetComponentInChildren<Magazine>();
            if (magazine == null) errors.Add("Weapon has no Magazine component");

            // Check required transforms
            CheckTransform(prefab, "MuzzlePoint", "Muzzle point transform missing");
            CheckTransform(prefab, "EjectionPort", "Ejection port transform missing");
            CheckTransform(prefab, "Grip_L", "Left grip transform missing");
            CheckTransform(prefab, "Grip_R", "Right grip transform missing");
        }

        private void ValidateItem(GameObject prefab, Item item)
        {
            var throwable = prefab.GetComponent<Throwable>();
            if (throwable == null) errors.Add("Throwable item has no Throwable component");

            CheckTransform(prefab, "Tip", "Tip transform missing (for penetration)");
        }

        private void ValidateChildStructure(GameObject prefab)
        {
            // Check for Model child
            var model = prefab.transform.Find("Model");
            if (model == null)
            {
                warnings.Add("No 'Model' child found (mesh container)");
            }
            else
            {
                var renderers = model.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    warnings.Add("Model has no Renderers");
            }

            // Check for Magazine_Model
            var magModel = prefab.transform.Find("Magazine_Model");
            if (magModel == null)
                warnings.Add("No 'Magazine_Model' child found");
        }

        private void CheckTransform(GameObject prefab, string name, string errorMsg)
        {
            var t = prefab.transform.Find(name);
            if (t == null) errors.Add(errorMsg);
        }

        private void CheckMissingScripts(GameObject prefab)
        {
            var behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var b in behaviours)
            {
                if (b == null)
                {
                    errors.Add($"Missing script on {GetGameObjectPath(b.gameObject)}");
                }
            }
        }

        private string GetGameObjectPath(GameObject obj)
        {
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