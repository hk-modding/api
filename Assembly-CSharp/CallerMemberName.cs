namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Causes the method's name that is calling the function to be compiled into the method call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CallerMemberNameAttribute : Attribute
    {
    }
}
