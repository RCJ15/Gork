using System;

namespace Gork
{
    /// <summary>
    /// Will give out a Gork Parameter value to connected nodes via a <see cref="string"/> dropdown for Parameter name.
    /// </summary>
    public abstract class ParameterNode : GorkNode
    {
        [GorkWikiInfo("The name of the parameter this node outputs.\nIs displayed like a dropdown in the editor")]
        public string ParameterName = "Parameter Name";

        public abstract Type ParameterType { get; }

        public override object ValueCall(int port, Type type)
        {
            return GetParameter(ParameterName, type);
        }
    }
}
