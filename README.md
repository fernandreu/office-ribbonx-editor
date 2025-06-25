
# Office RibbonX Editor

[![Downloads](https://img.shields.io/github/downloads/fernandreu/office-ribbonx-editor/total.svg?style=popout)](https://github.com/fernandreu/office-ribbonx-editor/releases)
[![Release Version](https://img.shields.io/github/release/fernandreu/office-ribbonx-editor)](https://github.com/fernandreu/office-ribbonx-editor/releases/latest)
[![Pre-release Version](https://img.shields.io/github/v/release/fernandreu/office-ribbonx-editor?include_prereleases&label=pre-release)](https://github.com/fernandreu/office-ribbonx-editor/releases)
[![.NET](https://img.shields.io/badge/.NET-%3E%3D%209.0.0-informational)](https://dotnet.microsoft.com/download)
[![Build Status](https://dev.azure.com/fernandreu-public/OfficeRibbonXEditor/_apis/build/status/CI%20Pipeline?branchName=master&stageName=Build)](https://dev.azure.com/fernandreu-public/OfficeRibbonXEditor/_build/latest?definitionId=1&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=fernandreu_office-ribbonx-editor&metric=alert_status)](https://sonarcloud.io/dashboard?id=fernandreu_office-ribbonx-editor)

The Office RibbonX Editor is a standalone tool to edit the Custom UI part of Office open document file format. 
It contains both Office 2007 and Office 2010 custom UI schemas.

The Office 2010 custom UI schema is the latest schema and it's still being used in the latest versions of Office including
Office 2019, Office 2021 and Office 365.

To learn more about how to use these identifiers to customize the Office ribbon, backstage, and context menus visit:
 - [Change the Ribbon in Excel 2007-2016](https://www.rondebruin.nl/win/s2/win001.htm)
 - [Customizing the Office Fluent Ribbon for Developers](https://msdn.microsoft.com/en-us/library/aa338202(v=office.14).aspx)
 - [Introduction to the Office Backstage View for Developers](https://msdn.microsoft.com/en-us/library/ee691833(office.14).aspx)
 - [Office Fluent User Interface Control Identifiers](https://github.com/OfficeDev/office-fluent-ui-command-identifiers)


## Improvements

This GitHub project is a fork of [Custom UI Editor for Microsoft Office](https://github.com/OfficeDev/office-custom-ui-editor). Built
on Windows Forms, the original editor by Microsoft is useful on its own, but it has some limitations. Rather than trying to address
those limitations by performing small contributions to the original project, this repo offers a complete redesign of the project in
Windows Presentation Foundation (WPF).

Features of this overhauled editor include:
- [ScintillaNET](https://github.com/jacobslusser/ScintillaNET) (via [SctintillaNET.WPF](https://github.com/Stumpii/ScintillaNET.WPF/tree/master/ScintillaNET.WPF)) as text editor, with seamless syntax highlighting
- The TreeView allows you to have more than one file open, easily switching between different customUI files (for example,
for copying code from one file to another)
- A multi-tab layout, allowing to have multiple files open simultaneously (including icon previews)
- List of recently opened files showing up on the file menu (thanks to 
[RecentFileList](https://www.codeproject.com/Articles/23731/RecentFileList-a-WPF-MRU))
- A `Reload on Save` option that avoids losing any external changes (for more info, see [the section below](#how-does-the-reload-on-save-option-work))
- Possibility of customizing some aspects of the editor such as font size and color
- Plus all the features of the original Windows Forms project

![Screenshot](docs/Screenshot.png)


## Download / Build status

To download the latest release, go to the following link:

https://github.com/fernandreu/office-ribbonx-editor/releases/latest

To download the latest pre-release, go to the full list of releases on GitHub:

https://github.com/fernandreu/office-ribbonx-editor/releases

> **Note:** From v2.0 onwards, .NET Framework is no longer supported. The latest .NET Framework version is v1.9 and
> can be found [here](https://github.com/fernandreu/office-ribbonx-editor/releases/tag/v1.9.0).


## How does the `Reload on Save` option work?

An Office 2007+ file (`.xlsm`, `.xlam`, `.pptm`, `.docx`, etc.) is nothing more than a `.zip` file with a
custom extension. When the Office RibbonX Editor opens one of those files, it unzips it into a temporary
location first, and then it shows the content from there. To save the file, it will apply any changes to
the unzipped files, and zip everything back to its original location.

The way you would use the original Custom UI Editor is similar to the following:

1.	If the file you want to edit is open in Excel, close it first
2.	Open that file in the Custom UI Editor
3.	Edit the xml files, icons, etc.
4.	Save the file in the Custom UI Editor (and close it if you wish)
5.	Open the file back in Excel, and enjoy the changes you just made

However, **what would happen if you forget Step 1** and Excel has the same file open when you are using the
tool? Your workflow could then look like this instead:

1.	Open that file in the Custom UI Editor
2.	Edit xml files, icons, etc.
3.	You realise you had the file open in Excel, so you close it there first
    - But you also had unsaved changes in Excel, so you save the file before closing it
4.	Save the file in the Custom UI Editor (and close it if you wish)
    -	**Remember:** all this time since Step 1, the Custom UI Editor was looking at a temporary unzipped copy
    of the Excel file that did not include the changes saved in Step 3!
5.	Open the file back in Excel. The changes you made in the Custom UI Editor (Step 2) are there, but the ones
you did in Excel (Step 3) have disappeared

**The `Reload on Save` button adds an extra step to the process as a safety precaution in this scenario.** In
essence, Step 4 will no longer use the temporary unzipped copy of the Excel file that was generated in Step 1,
but will generate a new one instead. As a consequence, any external changes you might have done in the meantime
(i.e. Step 3) will no longer get lost. If you did not make any external changes, the `Reload on Save` button
won’t have any noticeable impact for you.


## Code Signing

The tool is no longer being signed using a certificate from a trusted root certification authority. Instead, each
build now uses a temporary, unique code signing certificate. Those will be signed using the following self-signed
certificate:

- Subject: `github.com/fernandreu`
- Thumbprint: `62530bc980ec95d70a9a0abf931a4c28877ef4c6`

This will give issues with Microsoft SmartScreen, but you should be able to skip any warnings. If you are unsure if
you should skip those warnings, there are a couple of things you could check:

- You can verify that the installer / binaries have been signed with a certificate whose root certificate matches the
  details above. If some bundled dlls were already signed (e.g. official Microsoft libraries), their original
  signatures will be kept
- The details of the temporary code signing certificates will also be made available for each build / release (just
  the public ones, such as the thumbprint or the public key). Hence, you can also check that the certificate used for
  the binaries / installer matches those details

For more information, see [#185](https://github.com/fernandreu/office-ribbonx-editor/issues/185).


## Do you want to see the tool in your language?

Any help improving existing translations or adding new ones is welcome. I will add your names to a list of
acknowledgments, either in this readme or in the About section of the tool itself.

### Improving an existing translation

If you get stuck in any step, feel free to [create an issue](https://github.com/fernandreu/office-ribbonx-editor/issues/new)
and I will assist you.

1. Find the file you want to edit on this GitHub project
    1. All translations are `Strings.xyz.resx` files stored in [src/OfficeRibbonXEditor/Resources](https://github.com/fernandreu/office-ribbonx-editor/tree/master/src/OfficeRibbonXEditor/Resources)
    2. The `xyz` part is what indicates the language contained in the file
    3. For example, the Spanish translation is stored in the `Strings.es.resx` file [here](https://github.com/fernandreu/office-ribbonx-editor/blob/master/src/OfficeRibbonXEditor/Resources/Strings.es.resx)
2. Click on the `Edit` button at the top-right corner
    1. This might trigger a fork of this project under your GitHub account (otherwise, it will occur when saving any changes)
3. Make any necessary changes
    1. The `<data>` tags are essentially the string resources throughout the application
    2. Their `name` attribute is how they are being identified internally. This might provide some hints about their intended use
    3. Otherwise, there might also be a child `<comment>` tag providing more details about a particular resource
    4. Only the child `<value>` tags should need modifications
4. Save the changes at the bottom
    1. This should trigger a commit of the file in your forked repository, and perhaps a pull request to this
      repository too
    2. If the pull request does not occur automatically, you might see some buttons either in your fork or here
      to do so

### Creating a translation for a new language

These steps are recommended for people that are already a bit familiar with the Git / GitHub workflow. If this 
is not your case, please [create an issue](https://github.com/fernandreu/office-ribbonx-editor/issues/new)
instead. I will then generate a template myself, so you will be able to follow the 
[previous steps](#improving-an-existing-translation) instead of these ones.

1. Create a copy of the `Strings.resx` file [here](https://github.com/fernandreu/office-ribbonx-editor/blob/master/src/OfficeRibbonXEditor/Resources/Strings.resx), which contains the default English language
    1. If you want, you can also create the copy from an existing translation (e.g. `Strings.es.resx` for Spanish [here](https://github.com/fernandreu/office-ribbonx-editor/blob/master/src/OfficeRibbonXEditor/Resources/Strings.es.resx))
2. Name the copy `Strings.xyz.resx`, with `xyz` being your language tag
    1. For a list of available language tags, see the table in [this page](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c)
    2. There is no need to specify the country in the language tag as well (i.e. `de`, `pt`, `ru`, etc. is enough)
3. Put the file in the same [src/OfficeRibbonXEditor/Resources](https://github.com/fernandreu/office-ribbonx-editor/tree/master/src/OfficeRibbonXEditor/Resources)
  folder where the original file was
4. Make any necessary changes to this file
    1. The `<data>` tags are essentially the string resources throughout the application
    2. Their `name` attribute is how they are being identified internally. This might provide some hints about their intended use
    3. Otherwise, there might also be a child `<comment>` tag providing more details about a particular resource
    4. Only the child `<value>` tags should need modifications
5. Modify the `LanguageChoice` class [here](https://github.com/fernandreu/office-ribbonx-editor/blob/master/src/OfficeRibbonXEditor/Helpers/LanguageChoice.cs)
  to add your new language into the `All` collection

It might be possible to perform all these steps directly in your GitHub fork via several commits. Otherwise,
you might need to have at least Git installed. Visual Studio should not be necessary unless you want to see
how your translation looks (you will be able to see it from the build artifacts of your pull request too).

### Special thanks to all translators so far

- Chinese: [bitaller](https://github.com/bitaller)
- Dutch: [pplanch](https://github.com/pplanch)
- French: [pplanch](https://github.com/pplanch)
- German: [carpac](https://github.com/carpac), [Claythve](https://github.com/Claythve), [Mo-Gul](https://github.com/Mo-Gul)
- Indonesian: [aliishaq-zz](https://github.com/aliishaq-zz)
- Portuguese: [ALeXceL](https://github.com/Alexcel)
- Turkish: [fatihmeh](https://github.com/fatihmeh)
