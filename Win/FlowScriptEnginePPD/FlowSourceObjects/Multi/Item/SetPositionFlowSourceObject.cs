using PPDFramework;
using System.Numerics;
using PPDMath;

namespace FlowScriptEnginePPD.FlowSourceObjects.Multi.Item
{
    [ToolTipText("Multi_Item_SetPosition_Summary")]
    public partial class SetPositionFlowSourceObject : ExecutableFlowSourceObject
    {
        GameComponent itemComponent;

        public override string Name
        {
            get { return "PPD.Multi.Item.SetPosition"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Manager.Items.TryGetValue("MultiItemComponent", out object value))
            {
                itemComponent = value as GameComponent;
            }
        }

        [ToolTipText("Multi_Item_SetPosition_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (itemComponent != null)
            {
                SetValue(nameof(Position));
                itemComponent.Position = Position;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
