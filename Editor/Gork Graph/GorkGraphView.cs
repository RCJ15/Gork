using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using Object = UnityEngine.Object;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class GorkGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<GorkGraphView, UxmlTraits> { }

        public GorkGraph Graph;
        private GorkNodeSearchWindow _searchWindow;
        private Vector3 _cachedMousePos;

        protected override bool canDeleteSelection => false;

        public GorkGraphView()
        {
            // Add the grid background
            Insert(0, new GridBackground());

            // Add manipulators
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Add our style sheets
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(GorkGraphEditor.GetPath("uss"));
            styleSheets.Add(styleSheet);

            // Setup search window
            _searchWindow = ScriptableObject.CreateInstance<GorkNodeSearchWindow>();
            _searchWindow.GraphView = this;

            // Open search window when node creation is requested
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
            };

            RegisterCallback<ValidateCommandEvent>(OnValidateCommand);
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

            // Subscribe to the graphViewChanged callback
            graphViewChanged += OnGraphViewChange;
        }

        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            _cachedMousePos = evt.mousePosition;
        }

        private void OnValidateCommand(ValidateCommandEvent evt)
        {
            //Debug.Log(evt.commandName);

            //-- Copy & Paste
            if (evt.commandName == "Copy")
            {
                CopySelection();
            }
            else if (evt.commandName == "Paste")
            {
                Paste(-contentViewContainer.WorldToLocal(_cachedMousePos));
            }
            else if (evt.commandName == "Cut")
            {
                List<GorkNodeView> nodes = CopySelection();

                // Create an undo group
                int group = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Cut Nodes");

                // Delete all the copied nodes
                foreach (GorkNodeView node in nodes)
                {
                    node.Delete();
                }

                // Collapse undo operations
                Undo.CollapseUndoOperations(group);
            }
            //-- Misc
            // Select All
            else if (evt.commandName == "SelectAll")
            {
                // Loop through all elements
                foreach (GraphElement element in graphElements)
                {
                    if (selection.Contains(element))
                    {
                        continue;
                    }

                    // Select them all
                    AddToSelection(element);
                }
            }
            // Delete selection
            else if (evt.commandName == "Delete" || evt.commandName == "SoftDelete")
            {
                List<GraphElement> elementsToRemove = new List<GraphElement>();

                foreach (GraphElement element in selection)
                {
                    elementsToRemove.Add(element);
                }

                ClearSelection();

                graphViewChanged.Invoke(new GraphViewChange() { elementsToRemove = elementsToRemove });

                // Remove elements
                foreach (GraphElement element in elementsToRemove)
                {
                    RemoveElement(element);
                }
            }
        }

        // This is a destructor
        ~GorkGraphView()
        {
            // Destroy dead Search Window object
            Object.DestroyImmediate(_searchWindow);
        }

        public void OnUndoRedo()
        {
            // Do nothing if there is no graph
            if (Graph == null)
            {
                return;
            }

            // Save assets and reimport the graph
            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Graph), ImportAssetOptions.ForceUpdate);

            // Reopen the graph to refresh the graph view
            OpenGraph(Graph);

            Debug.Log("On undo Redo");
        }

        /// <summary>
        /// Returns the <see cref="GorkNodeView"/> for the <see cref="GorkNode"/>.
        /// </summary>
        public GorkNodeView GetNodeView(GorkNode node) => GetNodeByGuid(node.GUID) as GorkNodeView;

        /// <summary>
        /// Removes all previous elements and opens a new <see cref="GorkGraph"/> to edit.
        /// </summary>
        public void OpenGraph(GorkGraph graph)
        {
            // Set the graph variable
            Graph = graph;

            // Cache the object which we need to remove
            IEnumerable<GraphElement> elementsToRemove = graphElements;

            // Disable all GorkGroup elements so they won't wrongly delete their own data
            foreach (GorkGroup group in elementsToRemove.OfType<GorkGroup>())
            {
                group.Enabled = false;
            }

            // Disconnect all connections on all edges
            foreach (GorkEdge edge in elementsToRemove.OfType<GorkEdge>())
            {
                if (edge.output != null)
                {
                    edge.output.Disconnect(edge);
                }

                if (edge.input != null)
                {
                    edge.input.Disconnect(edge);
                }

                edge.output = null;
                edge.input = null;
            }

            // Lasty, actually remove all of the elements
            foreach (GraphElement element in elementsToRemove)
            {
                RemoveElement(element);
            }

            // Create all nodes in the GraphView by looping through all of the nodes in the graph
            Graph.Nodes.ForEach(node =>
            {
                // Ignore empty nodes
                if (node == null)
                {
                    return;
                }

                // Create the node view
                GorkNodeView nodeView = CreateNodeView(node);
            });

            // Create all the edges (connections) by looping through all of the nodes in the graph again
            // (we do this after so we already have all the nodes created in the graph)
            Graph.Nodes.ForEach(node =>
            {
                // Return if the connections are (somehow) null
                if (node.AllConnections == null)
                {
                    return;
                }

                // Get the parent node view
                GorkNodeView parentNodeView = GetNodeView(node);

                // Create connections for the node by looping through all connections on the node
                int portLength = node.AllConnections.Count;

                for (int portIndex = 0; portIndex < portLength; portIndex++)
                {
                    if (portIndex >= parentNodeView.OutputPorts.Count)
                    {
                        continue;
                    }

                    // Get the parent port
                    GorkPort parentPort = parentNodeView.OutputPorts[portIndex];

                    var port = node.AllConnections[portIndex];
                    int connectionLength = port.Connections.Count;

                    // Loop through the connections on the port
                    for (int i = 0; i < connectionLength; i++)
                    {
                        var connection = port.Connections[i];

                        if (connection.Node == null)
                        {
                            continue;
                        }

                        // Get the child node view and the child port
                        GorkNodeView childNodeView = GetNodeView(connection.Node);
                        GorkPort childPort = childNodeView.InputPorts[connection.PortIndex];

                        // Connect the parent port to the child port
                        GorkEdge edge = parentPort.GorkConnectTo(childPort);

                        // Add the edge to the graph
                        AddElement(edge);
                    }
                }
            });

            // Create gork groups
            Graph.GorkGroups.ForEach(groupData =>
            {
                GorkGroup group = new GorkGroup(groupData, this);

                AddElement(group);

                group.Enabled = false;

                foreach (GorkNode node in groupData.Nodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    // Add the GorkNodes view to the group
                    group.AddElement(GetNodeView(node));
                }

                group.Enabled = true;
            });
        }

        private GraphViewChange OnGraphViewChange(GraphViewChange change)
        {
            // Check if we have any elements to remove
            if (change.elementsToRemove != null)
            {
                // Loop through all the elements that are being removed
                change.elementsToRemove.ForEach(element =>
                {
                    // Remove node
                    GorkNodeView nodeView = element as GorkNodeView;

                    if (nodeView != null)
                    {
                        // Delete node from graph
                        Graph.DeleteNode(nodeView.Node);
                    }

                    // Remove edge
                    GorkEdge edge = element as GorkEdge;
                    
                    if (edge != null)
                    {
                        // Get both Gork Ports
                        GorkPort childPort = edge.input as GorkPort;
                        GorkPort parentPort = edge.output as GorkPort;

                        // Remove the connection from the Graph
                        Graph.RemoveConnection(parentPort.Node, parentPort.PortIndex, childPort.Node, childPort.PortIndex);
                    }

                    // Remove group
                    GorkGroup group = element as GorkGroup;

                    if (group != null)
                    {
                        // Remove GroupData from graph
                        if (Graph.GorkGroups.Contains(group.GroupData))
                        {
                            Undo.RecordObject(Graph, $"Removed \"{group.name}\" Group");
                            Graph.GorkGroups.Remove(group.GroupData);
                        }
                    }
                });
            }

            // Check if we have any edges to create
            if (change.edgesToCreate != null)
            {
                // Loop through all of the edges to create
                change.edgesToCreate.ForEach(edge =>
                {
                    // Get both Gork Ports
                    GorkPort childPort = edge.input as GorkPort;
                    GorkPort parentPort = edge.output as GorkPort;

                    // Add the connection to the Graph
                    Graph.AddConnection(parentPort.Node, parentPort.PortIndex, childPort.Node, childPort.PortIndex);
                });
            }

            return change;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Add seperator if the menu already has some elements
            if (evt.menu.MenuItems().Count > 0)
            {
                evt.menu.AppendSeparator();
            }

            //base.BuildContextualMenu(evt);
            
            Vector2 graphMousePos = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            Vector2 guiMousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            // Add a few list which we will populate later
            List<GorkNodeView> nodes = null;
            List<GorkEdge> edges = null;
            List<GorkGroup> groups = null;

            // Loop through all the selected elements
            foreach (GraphElement element in selection)
            {
                // Store the element as a GorkNodeView
                GorkNodeView node = element as GorkNodeView;

                // Check if the cast worked
                if (node != null)
                {
                    // Create the node list if it's not null
                    if (nodes == null)
                    {
                        nodes = new List<GorkNodeView>();
                    }

                    // Add the node to the nodes list
                    nodes.Add(node);
                }

                // Store the element as a GorkGroup
                GorkGroup group = element as GorkGroup;

                // Check if the cast worked
                if (groups != null)
                {
                    // Create the group list if it's not null
                    if (groups == null)
                    {
                        groups = new List<GorkGroup>();
                    }

                    // Add the group to the groups list
                    groups.Add(group);
                }

                // Don't bother with edges if there are already some selected nodes
                if (nodes != null)
                {
                    continue;
                }

                // Store the element as a GorkEdge
                GorkEdge edge = element as GorkEdge;

                // Check if the cast worked
                if (edge != null)
                {
                    // Create the edge list if it's not null
                    if (edges == null)
                    {
                        edges = new List<GorkEdge>();
                    }

                    // Add the node to the nodes list
                    edges.Add(edge);
                }
            }

            // This will only add a seperator if any of the lists aren't null
            void AddNodeSeperator()
            {
                if (nodes != null || edges != null)
                {
                    evt.menu.AppendSeparator();
                }
            }

            // Add seperator
            AddNodeSeperator();

            // Add options for gork node view
            if (nodes != null)
            {
                // Add options to remove connection(s)
                evt.menu.AppendAction("Disconnect Input Ports", _ => nodes.ForEach(n => n.DisconnectInputPorts()));
                evt.menu.AppendAction("Disconnect Output Ports", _ => nodes.ForEach(n => n.DisconnectOutputPorts()));
                evt.menu.AppendAction("Disconnect All", _ => nodes.ForEach(n => n.DisconnectAll()));

                // Add a seperator
                evt.menu.AppendSeparator();

                // Add options to delete the gork node(s)
                evt.menu.AppendAction("Delete", _ => nodes.ForEach(n => n.Delete()));
                //evt.menu.AppendAction("Cut", _ => nodes.ForEach(n => n.Delete()));
            }
            // Add options for gork edge
            else if (edges != null)
            {
                // Add option to delete edge(s)
                evt.menu.AppendAction("Delete", _ => edges.ForEach(e => e.Delete()));
                evt.menu.AppendAction("Print", _ => edges.ForEach(e => Debug.Log($"{e} | Input: {(e.input.node as GorkNodeView).Node.name} | Output: {(e.output.node as GorkNodeView).Node.name}")));
            }

            // Add seperator (Again)
            AddNodeSeperator();

            // Add option to create node
            evt.menu.AppendAction("Create Node", _ =>
            {
                NodeCreationContext context = new NodeCreationContext() { screenMousePosition = guiMousePos };

                _searchWindow.Position = graphMousePos;
                nodeCreationRequest.Invoke(context);
            });

            // Add option to create a group
            evt.menu.AppendAction("Create Group", _ =>
            {
                // Create new GorkGroup
                GorkGroup group = new GorkGroup("New Group", graphMousePos, this);

                // Add to graph view
                AddElement(group);

                // Add group data to graph
                Undo.RecordObject(Graph, $"Added New Group");
                Graph.GorkGroups.Add(group.GroupData);

                // Add selected nodes to group
                if (nodes != null)
                {
                    nodes.ForEach(n => group.AddElement(n));
                }
            });
        }

        public override List<Port> GetCompatiblePorts(Port thisPort, NodeAdapter nodeAdapter)
        {
            Node node = thisPort.node;
            Direction direction = thisPort.direction;
            List<Port> compatiblePorts = new List<Port>();
            Type thisType = thisPort.portType;

            foreach (Port port in ports)
            {
                if (port.node == node || port.direction == direction)
                {
                    continue;
                }

                Type type = port.portType;

                if (thisType != type)
                {
                    MethodInfo method = nodeAdapter.GetTypeAdapter(thisType, type);

                    if (method == null)
                    {
                        continue;
                    }
                }

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        public GorkNodeView CreateNode(Type nodeType, Vector2 position)
        {
            if (nodeType == null)
            {
                return null;
            }

            GorkNode node = Graph.CreateNode(nodeType);
            node.Position = position;

            return CreateNodeView(node);
        }

        private GorkNodeView CreateNodeView(GorkNode node)
        {
            if (node == null)
            {
                return null;
            }

            GorkNodeView nodeView = new GorkNodeView(node, this);

            AddElement(nodeView);

            return nodeView;
        }

        #region Reading & Writing PasteData
        /// <summary>
        /// Copies all the currently selected <see cref="GorkNodeView"/> and writes them to the <see cref="EditorGUIUtility.systemCopyBuffer"/>. <para/>
        /// Will also return the currently selected nodes in case you want to use those.
        /// </summary>
        private List<GorkNodeView> CopySelection(List<GorkNodeView> selectedElements = null)
        {
            // Create own list if the provided list is nonexistent
            if (selectedElements == null)
            {
                selectedElements = new List<GorkNodeView>();

                // Populate the list with our selected objects
                // Also auto fail if we have nothing selected
                if (selection.Count <= 0)
                {
                    return selectedElements;
                }

                // Loop through all selected nodes
                foreach (GraphElement element in selection)
                {
                    GorkNodeView node = element as GorkNodeView;

                    if (node != null)
                    {
                        // Add the node to the list because the cast succeeded
                        selectedElements.Add(node);
                    }
                }
            }

            // No nodes = no copy
            if (selectedElements.Count <= 0)
            {
                return selectedElements;
            }

            // Create a list of PasteData which we are going to write to our clipboard by converting to json
            PasteData data = new PasteData();

            // Loop through all of our nodes
            foreach (GorkNodeView node in selectedElements)
            {
                // Add paste data
                data.Nodes.Add(node.GetPasteData());
            }

            // Set the copy buffer to the json of the string list
            EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(data);

            return selectedElements;
        }

        /// <summary>
        /// Pastes the <see cref="PasteData"/> on the <see cref="EditorGUIUtility.systemCopyBuffer"/>. Will do nothing if the <see cref="PasteData"/> conversion failed.
        /// </summary>
        private void Paste(Vector2 position)
        {
            Debug.Log(position);

            PasteData data;

            // Try to conver to json
            try
            {
                data = JsonUtility.FromJson<PasteData>(EditorGUIUtility.systemCopyBuffer);
            }
            catch (Exception)
            {
                Debug.Log("FAIL :(");
                return;
            }

            Debug.Log("Success!!");

            List<GorkNodeView> nodes = new List<GorkNodeView>();

            Vector2 meanPosition = Vector2.zero;
            int meanPosCount = 0;

            foreach (GorkNodeView.PasteData nodeData in data.Nodes)
            {
                // Get the type
                Type nodeType = Type.GetType(nodeData.NodeType);

                // Ignore this if the type getting was unsuccessful
                if (nodeType == null)
                {
                    continue;
                }

                // Create a new node
                GorkNodeView nodeView = CreateNode(nodeType, nodeData.Position);
                nodeView.LoadFromFieldData(nodeData.FieldData);

                nodes.Add(nodeView);

                // Add to the mean position
                meanPosition += nodeData.Position;
                meanPosCount++;
            }

            ClearSelection();

            meanPosition /= meanPosCount;

            Vector2 offset = meanPosition - position;

            foreach (GorkNodeView nodeView in nodes)
            {
                nodeView.Node.Position += offset;
                nodeView.UpdatePosition(nodeView.Node.Position);

                AddToSelection(nodeView);
            }
        }

        [Serializable]
        public class PasteData
        {
            public List<GorkNodeView.PasteData> Nodes = new List<GorkNodeView.PasteData>();
            public List<GorkGroup.PasteData> Groups = new List<GorkGroup.PasteData>();
            public List<GorkEdge.PasteData> Edges = new List<GorkEdge.PasteData>();
        }
        #endregion
    }
}
