using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Modding
{
	// Token: 0x020009C6 RID: 2502
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		// Token: 0x0600355A RID: 13658
		public void OnBeforeSerialize()
		{
			this.keys.Clear();
			this.values.Clear();
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				this.keys.Add(pair.Key);
				TValue value = pair.Value;
				this.values.Add(pair.Value);
			}
		}

		// Token: 0x0600355B RID: 13659
		public void OnAfterDeserialize()
		{
			base.Clear();
			if (this.keys.Count != this.values.Count)
			{
				throw new Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", new object[0]));
			}
			for (int i = 0; i < this.keys.Count; i++)
			{
				base.Add(this.keys[i], this.values[i]);
			}
		}

		// Token: 0x0600355C RID: 13660
		public SerializableDictionary()
		{
		}

		// Token: 0x04003BE3 RID: 15331
		[SerializeField]
		private List<TKey> keys = new List<TKey>();

		// Token: 0x04003BE4 RID: 15332
		[SerializeField]
		private List<TValue> values = new List<TValue>();
	}
}
