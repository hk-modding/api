using System;
using System.Diagnostics;
using UnityEngine;

namespace Modding
{
	// Token: 0x020009BE RID: 2494
	[Serializable]
	public class IModSettings : ISerializationCallbackReceiver
	{
		// Token: 0x0600332A RID: 13098
		protected IModSettings()
		{
			this.StringValues = new SerializableStringDictionary();
			this.IntValues = new SerializableIntDictionary();
			this.BoolValues = new SerializableBoolDictionary();
			this.FloatValues = new SerializableFloatDictionary();
		}

		// Token: 0x060035E5 RID: 13797
		public void OnBeforeSerialize()
		{
		}

		// Token: 0x060035E6 RID: 13798
		public void OnAfterDeserialize()
		{
		}

		// Token: 0x04003BEB RID: 15339
		[SerializeField]
		public SerializableStringDictionary StringValues;

		// Token: 0x04003BEC RID: 15340
		[SerializeField]
		public SerializableIntDictionary IntValues;

		// Token: 0x04003BED RID: 15341
		[SerializeField]
		public SerializableBoolDictionary BoolValues;

		// Token: 0x04003BEE RID: 15342
		[SerializeField]
		public SerializableFloatDictionary FloatValues;
	}
}
