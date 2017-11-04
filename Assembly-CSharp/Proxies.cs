namespace Modding
{
    public delegate bool GetBoolProxy(string originalSet);
    public delegate void SetBoolProxy(string originalSet, bool value);

    public delegate int GetIntProxy(string intName);
    public delegate void SetIntProxy(string intName, int val);

    public delegate int TakeDamageProxy(ref int hazardType, int damage);
    public delegate int TakeHealthProxy(int damage);

}
