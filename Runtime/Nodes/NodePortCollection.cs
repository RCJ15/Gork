using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A class that represents a collection of <see cref="NodePort"/> on a <see cref="GorkNode"/>. <para/>
    /// This class actually has the attribute ports and custom ports of a <see cref="GorkNode"/> stored as two seperate variables, but allows you to loop through it like the attribute ports and custom ports are apart of the same list.
    /// </summary>
    public class NodePortCollection : IEnumerable<NodePort>, ICollection<NodePort>
    {
        public NodePort[] AttributePorts { get; private set; }
        public int AttributePortsCount { get; private set; }

        public List<NodePort> CustomPorts { get; private set; }
        public int CustomPortsCount { get => CustomPorts.Count; }

        public int Count
        {
            get
            {
                return AttributePortsCount + CustomPortsCount;
            }
        }

        public bool IsReadOnly => false;

        public NodePort this[int i]
        {
            get
            {
                if (i < AttributePortsCount)
                {
                    return AttributePorts[i];
                }
                else
                {
                    return CustomPorts[i - AttributePortsCount];
                }
            }
        }

        public NodePortCollection(NodePort[] attributePorts, List<NodePort> customPorts)
        {
            AttributePorts = attributePorts;
            AttributePortsCount = attributePorts.Length;

            CustomPorts = customPorts;
        }

        public IEnumerator<NodePort> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(NodePort item)
        {
            int index = CustomPorts.IndexOf(item);

            if (index > -1)
            {
                return index;
            }

            for (int i = 0; i < AttributePortsCount; i++)
            {
                if (AttributePorts[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, NodePort item)
        {
            if (index < AttributePortsCount)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Inserting a port into the Attribute Port array is not allowed!");
#endif
                return;
            }

            CustomPorts.Insert(index - AttributePortsCount, item);
        }

        public void RemoveAt(int index)
        {
            if (index < AttributePortsCount)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Removing an Attribute Port is not allowed!");
#endif
                return;
            }

            CustomPorts.RemoveAt(index - AttributePortsCount);
        }

        public void Clear()
        {
            CustomPorts.Clear();
        }

        public bool Contains(NodePort item)
        {
            if (CustomPorts.Contains(item))
            {
                return true;
            }

            return AttributePorts.Contains(item);
        }

        public void CopyTo(NodePort[] array, int arrayIndex)
        {
            CustomPorts.CopyTo(array, arrayIndex - AttributePortsCount);
        }

        public void Add(NodePort item)
        {
            CustomPorts.Add(item);
        }

        bool ICollection<NodePort>.Remove(NodePort item)
        {
            return CustomPorts.Remove(item);
        }
    }
}
