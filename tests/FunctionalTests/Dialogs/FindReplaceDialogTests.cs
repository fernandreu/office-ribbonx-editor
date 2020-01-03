using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using ScintillaNET;

namespace OfficeRibbonXEditor.FunctionalTests.Dialogs
{
    public class FindReplaceDialogTests
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Always defined in SetUp
        private FindReplaceDialogViewModel viewModel;

        private Scintilla scintilla;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        private Action<IResultCollection?>? findAllAction;

        [SetUp]
        public void SetUp()
        {
            this.scintilla = new Scintilla();

            this.viewModel = new FindReplaceDialogViewModel();
            viewModel.OnLoaded((this.scintilla, FindReplaceAction.Find, (o, e) => findAllAction?.Invoke(e.Data)));
        }

        [Test]
        [TestCase("abcdefgh", "c", ExpectedResult = true)]
        [TestCase("abcdefgh", "z", ExpectedResult = false)]
        [TestCase(@"abc\tdef", @"\t", ExpectedResult = true)]
        [TestCase("abc\tdef", @"\t", ExpectedResult = false)]
        [TestCase("abcdefgh", ".", ExpectedResult = false)]
        public bool FindNextBasic(string text, string findText)
        {
            return this.FindNextBase(text, findText);
        }

        [Test]
        [TestCase(@"abc\tdef", @"\t", ExpectedResult = false)]
        [TestCase("abc\tdef", @"\t", ExpectedResult = true)]
        [TestCase("abcdefgh", ".", ExpectedResult = false)]
        public bool FindNextExtended(string text, string findText)
        {
            return this.FindNextBase(text, findText, x => x.IsExtendedSearch = true);
        }

        [Test]
        [TestCase("abc\tdef", @".", ExpectedResult = true)]
        [TestCase(@"abc\tdef", @"\t", ExpectedResult = false)]
        [TestCase("abc\tdef", @"\t", ExpectedResult = true)]
        [TestCase("abc def ghi", @". .", ExpectedResult = true)]
        public bool FindNextRegEx(string text, string findText)
        {
            return this.FindNextBase(text, findText, x => x.IsRegExSearch = true);
        }

        [Test]
        [TestCase("aaaa", "a", "b", ExpectedResult = "baaa")]
        [TestCase("aaaa", "b", "a", ExpectedResult = "aaaa")]
        public string ReplaceNextBasic(string text, string findText, string replaceText)
        {
            // Arrange
            this.scintilla.Text = text;
            this.viewModel.FindText = findText;
            this.viewModel.ReplaceText = replaceText;

            // Act
            
            // First click merely selects the text to be replaced
            this.viewModel.ReplaceNextCommand.Execute(null);

            // Second one should do the actual replacement
            this.viewModel.ReplaceNextCommand.Execute(null);

            return this.scintilla.Text;
        }

        [Test]
        [TestCase("abcdabcdabcd", "c", ExpectedResult = 3)]
        [TestCase("abcdabcdabcd", "cb", ExpectedResult = 0)]
        public int FindAllBasic(string text, string findText)
        {
            // Arrange
            this.scintilla.Text = text;
            this.viewModel.FindText = findText;
            IResultCollection? result = null;
            this.findAllAction = x => result = x;

            // Act
            this.viewModel.FindAllCommand.Execute(null);

            return result?.Count ?? 0;
        }

        [Test]
        [TestCase("aaaa", "a", "b", ExpectedResult = "bbbb")]
        [TestCase("aaaa", "b", "a", ExpectedResult = "aaaa")]
        public string ReplaceAllBasic(string text, string findText, string replaceText)
        {
            // Arrange
            this.scintilla.Text = text;
            this.viewModel.FindText = findText;
            this.viewModel.ReplaceText = replaceText;

            // Act
            this.viewModel.ReplaceAllCommand.Execute(null);

            return this.scintilla.Text;
        }

        private bool FindNextBase(string text, string findText, Action<FindReplaceDialogViewModel>? configure = null)
        {
            // Arrange
            this.scintilla.Text = text;
            this.viewModel.FindText = findText;
            configure?.Invoke(this.viewModel);

            // Act
            this.viewModel.FindNextCommand.Execute(null);

            return !string.IsNullOrEmpty(this.scintilla.SelectedText);
        }
    }
}
