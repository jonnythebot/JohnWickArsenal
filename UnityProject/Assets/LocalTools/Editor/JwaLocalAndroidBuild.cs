using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JohnWickArsenal.LocalTools
{
    public static class JwaLocalAndroidBuild
    {
        private const string EditorVersion = "2022.3.20f1";
        private const string BundleName = "johnwickarsenal-cosmetics.bundle";
        private const string AssetRoot = "Assets/JohnWickArsenal/Models/";
        private static readonly string[] CosmeticRoots = {
            AssetRoot + "Suits/",
            AssetRoot + "Cosmetics/"
        };
        private static readonly string[] RequiredModels = {
            AssetRoot + "Suits/ClassicBlackSuit.obj",
            AssetRoot + "Cosmetics/Garments/DinnerJacket/DinnerJacket.obj",
            AssetRoot + "Cosmetics/Garments/DoubleBreastedSuit/DoubleBreastedSuit.obj",
            AssetRoot + "Cosmetics/Props/Briefcase/Briefcase.fbx"
        };

        [Serializable]
        private sealed class ImportReport
        {
            public string unityVersion;
            public string target;
            public string[] models;
        }

        [MenuItem("John Wick Arsenal/Validate Cosmetic Imports")]
        public static void ValidateMenu()
        {
            CheckEditor();
            ValidateModels();
        }

        [MenuItem("John Wick Arsenal/Export Android Cosmetic Review Bundle")]
        public static void ExportMenu()
        {
            CheckEditor();
            string parent = EditorUtility.OpenFolderPanel("Choose a parent folder for the cosmetic export", "", "");
            if (string.IsNullOrEmpty(parent))
                return;
            Export(Path.Combine(parent, "cosmetics-android-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")));
        }

        public static void ExportBatch() => Export(BatchOutput());

        public static void ValidateBatch()
        {
            CheckEditor();
            WriteImportReport(BatchOutput(), ValidateModels());
        }

        private static string BatchOutput()
        {
            string output = Environment.GetEnvironmentVariable("JWA_COSMETICS_OUTPUT");
            if (string.IsNullOrEmpty(output))
                throw new InvalidOperationException("Set JWA_COSMETICS_OUTPUT to an empty folder for the Unity report.");
            return output;
        }

        private static void Export(string output)
        {
            CheckEditor();
            Directory.CreateDirectory(output);
            if (Directory.EnumerateFileSystemEntries(output).Any())
                throw new InvalidOperationException($"Choose an empty export directory: {output}");

            string[] models = ValidateModels();
            var layout = new AssetBundleBuild { assetBundleName = BundleName, assetNames = models };
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(output, new[] { layout },
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
            if (manifest == null || !manifest.GetAllAssetBundles().Contains(BundleName))
                throw new InvalidOperationException("Unity failed to build the Android cosmetic review bundle.");

            WriteImportReport(output, models);
            Debug.Log($"Exported {models.Length} cosmetic models for Android at {Path.Combine(output, BundleName)}");
        }

        private static void CheckEditor()
        {
            if (Application.unityVersion != EditorVersion)
                throw new InvalidOperationException($"Use Unity {EditorVersion} for this project.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Install Android Build Support for this editor in Unity Hub.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException("Select Android in Build Settings or start Unity with -buildTarget Android.");
        }

        private static string[] ValidateModels()
        {
            string[] models = AssetDatabase.GetAllAssetPaths()
                .Where(path => CosmeticRoots.Any(root => path.StartsWith(root, StringComparison.Ordinal)))
                .Where(path => AssetImporter.GetAtPath(path) is ModelImporter)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            string[] missing = RequiredModels.Except(models, StringComparer.Ordinal).ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException($"Missing Unity model imports: {string.Join(", ", missing)}");

            foreach (string path in models)
            {
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var renderers = model == null ? Array.Empty<Renderer>() : model.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0 || renderers.Any(renderer => renderer.sharedMaterials.Length == 0
                    || renderer.sharedMaterials.Any(material => material == null)))
                    throw new InvalidOperationException($"Cosmetic model has missing renderers or material references: {path}");
            }
            Debug.Log($"Validated {models.Length} cosmetic model imports for Android.");
            return models;
        }

        private static void WriteImportReport(string output, string[] models)
        {
            File.WriteAllText(Path.Combine(output, "cosmetic-imports.json"), JsonUtility.ToJson(new ImportReport
            {
                unityVersion = Application.unityVersion,
                target = EditorUserBuildSettings.activeBuildTarget.ToString(),
                models = models
            }, true));
        }
    }
}
