using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BladeAndSorcery.Data;
using BladeAndSorcery.Gameplay;

namespace JohnWickArsenal
{
    /// <summary>
    /// Main mod entry point for John Wick Arsenal.
    /// Registers all weapons, items, and custom behaviors with the game.
    /// </summary>
    public class JohnWickArsenal : Mod
    {
        public const string MOD_ID = "johnwickarsenal";
        public const string MOD_NAME = "John Wick Arsenal";
        public const string VERSION = "1.0.0";

        // Weapon IDs for registration
        public static class WeaponIDs
        {
            // Pistols
            public const string HK_P30L = "jwa_hk_p30l";
            public const string GLOCK_34 = "jwa_glock_34";
            public const string STI_2011_COMBAT_MASTER = "jwa_sti_2011_combat_master";
            public const string TTI_PIT_VIPER = "jwa_tti_pit_viper";

            // Shotguns
            public const string BENELLI_M4 = "jwa_benelli_m4";
            public const string KELTEC_KSG = "jwa_keltec_ksg";

            // Revolvers
            public const string REMINGTON_1875 = "jwa_remington_1875";

            // Rifles
            public const string TTI_TR1_ULTRALIGHT = "jwa_tti_tr1_ultralight";

            // Throwables
            public const string THROWING_KNIFE = "jwa_throwing_knife";
            public const string PENCIL = "jwa_pencil";
        }

        // AssetBundle names
        public static class BundleNames
        {
            public const string MAIN = "johnwickarsenal";
        }

        // Prefab paths within AssetBundle
        public static class PrefabPaths
        {
            // Pistols
            public const string HK_P30L = "Weapons/Pistols/HK_P30L";
            public const string GLOCK_34 = "Weapons/Pistols/Glock_34";
            public const string STI_2011_COMBAT_MASTER = "Weapons/Pistols/STI_2011_CombatMaster";
            public const string TTI_PIT_VIPER = "Weapons/Pistols/TTI_PitViper";

            // Shotguns
            public const string BENELLI_M4 = "Weapons/Shotguns/Benelli_M4";
            public const string KELTEC_KSG = "Weapons/Shotguns/KelTec_KSG";

            // Revolvers
            public const string REMINGTON_1875 = "Weapons/Revolvers/Remington_1875";

            // Rifles
            public const string TTI_TR1_ULTRALIGHT = "Weapons/Rifles/TTI_TR1_Ultralight";

            // Throwables
            public const string THROWING_KNIFE = "Weapons/Throwables/ThrowingKnife";
            public const string PENCIL = "Weapons/Throwables/Pencil";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            Debug.Log($"[{MOD_NAME}] v{VERSION} loading...");

            RegisterWeapons();
            RegisterItems();
            RegisterCustomBehaviors();

            Debug.Log($"[{MOD_NAME}] Loaded successfully!");
        }

        public override void OnUnload()
        {
            Debug.Log($"[{MOD_NAME}] Unloading...");
            base.OnUnload();
        }

        private void RegisterWeapons()
        {
            var weaponRegistry = Catalog.GetData<WeaponData>(WeaponIDs.HK_P30L);
            if (weaponRegistry == null)
            {
                RegisterPistols();
                RegisterShotguns();
                RegisterRevolvers();
                RegisterRifles();
            }
        }

