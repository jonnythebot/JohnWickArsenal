using System.Collections.Generic;
using UnityEngine;

namespace JohnWickArsenal.NPCs
{
    /// <summary>
    /// NPC Cosmetic Equipment System for Blade & Sorcery Nomad.
    /// Prepares non-combat cosmetic equipment for NPCs (display props, wardrobe).
    /// Does NOT affect combat, targeting, or weapon mechanics.
    /// </summary>
    public class JWA_NPCCosmeticSetup : MonoBehaviour
    {
        [Header("Cosmetic Wardrobe Definitions")]
        [Tooltip("Static suit meshes for wardrobe authoring. Applying to characters requires skin weights and rig fitting.")]
        public List<CosmeticWardrobeEntry> wardrobeEntries = new List<CosmeticWardrobeEntry>();
        
        [Header("Display Props")]
        [Tooltip("Non-functional props for NPCs to hold/display")]
        public List<DisplayPropEntry> displayProps = new List<DisplayPropEntry>();
        
        [Header("Animation Compatibility")]
        [Tooltip("Reference animations for pose validation")]
        public AnimationClip idlePose;
        public AnimationClip walkPose;
        public AnimationClip holdPistolPose;
        public AnimationClip holdRiflePose;
        public AnimationClip holdKnifePose;
        
        [System.Serializable]
        public class CosmeticWardrobeEntry
        {
            public string suitName;                    // e.g., "ClassicBlackSuit", "DoubleBreastedSuit"
            public string displayName;                 // e.g., "Classic Black Suit (Ch.1)"
            public GameObject staticMeshPrefab;        // Static mesh for authoring/preview
            public Texture2D albedoTexture;            // 1K albedo
            public Texture2D normalTexture;            // 1K normal/bump
            public string sourceLicense;               // CC0, CC BY 4.0, etc.
            public string sourceURL;                   // Original asset URL
            public int triangleCount;                  // For Quest optimization tracking
            public bool requiresRigging;               // True - static meshes need skin weights
            public string[] compatibleRigs;            // Target character rigs (when implemented)
        }
        
        [System.Serializable]
        public class DisplayPropEntry
        {
            public string propName;                    // e.g., "DisplayPistol_1911"
            public string displayName;                 // e.g., "1911 Display Prop"
            public GameObject propPrefab;              // Prop with proper hand attachment hierarchy
            public PropType propType;                  // Pistol, Rifle, Knife, Briefcase, etc.
            public Transform handAttachPoint;          // Transform matching NPC hand bone
            public bool isPriorityProp;                // If true, this is the primary display prop
            public string animationState;              // Animation state name (e.g., "HoldPistol")
        }
        
        public enum PropType
        {
            Pistol,
            Revolver,
            Rifle,
            Shotgun,
            Knife,
            Briefcase,
            Pencil,
            Custom
        }
        
