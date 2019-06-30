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

        public event Action<int> Progressed;

        private MarkManager markManager;
        private EventManager eventManager;
        private FlowScriptManager scriptManager;
        private float startTime;
        private float endTime;
        private int[] evaPoint;
        private float safeCoolArea;
        private float safeGoodArea;
        private bool doJust;
        private bool doPermutation;
        private bool doDumpNotes;
        private bool includeFine;
        private ButtonType[] buttons;
        private ItemType autoType;
        private ISceneBase scene;

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
            switch (autoType)
            {
                case ItemType.Auto1:
                    doJust = true;
                    doPermutation = false;
                    doDumpNotes = false;
                    break;
                case ItemType.Auto2:
                    doJust = true;
                    doPermutation = true;
                    doDumpNotes = false;
                    break;
                case ItemType.Auto3:
                    doJust = false;
                    doPermutation = true;
                    doDumpNotes = false;
                    break;
                case ItemType.Auto4:
                    doJust = false;
                    doPermutation = true;
                    doDumpNotes = true;
                    break;
            }
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