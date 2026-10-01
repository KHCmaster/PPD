using PPDMultiCommon.Data;
using System;
using System.Collections.Generic;

namespace PPDMulti
{
    class ItemUseManager
    {
        private readonly List<ItemType> autoUseItemTypes;

        private static readonly ItemUseManager manager;

        public static ItemUseManager Manager
        {
            get
            {
                return manager;
            }
        }

        static ItemUseManager()
        {
            manager = new ItemUseManager();
        }

        private ItemUseManager()
        {
            autoUseItemTypes = [];
            foreach (string split in SkinSetting.Setting.AutoUseItemTypes.Split(','))
            {
                if (int.TryParse(split, out int result))
                {
                    autoUseItemTypes.Add((ItemType)result);
                }
            }
        }

        private void Save()
        {
            string[] array = new string[autoUseItemTypes.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ((int)autoUseItemTypes[i]).ToString();
            }

            SkinSetting.Setting.AutoUseItemTypes = String.Join(",", array);
        }

        public ItemType[] AutoUseItemTypes
        {
            get
            {
                return [.. autoUseItemTypes];
            }
        }

        public void ToggleAutoUse(ItemType itemType)
        {
            if (autoUseItemTypes.Contains(itemType))
            {
                autoUseItemTypes.Remove(itemType);
                Save();
            }
            else
            {
                autoUseItemTypes.Add(itemType);
                Save();
            }
        }

        public bool IsAutoUse(ItemType itemType)
        {
            return autoUseItemTypes.Contains(itemType);
        }
    }
}
