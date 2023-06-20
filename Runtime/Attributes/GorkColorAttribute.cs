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
    /// Modifies the <see cref="Color"/> of a Gork Port. This is purely visual and won't affact anything else other than the color.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
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
        public static readonly Dictionary<Type, GorkPortColorAttribute> Attributes = new Dictionary<Type, GorkPortColorAttribute>();

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
}
