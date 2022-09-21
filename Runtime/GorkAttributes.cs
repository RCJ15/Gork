using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// <see cref="Attribute"/> for <see cref="GorkNode"/> that contains some important information. <para/>
    /// Every <see cref="GorkNode"/> should implement this.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GorkNodeInfoAttribute : Attribute
    {
        public string DisplayName = null;
        public string NodeName { get; private set; }
        private (float, float, float)? _color = null;

        public GorkNodeInfoAttribute() { }

        public GorkNodeInfoAttribute(string displayName)
        {
            DisplayName = displayName;

            string[] split = displayName.Split('/');
            NodeName = split[split.Length - 1];
        }

        public GorkNodeInfoAttribute(string displayName, (float, float, float) color)
        {
            DisplayName = displayName;

            string[] split = displayName.Split('/');
            NodeName = split[split.Length - 1];

            _color = color;
        }

        public GorkNodeInfoAttribute(string displayName, string hexadecimalColor)
        {
            DisplayName = displayName;

            string[] split = displayName.Split('/');
            NodeName = split[split.Length - 1];

            if (!hexadecimalColor.StartsWith('#'))
            {
                hexadecimalColor = $"#{hexadecimalColor}";
            }

            if (ColorUtility.TryParseHtmlString(hexadecimalColor, out Color color))
            {
                _color = (color.r, color.g, color.b);
            }
        }

        public Color? GetColor()
        {
            if (!_color.HasValue)
            {
                return null;
            }

            (float, float, float) color = _color.Value;

            return new Color(color.Item1, color.Item2, color.Item3);
        }

#if UNITY_EDITOR
        /// <summary>
        /// A static array of all the attributes.
        /// </summary>
        public static GorkNodeInfoAttribute[] Attributes;
        public static int Length;

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of the <see cref="GorkNodeInfoAttribute"/> and which <see cref="Type"/> they belong to.
        /// </summary>
        public static Dictionary<GorkNodeInfoAttribute, Type> AttributeTypes = new Dictionary<GorkNodeInfoAttribute, Type>();

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> that have a <see cref="GorkNodeInfoAttribute"/> attached.
        /// </summary>
        public static Dictionary<Type, GorkNodeInfoAttribute> TypeAttributes = new Dictionary<Type, GorkNodeInfoAttribute>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            List<GorkNodeInfoAttribute> attributeList = new List<GorkNodeInfoAttribute>();

            // Use the TypeCache to loop through all of the Types with the GorkNodeInfoAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<GorkNodeInfoAttribute>())
            {
                // Get the attribute
                GorkNodeInfoAttribute attribute = type.GetCustomAttribute<GorkNodeInfoAttribute>();

                // Add the attribute to the list
                attributeList.Add(attribute);

                // Also add the attribute and type to the Dictionaries
                AttributeTypes.Add(attribute, type);
                TypeAttributes.Add(type, attribute);
            }

            // Save our list as an array
            Attributes = attributeList.ToArray();

            // Cache length
            Length = Attributes.Length;
        }
#endif
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class GorkPortAttribute : Attribute
    {
        public static readonly Type DefaultType = typeof(DummyClass);

        private class DummyClass { }

        public string PortName = "Input";
        public Type PortType = DefaultType;
        public bool DisplayType = true;

        public GorkPortAttribute(string portName)
        {
            PortName = portName;
            PortType = DefaultType;
        }

        public GorkPortAttribute(string portName, Type portType)
        {
            PortName = portName;
            PortType = DefaultType;
            PortType = portType;
        }

        public GorkPortAttribute(string portName, Type portType, bool displayType)
        {
            PortName = portName;
            PortType = DefaultType;
            PortType = portType;
            DisplayType = displayType;
        }
    }

    /// <summary>
    /// Attach this <see cref="Attribute"/> to your <see cref="GorkNode"/> to add an Input port to it. <para/>
    /// NOTE: Add more of these for even more ports. Also note that adding this <see cref="Attribute"/> is not required for a <see cref="GorkNode"/>.
    /// </summary>
    public class GorkInputPortAttribute : GorkPortAttribute
    {
        public static readonly GorkPortAttribute[] TemplatePortArray = new GorkInputPortAttribute[] { new GorkInputPortAttribute("Input", DefaultType, true) };

        public GorkInputPortAttribute(string portName) : base(portName) { }
        public GorkInputPortAttribute(string portName, Type portType) : base(portName, portType) { }
        public GorkInputPortAttribute(string portName, Type portType, bool displayType) : base(portName, portType, displayType) { }

#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="GorkInputPortAttribute"/> attached.
        /// </summary>
        public static Dictionary<Type, GorkInputPortAttribute[]> Attributes = new Dictionary<Type, GorkInputPortAttribute[]>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            // Use the TypeCache to loop through all of the Types with the GorkInputPortAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<GorkInputPortAttribute>())
            {
                // Get the attributes
                IEnumerable<GorkInputPortAttribute> attribute = type.GetCustomAttributes<GorkInputPortAttribute>();

                // Also add the attribute and type to the Dictionaries
                Attributes.Add(type, attribute.ToArray());
            }
        }
