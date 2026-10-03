using PPDEditorCommon;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditor
{
    /// <summary>
    /// パラメーターのプリセットを入れるフォルダーです。プロパティのドックとタイムラインのメニューで同じものを使う。
    /// </summary>
    public class ParameterPresetFolder(string name)
    {
        public string Name
        {
            get;
        } = name;

        public List<ParameterPresetFolder> Folders
        {
            get;
        } = [];

        public List<ParameterPreset> Presets
        {
            get;
        } = [];

        /// <summary>
        /// 下のフォルダーも含めて、すべてのプリセットを返す。
        /// </summary>
        public IEnumerable<ParameterPreset> GetAllPresets()
        {
            return Folders.SelectMany(f => f.GetAllPresets()).Concat(Presets);
        }
    }
}
