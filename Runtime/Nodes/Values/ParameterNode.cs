#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Will give out a Gork Parameter value to connected nodes via a <see cref="string"/> value for Parameter name.
    /// </summary>
    [GorkNodeInfo("Parameter/Float Parameter", GorkColors.FLOAT_COLOR, 0)]
    [GorkNodeInfo("Parameter/Int Parameter", GorkColors.INT_COLOR, 1)]
    [GorkNodeInfo("Parameter/Bool Parameter", GorkColors.BOOL_COLOR, 2)]
    [GorkNodeInfo("Parameter/String Parameter", GorkColors.STRING_COLOR, 3)]
    [NoInputPorts]
    public class ParameterNode : GorkNode
    {
        public string ParameterName = "Parameter Name";

        protected override void BuildOutputTypesList(List<Type> list)
        {
            switch (AttributeID)
            {
                default:
                    list.Add(typeof(float));
                    break;

                case 1:
                    list.Add(typeof(int));
                    break;

                case 2:
                    list.Add(typeof(bool));
                    break;

                case 3:
                    list.Add(typeof(string));
                    break;
            }
        }

#if UNITY_EDITOR
        public override void Initialize(Node node)
        {
            TextField field = new TextField();
            field.value = ParameterName;
            field.RegisterValueChangedCallback(data =>
            {
                Undo.RecordObject(this, $"Modified Property in {name}");
                ParameterName = data.newValue;
            });

            node.inputContainer.Add(field);
            node.RefreshExpandedState();
        }

        public override void OnViewEnable()
        {
            Type type;

            switch (AttributeID)
            {
                default:
                    type = typeof(float);
                    break;

                case 1:
                    type = typeof(int);
                    break;

                case 2:
                    type = typeof(bool);
                    break;

                case 3:
                    type = typeof(string);
                    break;
            }

            SetOutputPort(0, "Value", type, false);
            UpdateNodeView();
        }
#endif

        public override object ValueCall(int port, Type type)
        {
            return GetParameter(ParameterName, type);
        }
    }
}
