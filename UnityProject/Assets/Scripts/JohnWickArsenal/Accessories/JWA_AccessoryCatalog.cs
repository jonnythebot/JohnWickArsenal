using System.Collections.Generic;
using UnityEngine;

namespace JohnWickArsenal.Accessories
{
    /// <summary>
    /// Catalog of all available cosmetic accessories.
    /// This is a ScriptableObject for easy editing in Unity Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "JWA_AccessoryCatalog", menuName = "John Wick Arsenal/Accessory Catalog")]
    public class JWA_AccessoryCatalog : ScriptableObject
    {
        [System.Serializable]
        public class AccessoryEntry
        {
            public JWA_AccessoryBase.AccessoryType type;
            public string variant;
            public GameObject prefab;
            public string description;
            public int sortOrder;
        }
        
        [Header("Flashlights")]
        public List<AccessoryEntry> flashlights = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Flashlight, variant = "Tactical_Standard", description = "Standard tactical weapon light", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Flashlight, variant = "Tactical_Compact", description = "Compact tactical light for pistols", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Flashlight, variant = "Tactical_IR", description = "IR illuminator for NVG use", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Flashlight, variant = "Pressure_Switch", description = "Remote pressure switch (cosmetic)", sortOrder = 3 },
        };
        
        [Header("Laser Modules")]
        public List<AccessoryEntry> laserModules = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.LaserModule, variant = "Visible_Red", description = "Visible red laser diode housing", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.LaserModule, variant = "Visible_Green", description = "Visible green laser diode housing", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.LaserModule, variant = "IR_PEQ", description = "PEQ-style IR laser/illuminator housing", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.LaserModule, variant = "DBAL_Combo", description = "DBAL-style visible/IR combo housing", sortOrder = 3 },
        };
        
        [Header("Grips")]
        public List<AccessoryEntry> grips = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Grip, variant = "Vertical_Tactical", description = "Vertical foregrip, tactical style", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Grip, variant = "Angled_Magpul", description = "Angled foregrip (Magpul AFG style)", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Grip, variant = "Hand_Stop", description = "Barricade hand stop", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Grip, variant = "Pistol_Stippled", description = "Stippled pistol grip texture", sortOrder = 3 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Grip, variant = "Pistol_Rubberized", description = "Rubberized pistol grip", sortOrder = 4 },
        };
        
        [Header("Sights")]
        public List<AccessoryEntry> sights = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "RMR_Type2", description = "Trijicon RMR Type 2 red dot housing", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "RMRcc", description = "Compact RMR for pistols", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "EOTech_EXPS3", description = "EOTech EXPS3 holographic sight housing", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "Aimpoint_T2", description = "Aimpoint Micro T-2 red dot housing", sortOrder = 3 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "Magnifier_G33", description = "G33-style flip-to-side magnifier housing", sortOrder = 4 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "BUIS_Front", description = "Backup front sight (folding)", sortOrder = 5 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Sight, variant = "BUIS_Rear", description = "Backup rear sight (folding)", sortOrder = 6 },
        };
        
        [Header("Muzzle Devices")]
        public List<AccessoryEntry> muzzleDevices = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Suppressor, variant = "Pistol_Short", description = "Short pistol suppressor outer tube", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.Suppressor, variant = "Rifle_Full", description = "Full-size rifle suppressor outer tube", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.MuzzleBrake, variant = "Compensator_3Port", description = "3-port compensator", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.MuzzleBrake, variant = "Compensator_5Port", description = "5-port compensator (TTI style)", sortOrder = 3 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.MuzzleBrake, variant = "Flash_Hider_A2", description = "A2 birdcage flash hider", sortOrder = 4 },
        };
        
        [Header("Rail Covers")]
        public List<AccessoryEntry> railCovers = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.RailCover, variant = "Picatinny_2Slot", description = "2-slot Picatinny rail cover", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.RailCover, variant = "Picatinny_4Slot", description = "4-slot Picatinny rail cover", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.RailCover, variant = "MLOK_3Slot", description = "3-slot M-LOK rail cover", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.RailCover, variant = "MLOK_5Slot", description = "5-slot M-LOK rail cover", sortOrder = 3 },
        };
        
        [Header("Sling Mounts")]
        public List<AccessoryEntry> slingMounts = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.SlingMount, variant = "QD_Socket", description = "Quick-detach sling socket", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.SlingMount, variant = "Loop_Standard", description = "Standard sling loop", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.SlingMount, variant = "ASH_Plate", description = "ASH receiver plate mount", sortOrder = 2 },
        };
        
        [Header("Magwell & Charging Handles")]
        public List<AccessoryEntry> magwellCharging = new List<AccessoryEntry>
        {
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.MagwellFlare, variant = "Flared_Standard", description = "Standard flared magwell", sortOrder = 0 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.MagwellFlare, variant = "Flared_Extended", description = "Extended flared magwell", sortOrder = 1 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.ChargingHandle, variant = "Extended_Latch", description = "Extended charging handle latch", sortOrder = 2 },
            new AccessoryEntry { type = JWA_AccessoryBase.AccessoryType.ChargingHandle, variant = "Ambidextrous", description = "Ambidextrous charging handle", sortOrder = 3 },
        };
        
        /// <summary>
        /// Get all accessories of a specific type
        /// </summary>
        public List<AccessoryEntry> GetAccessories(JWA_AccessoryBase.AccessoryType type)
        {
            switch (type)
            {
                case JWA_AccessoryBase.AccessoryType.Flashlight: return flashlights;
                case JWA_AccessoryBase.AccessoryType.LaserModule: return laserModules;
                case JWA_AccessoryBase.AccessoryType.Grip: return grips;
                case JWA_AccessoryBase.AccessoryType.Sight: return sights;
                case JWA_AccessoryBase.AccessoryType.Suppressor: return muzzleDevices.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.Suppressor);
                case JWA_AccessoryBase.AccessoryType.MuzzleBrake: return muzzleDevices.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.MuzzleBrake);
                case JWA_AccessoryBase.AccessoryType.RailCover: return railCovers;
                case JWA_AccessoryBase.AccessoryType.SlingMount: return slingMounts;
                case JWA_AccessoryBase.AccessoryType.MagwellFlare: return magwellCharging.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.MagwellFlare);
                case JWA_AccessoryBase.AccessoryType.ChargingHandle: return magwellCharging.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.ChargingHandle);
                default: return new List<AccessoryEntry>();
            }
        }
        
        /// <summary>
        /// Get all accessories across all categories
        /// </summary>
        public List<AccessoryEntry> GetAllAccessories()
        {
            var all = new List<AccessoryEntry>();
            all.AddRange(flashlights);
            all.AddRange(laserModules);
            all.AddRange(grips);
            all.AddRange(sights);
            all.AddRange(muzzleDevices);
            all.AddRange(railCovers);
            all.AddRange(slingMounts);
            all.AddRange(magwellCharging);
            return all;
        }
    }
}