        private void RegisterPistols()
        {
            // H&K P30L - John's signature weapon from Chapter 1
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.HK_P30L,
                name = "H&K P30L",
                description = "Heckler & Koch P30L with compensator - John Wick's signature sidearm from Chapter 1",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.HK_P30L}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Handgun,
                caliber = "9x19mm",
                magazineCapacity = 15,
                fireRate = 800,
                muzzleVelocity = 380,
                damage = 35f,
                range = 50f,
                recoil = new Vector2(0.8f, 1.2f),
                spread = 0.5f,
                reloadTime = 2.5f,
                customData = new Dictionary<string, object>
                {
                    { "isCompensated", true },
                    { "compensatorReduction", 0.3f },
                    { "tacticalRail", true },
                    { "nightSights", true }
                }
            });

            // Glock 34 - Chapter 2, competition pistol
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.GLOCK_34,
                name = "Glock 34",
                description = "Glock 34 Gen 5 MOS - Competition/Long-slide variant used in Chapter 2",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.GLOCK_34}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Handgun,
                caliber = "9x19mm",
                magazineCapacity = 17,
                fireRate = 750,
                muzzleVelocity = 390,
                damage = 33f,
                range = 55f,
                recoil = new Vector2(0.7f, 1.0f),
                spread = 0.4f,
                reloadTime = 2.2f,
                customData = new Dictionary<string, object>
                {
                    { "mosCut", true },
                    { "extendedSlideRelease", true },
                    { "minusConnector", true },
                    { "extendedMagRelease", true }
                }
            });

            // STI 2011 Combat Master - Chapter 3/4, race gun
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.STI_2011_COMBAT_MASTER,
                name = "STI 2011 Combat Master",
                description = "STI 2011 Combat Master .40 S&W - High-end race gun with compensator",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.STI_2011_COMBAT_MASTER}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Handgun,
                caliber = ".40 S&W",
                magazineCapacity = 20,
                fireRate = 900,
                muzzleVelocity = 400,
                damage = 42f,
                range = 60f,
                recoil = new Vector2(0.6f, 0.9f),
                spread = 0.3f,
                reloadTime = 2.0f,
                customData = new Dictionary<string, object>
                {
                    { "compensated", true },
                    { "redDotReady", true },
                    { "matchGradeBarrel", true },
                    { "extendedMagWell", true },
                    { "triggerPull", 2.5f }
                }
            });

            // TTI Pit Viper - Chapter 4, custom 2011
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.TTI_PIT_VIPER,
                name = "TTI Pit Viper",
                description = "Taran Tactical Innovations Pit Viper - Custom 2011 in 9mm Major, John's Chapter 4 primary",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.TTI_PIT_VIPER}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Handgun,
                caliber = "9mm Major",
                magazineCapacity = 21,
                fireRate = 950,
                muzzleVelocity = 420,
                damage = 45f,
                range = 65f,
                recoil = new Vector2(0.5f, 0.8f),
                spread = 0.25f,
                reloadTime = 1.8f,
                customData = new Dictionary<string, object>
                {
                    { "compensated", true },
                    { "redDotMounted", true },
                    { "goldAccents", true },
                    { "customGrip", true },
                    { "matchTrigger", true },
                    { "majorPowerFactor", true }
                }
            });
        }

        private void RegisterShotguns()
        {
            // Benelli M4 - Chapter 1, tactical shotgun
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.BENELLI_M4,
                name = "Benelli M4 Super 90",
                description = "Benelli M4 Super 90 - Semi-auto tactical shotgun, used in the Red Circle club scene",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.BENELLI_M4}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Shotgun,
                caliber = "12 Gauge",
                magazineCapacity = 7,
                fireRate = 200,
                muzzleVelocity = 380,
                damage = 120f,
                range = 35f,
                recoil = new Vector2(2.5f, 3.5f),
                spread = 3.0f,
                pelletCount = 9,
                reloadTime = 0.5f, // Per shell
                reloadType = WeaponData.ReloadType.Manual,
                customData = new Dictionary<string, object>
                {
                    { "ghostRingSights", true },
                    { "pistolGrip", false },
                    { "collapsibleStock", true },
                    { "shellCarrier", 6 },
                    { "autoRegulatingGasSystem", true }
                }
            });

            // Kel-Tec KSG - Chapter 3, bullpup shotgun
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.KELTEC_KSG,
                name = "Kel-Tec KSG",
                description = "Kel-Tec KSG - Dual-tube bullpup 12 gauge, high capacity tactical shotgun",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.KELTEC_KSG}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Shotgun,
                caliber = "12 Gauge",
                magazineCapacity = 14, // 7+7 dual tube
                fireRate = 180,
                muzzleVelocity = 390,
                damage = 115f,
                range = 40f,
                recoil = new Vector2(2.2f, 3.0f),
                spread = 2.8f,
                pelletCount = 9,
                reloadTime = 0.45f,
                reloadType = WeaponData.ReloadType.Manual,
                customData = new Dictionary<string, object>
                {
                    { "dualTube", true },
                    { "tubeSelector", true },
                    { "bullpup", true },
                    { "picatinnyTop", true },
                    { "forwardGrip", true }
                }
            });
        }

        private void RegisterRevolvers()
        {
            // Remington 1875 - Chapter 2, antique revolver
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.REMINGTON_1875,
                name = "Remington 1875",
                description = "Remington Model 1875 Single Action Army - Antique .45 Colt revolver from the Continental's armory",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.REMINGTON_1875}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Revolver,
                caliber = ".45 Colt",
                magazineCapacity = 6,
                fireRate = 120,
                muzzleVelocity = 250,
                damage = 55f,
                range = 45f,
                recoil = new Vector2(3.0f, 4.0f),
                spread = 1.0f,
                reloadTime = 8.0f, // Full cylinder reload
                reloadType = WeaponData.ReloadType.BreakAction,
                customData = new Dictionary<string, object>
                {
                    { "singleAction", true },
                    { "loadingGate", true },
                    { "ejectorRod", true },
                    { "antiqueFinish", true },
                    { "caseHardenedFrame", true }
                }
            });
        }

        private void RegisterRifles()
        {
            // TTI TR-1 Ultralight - Chapter 4, custom AR
            RegisterWeapon(new WeaponData
            {
                id = WeaponIDs.TTI_TR1_ULTRALIGHT,
                name = "TTI TR-1 Ultralight",
                description = "Taran Tactical Innovations TR-1 Ultralight - Custom AR-15 in 5.56mm, skeletonized build",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.TTI_TR1_ULTRALIGHT}",
                weaponType = WeaponData.Type.Gun,
                gunType = WeaponData.GunType.Rifle,
                caliber = "5.56x45mm",
                magazineCapacity = 30,
                fireRate = 850,
                muzzleVelocity = 920,
                damage = 48f,
                range = 300f,
                recoil = new Vector2(0.9f, 1.4f),
                spread = 0.4f,
                reloadTime = 2.0f,
                customData = new Dictionary<string, object>
                {
                    { "skeletonizedReceiver", true },
                    { "carbonFiberHandguard", true },
                    { "redDotMounted", true },
                    { "compensator", true },
                    { "binaryTrigger", false },
                    { "lightweightBCG", true },
                    { "adjustableGasBlock", true }
                }
            });
        }

        private void RegisterItems()
        {
            // Throwing Knives - Chapter 1/3, various scenes
            RegisterItem(new ItemData
            {
                id = WeaponIDs.THROWING_KNIFE,
                name = "Throwing Knife",
                description = "Balanced throwing knife - Silent, lethal, and always retrievable",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.THROWING_KNIFE}",
                itemType = ItemData.Type.Throwable,
                throwableType = ItemData.ThrowableType.Knife,
                damage = 65f,
                throwForce = 18f,
                spinRate = 720f,
                stickChance = 0.95f,
                retrievable = true,
                maxStack = 5,
                customData = new Dictionary<string, object>
                {
                    { "balanced", true },
                    { "doubleEdged", true },
                    { "paracordWrap", true },
                    { "silentKill", true },
                    { "headshotMultiplier", 3.0f }
                }
            });

            // The Pencil - Chapter 2/3, legendary improvised weapon
            RegisterItem(new ItemData
            {
                id = WeaponIDs.PENCIL,
                name = "Pencil",
                description = "\"People keep asking if I'm back. Yeah, I'm thinking I'm back.\" - A standard HB graphite pencil. In the right hands, deadlier than a gun.",
                prefabAddress = $"{BundleNames.MAIN}:{PrefabPaths.PENCIL}",
                itemType = ItemData.Type.Throwable,
                throwableType = ItemData.ThrowableType.Improvised,
                damage = 999f, // One-hit kill to head/neck
                throwForce = 25f,
                spinRate = 1080f,
                stickChance = 0.85f,
                retrievable = true,
                maxStack = 3,
                customData = new Dictionary<string, object>
                {
                    { "isPencil", true },
                    { "instantKillHeadshot", true },
                    { "instantKillNeck", true },
                    { "penetrationDepth", 0.15f },
                    { "johnWickEasterEgg", true },
                    { "durability", 3 }, // Breaks after 3 kills
                    { "canWrite", false }
                }
            });
        }

        private void RegisterCustomBehaviors()
        {
            // Register custom MonoBehaviours for special weapon mechanics
            ModBehaviorRegistry.Register<JohnWickPistolBehavior>();
            ModBehaviorRegistry.Register<JohnWickShotgunBehavior>();
            ModBehaviorRegistry.Register<JohnWickRevolverBehavior>();
            ModBehaviorRegistry.Register<JohnWickRifleBehavior>();
            ModBehaviorRegistry.Register<ThrowingKnifeBehavior>();
            ModBehaviorRegistry.Register<PencilBehavior>();
        }

        private void RegisterWeapon(WeaponData data)
        {
            try
            {
                Catalog.RegisterData(data);
                Debug.Log($"[{MOD_NAME}] Registered weapon: {data.name} ({data.id})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{MOD_NAME}] Failed to register weapon {data.id}: {e.Message}");
            }
        }

        private void RegisterItem(ItemData data)
        {
            try
            {
                Catalog.RegisterData(data);
                Debug.Log($"[{MOD_NAME}] Registered item: {data.name} ({data.id})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{MOD_NAME}] Failed to register item {data.id}: {e.Message}");
            }
        }
    }
}