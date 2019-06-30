using FlowScriptEngine;
using PPDFramework;
using PPDFramework.Vertex;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.VertexBuffer
{
    [ToolTipText("VertexBuffer_Write_Summary")]
    public partial class WriteFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.VertexBuffer.Write"; }
        }

        [ToolTipText("VertexBuffer_Write_Object")]
        public VertexInfo Object
        {
            private get;
            set;
        }

        [ToolTipText("VertexBuffer_Write_Vertices")]
        public IEnumerable<object> Vertices
        {
            private get;
            set;
        }

        [ToolTipText("VertexBuffer_Write_Offset")]
        public int Offset
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Vertices));
            if (Object != null && Vertices != null)
            {
                SetValue(nameof(Offset));
                Object.Write(Vertices.OfType<ColoredTexturedVertex>().ToArray(), Offset);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
