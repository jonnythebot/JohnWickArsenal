using System;
using System.Collections.Generic;
using UnityEngine;

namespace JohnWickArsenal.Utils
{
    /// <summary>
    /// Registry for custom MonoBehaviours to ensure they're properly registered with the mod system
    /// </summary>
    public static class ModBehaviorRegistry
    {
        private static readonly HashSet<Type> registeredBehaviors = new HashSet<Type>();

        public static void Register<T>() where T : MonoBehaviour
        {
            Type type = typeof(T);
            if (registeredBehaviors.Add(type))
            {
                Debug.Log($"[JWA] Registered behavior: {type.Name}");
            }
        }

        public static bool IsRegistered<T>() where T : MonoBehaviour
        {
            return registeredBehaviors.Contains(typeof(T));
        }

        public static IEnumerable<Type> GetRegisteredBehaviors()
        {
            return registeredBehaviors;
        }
    }
}