using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// マークのパラメーター（キーと値）を編集するダイアログです。
    /// </summary>
    public partial class EditParameterViewModel(Dictionary<string, string> parameters, string[] existKeys) : DialogViewModel
    {
        readonly HashSet<string> existKeys = [.. existKeys];

        public ObservableCollection<ParameterItem> Items { get; } = [.. parameters.Select(p => new ParameterItem { Key = p.Key, Value = p.Value })];

        public Dictionary<string, string> Parameters
        {
            get { return Items.ToDictionary(item => item.Key ?? "", item => item.Value ?? ""); }
        }

        // 使われていない名前（Key1, Key2…）で足す
        [RelayCommand]
        private void Add()
        {
            var keys = new HashSet<string>(Items.Select(i => i.Key));
            int index = 1;
            string name;
            while (true)
            {
                name = String.Format("Key{0}", index);
                if (!keys.Contains(name) && !existKeys.Contains(name))
                {
                    break;
                }
                index++;
            }
            Items.Add(new ParameterItem { Key = name, Value = "" });
        }

        /// <summary>
        /// 選んだ項目（DataGrid の SelectedItems）を消します。
        /// </summary>
        [RelayCommand]
        private void Remove(IList selectedItems)
        {
            foreach (var item in selectedItems.OfType<ParameterItem>().ToArray())
            {
                Items.Remove(item);
            }
        }

        protected override bool Validate()
        {
            var keys = new HashSet<string>();
            if (Items.Any(item => !keys.Add(item.Key ?? "")))
            {
                ShowMessage(Texts["DuplicateKeyError"]);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// パラメーターの 1 つです。
    /// </summary>
    public class ParameterItem
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
