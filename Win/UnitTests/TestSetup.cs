using System.Runtime.CompilerServices;

namespace UnitTests
{
    /// <summary>
    /// テストを始める前の準備です。
    /// </summary>
    static class TestSetup
    {
        /// <summary>
        /// ゲームの設定（PPDSetting）を、PPD.ini のない一時フォルダーで先に作っておきます。
        /// </summary>
        /// <remarks>
        /// 設定は初めて使ったときに、今のフォルダーの PPD.ini から作られる。後で作られると、そのときに今のフォルダーが
        /// ほかのテストの一時フォルダー（テスト用の PPD.ini がある）になっていることがあり、判定のずれ（adjustgaptime）などが
        /// 残って、ゲームの判定のテストがたまに失敗した。
        /// </remarks>
        [ModuleInitializer]
        internal static void CreateDefaultSetting()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            PPDFramework.PPDSetting.Initialize();
        }
    }
}
