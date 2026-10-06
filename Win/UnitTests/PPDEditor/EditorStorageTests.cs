using PPDEditor.FlowScript;
using System.IO;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターのスクリプトが値を残す場所（Storage、PPDEditor_Storage.xml）のテストです。今のフォルダーを一時フォルダーにする。
    /// </summary>
    public class EditorStorageTests
    {
        [Fact]
        public void Values_AreSavedOnlyWhenChanged()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var storage = new Storage();
            Assert.Null(storage.GetValue("script", "key"));

            // 変わらなければ保存しない
            storage.Save();
            Assert.False(File.Exists("PPDEditor_Storage.xml"));
            storage.SetValue("script", "key", "value");
            storage.SetValue("other", "key", "other");
            Assert.True(storage.IsChanged);
            storage.Save();
            Assert.False(storage.IsChanged);
            storage.SetValue("script", "key", "value");
            Assert.False(storage.IsChanged);

            // 保存したものを次に読む（保存の鍵ごとに分ける）
            var loaded = new Storage();
            Assert.Equal(("value", "other"), (loaded.GetValue("script", "key"), loaded.GetValue("other", "key")));
            Assert.Null(loaded.GetValue("script", "other"));
        }
    }
}
