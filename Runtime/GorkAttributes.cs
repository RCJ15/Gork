using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// An <see cref="Attribute"/> which contains important information about a <see cref="GorkNode"/>. <para/>
    /// Each one of these attriutes added to a <see cref="GorkNode"/ > will create an entry in the gork node creation menu. <para/>
    /// Every <see cref="GorkNode"/> should implement this.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class GorkNodeInfoAttribute : Attribute
    {
        public string DisplayName = null;
        public string NodeName { get; private set; }
        private (float, float, float)? _color = null;
        public int Order;

        public bool HasWiki => !string.IsNullOrEmpty(DisplayName) && 
            (!string.IsNullOrEmpty(WikiSummary) || !string.IsNullOrEmpty(WikiDescription) || 
            !string.IsNullOrEmpty(WikiFields) || !string.IsNullOrEmpty(WikiMethods) || 
            !string.IsNullOrEmpty(WikiInputPorts) || !string.IsNullOrEmpty(WikiOutputPorts) || 
            !string.IsNullOrEmpty(WikiUsage));

        public string WikiName = null;
        public string WikiSummary = null;
        public string WikiDescription = null;
        public string WikiFields = null;
        public string WikiMethods= null;
        public string WikiInputPorts = null;
        public string WikiOutputPorts = null;
        public string WikiUsage = null;

        public GorkNodeInfoAttribute() { }

        public GorkNodeInfoAttribute(string name)
        {
            DisplayName = name;

            NodeName = name.Substring(name.LastIndexOf('/') + 1);
        }

        public GorkNodeInfoAttribute(string name, int order) : this(name)
        {
            Order = order;
        }

        public GorkNodeInfoAttribute(string name, (float, float, float) color) : this(name)
        {
            _color = color;
        }

        public GorkNodeInfoAttribute(string name, (float, float, float) color, int order) : this(name, color)
        {
            Order = order;
        }

        public GorkNodeInfoAttribute(string name, string hexadecimalColor) : this(name)
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

        public GorkNodeInfoAttribute(string name, string hexadecimalColor, int order) : this(name, hexadecimalColor)
        {
            Order = order;
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
        public static readonly Dictionary<GorkNodeInfoAttribute, Type> AttributeTypes = new Dictionary<GorkNodeInfoAttribute, Type>();

        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> that have a <see cref="GorkNodeInfoAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, List<GorkNodeInfoAttribute>> TypeAttributes = new Dictionary<Type, List<GorkNodeInfoAttribute>>();

        [InitializeOnLoadMethod]
        private static void CacheTypes()
        {
            List<GorkNodeInfoAttribute> attributeList = new List<GorkNodeInfoAttribute>();

            // Use the TypeCache to loop through all of the Types with the GorkNodeInfoAttribute attached
            foreach (Type type in TypeCache.GetTypesWithAttribute<GorkNodeInfoAttribute>())
            {
                // Get the attribute
                IEnumerable<GorkNodeInfoAttribute> attributes = type.GetCustomAttributes<GorkNodeInfoAttribute>();

                foreach (GorkNodeInfoAttribute attribute in attributes)
                {
                    // Add the attribute to the list
                    attributeList.Add(attribute);

                    // Also add the attribute and type to the Dictionaries
                    AttributeTypes.Add(attribute, type);

                    if (!TypeAttributes.ContainsKey(type))
                    {
                        TypeAttributes.Add(type, new List<GorkNodeInfoAttribute>());
                    }

                    TypeAttributes[type].Add(attribute);
                }
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
        public string PortName = "Input";
        public Type PortType = GorkUtility.SignalType;

        public string WikiDescription = null;

        public GorkPortAttribute(string portName, Type portType)
        {
            PortName = portName;
            PortType = portType;
        }

        public GorkPortAttribute(string portName) : this(portName, GorkUtility.SignalType)
        {

        }
    }

    /// <summary>
    /// Attach this <see cref="Attribute"/> to your <see cref="GorkNode"/> to add an Input port to it. <para/>
    /// NOTE: Add more of these for even more ports. Also note that adding this <see cref="Attribute"/> is not required for a <see cref="GorkNode"/>.
    /// </summary>
    public class GorkInputPortAttribute : GorkPortAttribute
    {
        public static readonly GorkPortAttribute[] TemplatePortArray = new GorkInputPortAttribute[] { new GorkInputPortAttribute("Input", GorkUtility.SignalType) };

        public GorkInputPortAttribute(string portName) : base(portName) { }
        public GorkInputPortAttribute(string portName, Type portType) : base(portName, portType) { }

#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="GorkInputPortAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, GorkInputPortAttribute[]> Attributes = new Dictionary<Type, GorkInputPortAttribute[]>();

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
        public static readonly GorkPortAttribute[] TemplatePortArray = new GorkOutputPortAttribute[] { new GorkOutputPortAttribute("Output", GorkUtility.SignalType) };

        public GorkOutputPortAttribute(string portName) : base(portName) { }
        public GorkOutputPortAttribute(string portName, Type portType) : base(portName, portType) { }

#if UNITY_EDITOR
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="GorkOutputPortAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, GorkOutputPortAttribute[]> Attributes = new Dictionary<Type, GorkOutputPortAttribute[]>();

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
        public static readonly Dictionary<Type, NoInputPortsAttribute[]> Attributes = new Dictionary<Type, NoInputPortsAttribute[]>();

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
        public static readonly Dictionary<Type, NoOutputPortsAttribute[]> Attributes = new Dictionary<Type, NoOutputPortsAttribute[]>();

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

    /// <summary>
    /// A <see cref="Attribute"/> that will tell <see cref="GorkGraph"/> that this Field should not be saved when copying a <see cref="GorkNode"/> in the graph editor. <para/>
    /// This attribute is really only useful in the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DontSaveInGorkGraphAttribute : Attribute
    {

    }

    /// <summary>
    /// Use this <see cref="Attribute"/> to mark a Class as a Gork Converter. <para/>
    /// Then when you have marked the class, also mark a STATIC method that takes in a SINGLE parameter and RETURNS some value to create a GorkConvertion. <para/>
    /// See example for Float to Int here: <see cref="BaseGorkConvertions"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GorkConverterAttribute : Attribute
    {
        /// <summary>
        /// A static <see cref="Dictionary{TKey, TValue}"/> of all the <see cref="Type"/> and all of their <see cref="GorkConverterAttribute"/> attached.
        /// </summary>
        public static readonly Dictionary<Type, GorkConverterAttribute> Attributes = new Dictionary<Type, GorkConverterAttribute>();

        public static readonly Dictionary<Type, List<MethodInfo>> ConverterMethods = new Dictionary<Type, List<MethodInfo>>();
        public static readonly Dictionary<MethodInfo, Type> ParameterType = new Dictionary<MethodInfo, Type>();
        public static readonly Dictionary<MethodInfo, Type> ReturnType = new Dictionary<MethodInfo, Type>();

        public static readonly Dictionary<Type, Dictionary<Type, MethodInfo>> GorkConvertion = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        private static void CacheTypes()
        {
            // Use the TypeCache to loop through all of the Types with the GorkPortColorAttribute attached
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // Get the attribute
                    GorkConverterAttribute attribute = type.GetCustomAttribute<GorkConverterAttribute>();

                    if (attribute == null)
                    {
                        continue;
                    }

                    // Also add the attribute and type to the Dictionaries
                    Attributes.Add(type, attribute);

                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (!method.IsStatic)
                        {
                            continue;
                        }

                        if (method.GetCustomAttribute<GorkConverterAttribute>() == null)
                        {
                            continue;
                        }

                        // Parameter check
                        ParameterInfo[] parameters = method.GetParameters();

                        if (parameters.Length != 1)
                        {
                            continue;
                        }

                        Type parameterType = parameters[0].ParameterType;

                        // Return parameter check
                        if (method.ReturnParameter == null)
                        {
                            continue;
                        }

                        Type returnType = method.ReturnType;

                        // Add to dictionaries
                        if (!ConverterMethods.ContainsKey(type))
                        {
                            ConverterMethods.Add(type, new List<MethodInfo>());
                        }

                        if (!GorkConvertion.ContainsKey(parameterType))
                        {
                            GorkConvertion.Add(parameterType, new Dictionary<Type, MethodInfo>());
                        }

                        if (GorkConvertion[parameterType].ContainsKey(returnType))
                        {
#if UNITY_EDITOR
                            Debug.LogWarning($"Method \"{method.Name}\" can't be added. There already is a Gork Converter method for converting \"{parameterType.Name}\" to \"{returnType.Name}\"! The method is \"{GorkConvertion[parameterType][returnType].Name}\"");
#endif
                            continue;
                        }

                        // Add to dictionary
                        ConverterMethods[type].Add(method);

                        ParameterType[method] = parameterType;
                        ReturnType[method] = returnType;

                        GorkConvertion[parameterType].Add(returnType, method);
                    }

                    //-- Done Method Loop
                }

                //-- Done Type Loop
            }

            //-- Done Assembly Loop
        }
    }

    /// <summary>
    /// Use this <see cref="Attribute"/> in a <see cref="GorkNode"/> field or method to make it appear in the Nodes Gork Wiki Page. <para/>
    /// Do note that you have to add wiki information to the node via the <see cref="GorkNodeInfoAttribute"/>.
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