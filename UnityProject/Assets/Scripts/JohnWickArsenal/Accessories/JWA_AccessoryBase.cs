using UnityEngine;

namespace JohnWickArsenal.Accessories
{
    /// <summary>
    /// Base component for all cosmetic accessories.
    /// Attach to accessory prefabs to identify them and provide attachment metadata.
    /// </summary>
    public class JWA_AccessoryBase : MonoBehaviour
    {
        public enum AccessoryType
        {
            Flashlight,
            LaserModule,
            Grip,
            Sight,
            Suppressor,
            MuzzleBrake,
            RailCover,
            SlingMount,
            MagwellFlare,
            ChargingHandle
        }

        [Header("Accessory Identity")]
        public AccessoryType type = AccessoryType.Flashlight;
        public string displayName = "Accessory";
        public string variant = "Default";
        
        [Header("Attachment Points")]
        public Transform mountPoint;           // Primary attachment point (required)
        public Transform offsetPoint;          // Optional offset for fine positioning
        
        [Header("Compatibility")]
        public string[] compatibleMounts = new string[] { "Picatinny", "M-LOK" };
        public bool requiresGunsmith = false;  // If true, only attachable at workbench
        
        [Header("Visual")]
        public bool isCosmeticOnly = true;     // Always true - no gameplay effect
        public int maxPolygonCount = 500;      // Quest optimization target
        
        /// <summary>
        /// Returns a unique identifier for this accessory variant
        /// </summary>
        public string GetAccessoryID()
        {
            return $"JWA_Accessory_{type}_{variant}".Replace(" ", "_");
        }
        
        /// <summary>
        /// Validates the accessory prefab setup
        /// </summary>
        public bool ValidateSetup()
        {
            if (mountPoint == null)
            {
                Debug.LogError($"[{GetAccessoryID()}] Missing mountPoint!");
                return false;
            }
            
            var renderers = GetComponentsInChildren<Renderer>(true);
            int triCount = 0;
            foreach (var r in renderers)
            {
                if (r is MeshRenderer mr && mr.GetComponent<MeshFilter>())
                {
                    triCount += mr.GetComponent<MeshFilter>().sharedMesh?.triangles.Length / 3 ?? 0;
                }
                else if (r is SkinnedMeshRenderer smr)
                {
                    triCount += smr.sharedMesh?.triangles.Length / 3 ?? 0;
                }
            }
            
            if (triCount > maxPolygonCount)
            {
                Debug.LogWarning($"[{GetAccessoryID()}] Triangle count ({triCount}) exceeds target ({maxPolygonCount})");
            }
            
            return true;
        }
    }
}
