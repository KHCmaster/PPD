using BezierCaliculator;
using CommunityToolkit.Mvvm.Input;
using Effect2D;
using Effect2DEditor.Command;
using Effect2DEditor.Models;
using Effect2DEditor.ViewModels.Dialogs;
using System;
using System.IO;
using PointF = System.Drawing.PointF;

namespace Effect2DEditor.ViewModels
{
    /// <summary>
    /// タイムラインから使う、エディターの機能です。
    /// </summary>
    public interface ITimeLineHost
    {
        void RefreshCanvas();

        void RefreshTimeLine();

        /// <summary>
        /// 選んだステートで、変化の割合を変えられるかどうかです。
        /// </summary>
        bool IsRatioMakerAvailable { set; }

        void MoveState(int beforeframe, int newframe, TimeLine.KeyOperationMode opmode);

        void AddState(int itemindex, int keyindex, bool iskey);

        void DeleteState(int itemindex, int keyindex);

        /// <summary>
        /// エフェクトの順番を入れ替えます。
        /// </summary>
        void ChangeIndex(int index, int beforeindex);
    }

    // 編集の履歴に残す操作
    public sealed partial class MainWindowViewModel : ITimeLineHost, IPropertyHost
    {
        void CommandManager_CommandChanged(object sender, EventArgs e)
        {
            saved = false;
            EffectManager.CheckFrameLength();
            view.AdjustTimeLineHScroll();
            view.RefreshTimeLine();
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        void SelectedManager_StateChanged(object sender, StateChangeEventArgs e)
        {
            SelectState(e);
            RefreshState();
        }

        /// <summary>
        /// 履歴に残す操作を実行します。失敗したときは知らせる。
        /// </summary>
        private void Run(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                view.ShowMessage(e.Message);
            }
        }

        private void ExecuteAndAdd(CommandBase command)
        {
            command.Execute();
            CommandManager.AddCommand(command);
        }

        private static string EffectName(IEffect effect)
        {
            return Path.GetFileNameWithoutExtension(((BaseEffect)effect).Filename);
        }

        #region エフェクト

        /// <summary>
        /// 画像かエフェクトのファイルを選んで、エフェクトに加えます。
        /// </summary>
        [RelayCommand]
        private void AddFiles()
        {
            foreach (var path in view.OpenFiles(EffectFile.AddFilter, true))
            {
                AddFile(path);
            }
        }

        /// <summary>
        /// タイムラインに落としたファイルを、エフェクトに加えます（加えられないファイルは除く）。
        /// </summary>
        public void AddDroppedFiles(string[] paths)
        {
            foreach (var path in paths)
            {
                if (EffectFile.CanAdd(path))
                {
                    AddFile(path);
                }
            }
        }

        private void AddFile(string path)
        {
            Run(() =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (Path.GetExtension(path) == ".etd")
                {
                    ExecuteAndAdd(new LoadEffectCommand(EffectManager, Texts["CAddEffect"] + name, EffectManager.Effects.Count, path));
                }
                else
                {
                    ExecuteAndAdd(new AddEffectCommand(EffectManager, Texts["CAddImage"] + name, EffectManager.Effects.Count, path));
                    view.MoveTimeLineVScroll(0);
                }
            });
        }

        [RelayCommand]
        private void DeleteEffect()
        {
            if (SelectedManager.Effect == null)
            {
                return;
            }
            var index = EffectManager.Effects.IndexOf(SelectedManager.Effect);
            if (index == -1)
            {
                return;
            }
            Run(() =>
            {
                ExecuteAndAdd(new DeleteEffectCommand(EffectManager, Texts["CDelete"] + EffectName(EffectManager.Effects[index]), index));
                view.MoveTimeLineVScroll(-1);
            });
        }

        [RelayCommand]
        private void ReverseEffects()
        {
            if (EffectManager.Effects.Count >= 1 && SelectedManager.Effect != null)
            {
                Run(() => ExecuteAndAdd(new ReverseEffectsCommand(EffectManager, Texts["CReversePermutation"])));
            }
        }

        [RelayCommand]
        private void CopyEffect()
        {
            if (EffectManager.Effects.Count >= 1 && SelectedManager.Effect != null)
            {
                Run(() => ExecuteAndAdd(new CopyEffectCommand(EffectManager, Texts["CCopy"] + EffectName(SelectedManager.Effect), SelectedManager)));
            }
        }

