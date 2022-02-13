using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MonoMod.Utils;

namespace Modding
{
    /// <summary>
    ///     A class to aid in reflection while caching it.
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, FieldInfo>> Fields = new();
        private static readonly ConcurrentDictionary<FieldInfo, Delegate> FieldGetters = new();
        private static readonly ConcurrentDictionary<FieldInfo, Delegate> FieldSetters = new();
        
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>> Properties = new();
        private static readonly ConcurrentDictionary<PropertyInfo, Delegate> PropertyGetters = new();
        private static readonly ConcurrentDictionary<PropertyInfo, Delegate> PropertySetters = new();
        
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>> Methods = new();
        private static readonly ConcurrentDictionary<MethodInfo, FastReflectionDelegate> MethodsDelegates = new();

        private static bool _preloaded;

        /// <summary>
        ///     Caches all fields on a type to frontload cost of reflection
        /// </summary>
        /// <typeparam name="T">The type to cache</typeparam>
        private static void CacheFields<T>()
        {
            Type t = typeof(T);

            if (!Fields.TryGetValue(t, out ConcurrentDictionary<string, FieldInfo> tFields))
            {
                tFields = new ConcurrentDictionary<string, FieldInfo>();
            }
            
            const BindingFlags privStatic = BindingFlags.NonPublic | BindingFlags.Static;
            const BindingFlags all = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            // Not gonna redesign this class to avoid reflection, this shouldn't be called during gameplay anyway

            MethodInfo getInstanceFieldGetter = typeof(ReflectionHelper).GetMethod(nameof(GetInstanceFieldGetter), privStatic);
            MethodInfo getStaticFieldGetter = typeof(ReflectionHelper).GetMethod(nameof(GetStaticFieldGetter), privStatic);
            MethodInfo getInstanceFieldSetter = typeof(ReflectionHelper).GetMethod(nameof(GetInstanceFieldSetter), privStatic);
            MethodInfo getStaticFieldSetter = typeof(ReflectionHelper).GetMethod(nameof(GetStaticFieldSetter), privStatic);
            
            Parallel.ForEach
            (
                t.GetFields(all),
                field =>
                {
                    tFields[field.Name] = field;

                    // Don't need to preload consts, immutable anyways.
                    if (field.IsLiteral)
                        return;
                    
                    object[] @params = { field };

                    // Getters
                    if (field.IsStatic)
                        getStaticFieldGetter?.MakeGenericMethod(field.FieldType).Invoke(null, @params);
                    else
                        getInstanceFieldGetter?.MakeGenericMethod(t, field.FieldType).Invoke(null, @params);

                    // Don't get a setter if it's readonly
                    if (field.IsInitOnly) 
                        return;

                    if (field.IsStatic)
                        getStaticFieldSetter?.MakeGenericMethod(field.FieldType).Invoke(null, @params);
                    else
                        getInstanceFieldSetter?.MakeGenericMethod(t, field.FieldType).Invoke(null, @params);
                }
            );
        }
        
        internal static void PreloadCommonTypes()
        {
            if (_preloaded)
                return;

            var watch = new Stopwatch();
            watch.Start();

            Parallel.Invoke
            (
                CacheFields<PlayerData>,
                CacheFields<HeroController>,
                CacheFields<HeroControllerStates>,
                CacheFields<GameManager>,
                CacheFields<UIManager>
            );

            watch.Stop();

            Logger.APILogger.Log($"Preloaded reflection in {watch.ElapsedMilliseconds}ms");

            _preloaded = true;
        }

        #region Fields

         /// <summary>
        ///     Gets a field on a type
        /// </summary>
        /// <param name="t">Type</param>
        /// <param name="field">Field name</param>
        /// <param name="instance"></param>
        /// <returns>FieldInfo for field or null if field does not exist.</returns>
        [PublicAPI]
        public static FieldInfo GetFieldInfo(Type t, string field, bool instance = true)
        {
            if (!Fields.TryGetValue(t, out ConcurrentDictionary<string, FieldInfo> typeFields))
            {
                Fields[t] = typeFields = new ConcurrentDictionary<string, FieldInfo>();
            }

            if (typeFields.TryGetValue(field, out FieldInfo fi))
            {
                return fi;
            }

            fi = t.GetField
            (
                field,
                BindingFlags.NonPublic | BindingFlags.Public | (instance ? BindingFlags.Instance : BindingFlags.Static)
            );

            if (fi != null)
            {
                typeFields.TryAdd(field, fi);
            }

            return fi;
        }

