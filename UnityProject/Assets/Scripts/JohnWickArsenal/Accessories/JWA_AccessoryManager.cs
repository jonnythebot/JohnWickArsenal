using System.Collections.Generic;
using UnityEngine;

namespace JohnWickArsenal.Accessories
{
    /// <summary>
    /// Manages cosmetic accessory attachment on weapons.
    /// Purely visual - no gameplay mechanics affected.
    /// </summary>
    public class JWA_AccessoryManager : MonoBehaviour
    {
        [Header("References")]
        public JWA_AccessoryCatalog catalog;
        
        [Header("Mount Points (Auto-detected or Manual)")]
        public Transform picatinnyMount;
        public Transform mLokMount;
        public Transform barrelMount;
        public Transform gripMount;
        public Transform receiverMount;
        public Transform stockMount;
        
        [Header("Current Attachments")]
        [SerializeField] private List<AttachedAccessory> attachedAccessories = new List<AttachedAccessory>();
        
        [System.Serializable]
        private class AttachedAccessory
        {
            public JWA_AccessoryBase accessory;
            public Transform mountPoint;
            public JWA_AccessoryBase.AccessoryType type;
        }
        
        /// <summary>
        /// Attach a cosmetic accessory to the weapon
        /// </summary>
        public bool AttachAccessory(GameObject accessoryPrefab, Transform mountPoint)
        {
            if (accessoryPrefab == null || mountPoint == null)
                return false;
            
            var accessory = accessoryPrefab.GetComponent<JWA_AccessoryBase>();
            if (accessory == null)
            {
                Debug.LogWarning("[JWA_AccessoryManager] Prefab missing JWA_AccessoryBase component");
                return false;
            }
            
            // Check compatibility
            bool compatible = false;
            foreach (var mount in accessory.compatibleMounts)
            {
                if (mountPoint.name.Contains(mount) || mountPoint.CompareTag(mount))
                {
                    compatible = true;
                    break;
                }
            }
            
            if (!compatible)
            {
                Debug.LogWarning($"[{accessory.GetAccessoryID()}] Not compatible with mount {mountPoint.name}");
                return false;
            }
            
            // Check if mount already has this type
            if (HasAccessoryTypeOnMount(accessory.type, mountPoint))
            {
                Debug.LogWarning($"[{mountPoint.name}] Already has {accessory.type} attached");
                return false;
            }
            
            // Instantiate and attach
            var instance = Instantiate(accessoryPrefab, mountPoint);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            
            var instanceAccessory = instance.GetComponent<JWA_AccessoryBase>();
            if (instanceAccessory.mountPoint != null)
            {
                // Align to accessory's mountPoint
                instance.transform.SetParent(mountPoint, false);
                instanceAccessory.mountPoint.localPosition = Vector3.zero;
                instanceAccessory.mountPoint.localRotation = Quaternion.identity;
            }
            
            attachedAccessories.Add(new AttachedAccessory
            {
                accessory = instanceAccessory,
                mountPoint = mountPoint,
                type = accessory.type
            });
            
            Debug.Log($"[JWA_AccessoryManager] Attached {accessory.GetAccessoryID()} to {mountPoint.name}");
            return true;
        }
        
        /// <summary>
        /// Remove an accessory by type from a specific mount
        /// </summary>
        public bool RemoveAccessory(JWA_AccessoryBase.AccessoryType type, Transform mountPoint)
        {
            for (int i = attachedAccessories.Count - 1; i >= 0; i--)
            {
                var attached = attachedAccessories[i];
                if (attached.type == type && attached.mountPoint == mountPoint && attached.accessory != null)
                {
                    Destroy(attached.accessory.gameObject);
                    attachedAccessories.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Remove all accessories
        /// </summary>
        public void RemoveAllAccessories()
        {
            foreach (var attached in attachedAccessories)
            {
                if (attached.accessory != null)
                    Destroy(attached.accessory.gameObject);
            }
            attachedAccessories.Clear();
        }
        
        /// <summary>
        /// Check if a mount has a specific accessory type
        /// </summary>
        public bool HasAccessoryTypeOnMount(JWA_AccessoryBase.AccessoryType type, Transform mountPoint)
        {
            foreach (var attached in attachedAccessories)
            {
                if (attached.type == type && attached.mountPoint == mountPoint)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get all currently attached accessories
        /// </summary>
        public List<JWA_AccessoryBase> GetAttachedAccessories()
        {
            var list = new List<JWA_AccessoryBase>();
            foreach (var attached in attachedAccessories)
            {
                if (attached.accessory != null)
                    list.Add(attached.accessory);
            }
            return list;
        }
        
        /// <summary>
        /// Validate all attached accessories
        /// </summary>
        public void ValidateAll()
        {
            foreach (var attached in attachedAccessories)
            {
                if (attached.accessory != null)
                    attached.accessory.ValidateSetup();
            }
        }
        
        private void OnValidate()
        {
            if (catalog == null)
            {
                catalog = Resources.Load<JWA_AccessoryCatalog>("JWA_AccessoryCatalog");
            }
        }
    }
}
