using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using ThunderRoad;
using ThunderRoad.AssetSorcery;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JohnWickArsenal.BasSDK.Editor
{
    public static class JwaCosmeticsExporter
    {
        private const string ModFolder = "JohnWickArsenalCosmetics";
        private const string AssetRoot = "Assets/JwaCosmetics";
        private const string ItemId = "jwa_cosmetic_briefcase";
        private const string ItemName = "Briefcase";
        private const string ItemAddress = "JWA.Cosmetics.Prop.Briefcase";
        private const string IconAddress = ItemAddress + ".Icon";
        private const string PrefabPath = AssetRoot + "/Briefcase.prefab";
        private const string IconPath = AssetRoot + "/Briefcase.png";
        private const string ModelPath = AssetRoot + "/Briefcase.fbx";
        private const string GripMarker = "JwaCosmeticGrip";
        private const string ColliderName = "Case";
        private const string ColliderId = "PropDefault";
        private const string HandleName = "Handle";
        private const string HandleId = "ObjectHandleProp";
        private const float Mass = 1.1f;

        public static void ExportBatch()
        {
            string output = Environment.GetEnvironmentVariable("JWA_NOMAD_COSMETICS_OUTPUT");
            if (string.IsNullOrWhiteSpace(output))
                throw new InvalidOperationException("Set JWA_NOMAD_COSMETICS_OUTPUT to an empty export directory.");
            output = Path.GetFullPath(output);
            Directory.CreateDirectory(output);
            if (Directory.EnumerateFileSystemEntries(output).Any())
                throw new InvalidOperationException($"The export directory must be empty: {output}");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException("Launch the SDK project with -buildTarget Android.");
            int androidQuality = Array.IndexOf(QualitySettings.names, "Android");
            if (androidQuality < 0)
                throw new InvalidOperationException("The official SDK project is missing its Android quality profile.");
            QualitySettings.SetQualityLevel(androidQuality, false);
            if (Common.GetQualityLevel(true) != ThunderRoad.QualityLevel.Android)
                throw new InvalidOperationException("The SDK did not select the Android platform profile.");
            AssetSorceryPlatform.AssetSorceryShaderSetPlatform(AssetSorceryPlatformRuntime.AssetSorceryGetBuildPlatform(true));

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                throw new InvalidOperationException("Open the official Warpfrog BasSDK project to export this prop.");
            if (settings.FindGroup(ModFolder) != null)
                throw new InvalidOperationException($"The Addressables group {ModFolder} already exists.");

            BuildCasePrefab();
            ImportSprite();
            var addressableGroup = settings.CreateGroup(ModFolder, false, false, true, null, typeof(BundledAssetGroupSchema));
            Register(settings, addressableGroup, PrefabPath, ItemAddress);
            Register(settings, addressableGroup, IconPath, IconAddress);
            var sprite = AssetDatabase.LoadAllAssetsAtPath(IconPath).OfType<Sprite>().Single();
            string spriteAddress = Catalog.GetAddressFromPrefab(sprite);
            if (string.IsNullOrWhiteSpace(spriteAddress))
                throw new InvalidOperationException("Could not resolve the item book sprite address.");
            AssetDatabase.SaveAssets();

            var exportGroup = ScriptableObject.CreateInstance<AssetBundleGroup>();
            try
            {
                exportGroup.folderName = ModFolder;
                exportGroup.addressableAssetGroups = new List<AddressableAssetGroup> { addressableGroup };
                exportGroup.modName = "John Wick Arsenal Cosmetics";
                exportGroup.modDescription = "A carryable briefcase prop for the Sandbox item book.";
                exportGroup.modAuthor = "John Wick Arsenal contributors";
                exportGroup.modVersion = "1.0.0";
                exportGroup.modThumbnail = "icon.png";
                if (!AssetBundleBuilder.Build(exportGroup, false))
                    throw new InvalidOperationException("The Warpfrog SDK could not export Android Addressables.");

                AssetBundleBuilder.CopyDirectory(Path.GetFullPath(AssetBundleBuilder.assetsLocalPath), output);
                string manifest = Path.Combine(AssetBundleBuilderGUI.GenerateManifest(exportGroup), "manifest.json");
                File.Copy(manifest, Path.Combine(output, "manifest.json"));
                File.Copy(IconPath, Path.Combine(output, "icon.png"));
                File.Copy(AssetRoot + "/ASSET_CREDITS.txt", Path.Combine(output, "ASSET_CREDITS.txt"));
                File.WriteAllText(Path.Combine(output, $"Item_{ItemId}.json"), MakeItem(spriteAddress).ToString());
            }
            finally
            {
                Object.DestroyImmediate(exportGroup);
            }

            Debug.Log($"Exported the briefcase Nomad mod to {output}");
        }

        private static void BuildCasePrefab()
        {
            if (AssetImporter.GetAtPath(ModelPath) is not ModelImporter importer)
                throw new InvalidOperationException($"Unity did not import {ModelPath} as a model.");
            if (!importer.preserveHierarchy)
            {
                importer.preserveHierarchy = true;
                importer.SaveAndReimport();
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
                throw new InvalidOperationException($"Unity could not load the imported model at {ModelPath}.");
            var root = new GameObject(ItemName);
            try
            {
                var visual = Object.Instantiate(model, root.transform, false);
                var marker = visual.GetComponentsInChildren<Transform>(true).SingleOrDefault(child => child.name == GripMarker);
                if (marker == null)
                    throw new InvalidOperationException($"The briefcase model is missing the {GripMarker} transform.");

                var meshes = visual.GetComponentsInChildren<MeshRenderer>();
                if (meshes.Length == 0)
                    throw new InvalidOperationException("The briefcase model has no visible meshes.");
                Bounds bounds = meshes[0].bounds;
                Material material = CreateMaterial();
                foreach (var mesh in meshes)
                {
                    bounds.Encapsulate(mesh.bounds);
                    mesh.sharedMaterials = mesh.sharedMaterials.Select(_ => material).ToArray();
                }

                var body = root.AddComponent<Rigidbody>();
                body.mass = Mass;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                var item = root.AddComponent<Item>();
                item.itemId = ItemId;
                var handle = AddPoint(root.transform, HandleName, marker.position).gameObject.AddComponent<Handle>();
                handle.transform.rotation = marker.rotation;
                handle.interactableId = HandleId;
                handle.touchRadius = 0.075f;
                handle.orientationDefaultRight = handle.AddOrientation(Side.Right, Vector3.zero, Quaternion.identity);
                handle.orientationDefaultLeft = handle.AddOrientation(Side.Left, Vector3.zero, Quaternion.identity);
                foreach (var pose in handle.orientations)
                {
                    pose.handle = handle;
                    pose.targetHandPoseId = "HandleSmall";
                    pose.targetWeight = 1f;
                }
                item.mainHandleRight = item.mainHandleLeft = handle;

                var colliderGroup = AddPoint(root.transform, ColliderName, Vector3.zero).gameObject.AddComponent<ColliderGroup>();
                colliderGroup.colliderGroupId = ColliderId;
                var collider = colliderGroup.gameObject.AddComponent<BoxCollider>();
                // Keep the box on the case body and leave the bridge open while grasping it.
                bounds.SetMinMax(bounds.min, new Vector3(bounds.max.x,
                    Mathf.Clamp(marker.position.y - 0.015f, bounds.center.y, bounds.max.y), bounds.max.z));
                collider.center = bounds.center;
                collider.size = bounds.size;
                collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/SDK/PhysicMaterials/Leather.physicMaterial");
                if (collider.sharedMaterial == null)
                    throw new InvalidOperationException("The SDK's leather collision material is unavailable.");

                item.spawnPoint = AddPoint(root.transform, "SpawnPoint", new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
                item.holderPoint = AddPoint(root.transform, "HolderPoint", bounds.center);
                item.parryPoint = AddPoint(root.transform, "ParryPoint", bounds.center);
                item.preview = AddPoint(root.transform, "Preview", bounds.center).gameObject.AddComponent<Preview>();
                item.preview.renderers = new List<Renderer>(meshes);
                item.preview.size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 1.2f;
                item.preview.transform.localScale = Vector3.one * item.preview.size;

                Physics.SyncTransforms();
                item.SetupDefaultComponents();
                item.audioSource.playOnAwake = false;
                item.audioSource.spatialBlend = 1f;
                if (PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) == null)
                    throw new IOException($"Could not create {PrefabPath}.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Transform AddPoint(Transform root, string name, Vector3 position)
        {
            Transform point = new GameObject(name).transform;
            point.SetParent(root, false);
            point.position = position;
            return point;
        }

        private static Material CreateMaterial()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/SDK/Shadowood/LitMoss/LitMoss-ASshader.ASshader")
                ?? Shader.Find("ThunderRoad/LitMoss - ASshader");
            if (shader == null)
                throw new InvalidOperationException("The SDK's Android LitMoss shader is unavailable.");

            string maps = AssetRoot + "/Briefcase.fbm/Briefcase_";
            var albedo = ImportTexture(maps + "Albedo.png", TextureImporterType.Default);
            var normal = ImportTexture(maps + "Normal.png", TextureImporterType.NormalMap);
            var material = new Material(shader) { name = ItemName };
            material.SetTexture("_BaseMap", albedo);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_UseNormalMap", 1f);
            material.SetFloat("_Smoothness", 0.42f);
            AssetDatabase.CreateAsset(material, AssetRoot + "/Briefcase.mat");
            return material;
        }

        private static Texture2D ImportTexture(string path, TextureImporterType type)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                throw new FileNotFoundException("Missing Unity texture input.", path);
            importer.textureType = type;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path)
                ?? throw new InvalidOperationException($"Unity could not load the imported texture at {path}.");
        }

        private static void ImportSprite()
        {
            if (AssetImporter.GetAtPath(IconPath) is not TextureImporter importer)
                throw new FileNotFoundException("Missing the briefcase item book image.", IconPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }

        private static void Register(AddressableAssetSettings settings, AddressableAssetGroup group, string path, string address)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                throw new FileNotFoundException("Unity asset GUID could not be found.", path);
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
            entry.SetLabel("Android", true, true);
        }

        private static JObject MakeItem(string spriteAddress) => new JObject
        {
            ["$type"] = "ThunderRoad.ItemData, ThunderRoad",
            ["id"] = ItemId,
            ["version"] = 4,
            ["displayName"] = ItemName,
            ["description"] = "A closed briefcase with a carry handle.",
            ["author"] = "John Wick Arsenal contributors",
            ["type"] = "Prop",
            ["category"] = "Cosmetics",
            ["prefabAddress"] = ItemAddress,
            ["iconAddress"] = spriteAddress,
            ["allowedStorage"] = "SandboxAllItems",
            ["grippable"] = true,
            ["overrideMassAndDrag"] = true,
            ["mass"] = Mass,
            ["drag"] = 0.4f,
            ["angularDrag"] = 0.05f,
            ["modules"] = new JArray(),
            ["damagers"] = new JArray(),
            ["colliderGroups"] = new JArray(new JObject
            {
                ["$type"] = "ThunderRoad.ItemData+ColliderGroup, ThunderRoad",
                ["transformName"] = ColliderName,
                ["colliderGroupId"] = ColliderId
            }),
            ["Interactables"] = new JArray(new JObject
            {
                ["$type"] = "ThunderRoad.ItemData+Interactable, ThunderRoad",
                ["transformName"] = HandleName,
                ["interactableId"] = HandleId
            })
        };
    }
}
