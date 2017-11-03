using System;

namespace Modding
{
	// Token: 0x020009B2 RID: 2482
	public class Mod
	{
		// Token: 0x0600329D RID: 12957 RVA: 0x000026F3 File Offset: 0x000008F3
		public virtual void Initialize()
		{
		}

		// Token: 0x0600329E RID: 12958 RVA: 0x000026F3 File Offset: 0x000008F3
		public virtual void Unload()
		{
		}

		// Token: 0x0600329F RID: 12959 RVA: 0x00025530 File Offset: 0x00023730
		public virtual string GetVersion()
		{
			return "UNKNOWN";
		}
	}
}
