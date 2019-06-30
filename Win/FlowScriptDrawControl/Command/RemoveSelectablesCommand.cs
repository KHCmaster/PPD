using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptDrawControl.Command
{
    class RemoveSelectablesCommand : CommandBase
    {
        private SelectableControl[] controls;
        private SourceControl[] sources;
        private CommentControl[] comments;
        private FlowAreaControl flowAreaControl;
        private RemoveFlowCommand[] removeFlowCommands;
        private RemoveBoundCommentCommand[] removeBoundCommentCommands;
        private RemoveSelectableFromScopeCommand[] removeFromScopeCommands;

        public RemoveSelectablesCommand(FlowAreaControl flowAreaControl, SelectableControl[] controls)
        {
            this.flowAreaControl = flowAreaControl;
            this.controls = controls;
        }

        public override void Execute()
        {
            sources = controls.Where(c => c is SourceControl).Cast<SourceControl>().ToArray();
            comments = controls.Where(c => c is CommentControl).Cast<CommentControl>().ToArray();
            RemoveFlow();
            RemoveBoundComment();
            RemoveFromScope();
            foreach (CommentControl comment in comments)
            {
                flowAreaControl.controlCanvas.Children.Remove(comment);
            }
            foreach (SourceControl source in sources)
            {
                flowAreaControl.controlCanvas.Children.Remove(source);
            }
        }

        private void RemoveFlow()
        {
            var listFlow = new List<RemoveFlowCommand>();
            foreach (SourceControl sourceControl in sources)
            {
                foreach (SourceItemControl inItem in sourceControl.InItems)
                {
                    if (inItem.CurrentItem.InConnection != null &&
                        sources.FirstOrDefault(s => s.CurrentSource == inItem.CurrentItem.InConnection.Target.Source) == null)
                    {
                        listFlow.Add(new RemoveFlowCommand(flowAreaControl, flowAreaControl.GetArrowControl(inItem)));
                    }
                }
                foreach (SourceItemControl outItem in sourceControl.OutItems)
                {
                    foreach (Connection connection in outItem.CurrentItem.OutConnections)
                    {
                        listFlow.Add(new RemoveFlowCommand(flowAreaControl, flowAreaControl.GetArrowControl(connection.Target)));
                    }
                }
            }
            removeFlowCommands = listFlow.ToArray();

            foreach (RemoveFlowCommand command in removeFlowCommands)
            {
                command.Execute();
            }
        }

        private void RemoveBoundComment()
        {
            var removeBoundCommentList = new List<RemoveBoundCommentCommand>();
            foreach (SourceControl sourceControl in sources)
            {
                if (sourceControl.CurrentSource.Comment != null)
                {
                    var boundComment = flowAreaControl.GetAllBoundCommentControls().FirstOrDefault(c => c.CurrentComment == sourceControl.CurrentSource.Comment);
                    if (boundComment != null)
                    {
                        removeBoundCommentList.Add(new RemoveBoundCommentCommand(flowAreaControl, sourceControl, boundComment));
                    }
                }
            }
            removeBoundCommentCommands = removeBoundCommentList.ToArray();
            foreach (RemoveBoundCommentCommand command in removeBoundCommentCommands)
            {
                command.Execute();
            }
        }

        private void RemoveFromScope()
        {
            var removeFromScopeList = new List<RemoveSelectableFromScopeCommand>();
            foreach (SelectableControl control in controls)
            {
                var scopeControl = flowAreaControl.GetAllScopeControls().FirstOrDefault(s => s.Contains(control));
                if (scopeControl != null)
                {
                    removeFromScopeList.Add(new RemoveSelectableFromScopeCommand(flowAreaControl, scopeControl, control));
                }
            }
            removeFromScopeCommands = removeFromScopeList.ToArray();
            foreach (RemoveSelectableFromScopeCommand command in removeFromScopeCommands)
            {
                command.Execute();
            }
        }

        public override void Undo()
        {
            foreach (CommentControl comment in comments)
            {
                flowAreaControl.controlCanvas.Children.Add(comment);
            }
            foreach (SourceControl source in sources)
            {
                flowAreaControl.controlCanvas.Children.Add(source);
            }
            foreach (RemoveSelectableFromScopeCommand command in removeFromScopeCommands)
            {
                command.Undo();
            }
            foreach (RemoveBoundCommentCommand command in removeBoundCommentCommands)
            {
                command.Undo();
            }
            foreach (RemoveFlowCommand command in removeFlowCommands)
            {
                command.Undo();
            }
        }
    }
}