        [RelayCommand]
        private void ClearEffect()
        {
            if (EffectManager.Effects.Count >= 1)
            {
                Run(() => ExecuteAndAdd(new ClearEffectCommand(EffectManager, Texts["CClearEffect"])));
            }
        }

        public void ChangeIndex(int index, int beforeindex)
        {
            Run(() => ExecuteAndAdd(new ChangeIndexCommand(EffectManager, Texts["CChangePermutation"] + EffectName(EffectManager.Effects[beforeindex]), index, beforeindex)));
        }

        /// <summary>
        /// 並べた画像を順に表示するアニメーションを作ります（編集の履歴は消える）。
        /// </summary>
        [RelayCommand]
        private void CreateAnimation()
        {
            var viewModel = new AskFrameCountViewModel();
            if (view.ShowDialog(viewModel) && view.Confirm(Texts["AskAnimation"], Texts["Confirm"]))
            {
                AnimationBuilder.Build(EffectManager, viewModel.FrameCount, viewModel.WithReverse);
                CommandManager.ClearAll();
                view.AdjustTimeLineHScroll();
                view.RefreshTimeLine();
            }
        }

        #endregion

        #region ステート

        private void SelectState(StateChangeEventArgs ea)
        {
            Run(() =>
            {
                if (CommandManager.LastDoneCommand is SelectStateCommand last && last.StateIndex == ea.StateIndex && last.ItemIndex == ea.ItemIndex)
                {
                    return;
                }
                var name = EffectName(EffectManager.Effects[ea.ItemIndex]);
                // 選んだときにはもう選んであるので、実行はしないで履歴にだけ残す
                CommandManager.AddCommand(new SelectStateCommand(EffectManager, Texts["CSelectState"] + name + "," + ea.StateIndex + Texts["CState"],
                    SelectedManager, ea.LastItemIndex, ea.ItemIndex, ea.LastStateIndex, ea.StateIndex));
            });
        }

        public void MoveState(int beforeframe, int newframe, TimeLine.KeyOperationMode opmode)
        {
            if (beforeframe == 0)
            {
                return;
            }
            Run(() =>
            {
                var isFixed = opmode == TimeLine.KeyOperationMode.LeftMoveOnly || opmode == TimeLine.KeyOperationMode.RightMoveOnly;
                // タイムラインで動かしたときにはもう動いているので、実行はしないで履歴にだけ残す
                CommandManager.AddCommand(new MoveStateCommand(EffectManager, Texts["CState"] + (isFixed ? Texts["CFixed"] : "") + Texts["CMove"] + EffectName(SelectedManager.Effect),
                    beforeframe, newframe, opmode, SelectedManager));
            });
        }

        public void AddState(int itemindex, int keyindex, bool iskey)
        {
            Run(() =>
            {
                ExecuteAndAdd(new AddStateCommand(EffectManager, (iskey ? Texts["CKey"] : "") + Texts["CInsertState"] + EffectName(EffectManager.Effects[itemindex]), iskey, itemindex, keyindex));
                RefreshState();
            });
        }

        public void DeleteState(int itemindex, int keyindex)
        {
            Run(() => ExecuteAndAdd(new DeleteStateCommand(EffectManager, Texts["CDeleteState"] + EffectName(EffectManager.Effects[itemindex]), itemindex, keyindex)));
        }

        void IPropertyHost.ChangeStateValue(RatioType type, float value)
        {
            if (SelectedManager.State == null)
            {
                return;
            }
            Run(() =>
            {
                // 続けて同じ値を変えたときは、履歴を 1 つにまとめる
                if (CommandManager.LastCommand is StateParameterChangeCommand last && last.Type == type)
                {
                    last.Value = value;
                    last.SpecialExecute();
                    return;
                }
                ExecuteAndAdd(new StateParameterChangeCommand(EffectManager, Texts["CChangeParameter"] + type, SelectedManager, type, value));
            });
            EffectManager?.Update(view.CurrentFrame, null);
            view.RefreshCanvas();
        }

        void IPropertyHost.ChangeBlendMode(BlendMode blendMode)
        {
            if (SelectedManager.State == null)
            {
                return;
            }
            Run(() =>
            {
                if (CommandManager.LastCommand is StateBlendModeChangeCommand last && last.BlendMode == blendMode)
                {
                    last.BlendMode = blendMode;
                    last.SpecialExecute();
                    return;
                }
                ExecuteAndAdd(new StateBlendModeChangeCommand(EffectManager, Texts["CChangeBlend"] + blendMode, SelectedManager, blendMode));
            });
            EffectManager?.Update(view.CurrentFrame, null);
            view.RefreshCanvas();
        }