#endif
    }

    /// <summary>
    /// Attach this <see cref="Attribute"/> to your <see cref="GorkNode"/> to add an Output port to it. <para/>
    /// NOTE: Add more of these for even more ports. Also note that adding this <see cref="Attribute"/> is not required for a <see cref="GorkNode"/>.
    /// </summary>
    public class GorkOutputPortAttribute : GorkPortAttribute
    {
        public static readonly GorkPortAttribute[] TemplatePortArray = new GorkOutputPortAttribute[] { new GorkOutputPortAttribute("Output", DefaultType, true) };

        public GorkOutputPortAttribute(string portName) : base(portName) { }
        public GorkOutputPortAttribute(string portName, Type portType) : base(portName, portType) { }
        public GorkOutputPortAttribute(string portName, Type portType, bool displayType) : base(portName, portType, displayType) { }

#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="GorkOutputPortAttribute"/> attached.
        /// </summary>
        public static Dictionary<Type, GorkOutputPortAttribute[]> Attributes = new Dictionary<Type, GorkOutputPortAttribute[]>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            // Use the TypeCache to loop through all of the Types with the GorkOutputPortAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<GorkOutputPortAttribute>())
            {
                // Get the attributes
                IEnumerable<GorkOutputPortAttribute> attribute = type.GetCustomAttributes<GorkOutputPortAttribute>();

                // Also add the attribute and type to the Dictionaries
                Attributes.Add(type, attribute.ToArray());
            }
        }
#endif
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NoInputPortsAttribute : Attribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="NoInputPortsAttribute"/> attached.
        /// </summary>
        public static Dictionary<Type, NoInputPortsAttribute[]> Attributes = new Dictionary<Type, NoInputPortsAttribute[]>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            // Use the TypeCache to loop through all of the Types with the NoInputPortsAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<NoInputPortsAttribute>())
            {
                // Get the attributes
                IEnumerable<NoInputPortsAttribute> attribute = type.GetCustomAttributes<NoInputPortsAttribute>();

                // Also add the attribute and type to the Dictionaries
                Attributes.Add(type, attribute.ToArray());
            }
        }
#endif
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NoOutputPortsAttribute : Attribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="NoOutputPortsAttribute"/> attached.
        /// </summary>
        public static Dictionary<Type, NoOutputPortsAttribute[]> Attributes = new Dictionary<Type, NoOutputPortsAttribute[]>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            // Use the TypeCache to loop through all of the Types with the NoOutputPortsAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<NoOutputPortsAttribute>())
            {
                // Get the attributes
                IEnumerable<NoOutputPortsAttribute> attribute = type.GetCustomAttributes<NoOutputPortsAttribute>();

                // Also add the attribute and type to the Dictionaries
                Attributes.Add(type, attribute.ToArray());
            }
        }
#endif
    }

    /// <summary>
    /// Modifies the <see cref="Color"/> of a Gork Port. This is purely visual and won't affact anything else other than the color.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GorkPortColorAttribute : Attribute
    {
        private float _r, _g, _b;

        public GorkPortColorAttribute(float r, float g, float b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public GorkPortColorAttribute(string hexadecimal)
        {
            Color col;

            if (!hexadecimal.StartsWith('#'))
            {
                hexadecimal = $"#{hexadecimal}";
            }

            if (!ColorUtility.TryParseHtmlString(hexadecimal, out col))
            {
                col = Color.white;
            }

            _r = col.r;
            _g = col.g;
            _b = col.b;
        }

        public Color GetColor()
        {
            return new Color(_r, _g, _b);
        }

#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="GorkPortColorAttribute"/> attached.
        /// </summary>
        public static Dictionary<Type, GorkPortColorAttribute> Attributes = new Dictionary<Type, GorkPortColorAttribute>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            // Use the TypeCache to loop through all of the Types with the GorkPortColorAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<GorkPortColorAttribute>())
            {
                // Get the attributes
                GorkPortColorAttribute attribute = type.GetCustomAttribute<GorkPortColorAttribute>();

                // Also add the attribute and type to the Dictionaries
                Attributes.Add(type, attribute);
            }
        }
#endif
    }

    /// <summary>
    /// A <see cref="Attribute"/> that will tell <see cref="GorkGraph"/> that this Field should not be saved when copying a <see cref="GorkNode"/> in the graph editor. <para/>
    /// This attribute is really only useful in the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DontSaveInGorkGraphAttribute : Attribute
    {

    }
}