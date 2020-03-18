using FlaUI.Core;
using FlaUI.Core.Patterns;

namespace OfficeRibbonXEditor.UITests.Helpers
{
    public class SelectedTextRange : TextRange
    {
        public SelectedTextRange(ITextPattern textPattern, IValuePattern valuePattern, ITextRange range) : base(textPattern, valuePattern, range)
        {
        }

        public override int Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                this.Range.Select();
            }
        }

        public override int EndPosition
        {
            get => base.EndPosition;
            set
            {
                base.EndPosition = value;
                this.Range.Select();
            }
        }

        public override int Line
        {
            get => base.Line;
            set
            {
                base.Line = value;
                this.Range.Select();
            }
        }
    }
}
