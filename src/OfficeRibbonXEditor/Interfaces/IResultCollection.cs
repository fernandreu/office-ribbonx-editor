﻿using ScintillaNET;

namespace OfficeRibbonXEditor.Interfaces;

public interface IResultCollection
{
    string Header { get; }

    bool IsEmpty { get; }

    int Count { get; }

    void AddToPanel(Scintilla editor, Scintilla resultsPanel);

    void GoToPosition(int pos, Scintilla editor, Scintilla resultsPanel);
}