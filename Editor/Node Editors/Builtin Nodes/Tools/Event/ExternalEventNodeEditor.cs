using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomGorkNodeEditor(typeof(ExternalEventNode), Inherit = true)]
    public class ExternalEventNodeEditor : EventNodeEditor
    {
        public override GorkGraph.Event.Type EventType => GorkGraph.Event.Type.External;

        protected override void Draw(List<VisualElement> elements)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnExpand()
        {
            outputContainer.Add(IMGUIContainer);
        }

        public override void OnCollapse()
        {
            outputContainer.Remove(IMGUIContainer);
        }
    }
}
