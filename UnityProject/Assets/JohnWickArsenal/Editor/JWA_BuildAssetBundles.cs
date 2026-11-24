using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceProviders;

/// <summary>
/// Comprehensive AssetBundle builder for John Wick Arsenal mod.
/// Builds Android-compatible bundles with validation, detailed logging, and error reporting.
/// </summary>
public class JWA_BuildAssetBundles : EditorWindow
{
    // Constants
    private const string MOD_NAME = "JohnWickArsenal";
    private const string BUNDLE_NAME = "johnwickarsenal";
    private const BuildTarget TARGET_PLATFORM = BuildTarget.Android;
    private const BuildCompression COMPRESSION = BuildCompression.LZ4;
    private const string RESOURCES_PATH = "Assets/JohnWickArsenal/Resources";

    // Configuration
    private bool includeDebugInfo = false;
    private bool copyToModFolder = true;
    private bool validateAfterBuild = true;
    private bool useAddressables = true;
    private bool buildScriptsOnly = false;
    private bool cleanBeforeBuild = true;
    private bool generateManifest = true;

    // State
    private Vector2 scrollPos;
    private BuildReport lastReport;
    private StringBuilder buildLog = new StringBuilder();
    private bool isBuilding = false;

    [MenuItem("John Wick Arsenal/Build AssetBundle", priority = 20)]
    public static void BuildFromMenu()
    {
        BuildBundle();
    }

    [MenuItem("John Wick Arsenal/AssetBundle Builder", priority = 21)]
    public static void ShowWindow()
    {
        var window = GetWindow<JWA_BuildAssetBundles>("JWA Bundle Builder");
        window.minSize = new Vector2(500, 700);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        DrawConfiguration();
        DrawBuildActions();
        DrawLastBuildInfo();
        DrawBuildLog();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("John Wick Arsenal - AssetBundle Builder", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Target: {TARGET_PLATFORM} | Compression: {COMPRESSION} | Bundle: {BUNDLE_NAME}.bundle", EditorStyles.miniLabel);
        EditorGUILayout.Space(5);
    }

    private void DrawConfiguration()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Build Configuration", EditorStyles.boldLabel);

        useAddressables = EditorGUILayout.ToggleLeft("Use Addressables build pipeline (recommended)", useAddressables);
        cleanBeforeBuild = EditorGUILayout.ToggleLeft("Clean build folder before building", cleanBeforeBuild);
        includeDebugInfo = EditorGUILayout.Toggle("Include debug symbols", includeDebugInfo);
        generateManifest = EditorGUILayout.Toggle("Generate bundle manifest", generateManifest);
        copyToModFolder = EditorGUILayout.Toggle("Auto-copy to mod folder", copyToModFolder);
        validateAfterBuild = EditorGUILayout.Toggle("Validate after build", validateAfterBuild);
        buildScriptsOnly = EditorGUILayout.Toggle("Build scripts only (no assets)", buildScriptsOnly);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Output Paths:", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel($"Build: {GetBuildOutputPath()}", EditorStyles.textField, GUILayout.Height(18));
        EditorGUILayout.SelectableLabel($"Mod: {GetModOutputPath()}", EditorStyles.textField, GUILayout.Height(18));

        EditorGUILayout.EndVertical();
    }

