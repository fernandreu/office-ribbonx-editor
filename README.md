
Office RibbonX Editor
===============

This GitHub project is a fork of [Custom UI Editor for Microsoft Office](https://github.com/OfficeDev/office-custom-ui-editor). Built on Windows Forms, the original editor by Microsoft is useful on its own, but it has some limitations. Rather than
trying to address those limitations by performing small contributions to the original project, this repo offers a complete redesign
of the project in Windows Presentation Foundation (WPF).

Features of this overhauled editor include:
- [ScintillaNET](https://github.com/jacobslusser/ScintillaNET) (via [SctintillaNET.WPF](https://github.com/Stumpii/ScintillaNET.WPF/tree/master/ScintillaNET.WPF)) as text editor, with seamless syntax highlighting
- The TreeView allows you to have more than one file open, easily switching between different customUI files (for example,
for copying code from one file to another)
- List of recently opened files showing up on the file menu (thanks to 
[RecentFileList](https://www.codeproject.com/Articles/23731/RecentFileList-a-WPF-MRU))
- Possibility of reloading a file's contents (i.e. those not viewed in the editor, such as spreadsheet values) before
saving it. This avoids accidental loss of data. For example, an Excel file could be opened in the editor first, then
edited in Excel, then saved in the editor, at which point you would lose those changes
- Possibility of customizing some aspects of the editor such as font size and color
- Plus all the features of the original Windows Forms project

![Screenshot](Screenshot.png)


Download / Build status
-------------------------------

To download the latest release, go to the following link:

https://github.com/fernandreu/office-ribbonx-editor/releases/latest

To download the latest development build instead, click on the icon below, then go to the Artifacts menu at the top-right corner:

[![Build Status](https://dev.azure.com/fernandreu-public/OfficeRibbonXEditor/_apis/build/status/BuildAndTest?branchName=master)](https://dev.azure.com/fernandreu-public/OfficeRibbonXEditor/_build/latest?definitionId=1&branchName=master)


Other info
---------------------------

*This section has been partially borrowed from the [original Windows Forms project](https://github.com/OfficeDev/office-custom-ui-editor).*

The Office Custom UI Editor is a standalone tool to edit the Custom UI part of Office open document file format. 
It contains both Office 2007 and Office 2010 custom UI schemas.

The Office 2010 custom UI schema is the latest schema and it's still being used in the latest versions of Office including
Office 2013, Office 2016 and Office 365.

To learn more about how to use these identifiers to customize the Office ribbon, backstage, and context menus visit:
 - [Change the Ribbon in Excel 2007-2016](https://www.rondebruin.nl/win/s2/win001.htm)
 - [Customizing the Office Fluent Ribbon for Developers](https://msdn.microsoft.com/en-us/library/aa338202(v=office.14).aspx)
 - [Introduction to the Office Backstage View for Developers](https://msdn.microsoft.com/en-us/library/ee691833(office.14).aspx)
 - [Office Fluent User Interface Control Identifiers](https://github.com/OfficeDev/office-fluent-ui-command-identifiers)
