using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor でプロジェクトを作って使うテストの、起動と共通の操作です。
    /// </summary>
    /// <remarks>
    /// 音は、起動する前に作業のフォルダに無音の WAV を作って使う。プロジェクトは作業のフォルダの Projects に作る。
    /// ドックは、ゲームの画面を真ん中、タイムラインを下（高さ 300）、プロパティを右に置き、テストで使うドックは真ん中の右に（上下に分けて）大きく置く。
    /// 小さくドックしていると、中を探すのに時間がかかり、不安定になるため。
    /// 浮かせると、AvalonDock は中身を別のウィンドウに入れるので、UI Automation から中が見えない。
    /// </remarks>
    public abstract class PPDEditorProjectTestBase : IDisposable
    {
        protected const string ProjectName = "uitest";
        protected string workDirectory;
        protected readonly AppSession session;

        /// <param name="largeDocks">真ん中の右に大きく置くドック（配置のファイルの名前。PPDEditor.ResourceManager など）。</param>
        protected PPDEditorProjectTestBase(params string[] largeDocks)
        {
            session = AppSession.Start(Path.Combine("PPDEditor", "PPDEditor"), "PPDEditor.exe", new AppStartOptions
            {
                ExcludedFiles = ["Projects", "projectcache.dat", "PPDFramework.setting.db", "PPDUpdater.ini", "dockforminfo.xml"],
                MainWindowTitle = "PPDEditor",
                StartupTimeout = TimeSpan.FromMinutes(1),
                Prepare = directory =>
                {
                    workDirectory = directory;
                    File.WriteAllText(Path.Combine(directory, "dockforminfo.xml"), CreateLayout(largeDocks));
                    WriteSilentWave(Path.Combine(directory, "sound.wav"), seconds: 3);
                },
            });
            try
            {
                // 作者名がないときは、起動したときに入力のダイアログが出るので閉じる
                session.ModalWindowsOf().FirstOrDefault()?.Close();
            }
            catch
            {
                session.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            session.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// ドックの配置のファイル（DockPanel Suite の形式）を作ります。
        /// </summary>
        static string CreateLayout(string[] largeDocks)
        {
            string[] fixedDocks = ["PPDEditor.DXForm", "PPDEditor.TimeLineForm", "PPDEditor.InfoForm"];
            var all = fixedDocks.Concat(largeDocks).ToArray();
            var xml = new StringBuilder();
            xml.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
            xml.AppendLine("""<DockPanel FormatVersion="1.0" DockLeftPortion="0.25" DockRightPortion="300" DockTopPortion="0.25" DockBottomPortion="300" ActiveDocumentPane="0" ActivePane="-1">""");
            xml.AppendLine($"""<Contents Count="{all.Length}">""");
            for (var i = 0; i < all.Length; i++)
            {
                xml.AppendLine($"""<Content ID="{i}" PersistString="{all[i]}" AutoHidePortion="0.25" IsHidden="False" IsFloat="False" />""");
            }
            xml.AppendLine("</Contents>");
            // ペインは、内容と同じ番号にする（0 はゲームの画面、1 はタイムライン、2 はプロパティ、3 から後はテストで使うドック）
            string[] states = ["Document", "DockBottom", "DockRight"];
            xml.AppendLine($"""<Panes Count="{all.Length}">""");
            for (var i = 0; i < all.Length; i++)
            {
                var state = i < states.Length ? states[i] : "Document";
                xml.AppendLine($"""<Pane ID="{i}" DockState="{state}" ActiveContent="{i}"><Contents Count="1"><Content ID="0" RefID="{i}" /></Contents></Pane>""");
            }
            xml.AppendLine("</Panes>");
            xml.AppendLine("<DockWindows>");
            xml.Append("""<DockWindow ID="0" DockState="Document" ZOrderIndex="0">""");
            xml.Append($"""<NestedPanes Count="{1 + largeDocks.Length}"><Pane ID="0" RefID="0" PrevPane="-1" Alignment="Right" Proportion="0.5" />""");
            for (var i = 0; i < largeDocks.Length; i++)
            {
                // 1 つ目はゲームの画面の右に、2 つ目からは前のドックの下に
                var alignment = i == 0 ? "Right" : "Bottom";
                xml.Append($"""<Pane ID="{i + 1}" RefID="{i + 3}" PrevPane="{i}" Alignment="{alignment}" Proportion="0.5" />""");
            }
            xml.AppendLine("</NestedPanes></DockWindow>");
            xml.AppendLine("""<DockWindow ID="1" DockState="DockBottom" ZOrderIndex="1"><NestedPanes Count="1"><Pane ID="0" RefID="1" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes></DockWindow>""");
            xml.AppendLine("""<DockWindow ID="2" DockState="DockRight" ZOrderIndex="2"><NestedPanes Count="1"><Pane ID="0" RefID="2" PrevPane="-1" Alignment="Right" Proportion="0.5" /></NestedPanes></DockWindow>""");
            xml.AppendLine("</DockWindows>");
            xml.AppendLine("</DockPanel>");
            return xml.ToString();
        }

        /// <summary>
        /// 無音の WAV（16 ビット、モノラル、22050 Hz）を書きます。
        /// </summary>
        static void WriteSilentWave(string path, int seconds)
        {
            const int sampleRate = 22050;
            var dataLength = sampleRate * 2 * seconds;
            using var writer = new BinaryWriter(File.Create(path));
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataLength);
            writer.Write(new byte[dataLength]);
        }

        protected string ProjectFile => Path.Combine(workDirectory, "Projects", ProjectName + ".ppdproj");

        protected string ProjectDirectory => Path.Combine(workDirectory, "Projects", ProjectName);

        protected string LayerFile => Path.Combine(ProjectDirectory, "Base_layers", "layer0.ppd");

        protected void CreateProject()
        {
            AppSession.InvokeAsync(session.FindMenuItem("fileMenu", "NewProjectMenuItem"));
            var dialog = session.WaitForModal();
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("ProjectNameBox")).AsTextBox().Text = ProjectName;
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("ProjectMovieBox")).AsTextBox().Text = Path.Combine(workDirectory, "sound.wav");
            var ok = dialog.FindFirstDescendant(cf => cf.ByAutomationId("NewProjectOkButton")).AsButton();
            Assert.True(AppSession.WaitUntil(() => ok.IsEnabled), "OK を押せません");
            AppSession.InvokeAsync(ok);
            Assert.True(session.WaitForNoModal(), "ダイアログが閉じません: " + session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => File.Exists(ProjectFile)), "プロジェクトができません");
        }

        /// <summary>
        /// 「プロジェクトを開く」で、プロジェクトを開き直します。
        /// </summary>
        protected void ReopenProject()
        {
            AppSession.InvokeAsync(session.FindMenuItem("fileMenu", "OpenProjectMenuItem"));
            FileDialogs.Accept(session.WaitForFileDialog(), ProjectFile);
            Assert.True(session.WaitForNoModal(), "開けません: " + session.DescribeModalWindows());
        }

        /// <summary>
        /// 動画（音）を少し再生して一時停止し、今の時間を進めます。
        /// </summary>
        /// <remarks>
        /// タイムラインを押すほうが速いが、大きなドックに表があると、ウィンドウ全体からタイムラインを探すのに時間がかかる。
        /// 動画のメニューは上のほうにあるので、すぐに見つかる。
        /// </remarks>
        protected void PlayALittle()
        {
            session.FindMenuItem("movieMenu", "PlayMenuItem").Invoke();
            System.Threading.Thread.Sleep(1000);
            session.FindMenuItem("movieMenu", "PauseMenuItem").Invoke();
        }

        protected void Save()
        {
            var before = File.GetLastWriteTimeUtc(ProjectFile);
            session.FindMenuItem("fileMenu", "OverwriteProjectMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => File.GetLastWriteTimeUtc(ProjectFile) > before), "保存されません");
        }

        /// <summary>
        /// 保存したプロジェクトの値です（[キー]値 の行）。
        /// </summary>
        protected string ProjectValue(string key)
        {
            var line = File.ReadAllLines(ProjectFile).FirstOrDefault(l => l.StartsWith("[" + key + "]", StringComparison.Ordinal));
            return line?[(key.Length + 2)..];
        }
    }
}
