using System;
using UnityEngine;

namespace Modding
{
	[Serializable]
	public class IModSettings : ISerializationCallbackReceiver
	{
		protected IModSettings()
		{
			StringValues = new SerializableStringDictionary();
			IntValues = new SerializableIntDictionary();
			BoolValues = new SerializableBoolDictionary();
			FloatValues = new SerializableFloatDictionary();
		}
		public void OnBeforeSerialize()
		{
		}
		public void OnAfterDeserialize()
		{
		}
		[SerializeField]
		public SerializableStringDictionary StringValues;

		[SerializeField]
		public SerializableIntDictionary IntValues;

		[SerializeField]
		public SerializableBoolDictionary BoolValues;

		[SerializeField]
		public SerializableFloatDictionary FloatValues;

	}
}
