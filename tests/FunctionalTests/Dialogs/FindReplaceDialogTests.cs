using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using ScintillaNET;

namespace OfficeRibbonXEditor.FunctionalTests.Dialogs
{
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
    public class FindReplaceDialogTests
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Always defined in SetUp
        protected FindReplaceDialogViewModel ViewModel { get; set; }

        protected Scintilla Scintilla { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        private Action<IResultCollection?>? findAllAction;

        [SetUp]
        public virtual void SetUp()
        {
            this.Scintilla = new Scintilla();

            this.ViewModel = new FindReplaceDialogViewModel();
            ViewModel.OnLoaded((this.Scintilla, FindReplaceAction.Find, (o, e) => findAllAction?.Invoke(e.Data)));
        }

        [TearDown]
        public void TearDown()
        {
            this.Scintilla.Dispose();
        }

        // Basic
        [TestCase("abcdefgh", "c", false, false, ExpectedResult = true)]
        [TestCase("abcdefgh", "z", false, false, ExpectedResult = false)]
        [TestCase(@"abc\tdef", @"\t", false, false, ExpectedResult = true)]
        [TestCase("abc\tdef", @"\t", false, false, ExpectedResult = false)]
        [TestCase("abcdefgh", ".", false, false, ExpectedResult = false)]
        // Extended
        [TestCase(@"abc\tdef", @"\t", false, true, ExpectedResult = false)]
        [TestCase("abc\tdef", @"\t", false, true, ExpectedResult = true)]
        [TestCase("abcdefgh", ".", false, true, ExpectedResult = false)]
        // RegEx
        [TestCase("abc\tdef", @".", true, false, ExpectedResult = true)]
        [TestCase(@"abc\tdef", @"\t", true, false, ExpectedResult = false)]
        [TestCase("abc\tdef", @"\t", true, false, ExpectedResult = true)]
        [TestCase("abc def ghi", @". .", true, false, ExpectedResult = true)]
        public bool FindNext(string text, string findText, bool regEx, bool extended)
        {
            // Arrange
            this.SetText(text);
            this.ViewModel.FindText = findText;
            this.ViewModel.IsRegExSearch = regEx;
            this.ViewModel.IsExtendedSearch = extended;

            // Act
            this.ViewModel.FindNextCommand.Execute(null);

            return this.ViewModel.StatusText != "Match could not be found";
        }

        // Basic
        [TestCase("aaaa", "a", "b", false, false, ExpectedResult = "baaa")]
        [TestCase("aaaa", "b", "a", false, false, ExpectedResult = "aaaa")]
        // Extended
        [TestCase(@"abc\tdef", @"\t", "A", false, true, ExpectedResult = @"abc\tdef")]
        [TestCase("abc\tdef", @"\t", "A", false, true, ExpectedResult = "abcAdef")]
        [TestCase("abcdef", ".", "A", false, true, ExpectedResult = "abcdef")]
        // RegEx
        [TestCase("abc\tdef", @"\t", "A", true, false, ExpectedResult = "abcAdef")]
        [TestCase(@"abc\tdef", @"\t", "A", true, false, ExpectedResult = @"abc\tdef")]
        [TestCase("abc def ghi", @". .", "A", true, false, ExpectedResult = "abAef ghi")]
        public string ReplaceNext(string text, string findText, string replaceText, bool regEx, bool extended)
        {
            // Arrange
            this.SetText(text);
            this.ViewModel.FindText = findText;
            this.ViewModel.ReplaceText = replaceText;
            this.ViewModel.IsRegExSearch = regEx;
            this.ViewModel.IsExtendedSearch = extended;

            // Act

            // First click merely selects the text to be replaced
            this.ViewModel.ReplaceNextCommand.Execute(null);

            // Second one should do the actual replacement
            this.ViewModel.ReplaceNextCommand.Execute(null);

            return this.Scintilla.Text;
        }

        // Basic
        [TestCase("abcdabcdabcd", "c", false, false, ExpectedResult = 3)]
        [TestCase("abcdabcdabcd", "cb", false, false, ExpectedResult = 0)]
        // RegEx
        [TestCase(@"abc\tde\tf", @"\t", true, false, ExpectedResult = 0)]
        [TestCase("abc\tde\tf", @"\t", true, false, ExpectedResult = 2)]
        [TestCase("abcdefgh", "d.*f", true, false, ExpectedResult = 1)]
        public int FindAll(string text, string findText, bool regEx, bool extended)
        {
            // Arrange
            this.SetText(text);
            this.ViewModel.FindText = findText;
            this.ViewModel.IsRegExSearch = regEx;
            this.ViewModel.IsExtendedSearch = extended;
            IResultCollection? result = null;
            this.findAllAction = x => result = x;

            // Act
            this.ViewModel.FindAllCommand.Execute(null);

            return result?.Count ?? 0;
        }

        // Basic
        [TestCase("aaaa", "a", "b", false, false, ExpectedResult = "bbbb")]
        [TestCase("aaaa", "b", "a", false, false, ExpectedResult = "aaaa")]
        // RegEx
        [TestCase("1a12a23a3", ".a.", "b", true, false, ExpectedResult = "bbb")]
        [TestCase("1a12a23a3", "(.)a(.)", "$1b$2", true, false, ExpectedResult = "1b12b23b3")]
        public string ReplaceAll(string text, string findText, string replaceText, bool regEx, bool extended)
        {
            // Arrange
            this.SetText(text);
            this.ViewModel.FindText = findText;
            this.ViewModel.ReplaceText = replaceText;
            this.ViewModel.IsRegExSearch = regEx;
            this.ViewModel.IsExtendedSearch = extended;

            // Act
            this.ViewModel.ReplaceAllCommand.Execute(null);

            return this.Scintilla.Text;
        }

        private void SetText(string text)
        {
            this.Scintilla.Text = text;
            if (this.ViewModel.SearchSelection)
            {
                this.Scintilla.SelectionStart = 0;
                this.Scintilla.SelectionEnd = text.Length;
                Assert.AreEqual(text, this.Scintilla.SelectedText, "Selected text not set up correctly");
            }
        }
    }

    public class FindReplaceDialogTestsInSelection : FindReplaceDialogTests
    {
        public override void SetUp()
        {
            base.SetUp();
            this.ViewModel.SearchSelection = true;
            this.ViewModel.Wrap = true;
        }
    }
}
