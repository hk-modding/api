using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modding
{
    /// <summary>
    /// A class to aid in reflection while caching it.
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        /// <summary>
        /// Gets a field on a type
        /// </summary>
        /// <param name="t">Type</param>
        /// <param name="field">Field name</param>
        /// <param name="instance"></param>
        /// <returns>FieldInfo for field or null if field does not exist.</returns>
        public static FieldInfo GetField(Type t, string field, bool instance = true)
        {
            if (!Fields.TryGetValue(t, out Dictionary<string, FieldInfo> typeFields))
            {
                Fields.Add(t, typeFields = new Dictionary<string, FieldInfo>());
            }

            if (typeFields.TryGetValue(field, out FieldInfo fi))
            {
                return fi;
            }

            fi = t.GetField(field,
                            BindingFlags.NonPublic | BindingFlags.Public |
                            (instance ? BindingFlags.Instance : BindingFlags.Static));

            if (fi != null)
            {
                typeFields.Add(field, fi);
            }

            return fi;
        }


        /// <summary>
        /// Get a field on an object/type using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="instance">Whether or not to get an instance field or a static field</param>
        /// <typeparam name="T">Type of the object which the field holds.</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        public static T GetAttr<T>(object obj, string name, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return default(T);

            return (T) GetField(obj.GetType(), name, instance)?.GetValue(obj);
        }

        /// <summary>
        /// Set a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="val">Value to set the field to to</param>
        /// <param name="instance">Whether or not to get an instance field or a static field</param>
        /// <typeparam name="T">Type of the object which the field holds.</typeparam>
        public static void SetAttr<T>(object obj, string name, T val, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return;

            GetField(obj.GetType(), name, instance)?.SetValue(obj, val);
        }
    }
}