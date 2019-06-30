namespace FlowScriptDrawControl.Model
{
    public class ScopeChild : Selectable
    {
        private int scopeId;
        private Scope scope;

        public Scope Scope
        {
            get { return scope; }
            set
            {
                if (scope != value)
                {
                    scope = value;
                    RaisePropertyChanged("Scope");
                }
            }
        }

        internal int ScopeId
        {
            get
            {
                if (scope == null)
                {
                    return scopeId;
                }
                return scope.Id;
            }
            set
            {
                scopeId = value;
            }
        }
    }
}
