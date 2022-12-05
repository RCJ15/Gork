using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// Static utility class for GorkGraph in the UnityEditor.
    /// </summary>
    public static class GorkEditorUtility
    {
        /// <summary>
        /// Extension method which allows easy modification of the background texture on a <see cref="VisualElement"/>.
        /// </summary>
        public static void SetTexture(this VisualElement element, Texture2D texture)
        {
            StyleBackground styleBackground = element.style.backgroundImage;
            Background background = styleBackground.value;

            background.texture = texture;

            styleBackground.value = background;
            element.style.backgroundImage = styleBackground;
        }

        /// <summary>
        /// Converts and returns a <paramref name="base64"/> string into a <see cref="Texture2D"/>.
        /// </summary>
        public static Texture2D Texture2DFromBase64(string base64)
        {
            Texture2D texture = new Texture2D(1, 1);

            texture.LoadImage(Convert.FromBase64String(base64));

            // This ensures that the texture won't disappear when exiting playmode because that was an issue
            texture.hideFlags = HideFlags.HideAndDontSave;

            return texture;
        }
    }
}
