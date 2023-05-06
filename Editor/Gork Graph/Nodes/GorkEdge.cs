using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System;
using System.Reflection;

namespace Gork.Editor
{
    /// <summary>
    /// An <see cref="Edge"/> that connects different <see cref="GorkPort"/> together.
    /// </summary>
    public class GorkEdge : Edge
    {
        private static readonly MethodInfo _updateColorsMethod = typeof(Edge).GetMethod("UpdateEdgeControlColorsAndWidth", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);

        public GorkGraphView GraphView;

        /*
        public GorkEdge()
        {
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", _ =>
            {
            });
        }
        */

        public static void UpdateColors(Edge edge)
        {
            _updateColorsMethod.Invoke(edge, null);
        }

        public void Delete()
        {   
            GraphViewChange change = new GraphViewChange();
            change.elementsToRemove = new List<GraphElement>();

            // Add this as an element to remove
            change.elementsToRemove.Add(this);

            // Invoke graphViewChanged to update the GraphView
            GraphView.graphViewChanged?.Invoke(change);

            // Disconnect
            input.Disconnect(this);
            output.Disconnect(this);

            // Remove this element from the graph
            GraphView.RemoveElement(this);
        }

        #region PasteData
        [Serializable]
        public class PasteData
        {
            public string Input;
            public int InputPortIndex;

            public string Output;
            public int OutputPortIndex;
        }

        public PasteData GetPasteData() => new PasteData()
        {
            // Spaghetti
            Input = (input.node as GorkNodeView).Node.GUID,
            InputPortIndex = (input as GorkPort).PortIndex,

            Output = (output.node as GorkNodeView).Node.GUID,
            OutputPortIndex = (output as GorkPort).PortIndex,
        };
        #endregion
    }
}
