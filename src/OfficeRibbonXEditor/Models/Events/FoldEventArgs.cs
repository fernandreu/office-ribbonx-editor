using System;

namespace OfficeRibbonXEditor.Models.Events
{
    public class FoldEventArgs : EventArgs
    {
        public int Level { get; }

        public bool CurrentOnly { get; }

        public bool Unfold { get; }

        public FoldEventArgs(int level, bool unfold = false)
        {
            Level = level;
            Unfold = unfold;
        }

        public FoldEventArgs(bool currentOnly, bool unfold = false)
        {
            CurrentOnly = currentOnly;
            Unfold = unfold;
        }
    }
}
