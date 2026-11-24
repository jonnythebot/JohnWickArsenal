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
        private static readonly string[] CosmeticRoots = {
            "Assets/JohnWickArsenal/Models/Suits/",
            "Assets/JohnWickArsenal/Cosmetics/",
            "Assets/JohnWickArsenal/Models/Cosmetics/"
        };

        public static void ExportBatch()
        {
            string output = Environment.GetEnvironmentVariable("JWA_COSMETICS_OUTPUT");
            if (string.IsNullOrEmpty(output))
                throw new InvalidOperationException("Set JWA_COSMETICS_OUTPUT to an empty folder to run the batch export.");
            Export(output);
        }

        private static void Export(string output)
        {
            if (Application.unityVersion != EditorVersion)
                throw new InvalidOperationException($"Use Unity {EditorVersion} for this project.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException("Select Android in Build Settings or start Unity with -buildTarget Android.");

            Directory.CreateDirectory(output);
            if (Directory.EnumerateFileSystemEntries(output).Any())
                throw new InvalidOperationException($"Choose an empty export directory: {output}");

            string[] models = AssetDatabase.GetAllAssetPaths()
                .Where(path => CosmeticRoots.Any(root => path.StartsWith(root, StringComparison.Ordinal)))
                .Where(path => AssetImporter.GetAtPath(path) is ModelImporter)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (models.Length == 0)
                throw new InvalidOperationException("No Unity-imported cosmetic models were found.");

            foreach (string path in models)
            {
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null || model.GetComponentsInChildren<Renderer>(true).Length == 0)
                    throw new InvalidOperationException($"Cosmetic model has no imported renderers: {path}");
            }

            var layout = new AssetBundleBuild { assetBundleName = BundleName, assetNames = models };
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(output, new[] { layout },
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
            if (manifest == null || !manifest.GetAllAssetBundles().Contains(BundleName))
                throw new InvalidOperationException("Unity failed to build the Android cosmetic review bundle.");

            Debug.Log($"Exported {models.Length} cosmetic models for Android at {Path.Combine(output, BundleName)}");
        }
    }
}