        /// <summary>
        ///     Gets delegate getting field on type
        /// </summary>
        /// <param name="fi">FieldInfo for field.</param>
        /// <returns>Function which gets value of field</returns>
        private static Delegate GetInstanceFieldGetter<TType, TField>(FieldInfo fi)
        {
            if (FieldGetters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral)
            {
                throw new ArgumentException("Field cannot be const", nameof(fi));
            }

            var dm = new DynamicMethodDefinition
            (
                "FieldAccess" + fi.DeclaringType?.Name + fi.Name,
                typeof(TField),
                new[] { typeof(TType) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, fi);
            gen.Emit(OpCodes.Ret);

            d = dm.Generate().CreateDelegate(typeof(Func<TType, TField>));

            FieldGetters[fi] = d;

            return d;
        }
        
        private static Delegate GetStaticFieldGetter<TField>(FieldInfo fi)
        {
            if (FieldGetters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral)
            {
                throw new ArgumentException("Field cannot be const", nameof(fi));
            }

            var dm = new DynamicMethodDefinition
            (
                "FieldAccess" + fi.DeclaringType?.Name + fi.Name,
                typeof(TField),
                Type.EmptyTypes
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldsfld, fi);
            gen.Emit(OpCodes.Ret);

            d = dm.Generate().CreateDelegate(typeof(Func<TField>));

            FieldGetters[fi] = d;

            return d;
        }

        /// <summary>
        ///     Gets delegate setting field on type
        /// </summary>
        /// <param name="fi">FieldInfo for field.</param>
        /// <returns>Function which sets field passed as FieldInfo</returns>
        private static Delegate GetInstanceFieldSetter<TType, TField>(FieldInfo fi)
        {
            if (FieldSetters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral || fi.IsInitOnly)
            {
                throw new ArgumentException("Field cannot be readonly or const", nameof(fi));
            }

            var dm = new DynamicMethodDefinition
            (
                "FieldSet" + fi.DeclaringType?.Name + fi.Name,
                typeof(void),
                new[] { typeof(TType), typeof(TField) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, fi);
            gen.Emit(OpCodes.Ret);

            d = dm.Generate().CreateDelegate(typeof(Action<TType, TField>));

            FieldSetters[fi] = d;

            return d;
        }
        
        private static Delegate GetStaticFieldSetter<TField>(FieldInfo fi)
        {
            if (FieldSetters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral || fi.IsInitOnly)
            {
                throw new ArgumentException("Field cannot be readonly or const", nameof(fi));
            }

            var dm = new DynamicMethodDefinition
            (
                "FieldSet" + fi.DeclaringType?.Name + fi.Name,
                typeof(void),
                new[] { typeof(TField) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stsfld, fi);
            gen.Emit(OpCodes.Ret);

            d =  dm.Generate().CreateDelegate(typeof(Action<TField>));

            FieldSetters[fi] = d;

            return d;
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
        public static TCast GetField<TObject, TField, TCast>(TObject obj, string name, TCast @default = default)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name);

            return fi == null
                ? @default
                : (TCast)(object)((Func<TObject, TField>)GetInstanceFieldGetter<TObject, TField>(fi))(obj);
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
        public static TField GetField<TObject, TField>(TObject obj, string name)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name) ?? throw new MissingFieldException($"Field {name} does not exist!");

            return ((Func<TObject, TField>) GetInstanceFieldGetter<TObject, TField>(fi))(obj);
        }

        /// <summary>
        ///     Get a static field on an type using a string.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="TType">Type which static field resides upon</typeparam>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        [PublicAPI]
        public static TField GetField<TType, TField>(string name)
        {
            FieldInfo fi = GetFieldInfo(typeof(TType), name, false) ?? throw new MissingFieldException($"Field {name} does not exist!");

            return ((Func<TField>)GetStaticFieldGetter<TField>(fi))();
        }

        /// <summary>
        ///     Get a static field on an type using a string. (for static classes)
        /// </summary>
        /// <param name="type">Static Type which static field resides upon</param>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        [PublicAPI]
        public static TField GetField<TField>(Type type, string name)
        {
            FieldInfo fi = GetFieldInfo(type, name, false) ?? throw new MissingFieldException($"Field {name} does not exist!");

            return ((Func<TField>)GetStaticFieldGetter<TField>(fi))();
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
        public static void SetFieldSafe<TObject, TField>(TObject obj, string name, TField value)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name);

            if (fi == null)
            {
                return;
            }

            ((Action<TObject, TField>)GetInstanceFieldSetter<TObject, TField>(fi))(obj, value);
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
        public static void SetField<TObject, TField>(TObject obj, string name, TField value)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name) ?? throw new MissingFieldException($"Field {name} does not exist!");
            ((Action<TObject, TField>)GetInstanceFieldSetter<TObject, TField>(fi))(obj, value);
        }

        /// <summary>
        ///     Set a static field on an type using a string.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TType">Type which static field resides upon</typeparam>
        /// <typeparam name="TField">Type of field</typeparam>
        [PublicAPI]
        public static void SetField<TType, TField>(string name, TField value)
        {
            FieldInfo fi = GetFieldInfo(typeof(TType), name, false) ?? throw new MissingFieldException($"Field {name} does not exist!");
            ((Action<TField>)GetInstanceFieldSetter<TType, TField>(fi))(value);
        }

        /// <summary>
        ///     Set a static field on an type using a string. (for static classes)
        /// </summary>
        /// <param name="type">Static Type which static field resides upon</param>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TField">Type of field</typeparam>
        [PublicAPI]
        public static void SetField<TField>(Type type, string name, TField value)
        {
            FieldInfo fi = GetFieldInfo(type, name, false) ?? throw new MissingFieldException($"Field {name} does not exist!");
            ((Action<TField>)GetStaticFieldSetter<TField>(fi))(value);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a property on a type
        /// </summary>
        /// <param name="t">Type</param>
        /// <param name="property">Property name</param>
        /// <param name="instance"></param>
        /// <returns>PropertyInfo for property or null if property does not exist.</returns>
        [PublicAPI]
        public static PropertyInfo GetPropertyInfo(Type t, string property, bool instance = true)
        {
            if (!Properties.TryGetValue(t, out ConcurrentDictionary<string, PropertyInfo> typeProperties))
            {
                Properties[t] = typeProperties = new ConcurrentDictionary<string, PropertyInfo>();
            }

            if (typeProperties.TryGetValue(property, out PropertyInfo pi))
            {
                return pi;
            }

            pi = t.GetProperty
            (
                property,
                BindingFlags.NonPublic | BindingFlags.Public | (instance ? BindingFlags.Instance : BindingFlags.Static)
            );

            if (pi != null)
            {
                typeProperties.TryAdd(property, pi);
            }

            return pi;
        }

        /// <summary>
        ///     Get a property on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the property is on</param>
        /// <param name="name">Name of the property</param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        /// <returns>The value of a property on an object/type</returns>
        [PublicAPI]
        public static TProperty GetProperty<TObject, TProperty>(TObject obj, string name)
        {
            PropertyInfo pi = GetPropertyInfo(typeof(TObject), name) ?? throw new MissingFieldException($"Property {name} does not exist!");

            return ((Func<TObject, TProperty>)GetInstancePropertyGetter<TObject, TProperty>(pi))(obj);
        }

        /// <summary>
        ///     Get a static property on an type using a string.
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <typeparam name="TType">Type which static property resides upon</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>The value of a property on an object/type</returns>
        [PublicAPI]
        public static TProperty GetProperty<TType, TProperty>(string name)
        {
            PropertyInfo pi = GetPropertyInfo(typeof(TType), name, false);

            return pi == null ? default : ((Func<TProperty>)GetStaticPropertyGetter<TProperty>(pi))();
        }

        /// <summary>
        ///     Get a static property on an type using a string. (for static classes)
        /// </summary>
        /// <param name="type">Static Type which static property resides upon</param>
        /// <param name="name">Name of the property</param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>The value of a property on an object/type</returns>
        [PublicAPI]
        public static TProperty GetProperty<TProperty>(Type type, string name)
        {
            PropertyInfo pi = GetPropertyInfo(type, name, false);

            return pi == null ? default : ((Func<TProperty>)GetStaticPropertyGetter<TProperty>(pi))();
        }

        /// <summary>
        ///     Set a property on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the property is on</param>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value to set the property to</param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        [PublicAPI]
        public static void SetProperty<TObject, TProperty>(TObject obj, string name, TProperty value)
        {
            PropertyInfo pi = GetPropertyInfo(typeof(TObject), name) ?? throw new MissingFieldException($"Property {name} does not exist!");
            ((Action<TObject, TProperty>)GetInstancePropertySetter<TObject, TProperty>(pi))(obj, value);
        }

        /// <summary>
        ///     Set a static property on an type using a string.
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value to set the property to</param>
        /// <typeparam name="TType">Type which static property resides upon</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        [PublicAPI]
        public static void SetProperty<TType, TProperty>(string name, TProperty value)
        {
            PropertyInfo pi = GetPropertyInfo(typeof(TType), name, false) ?? throw new MissingFieldException($"Property {name} does not exist!");
            ((Action<TProperty>)GetStaticPropertySetter<TProperty>(pi))(value);
        }

        /// <summary>
        ///     Set a static property on an type using a string. (for static classes)
        /// </summary>
        /// <param name="type">Static Type which static property resides upon</param>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value to set the property to</param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        [PublicAPI]
        public static void SetProperty<TProperty>(Type type, string name, TProperty value)
        {
            PropertyInfo pi = GetPropertyInfo(type, name, false) ?? throw new MissingFieldException($"Property {name} does not exist!");
            ((Action<TProperty>)GetStaticPropertySetter<TProperty>(pi))(value);
        }


        private static Delegate GetInstancePropertyGetter<TType, TProperty>(PropertyInfo pi)
        {
            if (PropertyGetters.TryGetValue(pi, out Delegate d))
            {
                return d;
            }

            if (!pi.CanRead)
            {
                throw new ArgumentException($"Property doesn't have Get method", nameof(pi));
            }
            
            var dm = new DynamicMethodDefinition
            (
                "PropertyAccess" + pi.DeclaringType?.Name + pi.Name,
                typeof(TProperty),
                new[] { typeof(TType) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, pi.GetMethod);
            gen.Emit(OpCodes.Ret);

            d =  dm.Generate().CreateDelegate(typeof(Func<TType, TProperty>));

            PropertyGetters[pi] = d;

            return d;
        }
        
        private static Delegate GetStaticPropertyGetter<TProperty>(PropertyInfo pi)
        {
            if (PropertyGetters.TryGetValue(pi, out Delegate d))
            {
                return d;
            }

            if (!pi.CanRead)
            {
                throw new ArgumentException($"Property doesn't have Get method", nameof(pi));
            }

            var dm = new DynamicMethodDefinition
            (
                "PropertyAccess" + pi.DeclaringType?.Name + pi.Name,
                typeof(TProperty),
                Type.EmptyTypes
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Call, pi.GetMethod);
            gen.Emit(OpCodes.Ret);

            d =  dm.Generate().CreateDelegate(typeof(Func<TProperty>));

            PropertyGetters[pi] = d;

            return d;
        }
        
        private static Delegate GetInstancePropertySetter<TType, TProperty>(PropertyInfo pi)
        {
            if (PropertySetters.TryGetValue(pi, out Delegate d))
            {
                return d;
            }

            if (!pi.CanWrite)
            {
                throw new ArgumentException("Property doesn't have a Set method", nameof(pi));
            }

            var dm = new DynamicMethodDefinition
            (
                "PropertySet" + pi.DeclaringType?.Name + pi.Name,
                typeof(void),
                new[] { typeof(TType), typeof(TProperty) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, pi.SetMethod);
            gen.Emit(OpCodes.Ret);

            d =  dm.Generate().CreateDelegate(typeof(Action<TType, TProperty>));

            PropertySetters[pi] = d;

            return d;
        }
        
        private static Delegate GetStaticPropertySetter<TProperty>(PropertyInfo pi)
        {
            if (PropertySetters.TryGetValue(pi, out Delegate d))
            {
                return d;
            }

            if (!pi.CanWrite)
            {
                throw new ArgumentException("Property doesn't have a Set method", nameof(pi));
            }

            var dm = new DynamicMethodDefinition
            (
                "PropertySet" + pi.DeclaringType?.Name + pi.Name,
                typeof(void),
                new[] { typeof(TProperty) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, pi.SetMethod);
            gen.Emit(OpCodes.Ret);

            d = dm.Generate().CreateDelegate(typeof(Action<TProperty>));

            PropertySetters[pi] = d;

            return d;
        }

        #endregion

        #region Methods
        /// <summary>
        ///     Gets a method on a type 
        /// </summary>
        /// <param name="t">Type</param>
        /// <param name="method">Method name</param>
        /// <param name="instance"></param>
        /// <returns>MethodInfo for method or null if method does not exist.</returns>
        [PublicAPI]
        public static MethodInfo GetMethodInfo(Type t, string method, bool instance = true)
        {
            if (!Methods.TryGetValue(t, out ConcurrentDictionary<string, MethodInfo> typeMethods))
            {
                Methods[t] = typeMethods = new ConcurrentDictionary<string, MethodInfo>();
            }

            if (typeMethods.TryGetValue(method, out MethodInfo mi))
            {
                return mi;
            }

            mi = t.GetMethod
            (
                method,
                BindingFlags.NonPublic | BindingFlags.Public | (instance ? BindingFlags.Instance : BindingFlags.Static) 
            );

            if (mi != null)
            {
                typeMethods.TryAdd(method, mi);
            }

            return mi;
        }
        
        private static FastReflectionDelegate GetFastReflectionDelegate(MethodInfo mi)
        {
            if (MethodsDelegates.TryGetValue(mi, out FastReflectionDelegate d))
            {
                return d;
            }
            d = mi.GetFastDelegate();

            MethodsDelegates[mi] = d;

            return d;
        }
        
        /// <summary>
        ///     Call an instance method with a return type
        /// </summary>
        /// <param name="obj">Object of type which the method is on</param>
        /// <param name="name">Name of the method</param>
        /// <param name="param">The paramters that need to be passed into the method.</param>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        /// <typeparam name="TReturn">The return type of the method</typeparam>
        /// <returns>The specified return type</returns>
        [PublicAPI]
        public static TReturn CallMethod<TObject, TReturn>(TObject obj, string name, params object[] param)
        {
            MethodInfo mi = GetMethodInfo(typeof(TObject), name) ?? throw new MissingFieldException($"Method {name} does not exist!");
            return (TReturn) GetFastReflectionDelegate(mi).Invoke(obj, param.Length == 0 ? null : param);
        }

        /// <summary>
        ///     Call an instance method without a return type
        /// </summary>
        /// <param name="obj">Object of type which the method is on</param>
        /// <param name="name">Name of the method</param>
        /// <param name="param">The paramters that need to be passed into the method.</param>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        /// <returns>None</returns>
        [PublicAPI]
        public static void CallMethod<TObject>(TObject obj, string name, params object[] param)
        {
            MethodInfo mi = GetMethodInfo(typeof(TObject), name) ?? throw new MissingFieldException($"Method {name} does not exist!");
            GetFastReflectionDelegate(mi).Invoke(obj, param.Length == 0 ? null : param);
        }

        /// <summary>
        ///     Call a static method with a return type
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param">The paramters that need to be passed into the method.</param>
        /// <typeparam name="TType">The Type which static method resides upon</typeparam>
        /// <typeparam name="TReturn">The return type of the method</typeparam>
        /// <returns>The specified return type</returns>
        [PublicAPI]
        public static TReturn CallMethod<TType, TReturn>(string name, params object[] param) => CallMethod<TReturn>(typeof(TType), name, param);
        
        /// <summary>
        ///     Call a static method without a return type
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param">The paramters that need to be passed into the method.</param>
        /// <typeparam name="TType">The Type which static method resides upon</typeparam>
        /// <returns>None</returns>
        [PublicAPI]
        public static void CallMethod<TType>(string name, params object[] param) => CallMethod(typeof(TType), name, param);

        /// <summary>
        ///     Call a static method with a return type (for static classes)
        /// </summary>
        /// <param name="type">Static Type which static method resides upon</param>
        /// <param name="name">Name of the method</param>
        /// <param name="param">The paramters that need to be passed into the method.</param>
        /// <typeparam name="TReturn">The return type of the method</typeparam>
        /// <returns>The specified return type</returns>
        [PublicAPI]
        public static TReturn CallMethod<TReturn>(Type type, string name, params object[] param)
        {
            MethodInfo mi = GetMethodInfo(type, name, false) ?? throw new MissingFieldException($"Method {name} does not exist!");
            return (TReturn) GetFastReflectionDelegate(mi).Invoke(null, param.Length == 0 ? null : param);
        }

        /// <summary>
        ///     Call a static method without a return type (for static classes)
        /// </summary>
        /// <param name="type">Static Type which static method resides upon</param>
        /// <param name="name">Name of the method</param>
        /// <param name="param">The paramters that need to be passed into the method.</param>
        /// <returns>None</returns>
        [PublicAPI]
        public static void CallMethod(Type type, string name, params object[] param)
        {
            MethodInfo mi = GetMethodInfo(type, name, false) ?? throw new MissingFieldException($"Method {name} does not exist!");
            GetFastReflectionDelegate(mi).Invoke(null, param.Length == 0 ? null : param);
        }

        #endregion
    }
}
