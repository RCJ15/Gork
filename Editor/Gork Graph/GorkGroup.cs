using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// A single group in a <see cref="GorkGraph"/>.
    /// </summary>
    public class GorkGroup : Group
    {
        private GorkGraphView _graphView;
        private GorkGraph _graph;
        public GorkGraph.GroupData GroupData;
        public bool Enabled = true;

        private void OnCreate(GorkGraphView graphView)
        {
            _graphView = graphView;
            _graph = graphView.Graph;

            title = GroupData.Name;

            Rect rect = new Rect();
            rect.min = GroupData.Pos;

            SetPosition(rect);
            UpdateGeometryFromContent();

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            // Use reflection to get access to the text field
            FieldInfo field = typeof(Group).GetField("m_TitleEditor", BindingFlags.Instance | BindingFlags.NonPublic);

            TextField textField = field.GetValue(this) as TextField;

            textField.RegisterValueChangedCallback(change => GroupData.Name = change.newValue);
        }

        public GorkGroup(string title, Vector2 position, GorkGraphView graphView) : base()
        {
            GroupData = new GorkGraph.GroupData(title, position);

            OnCreate(graphView);
        }

        public GorkGroup(GorkGraph.GroupData groupData, GorkGraphView graphView) : base()
        {
            GroupData = groupData;

            OnCreate(graphView);
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {

        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);

            if (!Enabled)
            {
                return;
            }

            // Add nodes to the GroupData
            LoopThroughNodes(elements, node =>
            {
                if (!GroupData.Nodes.Contains(node))
                {
                    GroupData.Nodes.Add(node);
                }
            });
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);

            if (!Enabled)
            {
                return;
            }

            // Remove nodes to the GroupData
            LoopThroughNodes(elements, node =>
            {
                if (GroupData.Nodes.Contains(node))
                {
                    GroupData.Nodes.Remove(node);
                }
            });
        }

        private void LoopThroughNodes(IEnumerable<GraphElement> elements, Action<GorkNode> action)
        {
            foreach (GraphElement element in elements)
            {
                GorkNodeView nodeView = element as GorkNodeView;

                if (nodeView == null)
                {
                    return;
                }

                GorkNode node = nodeView.Node;

                action?.Invoke(node);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(_graph, $"Moved \"{title}\" Group");
            GroupData.Pos = newPos.min;
        }

        #region PasteData
        public PasteData GetPasteData() => new PasteData()
        {
            Name = GroupData.Name,
            Position = GroupData.Pos,
            Nodes = GroupData.Nodes,
        };

        [Serializable]
        public class PasteData
        {
            public string Name;
            public Vector2 Position;

            [SerializeReference]
            public List<GorkNode> Nodes = new List<GorkNode>();
        }
        #endregion
    }
}
