using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PPDEditor
{
    public enum ShortcutType
    {
        None = 0,
        Left0,
        Left1,
        Left2,
        Left3,
        Left4,
        Left5,
        Left6,
        Left7,
        Right0,
        Right1,
        Right2,
        Right3,
        Right4,
        Right5,
        Right6,
        Right7,
        Up0,
        Up1,
        Up2,
        Up3,
        Up4,
        Up5,
        Up6,
        Up7,
        Down0,
        Down1,
        Down2,
        Down3,
        Down4,
        Down5,
        Down6,
        Down7,
        DeleteMark,
        Next,
        Previous,
        NextAll,
        PreviousAll,
        CopyAngle,
        CopyPosition,
        Fusion,
        Defusion,
        Undo,
        Redo,
        CopyMark,
        ClearCopyBuffer,
        InterpolateAngleClockwise,
        InterpolateAngleUnclockwise,
        InterpolatePosition,
        Custom = Int32.MaxValue,
    }

    public class ShortcutManager
    {
        List<ShortcutInfo> list;

        public ShortcutManager()
        {
            list = new List<ShortcutInfo>();
        }

        public void RegisterShortcut(ShortcutInfo shortcutInfo)
        {
            list.Add(shortcutInfo);
        }

        public void ClearShortcut()
        {
            list.Clear();
        }

        public ShortcutInfo GetShortcut(Keys key, bool control, bool shift, bool alt)
        {
            foreach (ShortcutInfo info in list)
            {
                if (info.Key == key && info.Shift == shift && info.Control == control && info.Alt == alt)
                {
                    return info;
                }
            }

            return null;
        }

        public ShortcutInfo[] Shortcuts
        {
            get
            {
                return list.ToArray();
            }
        }
    }
}
