using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// Use this <see cref="Attribute"/> in a <see cref="GorkNode"/> field or method to make it appear in the Nodes Gork Wiki Page. <para/>
    /// Do note that you have to add wiki information to the node via the <see cref="GorkMenuItemAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GorkWikiInfoAttribute : Attribute
    {
        public string DisplayName = null;
        public bool DisplayType = true;
        public string Description = null;

        public GorkWikiInfoAttribute(string description)
        {
            Description = description;
        }

#if UNITY_EDITOR
        public static string GetTypeName(Type type)
        {
            if (type == typeof(float))
            {
                return "Float";
            }
            else if (type == typeof(int))
            {
                return "Int";
            }
            else if (type == typeof(bool))
            {
                return "Bool";
            }
            else
            {
                return type.Name;
            }
        }

        public void DisplayField(StringBuilder builder, FieldInfo field)
        {
            builder.Append("<b>");

            if (string.IsNullOrEmpty(DisplayName))
            {
                builder.Append(field.Name);
            }
            else
            {
                builder.Append(DisplayName);
            }

            if (DisplayType)
            {
                builder.Append(" (");
                builder.Append(GetTypeName(field.FieldType));
                builder.Append(')');
            }

            builder.Append("</b> - ");

            builder.Append(Description);

            if (!Description.EndsWith('.'))
            {
                builder.Append('.');
            }

            builder.AppendLine();
        }
        public void DisplayMethod(StringBuilder builder, MethodInfo method)
        {
            builder.Append("<b>");

            if (DisplayType)
            {
                builder.Append(GetTypeName(method.ReturnType));
                builder.Append(' ');
            }

            if (string.IsNullOrEmpty(DisplayName))
            {
                builder.Append(method.Name);
            }
            else
            {
                builder.Append(DisplayName);
            }

            if (DisplayType)
            {
                ParameterInfo[] parameters = method.GetParameters();
                int length = parameters.Length;

                for (int i = 0; i < length; i++)
                {
                    if (i == 0)
                    {
                        builder.Append('(');
                    }
                    else if (i < length - 1)
                    {
                        builder.Append(", ");
                    }

                    ParameterInfo parameter = parameters[i];

                    builder.Append(GetTypeName(parameter.ParameterType));

                    if (i == length - 1)
                    {
                        builder.Append(")");
                    }
                }
            }

            builder.Append("</b> - ");

            builder.Append(Description);

            if (!Description.EndsWith('.'))
            {
                builder.Append('.');
            }

            builder.AppendLine();
        }

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="FieldInfo"/> that have a <see cref="GorkWikiInfoAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, List<FieldInfo>> TypeFields = new Dictionary<Type, List<FieldInfo>>();
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="MethodInfo"/> that have a <see cref="GorkWikiInfoAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, List<MethodInfo>> TypeMethods = new Dictionary<Type, List<MethodInfo>>();

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="FieldInfo"/> that have a <see cref="GorkWikiInfoAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<FieldInfo, GorkWikiInfoAttribute> FieldAttributes = new Dictionary<FieldInfo, GorkWikiInfoAttribute>();
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="MethodInfo"/> that have a <see cref="GorkWikiInfoAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<MethodInfo, GorkWikiInfoAttribute> MethodAttributes = new Dictionary<MethodInfo, GorkWikiInfoAttribute>();

        [InitializeOnLoadMethod]
        private static void CacheAttributes()
        {
            // Use the TypeCache to loop through all of the Fields with the GorkWikiInfoAttribute attached
            foreach (FieldInfo field in TypeCache.GetFieldsWithAttribute<GorkWikiInfoAttribute>())
            {
                // Add the attribute and field to the Dictionary
                FieldAttributes.Add(field, field.GetCustomAttribute<GorkWikiInfoAttribute>());

                // Add the field to the TypeFields dictionary
                Type reflectedType = field.ReflectedType;

                if (!TypeFields.ContainsKey(reflectedType))
                {
                    TypeFields[reflectedType] = new List<FieldInfo>();
                }

                TypeFields[reflectedType].Add(field);
            }

            // Use the TypeCache to loop through all of the Methods with the GorkWikiInfoAttribute attached
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<GorkWikiInfoAttribute>())
            {
                // Add the attribute and method to the Dictionary
                MethodAttributes.Add(method, method.GetCustomAttribute<GorkWikiInfoAttribute>());

                // Add the method to the TypeMethods dictionary
                Type reflectedType = method.ReflectedType;

                if (!TypeMethods.ContainsKey(reflectedType))
                {
                    TypeMethods[reflectedType] = new List<MethodInfo>();
                }

                TypeMethods[reflectedType].Add(method);
            }
        }
#endif
    }
}
