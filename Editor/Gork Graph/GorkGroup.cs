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

        public TextField Text { get; private set; }

        private void OnCreate(GorkGraphView graphView)
        {
            _graphView = graphView;
            _graph = graphView.Graph;

            _graphView.GorkGroups[GroupData] = this;

            title = GroupData.Name;

            Rect rect = new Rect();
            rect.min = GroupData.Pos;

            SetPosition(rect);
            UpdateGeometryFromContent();

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            // Use reflection to get access to the text field
            FieldInfo field = typeof(Group).GetField("m_TitleEditor", BindingFlags.Instance | BindingFlags.NonPublic);

            Text = field.GetValue(this) as TextField;

            //Text.RegisterValueChangedCallback(change => GroupData.Name = change.newValue);
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

                    _graph.GetNodeGroup[node] = GroupData;
                    _graph.NodesInGroups.Add(node);
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

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Undo.RecordObject(_graph, $"Renamed \"{oldName}\" Group to \"{newName}\"");
            GroupData.Name = newName;
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
        public PasteData GetPasteData()
        {
            PasteData data = new PasteData()
            {
                Name = GroupData.Name,
                Position = GroupData.Pos,
            };

            foreach (GorkNode node in GroupData.Nodes)
            {
                data.Nodes.Add(node.GUID);
            }

            return data;
        }

        [Serializable]
        public class PasteData
        {
            public string Name;
            public Vector2 Position;

            public List<string> Nodes = new List<string>();
        }
        #endregion
    }
}
