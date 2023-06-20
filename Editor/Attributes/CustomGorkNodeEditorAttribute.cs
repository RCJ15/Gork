using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CustomGorkNodeEditorAttribute : Attribute
    {
        /// <summary>
        /// Defines which object type the custom editor class can edit.
        /// </summary>
        public Type InspectedType => _inspectedType;
        private Type _inspectedType;

        /// <summary>
        /// If true, child classes of the <see cref="InspectedType"/> will also show this editor.
        /// </summary>
        public bool Inherit;

        public CustomGorkNodeEditorAttribute(Type inspectedType)
        {
            _inspectedType = inspectedType;
        }

        /// <summary>
        /// A dictionary of every <see cref="GorkNode"/> type and what editor it should have.
        /// </summary>
        public static Dictionary<Type, Type> AssignedEditorTypes = new Dictionary<Type, Type>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            List<(CustomGorkNodeEditorAttribute, Type)> inheritAttributes = new List<(CustomGorkNodeEditorAttribute, Type)>();

            // Loop through every single type with the CustomGorkNodeEditorAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute(typeof(CustomGorkNodeEditorAttribute)))
            {
                // Continue if the type we are checking is not a GorkNodeEditor type
                if (!type.IsSubclassOf(typeof(GorkNodeEditor)))
                {
                    continue;
                }

                // Get every attribute attached to the type
                IEnumerable<CustomGorkNodeEditorAttribute> attributes = type.GetCustomAttributes<CustomGorkNodeEditorAttribute>();

                // Loop through all of the gotten attributes
                foreach (CustomGorkNodeEditorAttribute attribute in attributes)
                {
                    // Check if the target type already exists in the dictionary
                    if (AssignedEditorTypes.ContainsKey(attribute.InspectedType))
                    {
                        // Continue and display warning
                        Debug.LogWarning($"The type \"{attribute.InspectedType.Name}\" has more than one GorkNodeEditor assigned!");
                        continue;
                    }

                    if (attribute.Inherit)
                    {
                        inheritAttributes.Add((attribute, type));
                    }

                    // Add the types to the dictionary
                    AssignedEditorTypes.Add(attribute.InspectedType, type);
                }
            }

            foreach ((CustomGorkNodeEditorAttribute, Type) pair in inheritAttributes)
            {
                foreach (Type derivedType in TypeCache.GetTypesDerivedFrom(pair.Item1.InspectedType))
                {
                    if (AssignedEditorTypes.ContainsKey(derivedType))
                    {
                        continue;
                    }

                    AssignedEditorTypes.Add(derivedType, pair.Item2);
                }
            }
        }

    }
}
