using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modding
{
    public static class ReflectionHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

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

            Type t = obj.GetType();

            if (!Fields.ContainsKey(t))
            {
                Fields.Add(t, new Dictionary<string, FieldInfo>());
            }

            Dictionary<string, FieldInfo> typeFields = Fields[t];

            if (!typeFields.ContainsKey(name))
            {
                typeFields.Add(name,
                               t.GetField(name,
                                          BindingFlags.NonPublic | BindingFlags.Public |
                                          (instance ? BindingFlags.Instance : BindingFlags.Static)));
            }

            return (T) typeFields[name]?.GetValue(obj);
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

            Type t = obj.GetType();

            if (!Fields.ContainsKey(t))
            {
                Fields.Add(t, new Dictionary<string, FieldInfo>());
            }

            Dictionary<string, FieldInfo> typeFields = Fields[t];

            if (!typeFields.ContainsKey(name))
            {
                typeFields.Add(name,
                               t.GetField(name,
                                          BindingFlags.NonPublic | BindingFlags.Public |
                                          (instance ? BindingFlags.Instance : BindingFlags.Static)));
            }

            typeFields[name]?.SetValue(obj, val);
        }
    }
}