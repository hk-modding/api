using System;

namespace Modding
{
	// Token: 0x020009C5 RID: 2501
	public interface IMod
	{
		// Token: 0x06003320 RID: 13088
		void Initialize();

		// Token: 0x06003321 RID: 13089
		void Unload();

		// Token: 0x06003322 RID: 13090
		string GetVersion();
	}
}
