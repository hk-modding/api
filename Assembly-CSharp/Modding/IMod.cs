using System;

namespace Modding
{
	// Token: 0x020009C4 RID: 2500
	public interface IMod<T> : IMod where T : IModSettings
	{
		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x06003607 RID: 13831
		// (set) Token: 0x06003608 RID: 13832
		T Settings { get; set; }
	}
}