        void IPropertyHost.EditRatio(RatioType type)
        {
            if (SelectedManager.Set == null)
            {
                return;
            }
            var viewModel = new RatioChangeViewModel(SelectedManager.Set[type] is BezierRatioMaker brm && brm.Analyzer.BCPS != null ? brm.Analyzer.BCPS : null);
            if (view.ShowDialog(viewModel))
            {
                IRatioMaker maker = viewModel.IsLinear ? new LinearRatioMaker() : new BezierRatioMaker(viewModel.Bcps[0], viewModel.Bcps[1]);
                Run(() => ExecuteAndAdd(new SetRatioMakerChangeCommand(EffectManager, Texts["CChangeRatio"], SelectedManager, type, maker)));
            }
        }

        bool ITimeLineHost.IsRatioMakerAvailable
        {
            set { Property.IsRatioMakerAvailable = value; }
        }

        void ITimeLineHost.RefreshCanvas()
        {
            view.RefreshCanvas();
        }

        void ITimeLineHost.RefreshTimeLine()
        {
            view.RefreshTimeLine();
        }

        #endregion

        #region カンバスのベジエ

        /// <summary>
        /// カンバスでベジエの点を動かしたときの処理です。
        /// </summary>
        public void OnBezierEdited()
        {
            Run(() =>
            {
                var source = view.CanvasBezier;
                var bcps = new BezierControlPoint[source.Length];
                for (int i = 0; i < bcps.Length; i++) bcps[i] = (BezierControlPoint)source[i].Clone();
                // 続けて動かしたときは、履歴を 1 つにまとめる
                if (CommandManager.LastCommand is BezierPositionChangeCommand last)
                {
                    last.BCPS = bcps;
                    last.SpecialExecute();
                    return;
                }
                ExecuteAndAdd(new BezierPositionChangeCommand(EffectManager, Texts["CChangeBezierPosition"], SelectedManager, bcps));
            });
        }

        /// <summary>
        /// 位置の変化をベジエでなくします。
        /// </summary>
        [RelayCommand]
        private void ResetPath()
        {
            Run(() =>
            {
                var command = new ResetPathCommand(EffectManager, Texts["CResetPath"], SelectedManager);
                command.Execute();
                RefreshState();
                CommandManager.AddCommand(command);
            });
        }

        /// <summary>
        /// 選んでいるステートを、プロパティウィンドウとカンバスに表示します。
        /// </summary>
        private void RefreshState()
        {
            Property.SetState(SelectedManager.State);
            var set = SelectedManager.Set;
            if (set == null)
            {
                return;
            }
            if (set.IsBezierPosition)
            {
                view.SetCanvasBezier(set.BAnalyzer.BCPS, true);
            }
            else if (set[RatioType.X] is not ConstantRatioMaker)
            {
                // 始めと終わりの位置を点にして、ベジエで動かせるようにする
                var bcp = new BezierControlPoint();
                var bcp2 = new BezierControlPoint();
                bcp.Second = new PointF(set.StartState.X, set.StartState.Y);
                bcp2.Second = new PointF(set.EndState.X, set.EndState.Y);
                view.SetCanvasBezier([bcp, bcp2], true);
            }
            else
            {
                view.SetCanvasBezier([], false);
            }
            view.RefreshCanvas();
            if (Property.IsRatioMakerAvailable)
            {
                Property.SetBezierPosition(set.IsBezierPosition);
            }
        }

        #endregion

        #region 元に戻す

        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
            AfterUndoOrRedo(CommandManager.Undo());
        }

        private bool CanUndo()
        {
            return CommandManager.CanUndo;
        }

        [RelayCommand(CanExecute = nameof(CanRedo))]
        private void Redo()
        {
            AfterUndoOrRedo(CommandManager.Redo());
        }

        private bool CanRedo()
        {
            return CommandManager.CanRedo;
        }

        private void AfterUndoOrRedo(CommandBase command)
        {
            if (command is StateParameterChangeCommand)
            {
                Property.SetState(SelectedManager.State);
            }
            else if (command is ResetPathCommand || command is BezierPositionChangeCommand)
            {
                RefreshState();
            }
        }

        #endregion
    }
}
