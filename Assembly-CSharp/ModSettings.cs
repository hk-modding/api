using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace Modding
{
    /// <summary>
    ///     Base class for storing settings for a Mod in the save file.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public class ModSettings
    {
        /*
         * Old usage of serializable dictionaries kept for backwards compat.
         */
        #pragma warning disable 618
        /// <summary>
        ///     Bools to be Stored
        /// </summary>
        [SerializeField] public SerializableBoolDictionary BoolValues;

        /// <summary>
        ///     Float Values to be Stored
        /// </summary>
        [SerializeField] public SerializableFloatDictionary FloatValues;

        /// <summary>
        ///     Int Values to be Stored
        /// </summary>
        [SerializeField] public SerializableIntDictionary IntValues;

        /// <summary>
        ///     String Values to be Stored
        /// </summary>
        [SerializeField] public SerializableStringDictionary StringValues;

        /// <summary>
        ///     Initializes All Dictionaries
        /// </summary>
        protected ModSettings()
        {
            StringValues = new SerializableStringDictionary();
            IntValues = new SerializableIntDictionary();
            BoolValues = new SerializableBoolDictionary();
            FloatValues = new SerializableFloatDictionary();
        }
        #pragma warning restore 618

        /// <summary>
        ///     Hydrates the classes dictionaries with incoming data.
        /// </summary>
        /// <param name="incomingSettings">Incoming Settings</param>
        public void SetSettings(ModSettings incomingSettings)
        {
            StringValues = incomingSettings.StringValues;
            IntValues = incomingSettings.IntValues;
            BoolValues = incomingSettings.BoolValues;
            FloatValues = incomingSettings.FloatValues;
        }

        /// <summary>
        ///     Handles fetching of a value in the StringValues Dictionary
        /// </summary>
        /// <param name="defaultValue">Default Value to use if value is not found in the Settings Dictionary</param>
        /// <param name="name">Compiler Generated Name of Property</param>
        /// <returns>String Value for the dictionary</returns>
        public string GetString(string defaultValue = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return null;
            }

            return StringValues.ContainsKey(name) ? StringValues[name] : defaultValue;
        }

        /// <summary>
        ///     Handles setting of a value in the StringValues Dictionary
        /// </summary>
        /// <param name="value">Value to Set</param>
        /// <param name="name">Compiler Generated Name of the Property</param>
        public void SetString(string value, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (StringValues.ContainsKey(name))
            {
                StringValues[name] = value;
            }
            else
            {
                StringValues.Add(name, value);
            }
        }

        /// <summary>
        ///     Handles fetching of a value in the IntValues Dictionary
        /// </summary>
        /// <param name="defaultValue">Default Value to use if value is not found in the Settings Dictionary</param>
        /// <param name="name">Compiler Generated Name of Property</param>
        /// <returns>Int Value for the dictionary</returns>
        public int GetInt(int? defaultValue = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return 0;
            }

            return IntValues.ContainsKey(name) ? IntValues[name] : defaultValue ?? 0;
        }

        /// <summary>
        ///     Handles setting of a value in the IntValues Dictionary
        /// </summary>
        /// <param name="value">Value to Set</param>
        /// <param name="name">Compiler Generated Name of the Property</param>
        public void SetInt(int value, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (IntValues.ContainsKey(name))
            {
                IntValues[name] = value;
            }
            else
            {
                IntValues.Add(name, value);
            }
        }


        /// <summary>
        ///     Handles fetching of a value in the BoolValues Dictionary
        /// </summary>
        /// <param name="defaultValue">Default Value to use if value is not found in the Settings Dictionary</param>
        /// <param name="name">Compiler Generated Name of Property</param>
        /// <returns>Bool Value for the dictionary</returns>
        public bool GetBool(bool? defaultValue = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return false;
            }

            return BoolValues.ContainsKey(name) ? BoolValues[name] : defaultValue ?? false;
        }

        /// <summary>
        ///     Handles setting of a value in the BoolValues Dictionary
        /// </summary>
        /// <param name="value">Value to Set</param>
        /// <param name="name">Compiler Generated Name of the Property</param>
        public void SetBool(bool value, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (BoolValues.ContainsKey(name))
            {
                BoolValues[name] = value;
            }
            else
            {
                BoolValues.Add(name, value);
            }
        }

        /// <summary>
        ///     Handles fetching of a value in the FloatValues Dictionary
        /// </summary>
        /// <param name="defaultValue">Default Value to use if value is not found in the Settings Dictionary</param>
        /// <param name="name">Compiler Generated Name of Property</param>
        /// <returns>Float Value for the dictionary</returns>
        public float GetFloat(float? defaultValue = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return 0f;
            }

            return FloatValues.ContainsKey(name) ? FloatValues[name] : defaultValue ?? 0f;
        }

        /// <summary>
        ///     Handles setting of a value in the FloatValues Dictionary
        /// </summary>
        /// <param name="value">Value to Set</param>
        /// <param name="name">Compiler Generated Name of the Property</param>
        public void SetFloat(float value, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (FloatValues.ContainsKey(name))
            {
                FloatValues[name] = value;
            }
            else
            {
                FloatValues.Add(name, value);
            }
        }
    }

    // ReSharper disable once InconsistentNaming
#pragma warning disable 1591
    [Serializable]
    [Obsolete("Use ModSettings class instead.")]
    public class IModSettings : ModSettings { }
#pragma warning restore 1591
}