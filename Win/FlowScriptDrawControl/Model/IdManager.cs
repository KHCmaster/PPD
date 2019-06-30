namespace FlowScriptDrawControl.Model
{
    class IdManager
    {
        private int nextId;

        public int GetId()
        {
            return nextId++;
        }

        public void SetMaxId(int maxId)
        {
            nextId = maxId + 1;
        }
    }
}
