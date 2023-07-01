using System;

namespace Gork
{
    /// <summary>
    /// The base class for every <see cref="GorkNode"/> that gives out a constant value. <typeparamref name="T"/> is the type of value that the node should give out.
    /// </summary>
    public abstract class ValueNode<T> : GorkNode
    {
        public T Value;

        public override object ValueCall(int port, Type type)
        {
            if (type == typeof(T))
            {
                return Value;
            }

            return base.ValueCall(port, type);
        }
    }
}
