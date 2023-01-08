using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace Gork.Editor
{
    /// <summary>
    /// Always makes sure that the GORK define symbol is present in the scripting define symbols in the <see cref="PlayerSettings"/>, ALWAYS.
    /// </summary>
    public class GorkScriptingDefineSymbol
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            NamedBuildTarget currentBuildTarget =  NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            string currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(currentBuildTarget);

            foreach (string defineSymbol in currentDefineSymbols.Split(';'))
            {
                // We already have the gork define symbol so no need to add it again
                if (defineSymbol == "GORK")
                {
                    return;
                }
            }

            // Add the gork define symbol
            PlayerSettings.SetScriptingDefineSymbols(currentBuildTarget, currentDefineSymbols + ";GORK");
        }
    }
}
