using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Reads gork graphs and performs the functionality for the nodes.
    /// </summary>
    public class GorkGraphReader : MonoBehaviour
    {
        [SerializeField] protected GorkGraph graph;
        [SerializeField] protected GorkActivationMode activationMode;

        public GorkGraph Graph
        {
            get => graph;
            set
            {
                if (Playing)
                {
                    StopGraph();
                }

                graph = value;
            }
        }

        protected Dictionary<GorkNode, Coroutine> activeCoroutines = new Dictionary<GorkNode, Coroutine>();
        protected int coroutineCount;

        public bool Playing { get; private set; }

        protected virtual void Start()
        {
            // Start the graph OnStart
            if (activationMode == GorkActivationMode.OnStart)
            {
                StartGraph();
            }
        }
        protected virtual void Awake()
        {
            // Start the graph OnAwake
            if (activationMode == GorkActivationMode.OnAwake)
            {
                StartGraph();
            }
        }
        protected virtual void OnEnable()
        {
            // Start the graph WhenEnabled
            if (activationMode == GorkActivationMode.WhenEnabled)
            {
                StartGraph();
            }
        }

        private void Update()
        {
            if (coroutineCount > 0 && !Playing)
            {
                Playing = true;

                OnGraphStart();
            }
            else if (coroutineCount <= 0 && Playing)
            {
                Playing = false;

                OnGraphStop();
            }

#if UNITY_EDITOR
            // DBEUG
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Restart graph
                Debug.Log("YOU ARE DEBUGIG!!! Restarting graph");

                StopGraph();
                StartGraph();
            }
#endif
        }

        public virtual void StartGraph()
        {
            List<StartNode> startNodes = graph.GetAllNodesOfType<StartNode>();

            // There are no start nodes in the Graph so we cannot start!!! :( :( ;3;
            if (startNodes == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"The GorkGraph \"{graph.name}\" doesn't contain any StartNodes so starting is impossible!", graph);
#endif
                return;
            }

            // Subscribe to the GorkGraph
            graph.OnNodeCalled += StartNode;
            graph.OnGraphStop += StopGraph;
            graph.OnNodeStop += StopNode;

            // Send a signal through all StartNodes
            foreach (StartNode node in startNodes)
            {
                StartNode(node, 0);
            }
        }

        /// <summary>
        /// Starts a coroutine from the node.
        /// </summary>
        public virtual void StartNode(GorkNode node, int port)
        {
            activeCoroutines.Add(node, StartCoroutine(NodeCoroutine(node, port)));
            coroutineCount++;
        }

        /// <summary>
        /// Stops a currently active coroutine from the node.
        /// </summary>
        public virtual void StopNode(GorkNode node)
        {
            if (!activeCoroutines.TryGetValue(node, out Coroutine coroutine))
            {
                return;
            }

            Debug.Log("Stopped " + node.name);

            StopCoroutine(coroutine);

            activeCoroutines.Remove(node);

            coroutineCount--;
        }

        public virtual void StopGraph()
        {
            graph.OnNodeCalled -= StartNode;
            graph.OnGraphStop -= StopGraph;

            // Stop all active coroutines
            foreach (var pair in activeCoroutines)
            {
                StopCoroutine(pair.Value);
            }

            activeCoroutines.Clear();

            coroutineCount = 0;
        }

        protected virtual void OnGraphStart()
        {

        }

        protected virtual void OnGraphStop()
        {
            Debug.Log("Graph stopped!");
        }

        protected virtual IEnumerator NodeCoroutine(GorkNode node, int port)
        {
            yield return node.NodeIEnumerator(port);

            activeCoroutines.Remove(node);

            coroutineCount--;
        }

        #region Parameters
        #region Has Parameter
        /// <summary>
        /// Returns if a parameter with the given <paramref name="name"/> and <paramref name="type"/> exists or not.
        /// </summary>
        public bool HasParameter(string name, Type type) => graph.HasParameter(name, type);
        /// <summary>
        /// Returns if a parameter with the given <paramref name="name"/> and <paramref name="T"/> exists or not.
        /// </summary>
        public bool HasParameter<T>(string name) => graph.HasParameter<T>(name);

        /// <summary>
        /// Returns if a <see cref="float"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasFloat(string name) => graph.HasParameter<float>(name);
        /// <summary>
        /// Returns if a <see cref="int"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasInt(string name) => graph.HasParameter<int>(name);
        /// <summary>
        /// Returns if a <see cref="bool"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasBool(string name) => graph.HasParameter<bool>(name);
        /// <summary>
        /// Returns if a <see cref="string"/> parameter with the given <paramref name="name"/> exists or not.
        /// </summary>
        public bool HasString(string name) => graph.HasParameter<string>(name);
        #endregion

        #region Get Parameter
        /// <summary>
        /// Returns the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/>.
        /// </summary>
        public object GetParameter(string name, Type type) => graph.GetParameter(name, type);
        /// <summary>
        /// Returns the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/>.
        /// </summary>
        public T GetParameter<T>(string name) => graph.GetParameter<T>(name);

        /// <summary>
        /// Returns the <see cref="float"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public float GetFloat(string name) => graph.GetParameter<float>(name);
        /// <summary>
        /// Returns the <see cref="int"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public int GetInt(string name) => graph.GetParameter<int>(name);
        /// <summary>
        /// Returns the <see cref="bool"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public bool GetBool(string name) => graph.GetParameter<bool>(name);
        /// <summary>
        /// Returns the <see cref="string"/> parameter with the given <paramref name="name"/>.
        /// </summary>
        public string GetString(string name) => graph.GetParameter<string>(name);
        #endregion

        #region Set Parameter
        /// <summary>
        /// Sets the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetParameter(string name, Type type, object value) => graph.SetParameter(name, type, value);
        /// <summary>
        /// Sets the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetParameter<T>(string name, T value) => graph.SetParameter(name, value);

        /// <summary>
        /// Sets the <see cref="float"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetFloat(string name, float value) => graph.SetParameter(name, value);
        /// <summary>
        /// Sets the <see cref="int"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetInt(string name, int value) => graph.SetParameter(name, value);
        /// <summary>
        /// Sets the <see cref="bool"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetBool(string name, bool value) => graph.SetParameter(name, value);
        /// <summary>
        /// Sets the <see cref="string"/> parameter with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        public void SetString(string name, string value) => graph.SetParameter(name, value);
        #endregion

        #region Reset Parameter
        /// <summary>
        /// Resets the parameter with the type of <paramref name="type"/> and with the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetParameter(string name, Type type) => graph.ResetParameter(name, type);
        /// <summary>
        /// Resets the parameter with the type of <paramref name="T"/> and with the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetParameter<T>(string name) => graph.ResetParameter(name, typeof(T));

        /// <summary>
        /// Resets the <see cref="float"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetFloat(string name) => graph.ResetParameter<float>(name);
        /// <summary>
        /// Resets the <see cref="int"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetInt(string name) => graph.ResetParameter<int>(name);
        /// <summary>
        /// Resets the <see cref="bool"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetBool(string name) => graph.ResetParameter<bool>(name);
        /// <summary>
        /// Resets the <see cref="string"/> parameter the given <paramref name="name"/> to it's starting value set in the GorkEditorWindow.
        /// </summary>
        public void ResetString(string name) => graph.ResetParameter<string>(name);

        /// <summary>
        /// Resets all parameters in this <see cref="GorkGraph"/> back to their starting values set in the GorkEditorWindow.
        /// </summary>
        public void ResetAllParamters() => graph.ResetAllParamters();
        #endregion
        #endregion

        /// <summary>
        /// A special enum which determines when a <see cref="GorkGraph"/> gets activated in a <see cref="GorkGraphReader"/>
        /// </summary>
        [Serializable]
        public enum GorkActivationMode
        {
            OnStart,
            OnAwake,
            WhenEnabled,
            Manual,
        }
    }
}
