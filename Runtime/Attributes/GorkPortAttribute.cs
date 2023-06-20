using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// The base class for adding a <see cref="NodePort"/> to a <see cref="GorkNode"/>. <para/>
    /// You shouldn't use this by itself and instead use <see cref="GorkInputPortAttribute"/> or <see cref="GorkOutputPortAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class GorkPortAttribute : Attribute
    {
        /// <summary>
        /// The display name of the port in the editor.
        /// </summary>
        public string DisplayName => _displayName;
        protected string _displayName = null;

        /// <summary>
        /// The type of the port.
        /// </summary>
        public Type PortType => _portType;
        protected Type _portType = null;

        /// <summary>
        /// The order of how the port attributes on this node are placed. Is -1 by default.
        /// </summary>
        public int Order = -1;

        protected GorkPortAttribute()
        {

        }

        protected GorkPortAttribute(string displayName) : this()
        {
            _displayName = displayName;
        }

        protected GorkPortAttribute(Type portType) : this()
        {
            _portType = portType;
        }

        protected GorkPortAttribute(string displayName, Type portType) : this(displayName)
        {
            _portType = portType;
        }

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their attached <see cref="GorkPortAttribute"/>.
        /// </summary>
        public static readonly Dictionary<Type, IEnumerable<GorkPortAttribute>> Attributes = new Dictionary<Type, IEnumerable<GorkPortAttribute>>();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        private static void CacheTypes()
        {
            // Loop through every single assembly in the application
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Loop through every single type in the assembly
                foreach (Type type in assembly.GetTypes())
                {
                    // Get every port attribute attached to the type
                    IEnumerable<GorkPortAttribute> portAttributes = type.GetCustomAttributes<GorkPortAttribute>();

                    // The type has no port attributes :(
                    if (portAttributes == null || !portAttributes.Any())
                    {
                        continue;
                    }

                    // Add the enumerable and port to the dicitonary
                    Attributes.Add(type, portAttributes.OrderBy(attribute => attribute.Order));
                }
                //-- Type loop is done
            }
            //-- Assembly loop is done
        }

        /// <summary>
        /// A simple container class that contains info about a singular <see cref="GorkPortAttribute"/>. <para/>
        /// In reality, it just contains a reference to the <see cref="GorkPortAttribute"/> and it's attached <see cref="MemberInfo"/>.
        /// </summary>
        public class Info
        {
            public GorkPortAttribute Attribute;
            public MemberInfo Member;

            public Info(GorkPortAttribute attribute, MemberInfo member)
            {
                Attribute = attribute;
                Member = member;
            }
        }
    }
}
