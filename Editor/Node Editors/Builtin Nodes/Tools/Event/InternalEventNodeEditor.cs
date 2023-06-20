using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomGorkNodeEditor(typeof(InternalEventNode), Inherit = true)]
    public class InternalEventNodeEditor : EventNodeEditor
    {
        public override GorkGraph.Event.Type EventType => GorkGraph.Event.Type.Internal;

        protected override void Draw(List<VisualElement> elements)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnExpand()
        {
            inputContainer.Add(IMGUIContainer);
        }

        public override void OnCollapse()
        {
            inputContainer.Remove(IMGUIContainer);
        }
    }
}
