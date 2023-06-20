using System;

namespace Gork
{
    /// <summary>
    /// A <see cref="Attribute"/> that will tell <see cref="GorkGraph"/> that this Field should not be saved when copying a <see cref="GorkNode"/> in the graph editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DontSaveInGorkGraphAttribute : Attribute
    {

    }
}
