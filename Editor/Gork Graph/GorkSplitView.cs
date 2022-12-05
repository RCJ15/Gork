using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class GorkSplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<GorkSplitView, TwoPaneSplitView.UxmlTraits> { }
    }
}
