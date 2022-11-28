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

            // Send a signal through all StartNodes
            foreach (StartNode node in startNodes)
            {
                StartNode(node, 0);
            }
        }

        /// <summary>
        /// Starts a coroutine from the node
        /// </summary>
        public virtual void StartNode(GorkNode node, int port)
        {
            activeCoroutines.Add(node, StartCoroutine(NodeCoroutine(node, port)));
            coroutineCount++;
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

        /// <summary>
        /// A special enum which determines when a <see cref="GorkGraph"/> gets activated in a <see cref="GorkGraphReader"/>
        /// </summary>
        [System.Serializable]
        public enum GorkActivationMode
        {
            OnStart,
            OnAwake,
            WhenEnabled,
            Manual,
        }
    }
}
