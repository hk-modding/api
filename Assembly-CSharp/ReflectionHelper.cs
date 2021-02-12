using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace Modding
{
    /// <summary>
    ///     A class to aid in reflection while caching it.
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        private static readonly Dictionary<FieldInfo, Delegate> Getters = new Dictionary<FieldInfo, Delegate>();

        private static readonly Dictionary<FieldInfo, Delegate> Setters = new Dictionary<FieldInfo, Delegate>();

        private static bool _preloaded;

        /// <summary>
        ///     Caches all fields on a type to frontload cost of reflection
        /// </summary>
        /// <typeparam name="T">The type to cache</typeparam>
        public static void CacheFields<T>()
        {
            Type t = typeof(T);
            if (!Fields.TryGetValue(t, out Dictionary<string, FieldInfo> tFields))
            {
                tFields = new Dictionary<string, FieldInfo>();
            }

            // Not gonna redesign this class to avoid reflection, this shouldn't be called during gameplay anyway
            MethodInfo getGetter =
                typeof(ReflectionHelper).GetMethod(nameof(GetGetter), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo getSetter =
                typeof(ReflectionHelper).GetMethod(nameof(GetSetter), BindingFlags.NonPublic | BindingFlags.Static);

            foreach (FieldInfo field in t.GetFields(BindingFlags.NonPublic | BindingFlags.Public |
                                                    BindingFlags.Instance | BindingFlags.Static))
            {
                tFields[field.Name] = field;

                if (!field.IsLiteral)
                {
                    getGetter?.MakeGenericMethod(t, field.FieldType).Invoke(null, new object[] {field});
                }

                if (!field.IsLiteral && !field.IsInitOnly)
                {
                    getSetter?.MakeGenericMethod(t, field.FieldType).Invoke(null, new object[] { field });
                }
            }
        }

        /// <summary>
        ///     Gets a field on a type
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

        internal static void PreloadCommonTypes()
        {
            if (_preloaded)
            {
                return;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            CacheFields<PlayerData>();
            CacheFields<HeroController>();
            CacheFields<GameManager>();
            CacheFields<UIManager>();

            watch.Stop();

            Logger.APILogger.Log($"Preloaded reflection in {watch.ElapsedMilliseconds}ms");

            _preloaded = true;
        }

        /// <summary>
        ///     Gets delegate getting field on type
        /// </summary>
        /// <param name="fi">FieldInfo for field.</param>
        /// <returns>Function which gets value of field</returns>
        private static Delegate GetGetter<TType, TField>(FieldInfo fi)
        {
            if (Getters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral)
            {
                throw new ArgumentException("Field cannot be const", nameof(fi));
            }

            d = fi.IsStatic
                ? CreateGetStaticFieldDelegate<TType, TField>(fi)
                : CreateGetFieldDelegate<TType, TField>(fi);

            Getters.Add(fi, d);

            return d;
        }

        /// <summary>
        ///     Gets delegate setting field on type
        /// </summary>
        /// <param name="fi">FieldInfo for field.</param>
        /// <returns>Function which sets field passed as FieldInfo</returns>
        private static Delegate GetSetter<TType, TField>(FieldInfo fi)
        {
            if (Setters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral || fi.IsInitOnly)
            {
                throw new ArgumentException("Field cannot be readonly or const", nameof(fi));
            }

            d = fi.IsStatic
                ? CreateSetStaticFieldDelegate<TType, TField>(fi)
                : CreateSetFieldDelegate<TType, TField>(fi);

            Setters.Add(fi, d);

            return d;
        }

        /// <summary>
        ///     Create delegate returning value of static field.
        /// </summary>
        /// <param name="fi">FieldInfo of field</param>
        /// <typeparam name="TField">Field type</typeparam>
        /// <typeparam name="TType">Type which field resides upon</typeparam>
        /// <returns>Function returning static field</returns>
        private static Delegate CreateGetStaticFieldDelegate<TType, TField>(FieldInfo fi)
        {
            DynamicMethod dm = new DynamicMethod
            (
                "FieldAccess" + fi.DeclaringType?.Name + fi.Name,
                typeof(TField),
                new Type[0],
                typeof(TType)
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldsfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.CreateDelegate(typeof(Func<TField>));
        }

        /// <summary>
        ///     Create delegate returning value of field of object
        /// </summary>
        /// <param name="fi"></param>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <returns>Function which returns value of field of object parameter</returns>
        private static Delegate CreateGetFieldDelegate<TType, TField>(FieldInfo fi)
        {
            DynamicMethod dm = new DynamicMethod
            (
                "FieldAccess" + fi.DeclaringType?.Name + fi.Name,
                typeof(TField),
                new[] {typeof(TType)},
                typeof(TType)
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.CreateDelegate(typeof(Func<TType, TField>));
        }

        private static Delegate CreateSetFieldDelegate<TType, TField>(FieldInfo fi)
        {
            DynamicMethod dm = new DynamicMethod
            (
                "FieldSet" + fi.DeclaringType?.Name + fi.Name,
                typeof(void),
                new[] {typeof(TType), typeof(TField)},
                typeof(TType)
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.CreateDelegate(typeof(Action<TType, TField>));
        }

        private static Delegate CreateSetStaticFieldDelegate<TType, TField>(FieldInfo fi)
        {
            DynamicMethod dm = new DynamicMethod
            (
                "FieldSet" + fi.DeclaringType?.Name + fi.Name,
                typeof(void),
                new[] {typeof(TField)},
                typeof(TType)
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stsfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.CreateDelegate(typeof(Action<TField>));
        }

        /// <summary>
        ///     Get a field on an object using a string. Cast to TCast before returning and if field doesn't exist return default.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="default">Default return</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        /// <typeparam name="TCast">Type of return.</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        [PublicAPI]
        public static TCast GetAttr<TObject, TField, TCast>(TObject obj, string name, TCast @default = default(TCast))
        {
            FieldInfo fi = GetField(typeof(TObject), name);

            return fi == null
                ? @default
                : (TCast) (object) ((Func<TObject, TField>) GetGetter<TObject, TField>(fi))(obj);
        }

        /// <summary>
        ///     Get a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        [PublicAPI]
        public static TField GetAttr<TObject, TField>(TObject obj, string name)
        {
            FieldInfo fi = GetField(typeof(TObject), name) ?? throw new MissingFieldException();

            return ((Func<TObject, TField>) GetGetter<TObject, TField>(fi))(obj);
        }

        /// <summary>
        ///     Get a static field on an type using a string.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="TType">Type which static field resides upon</typeparam>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        [PublicAPI]
        public static TField GetAttr<TType, TField>(string name)
        {
            FieldInfo fi = GetField(typeof(TType), name, false);

            return fi == null ? default(TField) : ((Func<TField>) GetGetter<TType, TField>(fi))();
        }

        /// <summary>
        ///     Set a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        [PublicAPI]
        public static void SetAttrSafe<TObject, TField>(TObject obj, string name, TField value)
        {
            FieldInfo fi = GetField(typeof(TObject), name);

            if (fi == null)
            {
                return;
            }

            ((Action<TObject, TField>) GetSetter<TObject, TField>(fi))(obj, value);
        }

        /// <summary>
        ///     Set a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        [PublicAPI]
        public static void SetAttr<TObject, TField>(TObject obj, string name, TField value)
        {
            FieldInfo fi = GetField(typeof(TObject), name) ??
                           throw new MissingFieldException($"Field {name} does not exist!");

            ((Action<TObject, TField>) GetSetter<TObject, TField>(fi))(obj, value);
        }

        /// <summary>
        ///     Set a static field on an type using a string.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TType">Type which static field resides upon</typeparam>
        /// <typeparam name="TField">Type of field</typeparam>
        [PublicAPI]
        public static void SetAttr<TType, TField>(string name, TField value)
        {
            ((Action<TField>) GetGetter<TType, TField>(GetField(typeof(TType), name, false)))(value);
        }

        #region Obsolete

        /// <summary>
        ///     Set a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="val">Value to set the field to to</param>
        /// <param name="instance">Whether or not to get an instance field or a static field</param>
        /// <typeparam name="T">Type of the object which the field holds.</typeparam>
        [PublicAPI]
        [Obsolete("Use SetAttr<TType, TField> and SetAttr<TObject, TField>.")]
        public static void SetAttr<T>(object obj, string name, T val, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            GetField(obj.GetType(), name, instance)?.SetValue(obj, val);
        }

        /// <summary>
        ///     Get a field on an object/type using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="instance">Whether or not to get an instance field or a static field</param>
        /// <typeparam name="T">Type of the object which the field holds.</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        [PublicAPI]
        [Obsolete("Use GetAttr<TObject, TField>.")]
        public static T GetAttr<T>(object obj, string name, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name))
            {
                return default(T);
            }

            return (T) GetField(obj.GetType(), name, instance)?.GetValue(obj);
        }

        #endregion
    }
}