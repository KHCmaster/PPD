using FlowScriptControl.Classes;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    [Serializable]
    public class CustomTreeNode : TreeNode, ISerializable
    {
        public IToolTipText CustomToolTipText
        {
            get;
            private set;
        }

        public FlowSourceDumper Dumper
        {
            get;
            private set;
        }

        public bool IsFolder
        {
            get
            {
                return Dumper == null;
            }
        }

        public CustomTreeNode Data
        {
            get;
            private set;
        }

        public CustomTreeNode(IToolTipText toolTipText, string text)
            : base(text)
        {
            CustomToolTipText = toolTipText;
        }

        public CustomTreeNode(FlowSourceDumper dumper, string text) :
            base(text)
        {
            Dumper = dumper;
            CustomToolTipText = dumper;
        }

        public override object Clone()
        {
            CustomTreeNode ret = Dumper == null ? new CustomTreeNode(CustomToolTipText, Text) : new CustomTreeNode(Dumper, Text);
            ret.Name = Name;
            return ret;
        }

        #region ISerializable メンバー

        protected CustomTreeNode(SerializationInfo info, StreamingContext context)
        {
            var address = (IntPtr)info.GetValue("dataAddress", typeof(IntPtr));
            var handle = GCHandle.FromIntPtr(address);
            Data = (CustomTreeNode)handle.Target;
            handle.Free();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var handle = GCHandle.Alloc(this);
            var address = GCHandle.ToIntPtr(handle);
            info.AddValue("dataAddress", address);
        }

        #endregion
    }
}
