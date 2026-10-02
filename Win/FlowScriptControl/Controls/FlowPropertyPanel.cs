using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptControl.Controls
{
    /// <summary>
    /// 選んだノードの入力プロパティの値を編集するパネルです。
    /// </summary>
    /// <remarks>
    /// 値は、編集を終えたとき（フォーカスが外れたとき、Enter キー、選択肢を選んだとき）にノードへ反映する。
    /// 書式が正しくない値は、エラーを表示して編集前の値に戻す。
    /// </remarks>
    public class FlowPropertyPanel : UserControl
    {
        private readonly Grid grid;
        private FlowDrawPanel currentFlowDrawPanel;

        public FlowPropertyPanel()
        {
            grid = new Grid { Margin = new Thickness(2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 250 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Content = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = grid,
            };
        }

        public void MakeEmpty()
        {
            ChangeFlowSource(null, null, null);
        }

        public void ChangeFlowSource(FlowDrawPanel flowDrawPanel, FlowSourceObjectBase source, Dictionary<string, string> data)
        {
            currentFlowDrawPanel = flowDrawPanel;
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            if (flowDrawPanel == null || source == null || data == null)
            {
                return;
            }
            try
            {
                foreach (CustomMemberInfo<PropertyInfo> propertyInfo in source.InProperties)
                {
                    AddRow(source, propertyInfo, data);
                }
            }
            catch (Exception)
            {
            }
        }

        private void AddRow(FlowSourceObjectBase source, CustomMemberInfo<PropertyInfo> propertyInfo, Dictionary<string, string> data)
        {
            var row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var name = propertyInfo.MemberInfo.Name;
            var nameText = new TextBlock
            {
                Text = name,
                ToolTip = String.IsNullOrEmpty(propertyInfo.ToolTipText) ? null : propertyInfo.ToolTipText,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2, 1, 6, 1),
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            Grid.SetRow(nameText, row);
            grid.Children.Add(nameText);

            data.TryGetValue(name, out string value);
            var formatter = TypeFormatterManager.GetFormatter(propertyInfo.MemberInfo.PropertyType);
            FrameworkElement editor;
            if (formatter == null)
            {
                // 文字列から作れない型は、編集できない
                editor = new TextBox { IsReadOnly = true, Margin = new Thickness(1) };
            }
            else if (formatter.AllowedPropertyString != null && formatter.AllowedPropertyString.Length > 0)
            {
                editor = CreateComboBox(name, formatter, value);
            }
            else if (CanValueButtonClick(source.Name, name, propertyInfo.MemberInfo.PropertyType))
            {
                editor = CreateTextBoxWithButton(source.Name, name, propertyInfo.MemberInfo.PropertyType, formatter, value);
            }
            else
            {
                var textBox = CreateTextBox(name, formatter, value);
                textBox.Margin = new Thickness(1);
                editor = textBox;
            }
            Grid.SetRow(editor, row);
            Grid.SetColumn(editor, 1);
            grid.Children.Add(editor);
        }

        private ComboBox CreateComboBox(string name, TypeFormatterBase formatter, string value)
        {
            var comboBox = new ComboBox { Margin = new Thickness(1) };
            foreach (var item in formatter.AllowedPropertyString)
            {
                comboBox.Items.Add(item);
            }
            // 選択肢にない値（以前のバージョンで保存された値など）も、そのまま表示する
            var initial = value ?? formatter.AllowedPropertyString[0];
            if (!comboBox.Items.Contains(initial))
            {
                comboBox.Items.Add(initial);
            }
            comboBox.SelectedItem = initial;
            var lastValue = value;
            comboBox.SelectionChanged += (sender, e) =>
            {
                var text = comboBox.SelectedItem as string;
                if (text != lastValue)
                {
                    lastValue = Commit(name, formatter, text, lastValue, revert => comboBox.SelectedItem = revert);
                }
            };
            return comboBox;
        }

        private TextBox CreateTextBox(string name, TypeFormatterBase formatter, string value)
        {
            var textBox = new TextBox { Text = value ?? "", VerticalContentAlignment = VerticalAlignment.Center };
            var lastValue = value ?? "";
            void commit()
            {
                if (textBox.Text != lastValue)
                {
                    lastValue = Commit(name, formatter, textBox.Text, lastValue, revert => textBox.Text = revert ?? "") ?? "";
                }
            }
            textBox.LostKeyboardFocus += (sender, e) => commit();
            textBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    commit();
                    textBox.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    textBox.Text = lastValue;
                    e.Handled = true;
                }
            };
            return textBox;
        }

        private DockPanel CreateTextBoxWithButton(string sourceName, string name, Type propertyType, TypeFormatterBase formatter, string value)
        {
            var textBox = CreateTextBox(name, formatter, value);
            var button = new Button { Content = "...", Width = 24, Margin = new Thickness(2, 0, 0, 0) };
            button.Click += (sender, e) =>
            {
                var selected = ValueButtonClicked(sourceName, name, propertyType);
                if (selected != null)
                {
                    textBox.Text = selected;
                    textBox.Focus();
                    textBox.SelectAll();
                }
            };
            var panel = new DockPanel { Margin = new Thickness(1), LastChildFill = true };
            DockPanel.SetDock(button, Dock.Right);
            panel.Children.Add(button);
            panel.Children.Add(textBox);
            return panel;
        }

        // 値をノードに反映する。反映した値（書式が正しくなければ、元に戻した値）を返す
        private string Commit(string name, TypeFormatterBase formatter, string text, string lastValue, Action<string> revert)
        {
            if (currentFlowDrawPanel == null)
            {
                return lastValue;
            }
            if (String.IsNullOrEmpty(text))
            {
                currentFlowDrawPanel.ChangeCurrentSourceProperty(name, null);
                return text;
            }
            if (formatter != null && formatter.Format(text, out _))
            {
                currentFlowDrawPanel.ChangeCurrentSourceProperty(name, text);
                return text;
            }
            MessageBox.Show(String.Format("{0}\nex)\n{1}", currentFlowDrawPanel.LanguageProvider.FormatError, formatter?.CorrentFormat));
            revert(lastValue);
            return lastValue;
        }

        /// <summary>
        /// 値の候補を選ぶボタン（...）を表示するかどうかを返します。
        /// </summary>
        protected virtual bool CanValueButtonClick(string sourceName, string propertyName, Type propertyType)
        {
            return false;
        }

        /// <summary>
        /// 値の候補を選ぶボタンが押されたときに、設定する値を返します（null のときは変更しない）。
        /// </summary>
        protected virtual string ValueButtonClicked(string sourceName, string propertyName, Type propertyType)
        {
            return null;
        }
    }
}
