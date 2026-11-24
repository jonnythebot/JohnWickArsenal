using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Creates a basic cosmetic preview scene for Unity Cloud Build.
    /// Run via: Tools > John Wick Arsenal > Setup Cosmetic Preview Scene
    /// </summary>
    public class JWA_CosmeticPreviewSceneSetup : EditorWindow
    {
        [MenuItem("Tools/John Wick Arsenal/Setup Cosmetic Preview Scene")]
        public static void CreateCosmeticPreviewScene()
        {
            string scenePath = "Assets/JohnWickArsenal/Scenes/CosmeticPreview.unity";
            
            // Create Scenes directory if needed
            string scenesDir = "Assets/JohnWickArsenal/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesDir))
            {
                AssetDatabase.CreateFolder("Assets/JohnWickArsenal", "Scenes");
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Set up basic lighting for preview
            SetupLighting();
            
            // Create preview camera
            CreatePreviewCamera();
            
            // Create display model parent
            CreateDisplayModelParent();
            
            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[JWA_CosmeticPreviewSceneSetup] Cosmetic preview scene created at {scenePath}");
            EditorUtility.DisplayDialog("Complete", "Cosmetic preview scene created at " + scenePath, "OK");
        }

        private static void SetupLighting()
        {
            // Create directional light for preview
            var lightObj = new GameObject("PreviewLight");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            // Create ambient light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.35f, 0.4f);
            RenderSettings.ambientEquatorColor = new Color(0.2f, 0.2f, 0.25f);
            RenderSettings.ambientGroundColor = new Color(0.1f, 0.1f, 0.15f);
            RenderSettings.ambientIntensity = 1.0f;
        }

        private static void CreatePreviewCamera()
        {
            var camObj = new GameObject("PreviewCamera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.transform.position = new Vector3(0, 1.5f, 3f);
            cam.transform.rotation = Quaternion.Euler(0, 180, 0);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            cam.fieldOfView = 60f;
            
            // Add simple orbit script for preview
            camObj.AddComponent<PreviewOrbitCamera>();
        }

        private static void CreateDisplayModelParent()
        {
            var parent = new GameObject("DisplayModels");
            parent.transform.position = Vector3.zero;
            
            // Add component to auto-load display models at runtime
            parent.AddComponent<CosmeticPreviewLoader>();
        }
    }

    /// <summary>
    /// Simple orbit camera for preview
    /// </summary>
    public class PreviewOrbitCamera : MonoBehaviour
    {
        public float distance = 3f;
        public float xSpeed = 120f;
        public float ySpeed = 120f;
        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;
        public float distanceMin = 1f;
        public float distanceMax = 10f;

        private float x = 0f;
        private float y = 0f;

        void Start()
        {
            var angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
        }

        void LateUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                y = ClampAngle(y, yMinLimit, yMaxLimit);

                var rotation = Quaternion.Euler(y, x, 0);
                var position = rotation * new Vector3(0, 0, -distance) + Vector3.zero;

                transform.rotation = rotation;
                transform.position = position;
            }

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5f, distanceMin, distanceMax);
        }

        static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360;
            if (angle > 360) angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }

    /// <summary>
    /// Loads display models at runtime for preview
    /// </summary>
    public class CosmeticPreviewLoader : MonoBehaviour
    {
        void Start()
        {
            // Load display models from Resources or Addressables
            LoadDisplayModels();
        }

        private void LoadDisplayModels()
        {
            // This would load the display models from Resources/Addressables
            // For now, just log that we're ready
            Debug.Log("[CosmeticPreviewLoader] Cosmetic preview ready. Display models would be loaded from Resources/Addressables here.");
            
            // Example of loading from Resources:
            // var models = Resources.LoadAll<GameObject>("DisplayModels");
            // foreach (var model in models)
            // {
            //     var instance = Instantiate(model, transform);
            //     instance.name = model.name;
            // }
        }
    }
}
