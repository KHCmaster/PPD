using FlowScriptControl.Controls;
using PPDEditor.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDEditor.Controls
{
    public class CustomFlowPropertyPanel : FlowPropertyPanel
    {

        protected override bool CanValueButtonClick(string sourceName, string propertyName, Type propertyType)
        {
            if (propertyName.Contains("Time") && propertyType == typeof(float))
            {
                return true;
            }
            if (sourceName.Contains("ByID") && propertyType == typeof(int))
            {
                return true;
            }
            if (sourceName.Contains("Effect.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("Effect.Pool.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("Image.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("Number.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("Audio.Sound.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("Resource.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("Polygon.Value") && propertyName == "Path")
            {
                return true;
            }
            if (sourceName.Contains("PPDEditor.Mark.PosAndRotation") && propertyName == "FileName")
            {
                return true;
            }

            return base.CanValueButtonClick(sourceName, propertyName, propertyType);
        }

        protected override string ValueButtonClicked(string sourceName, string propertyName, Type propertyType)
        {
            if (propertyName.Contains("Time") && propertyType == typeof(float))
            {
                return ShowSuggest(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string,string>(Utility.Language["CurrentMovieTime"], ((float)WindowUtility.MainForm.CurrentTime).ToString())                });
            }
            if (sourceName.Contains("ByID") && propertyType == typeof(int))
            {
                Mark mark = WindowUtility.LayerManager.SelectedPpdSheet.SelectedMark;
                if (mark != null)
                {
                    return ShowSuggest(new KeyValuePair<string, string>[] {
                        new KeyValuePair<string,string>(Utility.Language["SelectedMarkID"], mark.ID.ToString())                    });
                }
            }
            if ((sourceName.Contains("Effect.Value") && propertyName == "Path") ||
                (sourceName.Contains("Effect.Pool.Value") && propertyName == "Path"))
            {
                var effects = WindowUtility.ResourceManager.GetEffectList();
                return ShowSuggest(effects.Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }
            if (sourceName.Contains("Image.Value") && propertyName == "Path")
            {
                var images = WindowUtility.ResourceManager.GetImageList();
                return ShowSuggest(images.Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }
            if (sourceName.Contains("Number.Value") && propertyName == "Path")
            {
                var images = WindowUtility.ResourceManager.GetImageList();
                return ShowSuggest(images.Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }
            if (sourceName.Contains("Audio.Sound.Value") && propertyName == "Path")
            {
                var sounds = WindowUtility.ResourceManager.GetSoundList();
                return ShowSuggest(sounds.Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }
            if (sourceName.Contains("Resource.Value") && propertyName == "Path")
            {
                var others = WindowUtility.ResourceManager.GetOthersList();
                return ShowSuggest(others.Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }
            if (sourceName.Contains("Polygon.Value") && propertyName == "Path")
            {
                var images = WindowUtility.ResourceManager.GetImageList();
                return ShowSuggest(images.Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }
            if (sourceName.Contains("PPDEditor.Mark.PosAndRotation") && propertyName == "FileName")
            {
                return ShowSuggest(Directory.GetFiles("posdat", "*.txt").Select(f => Path.GetFileNameWithoutExtension(f))
                    .Select(s => new KeyValuePair<string, string>(s, s)).ToArray());
            }

            return base.ValueButtonClicked(sourceName, propertyName, propertyType);
        }

        private string ShowSuggest(KeyValuePair<string, string>[] suggests)
        {
            var suggestForm = new SuggestForm();
            foreach (KeyValuePair<string, string> suggest in suggests)
            {
                suggestForm.AddSuggest(suggest.Key, suggest.Value);
            }
            suggestForm.SetLang();
            if (suggestForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return suggestForm.SelectedValue;
            }

            return null;
        }
    }
}
