namespace PPDShared
{
    /// <summary>
    /// リリースで配る PPD のフォルダー（Data\PPD）のファイルです。
    /// PPDReleaser がこの一覧でファイルをコピーしてバージョンの一覧（assembly_x64.xml）を作り、
    /// PPD は同じ一覧で、インストールしたファイルのバージョンを確かめる。
    /// </summary>
    /// <remarks>
    /// .NET では、実行ファイル（exe）は起動するためだけのもので、本体は同じ名前の dll にある。
    /// 起動するときに deps.json に書かれた dll がすべてあるかを確かめるので、deps.json に合わせて入れる。
    /// </remarks>
    static class ReleaseFiles
    {
        /// <summary>
        /// アプリの名前です（名前.exe、名前.dll、名前.runtimeconfig.json、名前.deps.json、名前.dll.config がある）。
        /// </summary>
        public static readonly string[] Applications =
        [
            "Effect2DEditor",
            "FlowScriptControlTest",
            "KeyConfiger",
            "PPD",
            "PPDConfig",
            "PPDEditor",
            "PPDExpansion",
            "PPDMultiServerService",
            "PPDUpdater",
        ];

        /// <summary>
        /// アプリの本体以外の、バージョンのあるファイルです。
        /// </summary>
        private static readonly string[] libraries =
        [
            "BezierCaliculator.dll",
            "BezierDrawControl.dll",
            "BlueSky.dll",
            "ColorCanvas.dll",
            "DirectShow.dll",
            "Effect2D.dll",
            "ErrorHandle.dll",
            "FlowScriptControl.dll",
            "FlowScriptDrawControl.dll",
            "FlowScriptEngine.dll",
            "FlowScriptEngineBasic.dll",
            "FlowScriptEngineBasicExtension.dll",
            "FlowScriptEngineConsole.dll",
            "FlowScriptEngineData.dll",
            "FlowScriptEnginePPD.dll",
            "FlowScriptEnginePPDEditor.dll",
            "FlowScriptEngineSlimDX.dll",
            "PPDConfiguration.dll",
            "PPDCore.dll",
            "PPDCoreModel.dll",
            "PPDEditorCommon.dll",
            "PPDExpansionCore.dll",
            "PPDFramework.dll",
            "PPDFrameworkCore.dll",
            "PPDInput.dll",
            "PPDMath.dll",
            "PPDMovie.dll",
            "PPDMultiCommon.dll",
            "PPDMultiServer.dll",
            "PPDPack.dll",
            "PPDShareComponent.dll",
            "PPDSound.dll",
            "AvalonDock.dll",
            "AvalonDock.Core.dll",
            "AvalonDock.Serializer.Xml.dll",
            "CommunityToolkit.Mvvm.dll",
            "DotNetProjects.DataVisualization.Toolkit.dll",
            "ICSharpCode.SharpZipLib.dll",
            "MessagePack.dll",
            "MessagePack.Annotations.dll",
            "Microsoft.NET.StringTools.dll",
            "Microsoft.Xaml.Behaviors.dll",
            "Newtonsoft.Json.dll",
            "SharpGen.Runtime.dll",
            "SharpGen.Runtime.COM.dll",
            "System.Data.SQLite.dll",
            "System.ServiceProcess.ServiceController.dll",
            "Vortice.Direct2D1.dll",
            "Vortice.Direct3D11.dll",
            "Vortice.DirectInput.dll",
            "Vortice.DirectSound.dll",
            "Vortice.DirectX.dll",
            "Vortice.DXGI.dll",
            "Vortice.Mathematics.dll",
            "YamlDotNet.dll",
            "runtimes\\win-x64\\native\\SQLite.Interop.dll",
            "skins\\PPDModSettingUI.dll",
            "skins\\PPDMulti.dll",
            "skins\\PPDMultiCommon.dll",
            "skins\\PPDMultiServer.dll",
            "skins\\PPDSingle.dll",
            "dlls\\FlowScriptEngineBasic.dll",
            "dlls\\FlowScriptEngineBasicExtension.dll",
            "dlls\\FlowScriptEngineConsole.dll",
            "dlls\\FlowScriptEngineData.dll",
            "dlls\\FlowScriptEnginePPD.dll",
            "dlls\\FlowScriptEnginePPDEditor.dll",
            "dlls\\FlowScriptEngineSlimDX.dll",
        ];

        /// <summary>
        /// バージョンを確かめるファイル（アプリの exe と dll、ライブラリ）です。
        /// </summary>
        public static string[] VersionedFiles
        {
            get
            {
                var files = new System.Collections.Generic.List<string>();
                foreach (var application in Applications)
                {
                    files.Add(application + ".exe");
                    files.Add(application + ".dll");
                }
                files.AddRange(libraries);
                return [.. files];
            }
        }

        /// <summary>
        /// 一緒に配る、バージョンのないファイル（起動の設定、依存の一覧、設定ファイル）です。
        /// </summary>
        public static string[] OtherFiles
        {
            get
            {
                var files = new System.Collections.Generic.List<string>();
                foreach (var application in Applications)
                {
                    files.Add(application + ".runtimeconfig.json");
                    files.Add(application + ".deps.json");
                    files.Add(application + ".dll.config");
                }
                files.Add("MultiRoomInfo.xml");
                // PPDMultiServerService が Windows で使う版（同じ名前のファイルが上にもあるので、バージョンは確かめない）
                files.Add("runtimes\\win\\lib\\net10.0\\System.ServiceProcess.ServiceController.dll");
                return [.. files];
            }
        }
    }
}
