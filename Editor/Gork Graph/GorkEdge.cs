using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// An <see cref="Edge"/> that connects different <see cref="GorkPort"/> together.
    /// </summary>
    public class GorkEdge : Edge
    {
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
        [SerializeField]
        public class PasteData
        {
            [SerializeReference]
            public GorkNode Input;

            [SerializeReference]
            public GorkNode Output;
        }
        #endregion
    }
}
