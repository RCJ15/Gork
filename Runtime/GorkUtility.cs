using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Static utility class for GorkGraph at runtime.
    /// </summary>
    public static class GorkUtility
    {
        #region Generic object serialization (Anything to string and back)
        // These two static MethodInfos are for when we convert to and from json
        private static readonly MethodInfo _toJsonMethod = typeof(GorkUtility).GetMethod("ToJsonGeneric", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _fromJsonMethod = typeof(GorkUtility).GetMethod("FromJsonGeneric", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly Dictionary<Type, MethodInfo> _genericToJsonMethods = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> _genericFromJsonMethods = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetGenericToJsonMethod(Type type)
        {
            if (!_genericToJsonMethods.ContainsKey(type))
            {
                _genericToJsonMethods.Add(type, _toJsonMethod.MakeGenericMethod(type));
            }

            return _genericToJsonMethods[type];
        }
        private static MethodInfo GetGenericFromJsonMethod(Type type)
        {
            if (!_genericFromJsonMethods.ContainsKey(type))
            {
                _genericFromJsonMethods.Add(type, _fromJsonMethod.MakeGenericMethod(type));
            }

            return _genericFromJsonMethods[type];
        }


        public static string ToJson(object obj, Type type)
        {
            return GetGenericToJsonMethod(type).Invoke(null, new object[] { obj }) as string;
        }

        public static object FromJson(string json, Type type)
        {
            return GetGenericFromJsonMethod(type).Invoke(null, new object[] { json });
        }

        /// <summary>
        /// Will convert a <paramref name="obj"/> into json and return the result. <para/>
        /// This is not meant to be used on it's own but is supposed to be used with Reflections <see cref="MethodInfo.MakeGenericMethod(Type[])"/> to turn a generic <see cref="object"/> into a <see cref="string"/>. (Given it's serializable)
        /// </summary>
        private static string ToJsonGeneric<T>(T obj)
        {
            if (obj == null)
            {
                return "";
            }

            // Use ValueHolder<T> as just converting the object for some reason doesn't work
            // This is a strange workaround but it works
            string json = JsonUtility.ToJson(new ValueHolder<T>(obj));

            // Cut off the start and ending parts of the json conversion to keep the json clean
            int startLength = ValueHolder<T>.START.Length;
            json = json.Substring(startLength, json.Length - startLength - ValueHolder<T>.END.Length);

            // Return the json
            return json;
        }

        /// <summary>
        /// Will convert a json into a new <see cref="object"/> and return the result. <para/>
        /// This is not meant to be used on it's own but is supposed to be used with Reflections <see cref="MethodInfo.MakeGenericMethod(Type[])"/> to turn a <see cref="string"/> into a generic <see cref="object"/>. (Given it's serializable)
        /// </summary>
        private static T FromJsonGeneric<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            // Add the start and end parts of the json back. These parts were cut in ToJson<T>()
            json = $"{ValueHolder<T>.START}{json}{ValueHolder<T>.END}";

            // Return the json value
            return JsonUtility.FromJson<ValueHolder<T>>(json).V;
        }

        /// <summary>
        /// Class that holds a value for Generic object serialization. In short, this is used to convert anything serializable into a <see cref="string"/>.
        /// </summary>
        [Serializable]
        public class ValueHolder<T>
        {
            public static readonly string START = $"{{\"{nameof(V)}\":";
            public const string END = "}";

            public T V;

            public ValueHolder(T obj)
            {
                V = obj;
            }
        }
        #endregion

        #region Extension Methods
        public static Type SignalType =>  GorkGraph.SignalType;

        /// <summary>
        /// Returns if this <see cref="Type"/> is a Gork Signal or not.
        /// </summary>
        public static bool IsSignal(this Type type)
        {
            return type == SignalType;
        }
        #endregion
    }
}
