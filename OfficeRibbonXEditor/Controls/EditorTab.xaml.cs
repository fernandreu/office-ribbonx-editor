using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OfficeRibbonXEditor.ViewModels;
using ScintillaNET;

namespace OfficeRibbonXEditor.Controls
{
    /// <summary>
    /// Interaction logic for EditorTab.xaml
    /// </summary>
    public partial class EditorTab : UserControl
    {
        private GridLength lastResultsHeight = new GridLength(150);

        public EditorTab()
        {
            this.InitializeComponent();
        }
        
        public int Zoom
        {
            get => this.Editor?.Zoom ?? 0;
            set
            {
                if (this.Editor == null || this.Editor.Zoom == value)
                {
                    return;
                }

                this.Editor.Zoom = value;
            }
        }

        private void OnScintillaUpdateUi(object sender, UpdateUIEventArgs e)
        {
            if (!(this.DataContext is EditorTabViewModel vm))
            {
                return;
            }

            if (!this.Editor.IsEnabled)
            {
                vm.LineStatus = "Ln 0, Col 0";
            }
            else
            {
                var pos = this.Editor.CurrentPosition;
                var line = this.Editor.LineFromPosition(pos);
                var col = this.Editor.GetColumn(pos);

                vm.LineStatus = $"Ln {line + 1},  Col {col + 1}";
            }
        }

        private void OnEditorZoomChanged(object sender, EventArgs e)
        {
            this.Zoom = this.Editor.Zoom;
        }

        // For some reason, the Scintilla editor always seems to have preference over the input gestures.
        // The only solution (so far) is to execute those when appropriate from this event handler.
        private void OnEditorKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            foreach (var ib in this.InputBindings)
            {
                if (!(ib is KeyBinding kb))
                {
                    return;
                }
                        
                var inputKey = KeyInterop.KeyFromVirtualKey(e.KeyValue);
                if (kb.Key != inputKey)
                {
                    continue;
                }

                if (kb.Modifiers.HasFlag(ModifierKeys.Control) != e.Control)
                {
                    continue;
                }
                        
                if (kb.Modifiers.HasFlag(ModifierKeys.Shift) != e.Shift)
                {
                    continue;
                }
                        
                if (kb.Modifiers.HasFlag(ModifierKeys.Alt) != e.Alt)
                {
                    continue;
                }

                e.SuppressKeyPress = true;
                e.Handled = true;

                // If this is not async, the lines above seem to be ignored and the character is still printed when
                // launching a dialog (e.g. Ctrl-O). See: https://github.com/fernandreu/office-ribbonx-editor/issues/20
                this.Dispatcher.InvokeAsync(() => kb.Command.Execute(null));

                return;
            }
        }

        private void OnEditorInsertCheck(object sender, InsertCheckEventArgs e)
        {
            if (!Properties.Settings.Default.AutoIndent)
            {
                return;
            }

            if (!e.Text.EndsWith("\r") && !e.Text.EndsWith("\n"))
            {
                return;
            }

            var startPos = this.Editor.Lines[this.Editor.LineFromPosition(this.Editor.CurrentPosition)].Position;
            var endPos = e.Position;
            var curLineText = this.Editor.GetTextRange(startPos, endPos - startPos); // Text until the caret
            var indent = Regex.Match(curLineText, "^[ \\t]*");
            e.Text += indent.Value;
            if (Regex.IsMatch(curLineText, "[^/]>\\s*$"))
            {
                // If the previous line finished with an open tag, add an indentation level to the next one 
                e.Text += new string(' ', Properties.Settings.Default.TabWidth);
            }
        }

        private void OnCloseFindResults(object sender, RoutedEventArgs e)
        {
            this.lastResultsHeight = this.ResultsRow.Height;
            this.ResultsRow.Height = new GridLength(0);
            this.ResultsSplitter.Visibility = Visibility.Collapsed;
        }
    }
}
