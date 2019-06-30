using System;
using System.Threading;

namespace FlowScriptEngine
{
    public class DebugController
    {
        private OperationType operationType;
        private ManualResetEvent resetEvent;
        private FlowSourceManager currentFlowSourceManager;

        public event Action<string, FlowSourceManager> SourceChanged;
        public event Action<FlowSourceManager> OperationWaited;
        public event Action<FlowSourceManager> OperationAccepted;
        public event Action<FlowSourceManager, FlowExecutionException> ErrorOccurred;

        public bool Waiting
        {
            get;
            private set;
        }

        public void Abort()
        {
            operationType = OperationType.Abort;
            resetEvent.Set();
        }

        public void Continue()
        {
            operationType = OperationType.Continue;
            resetEvent.Set();
        }

        public void StepIn()
        {
            operationType = OperationType.StepIn;
            resetEvent.Set();
        }

        public void ChangeSource(int id)
        {
            if (currentFlowSourceManager != null)
            {
                var serializedText = currentFlowSourceManager.SerializeSource(id);
                if (!String.IsNullOrEmpty(serializedText))
                {
                    OnSourceChanged(serializedText, currentFlowSourceManager);
                }
            }
        }

        internal void Error(FlowSourceManager flowSourceManager, FlowExecutionException e)
        {
            OnErrorOccurred(flowSourceManager, e);
        }

        internal OperationType WaitOperation(FlowSourceManager flowSourceManager)
        {
            currentFlowSourceManager = flowSourceManager;
            operationType = OperationType.Wait;
            resetEvent = new ManualResetEvent(false);
            Waiting = true;
            OnOperationWaited(flowSourceManager);
            while (operationType == OperationType.Wait)
            {
                resetEvent.WaitOne();
            }
            OnOperationAccepted(flowSourceManager);
            return operationType;
        }

        private void OnErrorOccurred(FlowSourceManager flowSourceManager, FlowExecutionException e)
        {
            ErrorOccurred?.Invoke(flowSourceManager, e);
        }

        private void OnOperationWaited(FlowSourceManager flowSourceManager)
        {
            OperationWaited?.Invoke(flowSourceManager);
        }

        private void OnOperationAccepted(FlowSourceManager flowSourceManager)
        {
            OperationAccepted?.Invoke(flowSourceManager);
        }

        internal void OnSourceChanged(string str, FlowSourceManager flowSourceManager)
        {
            SourceChanged?.Invoke(str, flowSourceManager);
        }
    }
}
