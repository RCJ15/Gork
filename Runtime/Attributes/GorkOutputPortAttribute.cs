using System;

namespace Gork
{
    /// <summary>
    /// Attach this <see cref="Attribute"/> to a <see cref="GorkNode"/> script to make an output port appear in the editor.
    /// </summary>
    public class GorkOutputPortAttribute : GorkPortAttribute
    {
        public GorkOutputPortAttribute(string displayName) : base(displayName)
        {

        }

        public GorkOutputPortAttribute(Type portType) : base(portType)
        {

        }

        public GorkOutputPortAttribute(string displayName, Type portType) : base(displayName, portType)
        {

        }
    }
}
