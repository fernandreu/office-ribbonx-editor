﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.ViewModels
{
    using ResultsEventArgs = DataEventArgs<IResultCollection>;

    public class EditorTabViewModel : ViewModelBase, ITabItemViewModel
    {
        public EditorTabViewModel()
        {
            this.CutCommand = new RelayCommand(() => this.Cut?.Invoke(this, EventArgs.Empty));
            this.CopyCommand = new RelayCommand(() => this.Copy?.Invoke(this, EventArgs.Empty));
            this.PasteCommand = new RelayCommand(() => this.Paste?.Invoke(this, EventArgs.Empty));
            this.UndoCommand = new RelayCommand(() => this.Undo?.Invoke(this, EventArgs.Empty));
            this.RedoCommand = new RelayCommand(() => this.Redo?.Invoke(this, EventArgs.Empty));
            this.SelectAllCommand = new RelayCommand(() => this.SelectAll?.Invoke(this, EventArgs.Empty));
        }

        public RelayCommand CutCommand { get; }

        public RelayCommand CopyCommand { get; }

        public RelayCommand PasteCommand { get; }

        public RelayCommand UndoCommand { get; }

        public RelayCommand RedoCommand { get; }

        public RelayCommand SelectAllCommand { get; }

        public event EventHandler Cut;

        public event EventHandler Copy;

        public event EventHandler Paste;

        public event EventHandler Undo;

        public event EventHandler Redo;

        public event EventHandler SelectAll;

        private string title;

        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        private string statusText;

        public string StatusText
        {
            get => this.statusText;
            set => this.Set(ref this.statusText, value);
        }

        private int zoom;

        public int Zoom
        {
            get => this.zoom;
            set => this.Set(ref this.zoom, value);
        }

        public ScintillaLexer Lexer { get; set; }

        public OfficePartViewModel Part { get; set; }

        public MainWindowViewModel MainWindow { get; set; }

        /// <summary>
        /// This event will be fired whenever key editor properties (including current text and selection) need to be known. It is the
        /// listener who will need to specify the argument.
        /// </summary>
        public event EventHandler<DataEventArgs<EditorInfo>> ReadEditorInfo;
        
        public event EventHandler<ResultsEventArgs> ShowResults;

        /// <summary>
        /// This event will be fired when the contents of the editor need to be updated
        /// </summary>
        public event EventHandler<EditorChangeEventArgs> UpdateEditor;

        public EditorInfo EditorInfo
        {
            get
            {
                var e = new DataEventArgs<EditorInfo>();
                this.ReadEditorInfo?.Invoke(this, e);
                return e.Data;
            }
        }

        public void RaiseShowResults(ResultsEventArgs e)
        {
            this.ShowResults?.Invoke(this, e);
        }

        public void RaiseUpdateEditor(EditorChangeEventArgs e)
        {
            this.UpdateEditor?.Invoke(this, e);
        }

        public void ApplyChanges()
        {
            if (!this.Part.CanHaveContents)
            {
                return;
            }

            var e = new DataEventArgs<EditorInfo>();
            this.ReadEditorInfo?.Invoke(this, e);
            if (e.Data == null)
            {
                // This means that event handler was not listened by any view, or the view did not pass the editor contents back for some reason
                return;
            }
            
            this.Part.Contents = e.Data.Text;
        }
    }
}