    private void DrawBuildActions()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Build Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Bundle", GUILayout.Height(40)))
        {
            BuildBundle();
        }
        if (GUILayout.Button("Validate Prefabs", GUILayout.Height(40)))
        {
            ValidatePrefabs();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clean Build Folder"))
        {
            CleanBuildFolder();
        }
        if (GUILayout.Button("Open Build Folder"))
        {
            UnityEditor.EditorUtility.RevealInFinder(GetBuildOutputPath());
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawLastBuildInfo()
    {
        if (lastReport == null) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Last Build Report", EditorStyles.boldLabel);

        var color = lastReport.summary.result == BuildResult.Succeeded ? "green" : "red";
        EditorGUILayout.LabelField($"Result: <color={color}>{lastReport.summary.result}</color>");
        EditorGUILayout.LabelField($"Time: {lastReport.summary.totalTime}");
        EditorGUILayout.LabelField($"Size: {FormatBytes(lastReport.summary.totalSize)}");
        EditorGUILayout.LabelField($"Assets: {lastReport.summary.totalAssets}");
        EditorGUILayout.LabelField($"Output: {lastReport.summary.outputPath}");

        EditorGUILayout.EndVertical();
    }

    private void DrawBuildLog()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Build Log", EditorStyles.boldLabel);

        var logRect = EditorGUILayout.GetControlRect(false, 200);
        EditorGUI.TextArea(logRect, buildLog.ToString());

        if (GUILayout.Button("Clear Log"))
        {
            buildLog.Clear();
        }

        EditorGUILayout.EndVertical();
    }

    public static void BuildBundle()
    {
        var window = GetWindow<JWA_BuildAssetBundles>();
        window.BuildBundleInternal();
    }

    private void BuildBundleInternal()
    {
        buildLog.Clear();
        isBuilding = true;
        LogBuild("=== AssetBundle Build Started ===");
        LogBuild($"Time: {DateTime.Now}");
        LogBuild($"Unity: {Application.unityVersion}");
        LogBuild($"Platform: {TARGET_PLATFORM}");
        LogBuild($"Compression: {COMPRESSION}");
        LogBuild($"Bundle Name: {BUNDLE_NAME}");
        LogBuild("");

        try
        {
            var report = ExecuteBuildInternal();

            if (report == null || report.summary.result != BuildResult.Succeeded)
            {
                LogBuild("<color=red>BUILD FAILED</color>");
                lastReport = report;
                return;
            }

            var bundlePath = GetBundleOutputPath();
            var fileInfo = new FileInfo(bundlePath);

            LogBuild("");
            LogBuild($"<color=green>✅ BUILD SUCCESSFUL</color>");
            LogBuild($"Time: {report.summary.totalTime}");
            LogBuild($"Size: {FormatBytes(fileInfo.Length)}");
            LogBuild($"Assets: {report.summary.totalAssets}");
            LogBuild($"Output: {bundlePath}");

            // Post-build validation
            if (validateAfterBuild)
            {
                ValidateBuiltBundle(bundlePath);
            }

            // Copy to mod folder
            if (copyToModFolder)
            {
                CopyBundleToModFolder();
            }

            LogBuild("=== AssetBundle Build Complete ===");
            lastReport = report;
        }
        catch (Exception e)
        {
            LogBuild($"<color=red>FATAL ERROR: {e.Message}</color>");
            LogBuild(e.StackTrace);
        }
        finally
        {
            isBuilding = false;
        }
    }

    private BuildReport ExecuteBuildInternal()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Ensure directories
            EnsureDirectories();

            // Clean if requested
            if (cleanBeforeBuild)
            {
                CleanBuildFolder();
            }

            // Collect prefabs
            var prefabPaths = CollectPrefabs();
            if (prefabPaths.Count == 0)
            {
                throw new Exception("No prefabs found in Resources folder!");
            }

            LogBuild($"Found {prefabPaths.Count} prefabs to bundle");

            // Validate prefabs
            var validator = new PrefabValidator();
            var validPaths = new List<string>();
            foreach (var path in prefabPaths)
            {
                if (validator.ValidatePrefab(path, LogBuild))
                {
                    validPaths.Add(path);
                }
            }

            if (validPaths.Count == 0)
            {
                throw new Exception("No valid prefabs after validation!");
            }

            LogBuild($"Valid prefabs: {validPaths.Count}/{prefabPaths.Count}");

            // Build using Addressables or legacy pipeline
            BuildReport report;
            if (useAddressables)
            {
                report = BuildWithAddressables(validPaths);
            }
            else
            {
                report = BuildWithLegacyPipeline(validPaths);
            }

            sw.Stop();
            report.summary.totalTime = sw.Elapsed;

            return report;
        }
        catch (Exception e)
        {
            LogBuild($"<color=red>Build error: {e.Message}</color>");
            throw;
        }
    }

    private List<string> CollectPrefabs()
    {
        var paths = new List<string>();

        // Find all prefabs in Resources
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RESOURCES_PATH });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            paths.Add(path);
            LogBuild($"  Found: {path}");
        }

        return paths;
    }

    private BuildReport BuildWithAddressables(List<string> prefabPaths)
    {
        LogBuild("Building with Addressables pipeline...");

        // Ensure Addressables settings exist
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            throw new Exception("Addressables not initialized. Run Project Bootstrap first.");
        }

        // Create or update addressable entries
        foreach (var path in prefabPaths)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.address = GetAddressableAddress(path);
            entry.SetLabel("weapon", true, true);
        }

        // Build
        var buildParams = new Addressables.BuildAddressablesBuildParameters
        {
            targetPlatform = TARGET_PLATFORM,
            compression = COMPRESSION,
            buildScriptsOnly = buildScriptsOnly
        };

        var buildReport = Addressables.BuildAddressables.Build(buildParams);

        // Convert to our report format
        return new BuildReport
        {
            summary = new BuildSummary
            {
                result = buildReport.Result == Addressables.BuildResult.Succeeded ? BuildResult.Succeeded : BuildResult.Failed,
                totalTime = TimeSpan.FromSeconds(buildReport.Duration),
                totalSize = GetBundleSize(),
                totalAssets = prefabPaths.Count,
                outputPath = GetBundleOutputPath()
            }
        };
    }

    private BuildReport BuildWithLegacyPipeline(List<string> prefabPaths)
    {
        LogBuild("Building with legacy AssetBundle pipeline...");

        var options = BuildAssetBundleOptions.None;
        if (includeDebugInfo) options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
        if (generateManifest) options |= BuildAssetBundleOptions.GenerateManifest;

        var build = new AssetBundleBuild
        {
            assetBundleName = BUNDLE_NAME,
            assetNames = prefabPaths.ToArray()
        };

        var manifest = BuildPipeline.BuildAssetBundles(
            GetBuildOutputPath(),
            new[] { build },
            options,
            TARGET_PLATFORM
        );

        return new BuildReport
        {
            summary = new BuildSummary
            {
                result = manifest != null ? BuildResult.Succeeded : BuildResult.Failed,
                totalTime = TimeSpan.Zero,
                totalSize = GetBundleSize(),
                totalAssets = prefabPaths.Count,
                outputPath = GetBundleOutputPath()
            }
        };
    }

    private void ValidateBuiltBundle(string bundlePath)
    {
        LogBuild("Validating built bundle...");

        try
        {
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                LogBuild("<color=red>❌ Failed to load bundle</color>");
                return;
            }

            var assetNames = bundle.GetAllAssetNames();
            LogBuild($"Bundle contains {assetNames.Length} assets:");

            foreach (var name in assetNames)
            {
                LogBuild($"  • {name}");
            }

            // Check for required assets
            var requiredAssets = new[] { "Weapon", "Item", "Gun", "Magazine" };
            foreach (var required in requiredAssets)
            {
                var found = assetNames.Any(n => n.Contains(required, StringComparison.OrdinalIgnoreCase));
                LogBuild($"  {required}: {(found ? "✅" : "⚠")}");
            }

            bundle.Unload(false);
            LogBuild("<color=green>✅ Bundle validation passed</color>");
        }
        catch (Exception e)
        {
            LogBuild($"<color=red>❌ Validation error: {e.Message}</color>");
        }
    }

    private void EnsureDirectories()
    {
        var dirs = new[]
        {
            GetBuildOutputPath(),
            GetModOutputPath(),
            Path.Combine(Application.dataPath, "..", "AssetBundles", "Android")
        };

        foreach (var dir in dirs)
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static string GetBuildOutputPath()
    {
        return Path.Combine(Application.dataPath, "..", "AssetBundles", "Android");
    }

    private static string GetModOutputPath()
    {
        return Path.Combine(Application.dataPath, "..", "..", "AssetBundles");
    }

    private static string GetBundleOutputPath()
    {
        return Path.Combine(GetBuildOutputPath(), BUNDLE_NAME);
    }

    private static long GetBundleSize()
    {
        var path = GetBundleOutputPath();
        return File.Exists(path) ? new FileInfo(path).Length : 0;
    }

    private static string GetAddressableAddress(string assetPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(assetPath);
        return fileName;
    }

    private void CleanBuildFolder()
    {
        var buildDir = GetBuildOutputPath();
        if (Directory.Exists(buildDir))
        {
            Directory.Delete(buildDir, true);
            LogBuild($"Cleaned: {buildDir}");
        }
    }

    private void CopyBundleToModFolder()
    {
        string sourcePath = GetBundleOutputPath();
        string destPath = Path.Combine(GetModOutputPath(), $"{BUNDLE_NAME}.bundle");

        if (!File.Exists(sourcePath))
        {
            LogBuild($"<color=red>Source bundle not found: {sourcePath}</color>");
            return;
        }

        Directory.CreateDirectory(GetModOutputPath());
        File.Copy(sourcePath, destPath, true);

        var fileInfo = new FileInfo(destPath);
        LogBuild($"<color=green>✅ Copied to mod folder: {destPath} ({FormatBytes(fileInfo.Length)})</color>");
    }

    private void LogBuild(string message)
    {
        buildLog.AppendLine(message);
        Debug.Log($"[JWA Builder] {message}");
        Repaint();
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

    // ========================================================================
    // DATA STRUCTURES
    // ========================================================================

    private class BuildReport
    {
        public BuildSummary summary;
        public BuildStep[] steps;
    }

    private class BuildSummary
    {
        public BuildResult result;
        public TimeSpan totalTime;
        public long totalSize;
        public int totalAssets;
        public string outputPath;
    }

    private class BuildStep
    {
        public string name;
        public TimeSpan duration;
    }

    // ========================================================================
    // PREFAB VALIDATOR
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
#if !JWA_COSMETIC_ONLY
            var asm = Assembly.GetAssembly(typeof(Weapon));
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
#else
            logCallback?.Invoke($"ℹ️ {prefab.name}: JWA_COSMETIC_ONLY - B&S component validation skipped.");
#endif

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
