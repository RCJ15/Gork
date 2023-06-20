using System;

namespace Gork
{
    /// <summary>
    /// Attach this <see cref="Attribute"/> to a <see cref="GorkNode"/> script to make an input port appear in the editor.
    /// </summary>
    public class GorkInputPortAttribute : GorkPortAttribute
    {
        public GorkInputPortAttribute(string displayName) : base(displayName)
        {

        }

        public GorkInputPortAttribute(Type portType) : base(portType)
        {

        }

        public GorkInputPortAttribute(string displayName, Type portType) : base(displayName, portType)
        {

        }
    }
}
