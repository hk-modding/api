// ReSharper disable once CheckNamespace

namespace System.Runtime.CompilerServices
{
    /// <inheritdoc />
    /// <summary>
    ///     Causes the method's name that is calling the function to be compiled into the method call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CallerMemberNameAttribute : Attribute
    {
    }
}