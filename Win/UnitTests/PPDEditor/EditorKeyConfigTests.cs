using PPDEditor;
using PPDFramework;
using System.IO;
using System.Linq;
using Vortice.DirectInput;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターのキー設定（KeyConfigManager）の読み書きのテストです。
    /// </summary>
    public class EditorKeyConfigTests
    {
        [Fact]
        public void MissingFile_HasDefault()
        {
            using var folder = new TempFolder();
            var manager = new KeyConfigManager();

            manager.Load(folder.Combine("none.xml"));

            var config = Assert.Single(manager.Configs);
            Assert.Equal("default", config.Name);
            Assert.All(config.Keys, k => Assert.Equal(Key.Unknown, k));
            Assert.All(config.Buttons, b => Assert.Equal(-1, b));
        }

        [Fact]
        public void Xml_RoundTripAndSelectByName()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("keys.xml");
            var manager = new KeyConfigManager();
            manager.Load(path);
            manager.CurrentConfig.Name = "設定";
            manager.CurrentConfig.SetKeyMap(ButtonType.Circle, Key.D);
            manager.CurrentConfig.SetButtonMap(ButtonType.Circle, 3);
            manager.Write(path);

            var loaded = new KeyConfigManager();
            loaded.Load(path);

            Assert.Same(loaded[0], loaded.CurrentConfig);
            Assert.Equal("設定", loaded.CurrentConfig.Name);
            Assert.Equal(Key.D, loaded.CurrentConfig.GetKeyMap(ButtonType.Circle));
            Assert.Equal(3, loaded.CurrentConfig.Buttons[(int)ButtonType.Circle]);
            Assert.Equal(Key.D, loaded.CurrentConfig.Keys[(int)ButtonType.Circle]);
            // 変えたら配列も作り直す
            loaded.CurrentConfig.SetKeyMap(ButtonType.Circle, Key.F);
            Assert.Equal(Key.F, loaded.CurrentConfig.Keys[(int)ButtonType.Circle]);

            loaded.ChangeSettingFromName("ない");
            Assert.Equal(0, loaded.CurrentConfigIndex);
            loaded.CurrentConfigIndex = 5;
            Assert.Equal(0, loaded.CurrentConfigIndex);
        }

        [Fact]
        public void Xml_SeveralConfigsAndUnknownTypes()
        {
            using var folder = new TempFolder();
            var path = folder.Write("keys.xml", """
                <root>
                  <KeyConfigs>
                    <KeyConfig Name="a"><Config Type="Square" Key="30" Button="1" /></KeyConfig>
                    <KeyConfig Name="b"><Config Type="Nothing" Key="31" Button="2" /><Config Type="Cross" Key="32" Button="3" /></KeyConfig>
                  </KeyConfigs>
                </root>
                """);
            var manager = new KeyConfigManager();

            manager.Load(path);

            Assert.Equal(["a", "b"], manager.Configs.Select(c => c.Name));
            manager.ChangeSettingFromName("b");
            Assert.Equal(1, manager.CurrentConfigIndex);
            Assert.Equal(((Key)32, 3), (manager.CurrentConfig.GetKeyMap(ButtonType.Cross), manager.CurrentConfig.GetButtonMap(ButtonType.Cross)));
            Assert.Equal(Key.Unknown, manager.CurrentConfig.GetKeyMap(ButtonType.Square));
            Assert.Equal((Key)30, manager[0].GetKeyMap(ButtonType.Square));
        }

        [Fact]
        public void OldIniFormat_IsReadAndCanBeOverwritten()
        {
            using var folder = new TempFolder();
            // 古い形式（1 行に キー:ボタン）
            var path = folder.Write("keys.ini", "30:1\r\n31:2\r\nbad\r\n");
            var manager = new KeyConfigManager();

            manager.Load(path);

            var config = Assert.Single(manager.Configs);
            Assert.Equal(((Key)30, 1), (config.GetKeyMap(ButtonUtility.Array[0]), config.GetButtonMap(ButtonUtility.Array[0])));
            Assert.Equal(((Key)31, 2), (config.GetKeyMap(ButtonUtility.Array[1]), config.GetButtonMap(ButtonUtility.Array[1])));
            Assert.Equal(Key.Unknown, config.GetKeyMap(ButtonUtility.Array[2]));

            // 読んだファイルに、新しい形式で書き直せる
            manager.Write(path);
            var loaded = new KeyConfigManager();
            loaded.Load(path);
            Assert.Equal((Key)31, loaded.CurrentConfig.GetKeyMap(ButtonUtility.Array[1]));
            File.Delete(path);
        }
    }
}
