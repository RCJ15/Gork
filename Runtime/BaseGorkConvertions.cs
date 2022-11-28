using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Has the base Gork Conversions for Float to Int and Int to Float. <para/>
    /// Use this as an example of how to create your own Gork Conversions. <para/>
    /// See: <see cref="GorkConverterAttribute"/>.
    /// </summary>
    [GorkConverter]
    public class BaseGorkConvertions
    {
        /// <summary>
        /// Float to Int convertion.
        /// </summary>
        [GorkConverter]
        public static int FloatToInt(float val)
        {
            return Mathf.RoundToInt(val);
        }

        /// <summary>
        /// Int to Float convertion.
        /// </summary>
        [GorkConverter]
        public static float IntToFloat(int val)
        {
            return val;
        }
    }
}
