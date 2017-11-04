namespace Modding
{
    public interface IMod
    {
        void Initialize();
        void Unload();
        string GetVersion();
    }

    public interface IMod<T> : IMod where T : IModSettings
	{
		T Settings { get; set; }
	}
}