        /// <summary>
        /// Initialize default wardrobe entries for John Wick suits
        /// </summary>
        [ContextMenu("Initialize Default Wardrobe")]
        public void InitializeDefaultWardrobe()
        {
            wardrobeEntries.Clear();
            
            // Classic Black Suit (Chapter 1) - UPDATED with 1K textures
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "ClassicBlackSuit",
                displayName = "Classic Black Suit (Ch.1)",
                staticMeshPrefab = null, // Set in inspector: Assets/JohnWickArsenal/Models/Suits/ClassicBlackSuit
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "CC0 1.0 (MakeHuman Community)",
                sourceURL = "https://static.makehumancommunity.org/assets/assetpacks/suits01.html",
                triangleCount = 19202,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
            
            // Black Tactical Suit (Chapter 2)
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "BlackTacticalSuit",
                displayName = "Black Tactical Suit (Ch.2)",
                staticMeshPrefab = null,
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "Procedural (Quest-optimized)",
                sourceURL = "",
                triangleCount = 15000,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
            
            // Formal Three-Piece Suit (Chapter 3)
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "FormalThreePieceSuit",
                displayName = "Formal Three-Piece Suit (Ch.3)",
                staticMeshPrefab = null,
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "Procedural (Quest-optimized)",
                sourceURL = "",
                triangleCount = 15000,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
            
            // Armored Combat Suit (Chapter 4)
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "ArmoredCombatSuit",
                displayName = "Armored Combat Suit (Ch.4)",
                staticMeshPrefab = null,
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "Procedural (Quest-optimized)",
                sourceURL = "",
                triangleCount = 15000,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
            
            // Elite High-Protection Suit
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "EliteHighProtectionSuit",
                displayName = "Elite High-Protection Suit",
                staticMeshPrefab = null,
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "Procedural (Quest-optimized)",
                sourceURL = "",
                triangleCount = 15000,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
            
            // NEW: Double-Breasted Suit (Cosmetic)
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "DoubleBreastedSuit",
                displayName = "Double-Breasted Suit",
                staticMeshPrefab = null, // Set in inspector: Assets/JohnWickArsenal/Models/Cosmetics/Garments/DoubleBreastedSuit
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "CC0 1.0 (MakeHuman Community)",
                sourceURL = "https://static.makehumancommunity.org/assets/assetpacks/suits01.html",
                triangleCount = 10062,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
            
            // NEW: Dinner Jacket (Cosmetic)
            wardrobeEntries.Add(new CosmeticWardrobeEntry
            {
                suitName = "DinnerJacket",
                displayName = "Dinner Jacket / Tuxedo",
                staticMeshPrefab = null, // Set in inspector: Assets/JohnWickArsenal/Models/Cosmetics/Garments/DinnerJacket
                albedoTexture = null,
                normalTexture = null,
                sourceLicense = "CC0 1.0 (MakeHuman Community)",
                sourceURL = "https://static.makehumancommunity.org/assets/assetpacks/suits01.html",
                triangleCount = 15300,
                requiresRigging = true,
                compatibleRigs = new string[] { "Biped_Standard", "B&S_Nomad_Character" }
            });
        }
        
        /// <summary>
        /// Initialize default display props - prioritize ONE pistol display prop
        /// </summary>
        [ContextMenu("Initialize Default Display Props")]
        public void InitializeDefaultDisplayProps()
        {
            displayProps.Clear();
            
            // PRIORITY: Single pistol-shaped display prop (per requirements)
            displayProps.Add(new DisplayPropEntry
            {
                propName = "DisplayPistol_TTI_PitViper",
                displayName = "TTI Pit Viper Display Prop",
                propPrefab = null, // Set in inspector
                propType = PropType.Pistol,
                handAttachPoint = null, // Set to match NPC hand bone hierarchy
                isPriorityProp = true,  // PRIMARY - the one pistol display prop
                animationState = "HoldPistol"
            });
            
            // Additional display props (secondary, for variety)
            displayProps.Add(new DisplayPropEntry
            {
                propName = "DisplayRevolver_Remington1875",
                displayName = "Remington 1875 Display Prop",
                propPrefab = null,
                propType = PropType.Revolver,
                handAttachPoint = null,
                isPriorityProp = false,
                animationState = "HoldPistol"
            });
            
            displayProps.Add(new DisplayPropEntry
            {
                propName = "DisplayRifle_TTI_TR1",
                displayName = "TTI TR-1 Ultralight Display Prop",
                propPrefab = null,
                propType = PropType.Rifle,
                handAttachPoint = null,
                isPriorityProp = false,
                animationState = "HoldRifle"
            });
            
            displayProps.Add(new DisplayPropEntry
            {
                propName = "DisplayBriefcase",
                displayName = "Briefcase Display Prop",
                propPrefab = null, // Set in inspector: Assets/JohnWickArsenal/Models/Cosmetics/Props/Briefcase/Briefcase.fbx
                propType = PropType.Briefcase,
                handAttachPoint = null,
                isPriorityProp = false,
                animationState = "HoldBriefcase"
            });
            
            displayProps.Add(new DisplayPropEntry
            {
                propName = "DisplayKnife_ThrowingKnife",
                displayName = "Throwing Knife Display Prop",
                propPrefab = null,
                propType = PropType.Knife,
                handAttachPoint = null,
                isPriorityProp = false,
                animationState = "HoldKnife"
            });
            
            displayProps.Add(new DisplayPropEntry
            {
                propName = "DisplayPencil",
                displayName = "Pencil Display Prop",
                propPrefab = null,
                propType = PropType.Pencil,
                handAttachPoint = null,
                isPriorityProp = false,
                animationState = "HoldPencil"
            });
        }
        
