using System;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A class that represents a single port on a <see cref="GorkNode"/>.
    /// </summary>
    [Serializable]
    public class NodePort : ISerializationCallbackReceiver
    {
        [SerializeField] protected string name;
        public string Name { get => name; set => name = value; }

        [SerializeField] private string typeSerializedName;
        protected Type _type = null;
        public Type Type { get => _type; set => _type = value; }

        public NodePort()
        {

        }

        public NodePort(string name) : this()
        {
            this.name = name;
        }

        public NodePort(string name, Type type) : this(name)
        {
            _type = type;
        }

        public virtual void OnBeforeSerialize()
        {
            if (_type == null)
            {
                return;
            }

            typeSerializedName = _type.AssemblyQualifiedName;
        }

        public virtual void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(typeSerializedName))
            {
                return;
            }

            _type = Type.GetType(typeSerializedName);
        }
    }
}
