using System;

namespace Modding
{
	public class HookInfoAttribute : Attribute
	{
		public HookInfoAttribute(string desc, string hookLoc)
		{
            Description = desc;
            HookLocation = hookLoc;
		}
		public readonly string Description;
		public readonly string HookLocation;
	}
}
