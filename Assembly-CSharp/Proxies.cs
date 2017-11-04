namespace Modding
{
    /// <summary>
    /// Called when anything in the game tries to get an bool
    /// </summary>
    /// <param name="originalSet">Value's Name</param>
    /// <returns>The bool value</returns>
    public delegate bool GetBoolProxy(string originalSet);

    /// <summary>
    /// Called when anything in the game tries to set a bool
    /// </summary>
    /// <param name="originalSet">Name of the Bool</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetBoolProxy(string originalSet, bool value);

    /// <summary>
    /// Called when anything in the game tries to get an bool
    /// </summary>
    /// <param name="intName">Value's Name</param>
    /// <returns>The bool value</returns>
    public delegate int GetIntProxy(string intName);

    /// <summary>
    /// Called when anything in the game tries to set a int
    /// </summary>
    /// <param name="intName">Name of the Int</param>
    /// <param name="value">Value to be used</param>
    public delegate void SetIntProxy(string intName, int value);

    /// <summary>
    /// Called when damage is dealt to the player
    /// </summary>
    /// <param name="hazardType">The type of hazard that caused the damage.</param>
    /// <param name="damage">Amount of Damage</param>
    /// <returns>Modified Damage</returns>
    public delegate int TakeDamageProxy(ref int hazardType, int damage);

    
    /// <summary>
    /// Called when health is taken from the player
    /// </summary>
    /// <param name="damage">Amount of Damage</param>
    /// <returns>Modified Damaged</returns>
    public delegate int TakeHealthProxy(int damage);

}
