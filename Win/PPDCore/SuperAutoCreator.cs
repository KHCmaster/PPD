using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.Scene;
using PPDFramework.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PPDCore
{
    class SuperAutoCreator
    {
        enum PressingType : byte
        {
            None,
            Normal,
            AC,
        }

        /// <summary>
        /// 進捗を通知するイベントです（現在は通知していません）。
        /// </summary>
        public event Action<int> Progressed
        {
            add { }
            remove { }
        }

        private readonly MarkManager markManager;
        private readonly EventManager eventManager;
        private readonly FlowScriptManager scriptManager;
        private readonly float startTime;
        private readonly float endTime;
        private readonly int[] evaPoint;
        private readonly float safeCoolArea;
        private readonly float safeGoodArea;
        private readonly bool includeFine;
        private readonly ButtonType[] buttons;
        private readonly ItemType autoType;
        private readonly ISceneBase scene;

        public bool AllowAllButtons
        {
            get;
            set;
        }

        public bool AllowWarnScripts
        {
            get;
            set;
        }

        public SuperAutoCreator(ISceneBase scene, MarkManager markManager, EventManager eventManager, FlowScriptManager scriptManager, float startTime,
            float endTime, ItemType autoType, int[] evaPoint, bool includeFine)
        {
            this.scene = scene;
            this.markManager = markManager;
            this.eventManager = eventManager;
            this.scriptManager = scriptManager;
            this.startTime = startTime;
            this.endTime = endTime;
            this.autoType = autoType;
            this.evaPoint = evaPoint;
            this.includeFine = includeFine;
            buttons = new ButtonType[10];
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = (ButtonType)i;
            }
            safeCoolArea = PPDSetting.Setting.CoolArea - 0.001f;
            safeGoodArea = PPDSetting.Setting.GoodArea - 0.001f;
        }

        public bool CanCreate()
        {
            if (autoType == ItemType.Auto4)
            {
                if (!AllowWarnScripts)
                {
                    if (scriptManager.ContainsNode("PPD.GameResult.GainCurrentLife"))
                    {
                        MessageBox.Show(String.Format("Auto Type:{0} does not support Script PPD.GameResult.GainCurrentLife", autoType));
                        return false;
                    }
                    var ignoreNames = new string[] { "PPD.Mark.ProcessMissPressAny", "PPD.Mark.ProcessMissPressByID", "PPD.Mark.ProcessMissPressWithinTime" };
                    foreach (var ignoreName in ignoreNames)
                    {
                        if (scriptManager.ContainsNode(ignoreName))
                        {
                            MessageBox.Show(String.Format("Auto Type:{0} does not support Script {1}", autoType, ignoreName));
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public PPDPlayDecorder Create()
        {
            return PPDPlayDecorder.FromInputs(new PPDPlayInput[0]);
        }
    }
}