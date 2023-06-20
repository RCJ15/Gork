using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// An <see cref="Attribute"/> which contains important information about a <see cref="GorkNode"/>.
    /// Every <see cref="GorkNode"/> should implement this.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GorkMenuItemAttribute : Attribute
    {
        private string _menuPath = null;
        public string MenuPath => _menuPath;
        public string DisplayName { get; private set; }
        private (float, float, float)? _color = null;
        public int Order;

        public bool HasWiki => !string.IsNullOrEmpty(MenuPath) &&
            (!string.IsNullOrEmpty(WikiSummary) || !string.IsNullOrEmpty(WikiDescription) ||
            !string.IsNullOrEmpty(WikiFields) || !string.IsNullOrEmpty(WikiMethods) ||
            !string.IsNullOrEmpty(WikiInputPorts) || !string.IsNullOrEmpty(WikiOutputPorts) ||
            !string.IsNullOrEmpty(WikiUsage));

        public string WikiName = null;
        public string WikiSummary = null;
        public string WikiDescription = null;
        public string WikiFields = null;
        public string WikiMethods = null;
        public string WikiInputPorts = null;
        public string WikiOutputPorts = null;
        public string WikiUsage = null;

        public GorkMenuItemAttribute() { }

        public GorkMenuItemAttribute(string path)
        {
            _menuPath = path;

            DisplayName = path.Substring(path.LastIndexOf('/') + 1);
        }

        public GorkMenuItemAttribute(string name, (float, float, float) color) : this(name)
        {
            _color = color;
        }

        public GorkMenuItemAttribute(string name, string hexadecimalColor) : this(name)
        {
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
        public static GorkMenuItemAttribute[] Attributes;
        public static int Length;

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of the <see cref="GorkMenuItemAttribute"/> and which <see cref="Type"/> they belong to.
        /// </summary>
        public static readonly Dictionary<GorkMenuItemAttribute, Type> AttributeTypes = new Dictionary<GorkMenuItemAttribute, Type>();

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> that have a <see cref="GorkMenuItemAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, GorkMenuItemAttribute> TypeAttributes = new Dictionary<Type, GorkMenuItemAttribute>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            List<GorkMenuItemAttribute> attributeList = new List<GorkMenuItemAttribute>();

            // Use the TypeCache to loop through all of the Types with the GorkNodeInfoAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<GorkMenuItemAttribute>())
            {
                // Dissallow abstract classes from being added to the list
                if (type.IsAbstract)
                {
                    Debug.LogWarning("The class \"" + type.FullName + "\" is abstract and has a GorkMenuItem attribute attached. This is not allowed!");
                    continue;
                }

                // Get the attribute
                GorkMenuItemAttribute attribute = type.GetCustomAttribute<GorkMenuItemAttribute>();

                // Add the attribute to the list
                attributeList.Add(attribute);

                // Also add the attribute and type to the Dictionaries
                AttributeTypes.Add(attribute, type);

                TypeAttributes[type] = attribute;
            }

            // Save our list as an array
            Attributes = attributeList.ToArray();

            // Cache length
            Length = Attributes.Length;
        }
#endif
    }
}
