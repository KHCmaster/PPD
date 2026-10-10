using PPDEditor;
using PPDEditor.Command;
using System.Collections.Generic;
using System.Windows.Forms;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの元に戻す・やり直し（CommandManager、まとめたコマンド）と、小さな部品のテストです。
    /// </summary>
    public class EditorCommandManagerTests
    {
        sealed class Recording(string name, List<string> log, CommandType type = CommandType.Pos) : Command
        {
            public override void Execute() => log.Add("do " + name);
            public override void Undo() => log.Add("undo " + name);
            public override CommandType CommandType => type;
        }

        [Fact]
        public void Group_UndoesAndRedoesTogether()
        {
            var log = new List<string>();
            var manager = new CommandManager();
            var changes = new List<CommandType>();
            manager.CommandChanged += changes.Add;

            manager.StartGroupCommand();
            manager.AddCommand(new Recording("a", log, CommandType.Pos));
            manager.AddCommand(new Recording("b", log, CommandType.Time));
            manager.EndGroupCommand();
            // 何もないまとまりは積まない
            manager.StartGroupCommand();
            manager.EndGroupCommand();

            Assert.Equal((1, 0), (manager.DoneCommandNumber, manager.UndoneCommandNumber));
            Assert.Equal(CommandType.Pos | CommandType.Time, manager.LastDoneCommand.CommandType);
            manager.Undo();
            Assert.Equal((0, 1), (manager.DoneCommandNumber, manager.UndoneCommandNumber));
            Assert.Null(manager.LastDoneCommand);
            manager.Redo();
            Assert.Equal(["do a", "do b", "undo b", "undo a", "do a", "do b"], log);
            Assert.Single(manager.DoneCommand);
            Assert.Empty(manager.UndoneCommand);
            Assert.Equal(3, changes.Count);
        }

        [Fact]
        public void NewChange_ClearsRedo()
        {
            var log = new List<string>();
            var manager = new CommandManager();
            manager.AddCommand(new Recording("a", log));
            manager.Undo();
            Assert.True(manager.CanRedo);

            // 1 つのコマンドでも、まとめたコマンドでも、新しく変えたらやり直しはできない
            manager.StartGroupCommand();
            manager.AddCommand(new Recording("b", log));
            manager.EndGroupCommand();

            Assert.False(manager.CanRedo);
            manager.Undo();
            manager.AddCommand(new Recording("c", log));
            Assert.False(manager.CanRedo);

            manager.ClearAll();
            Assert.False(manager.CanUndo || manager.CanRedo);
        }

        [Fact]
        public void ShortcutText_AndScriptPath()
        {
            Assert.Equal("Ctrl+Shift+Alt+Z", new ShortcutInfo(Keys.Z, true, true, true, ShortcutType.Undo).GetCommandText());
            Assert.Equal("F1", new ShortcutInfo(Keys.F1, false, false, false, ShortcutType.Redo).GetCommandText());
            Assert.Equal(System.IO.Path.Combine("a", "b.fsml"), new ShortcutInfo(Keys.A, false, false, false, System.IO.Path.Combine("a", "b")).ScriptPathWithExtension);
        }

        [Fact]
        public void ExDirectory_ContainsFileName()
        {
            using var folder = new TempFolder();
            folder.Write("a.wav", "");

            Assert.True(ExDirectory.Contains(folder.Path, "a.wav"));
            Assert.False(ExDirectory.Contains(folder.Path, "b.wav"));
        }
    }
}
