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
}
