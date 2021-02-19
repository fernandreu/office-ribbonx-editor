using FlaUI.Core;
using FlaUI.Core.Definitions;
using FlaUI.Core.Patterns;

namespace OfficeRibbonXEditor.UITests.Helpers
{
    public class TextRange
    {
        protected ITextPattern TextPattern { get; }

        protected IValuePattern ValuePattern { get; }

        protected ITextRange Range { get; }

        public TextRange(ITextPattern textPattern, IValuePattern valuePattern, ITextRange range)
        {
            TextPattern = textPattern;
            ValuePattern = valuePattern;
            Range = range;
        }

        public virtual int Position
        {
            get => Range.CompareEndpoints(TextPatternRangeEndpoint.Start, TextPattern.DocumentRange, TextPatternRangeEndpoint.Start);
            set => Range.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, value - Position);
        }

        public virtual int EndPosition
        {
            get => Range.CompareEndpoints(TextPatternRangeEndpoint.End, TextPattern.DocumentRange, TextPatternRangeEndpoint.Start);
            set => Range.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, value - EndPosition);
        }

        public virtual int Line
        {
            get
            {
                var refLine = Range.Clone();
                refLine.ExpandToEnclosingUnit(TextUnit.Line);

                var line = TextPattern.DocumentRange.Clone();
                line.ExpandToEnclosingUnit(TextUnit.Line);

                var previous = int.MinValue;
                for (var i = 0; ; ++i)
                {
                    var comparison = refLine.CompareEndpoints(TextPatternRangeEndpoint.Start, line, TextPatternRangeEndpoint.Start);

                    if (comparison == 0)
                    {
                        return i;
                    }

                    if (comparison == previous)
                    {
                        // Somehow the pattern is misbehaving: avoid an infinite loop
                        return 0;
                    }

                    line.Move(TextUnit.Line, 1);
                }
            }
            set => Range.Move(TextUnit.Line, value - Line);
        }

        public string Text
        {
            get => Range.GetText(int.MaxValue);
            set
            {
                var text = ValuePattern.Value.Value;
                var start = Position;
                text = text.Substring(0, Position) + value + text.Substring(EndPosition);
                ValuePattern.SetValue(text);
                Position = start;
                EndPosition = start + value.Length;
            }
        }
    }
}
