using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ScintillaNET.WPF
{
    public class ScintillaAutomationPeer : UserControlAutomationPeer, IValueProvider
    {
        private readonly ScintillaWPF editor;

        public ScintillaAutomationPeer(ScintillaWPF owner) : base(owner)
        {
            this.editor = owner;
        }

        protected override string GetClassNameCore()
        {
            return nameof(ScintillaWPF);
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        public void SetValue(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get => this.editor.Text;
            set => this.editor.Text = value;
        }

        public bool IsReadOnly => false;
    }
}
