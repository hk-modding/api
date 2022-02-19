using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modding.Utils
{
    /// <summary>
    /// Class containing extensions used by the Modding API for interacting with Unity types.
    /// </summary>
    public static class UnityExtensions
    {
        /// <summary>
        /// Get the component of type T attached to GameObject go. If go does not have such a component, add
        /// that component (and return it).
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Find a game object by name in the scene. The object's name must be given in the hierarchy.
        /// </summary>
        /// <param name="scene">The scene to search.</param>
        /// <param name="objName">The name of the object in the hierarchy, with '/' separating parent GameObjects from child GameObjects.</param>
        /// <returns>The GameObject if found; null if not.</returns>
        /// <exception cref="ArgumentException">Thrown if the path to the game object is invalid.</exception>
        public static GameObject FindGameObject(this Scene scene, string objName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            return GetGameObjectFromArray(rootObjects, objName);
        }

        internal static GameObject GetGameObjectFromArray(GameObject[] objects, string objName)
        {
            // Split object name into root and child names based on '/'
            string rootName;
            string childName = null;

            int slashIndex = objName.IndexOf('/');
            if (slashIndex == -1)
            {
                rootName = objName;
            }
            else if (slashIndex == 0 || slashIndex == objName.Length - 1)
            {
                throw new ArgumentException("Invalid GameObject path");
            }
            else
            {
                rootName = objName.Substring(0, slashIndex);
                childName = objName.Substring(slashIndex + 1);
            }

            // Get root object
            GameObject obj = objects.FirstOrDefault(o => o.name == rootName);
            if (obj == null) return null;

            // Get child object
            if (childName != null)
            {
                Transform t = obj.transform.Find(childName);
                return t == null ? null : t.gameObject;
            }

            return obj;
        }
    }
}
