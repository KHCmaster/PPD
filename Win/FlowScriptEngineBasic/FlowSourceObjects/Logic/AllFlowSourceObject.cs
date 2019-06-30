using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_All_Summary", "Logic_All_Remark")]
    public partial class AllFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Logic_All_Out")]
        public event FlowEventHandler Out;
        private Dictionary<string, bool> connectDict;
        private string[] methodList = { "In0", "In1", "In2", "In3", "In4", "In5" };
        public AllFlowSourceObject()
        {
            connectDict = new Dictionary<string, bool>();
        }

        protected override void OnInitialize()
        {
            foreach (string methodName in methodList)
            {
                if (IsConnected(methodName, true))
                {
                    connectDict.Add(methodName, false);
                }
            }
        }

        protected override void OnReset()
        {
            foreach (var key in connectDict.Keys.ToArray())
            {
                connectDict[key] = false;
            }
        }

        public override string Name
        {
            get { return "Logic.All"; }
        }

        [ToolTipText("Logic_All_In0")]
        public void In0(FlowEventArgs e)
        {
            connectDict[methodList[0]] = true;
            CheckFire();
        }

        [ToolTipText("Logic_All_In1")]
        public void In1(FlowEventArgs e)
        {
            connectDict[methodList[1]] = true;
            CheckFire();
        }

        [ToolTipText("Logic_All_In2")]
        public void In2(FlowEventArgs e)
        {
            connectDict[methodList[2]] = true;
            CheckFire();
        }

        [ToolTipText("Logic_All_In3")]
        public void In3(FlowEventArgs e)
        {
            connectDict[methodList[3]] = true;
            CheckFire();
        }

        [ToolTipText("Logic_All_In4")]
        public void In4(FlowEventArgs e)
        {
            connectDict[methodList[4]] = true;
            CheckFire();
        }

        [ToolTipText("Logic_All_In5")]
        public void In5(FlowEventArgs e)
        {
            connectDict[methodList[5]] = true;
            CheckFire();
        }

        private void CheckFire()
        {
            foreach (bool b in connectDict.Values)
            {
                if (!b)
                {
                    return;
                }
            }
            FireEvent(Out);
        }

        [ToolTipText("Logic_All_Reset")]
        public void Reset(FlowEventArgs e)
        {
            OnReset();
        }
    }
}
