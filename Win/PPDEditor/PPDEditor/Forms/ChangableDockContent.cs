using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.Forms
{
    public class ChangableDockContent : DockContent
    {
        bool changed;
        public void ContentSaved()
        {
            changed = false;
            if (this.TabText.StartsWith("*") && this.TabText.Length > 1)
            {
                this.TabText = this.TabText.Substring(1);
            }
        }
        public void ContentChanged()
        {
            changed = true;
            if (!this.TabText.StartsWith("*"))
            {
                this.TabText = "*" + this.TabText;
            }

        }
        public bool IsContentChanged
        {
            get
            {
                return changed;
            }
        }
    }
}