        /// <summary>
        /// Get the priority display prop (the one pistol prop)
        /// </summary>
        public DisplayPropEntry GetPriorityDisplayProp()
        {
            return displayProps.Find(p => p.isPriorityProp);
        }
        
        /// <summary>
        /// Validate all wardrobe entries have required assets
        /// </summary>
        public List<string> ValidateWardrobe()
        {
            var issues = new List<string>();
            
            foreach (var entry in wardrobeEntries)
            {
                if (entry.staticMeshPrefab == null)
                    issues.Add($"[{entry.suitName}] Missing static mesh prefab");
                
                if (entry.albedoTexture == null)
                    issues.Add($"[{entry.suitName}] Missing albedo texture");
                
                if (entry.triangleCount > 20000)
                    issues.Add($"[{entry.suitName}] Triangle count ({entry.triangleCount}) may be too high for Quest");
                
                if (!entry.requiresRigging)
                    issues.Add($"[{entry.suitName}] Static meshes require rigging for character use");
            }
            
            foreach (var prop in displayProps)
            {
                if (prop.propPrefab == null)
                    issues.Add($"[{prop.propName}] Missing prop prefab");
                
                if (prop.handAttachPoint == null)
                    issues.Add($"[{prop.propName}] Missing hand attachment point");
            }
            
            // Ensure priority prop exists
            if (GetPriorityDisplayProp() == null)
                issues.Add("No priority display prop defined (isPriorityProp = true)");
            
            return issues;
        }
        
        /// <summary>
        /// Create a display prop prefab with proper hand attachment hierarchy
        /// </summary>
        [ContextMenu("Create Display Prop Template")]
        public void CreateDisplayPropTemplate()
        {
            var prop = new GameObject("DisplayProp_Template");
            prop.transform.position = Vector3.zero;
            prop.transform.rotation = Quaternion.identity;
            
            // Root for NPC hand attachment
            var handRoot = new GameObject("HandAttachPoint");
            handRoot.transform.SetParent(prop.transform);
            handRoot.transform.localPosition = Vector3.zero;
            handRoot.transform.localRotation = Quaternion.identity;
            
            // Model container
            var modelContainer = new GameObject("Model");
            modelContainer.transform.SetParent(prop.transform);
            modelContainer.transform.localPosition = Vector3.zero;
            modelContainer.transform.localRotation = Quaternion.identity;
            
            // Add JWA_NPCDisplayProp component
            var displayProp = prop.AddComponent<JWA_NPCDisplayProp>();
            displayProp.handAttachPoint = handRoot.transform;
            
            Debug.Log("Created DisplayProp_Template - add your model under 'Model' and configure handAttachPoint");
        }
    }
    
    /// <summary>
    /// Component for NPC display props - attach to prop prefabs
    /// </summary>
    public class JWA_NPCDisplayProp : MonoBehaviour
    {
        public Transform handAttachPoint;  // Aligns with NPC hand bone
        public JWA_NPCCosmeticSetup.PropType propType;
        public bool isCosmeticOnly = true;
        
        [Header("Visual Only")]
        public bool castShadows = true;
        public bool receiveShadows = true;
        public int renderQueue = 2000;  // Standard geometry
        
        private void Awake()
        {
            // Ensure cosmetic-only
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = receiveShadows;
                r.renderingLayerMask = 1;  // Default layer
            }
        }
        
        /// <summary>
        /// Align this prop to an NPC hand
        /// </summary>
        public void AttachToHand(Transform npcHandBone)
        {
            if (handAttachPoint == null || npcHandBone == null)
            {
                Debug.LogError("[JWA_NPCDisplayProp] Missing handAttachPoint or npcHandBone");
                return;
            }
            
            // Match the hand bone's position/rotation
            transform.SetParent(npcHandBone, false);
            transform.position = npcHandBone.position;
            transform.rotation = npcHandBone.rotation;
            
            // Apply handAttachPoint offset
            transform.localPosition = -handAttachPoint.localPosition;
            transform.localRotation = Quaternion.Inverse(handAttachPoint.localRotation);
        }
    }
}
