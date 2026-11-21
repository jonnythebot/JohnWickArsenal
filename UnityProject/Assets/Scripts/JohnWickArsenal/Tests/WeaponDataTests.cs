using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using JohnWickArsenal;
using JohnWickArsenal.Data;

namespace JohnWickArsenal.Tests
{
    /// <summary>
    /// Unit tests for weapon data validation and configuration loading
    /// Run with Unity Test Runner (EditMode)
    /// </summary>
    public class WeaponDataTests
    {
        [Test]
        public void ModConstants_HaveExpectedValues()
        {
            Assert.AreEqual("johnwickarsenal", JohnWickArsenal.MOD_ID);
            Assert.AreEqual("John Wick Arsenal", JohnWickArsenal.MOD_NAME);
            Assert.AreEqual("1.0.0", JohnWickArsenal.VERSION);
        }

        [Test]
        public void WeaponIDs_AllDefined()
        {
            // Pistols
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.HK_P30L));
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.GLOCK_34));
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.STI_2011_COMBAT_MASTER));
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.TTI_PIT_VIPER));

            // Shotguns
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.BENELLI_M4));
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.KELTEC_KSG));

            // Revolvers
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.REMINGTON_1875));

            // Rifles
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.TTI_TR1_ULTRALIGHT));

            // Throwables
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.THROWING_KNIFE));
            Assert.IsFalse(string.IsNullOrEmpty(JohnWickArsenal.WeaponIDs.PENCIL));
        }

        [Test]
        public void WeaponIDs_HaveCorrectPrefix()
        {
            var fields = typeof(JohnWickArsenal.WeaponIDs).GetFields();
            foreach (var field in fields)
            {
                string value = (string)field.GetValue(null);
                Assert.IsTrue(value.StartsWith("jwa_"), $"Weapon ID {field.Name} should start with 'jwa_'");
            }
        }

        [Test]
        public void PrefabPaths_AllDefined()
        {
            var fields = typeof(JohnWickArsenal.PrefabPaths).GetFields();
            Assert.AreEqual(10, fields.Length); // 10 weapons

            foreach (var field in fields)
            {
                string value = (string)field.GetValue(null);
                Assert.IsTrue(value.StartsWith("Weapons/"), $"Prefab path {field.Name} should start with 'Weapons/'");
            }
        }

        [Test]
        public void BundleNames_Correct()
        {
            Assert.AreEqual("johnwickarsenal", JohnWickArsenal.BundleNames.MAIN);
        }

        [Test]
        public void ConfigLoader_CanLoadWeaponConfig()
        {
            // This test requires the config files to exist
            string configPath = Path.Combine(Application.dataPath, "..", "ModConfig", "weapons.json");

            if (File.Exists(configPath))
            {
                ConfigLoader.LoadAll();
                var config = ConfigLoader.WeaponConfig;

                Assert.IsNotNull(config);
                Assert.IsNotNull(config.weapons);
                Assert.AreEqual(8, config.weapons.Count); // 8 guns
                Assert.IsNotNull(config.throwables);
                Assert.AreEqual(2, config.throwables.Count); // 2 throwables
            }
            else
            {
                Assert.Inconclusive("Config file not found at " + configPath);
            }
        }

        [Test]
        public void WeaponConfig_AllWeaponsHaveRequiredFields()
        {
            string configPath = Path.Combine(Application.dataPath, "..", "ModConfig", "weapons.json");

            if (File.Exists(configPath))
            {
                ConfigLoader.LoadAll();
                var config = ConfigLoader.WeaponConfig;

                foreach (var weapon in config.weapons)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(weapon.id), "Weapon missing ID");
                    Assert.IsFalse(string.IsNullOrEmpty(weapon.name), "Weapon missing name");
                    Assert.IsFalse(string.IsNullOrEmpty(weapon.category), "Weapon missing category");
                    Assert.IsFalse(string.IsNullOrEmpty(weapon.caliber), "Weapon missing caliber");
                    Assert.Greater(weapon.capacity, 0, "Weapon capacity must be > 0");
                    Assert.IsFalse(string.IsNullOrEmpty(weapon.unityPrefab), "Weapon missing prefab path");
                    Assert.IsFalse(string.IsNullOrEmpty(weapon.behavior), "Weapon missing behavior");
                }

                foreach (var throwable in config.throwables)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(throwable.id), "Throwable missing ID");
                    Assert.IsFalse(string.IsNullOrEmpty(throwable.name), "Throwable missing name");
                    Assert.Greater(throwable.damage, 0, "Throwable damage must be > 0");
                    Assert.Greater(throwable.throwForce, 0, "Throwable throwForce must be > 0");
                    Assert.IsFalse(string.IsNullOrEmpty(throwable.unityPrefab), "Throwable missing prefab path");
                    Assert.IsFalse(string.IsNullOrEmpty(throwable.behavior), "Throwable missing behavior");
                }
            }
            else
            {
                Assert.Inconclusive("Config file not found");
            }
        }

        [Test]
        public void PencilBehavior_HasEasterEggProperties()
        {
            var behaviorType = typeof(JohnWickArsenal.Behaviors.PencilBehavior);
            var fields = behaviorType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Check for key fields
            Assert.IsTrue(HasField(fields, "instantKillHeadshot"));
            Assert.IsTrue(HasField(fields, "instantKillNeck"));
            Assert.IsTrue(HasField(fields, "maxKillsBeforeBreak"));
            Assert.IsTrue(HasField(fields, "johnWickQuote"));
        }

        [Test]
        public void ThrowingKnifeBehavior_HasRetrievableLogic()
        {
            var behaviorType = typeof(JohnWickArsenal.Behaviors.ThrowingKnifeBehavior);
            var methods = behaviorType.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            Assert.IsTrue(HasMethod(methods, "RetrieveFromTarget"));
            Assert.IsTrue(HasMethod(methods, "StickToTarget"));
        }

        [Test]
        public void ShotgunBehavior_HasDualTubeSupport()
        {
            var behaviorType = typeof(JohnWickArsenal.Behaviors.JohnWickShotgunBehavior);
            var fields = behaviorType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsTrue(HasField(fields, "hasDualTube"));
            Assert.IsTrue(HasField(fields, "currentTube"));
            Assert.IsTrue(HasField(fields, "tubeA"));
            Assert.IsTrue(HasField(fields, "tubeB"));
        }

        [Test]
        public void RifleBehavior_HasGasSystem()
        {
            var behaviorType = typeof(JohnWickArsenal.Behaviors.JohnWickRifleBehavior);
            var fields = behaviorType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var methods = behaviorType.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            Assert.IsTrue(HasField(fields, "currentGasSetting"));
            Assert.IsTrue(HasField(fields, "gasBlockDial"));
            Assert.IsTrue(HasMethod(methods, "CycleGasSetting"));
            Assert.IsTrue(HasMethod(methods, "ApplyGasSetting"));
        }

        [Test]
        public void RevolverBehavior_HasLoadingGateAnimation()
        {
            var behaviorType = typeof(JohnWickArsenal.Behaviors.JohnWickRevolverBehavior);
            var fields = behaviorType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsTrue(HasField(fields, "loadingGate"));
            Assert.IsTrue(HasField(fields, "ejectorRod"));
            Assert.IsTrue(HasField(fields, "hammer"));
            Assert.IsTrue(HasField(fields, "isLoadingGateOpen"));
        }

        [Test]
        public void PistolBehavior_HasCompensatorSupport()
        {
            var behaviorType = typeof(JohnWickArsenal.Behaviors.JohnWickPistolBehavior);
            var fields = behaviorType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var methods = behaviorType.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            Assert.IsTrue(HasField(fields, "hasCompensator"));
            Assert.IsTrue(HasField(fields, "compensatorRecoilReduction"));
            Assert.IsTrue(HasField(fields, "compensatorGasPorts"));
            Assert.IsTrue(HasMethod(methods, "GetRecoilMultiplier"));
            Assert.IsTrue(HasMethod(methods, "GetSpreadMultiplier"));
        }

        private bool HasField(System.Reflection.FieldInfo[] fields, string name)
        {
            foreach (var f in fields)
            {
                if (f.Name == name) return true;
            }
            return false;
        }

        private bool HasMethod(System.Reflection.MethodInfo[] methods, string name)
        {
            foreach (var m in methods)
            {
                if (m.Name == name) return true;
            }
            return false;
        }
    }
}