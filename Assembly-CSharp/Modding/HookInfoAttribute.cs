using System;

namespace Modding
{
	// Token: 0x020009AF RID: 2479
	public class HookInfoAttribute : Attribute
	{
		// Token: 0x06003294 RID: 12948 RVA: 0x0002551A File Offset: 0x0002371A
		public HookInfoAttribute(string desc, string hookLoc)
		{
			this.Description = desc;
			this.HookLocation = hookLoc;
		}

		// Token: 0x04003B36 RID: 15158
		public readonly string Description;

		// Token: 0x04003B37 RID: 15159
		public readonly string HookLocation;
	}
}
