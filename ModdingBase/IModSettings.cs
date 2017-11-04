using System;
using UnityEngine;

namespace Modding
{
    /// <summary>
    /// Base class for storing settings for a Mod in the save file.
    /// </summary>
	[Serializable]
	public class IModSettings : ISerializationCallbackReceiver
	{
        /// <summary>
        /// Initializes All Dictionaries
        /// </summary>
		protected IModSettings()
		{
			StringValues = new SerializableStringDictionary();
			IntValues = new SerializableIntDictionary();
			BoolValues = new SerializableBoolDictionary();
			FloatValues = new SerializableFloatDictionary();
		}

        /// <inheritdoc />
		public void OnBeforeSerialize()
		{
		}

	    /// <inheritdoc />
		public void OnAfterDeserialize()
		{
		}

        /// <summary>
        /// String Values to be Stored
        /// </summary>
		[SerializeField]
		public SerializableStringDictionary StringValues;

        /// <summary>
        /// Int Values to be Stored
        /// </summary>
		[SerializeField]
		public SerializableIntDictionary IntValues;

        /// <summary>
        /// Bools to be Stored
        /// </summary>
		[SerializeField]
		public SerializableBoolDictionary BoolValues;

        /// <summary>
        /// Float Values to be Stored
        /// </summary>
		[SerializeField]
		public SerializableFloatDictionary FloatValues;

	}
}
