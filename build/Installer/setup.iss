
#ifndef ProjectGuid
  #define ProjectGuid "{{414e2267-764a-4e61-aa5e-f25f407dfafd}"
#endif
#ifndef AssemblyName
  #define AssemblyName "OfficeRibbonXEditor"
#endif
#ifndef AssemblyTitle
  #define AssemblyTitle "Office RibbonX Editor"
#endif
#ifndef Authors
  #define Authors "Fernando Andreu"
#endif
#ifndef ProjectUrl
  #define ProjectUrl "https://github.com/fernandreu/office-ribbonx-editor"
#endif
#ifndef Description
  #define Description "A tool to edit the Custom UI part of Office documents."
#endif
#ifndef Copyright
  #define Copyright 'Copyright (c) ' + GetDateTimeString('yyyy', '', '') + ' ' + Authors
#endif
#ifndef ExeName
  #define ExeName AssemblyName + '.exe'
#endif
#ifndef InputFolder
  #define InputFolder "../../src/OfficeRibbonXEditor/bin/Release/net6.0-windows"
#endif
#ifndef OutputFolder
  #define OutputFolder "../../src/OfficeRibbonXEditor/bin/Installer/net6.0-windows"
#endif
#ifndef VersionPrefix
  #define VersionPrefix GetFileVersion(InputFolder + '/' + ExeName)
#endif
#ifndef SupportedArchitectures
  #define SupportedArchitectures x64 arm64
#endif


[Setup]
AppId={#ProjectGuid}
AppName={#AssemblyTitle}
AppVersion={#VersionPrefix}
AppVerName={#AssemblyTitle}
AppPublisher={#Authors}
AppPublisherURL={#ProjectUrl}
AppSupportURL={#ProjectUrl}
AppUpdatesURL={#ProjectUrl}
DefaultDirName={autopf}\{#AssemblyTitle}
DefaultGroupName={#AssemblyTitle}
DisableProgramGroupPage=no
DisableDirPage=auto
LicenseFile=..\..\LICENSE
Compression=lzma
SolidCompression=yes
WizardStyle=modern
VersionInfoVersion={#VersionPrefix}
VersionInfoDescription={#Description}
VersionInfoTextVersion={#VersionPrefix}
VersionInfoCopyright={#Copyright}
PrivilegesRequired=admin
OutputDir={#OutputFolder}
OutputBaseFilename={#AssemblyName}
UninstallDisplayIcon={app}\{#ExeName}
AllowNoIcons=yes
ArchitecturesAllowed={#SupportedArchitectures}
ArchitecturesInstallIn64BitMode={#SupportedArchitectures}

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#InputFolder}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]  
Name: "{group}\{#AssemblyTitle}"; Filename: "{app}\{#ExeName}" 
Name: "{commondesktop}\{#AssemblyTitle}"; Filename: "{app}\{#ExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#ExeName}"; Description: "Launch application"; Flags: postinstall nowait skipifsilent unchecked

[Code]
      
function ProcessUninstallPath(Path: String): String;
var     
  sRet: String;
  sUnInstPath: String;
begin
  sUnInstPath := ExpandConstant(Path);
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sRet) then
    RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sRet);
   Result:=sRet;
end;

function GetUninstallString(Guid: String): String;
var
  sRet : String;
begin      
  sRet := '';
  if sRet = '' then sRet := ProcessUninstallPath('SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\' + Guid);
  if sRet = '' then sRet := ProcessUninstallPath('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + Guid); 
  if sRet = '' then sRet := ProcessUninstallPath('SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\' + Guid + '_is1');
  if sRet = '' then sRet := ProcessUninstallPath('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + Guid + '_is1');
  Result:=sRet;
end;

function UnInstallOldVersion(Guid: String): Integer;
var
  sUnInstallString: String;
  sArgs: String;       
  iPos: Integer;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString(Guid);
  if sUnInstallString <> '' then begin           
    sUnInstallString := RemoveQuotes(sUninstallString);
    iPos := Pos('MSIEXEC.EXE', Uppercase(sUnInstallString));
    if iPos > 0 then begin
      // Old Wix installers (msi-based) will be like: 'MsiExec.exe {ProjectGuid}
      // We need to insert some arguments before the Guid (or any other argument)       
      sArgs := '/QUIET /PASSIVE ' + Copy(sUnInstallString, iPos+12, 500);
      sUnInstallString := Copy(sUnInstallString, 1, iPos+11);
    end else begin
      // New InnoSetup installers simply call their custom unins000.exe with no arguments
      sArgs := '/VERYSILENT /NORESTART /SUPPRESSMSGBOXES';
    end;
    if Exec(sUnInstallString, sArgs,'', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then begin
    // Attempt to uninstall all previous release versions        
    UnInstallOldVersion('{{40AA9F04-AA99-4F5A-A883-30ECBD75D60D}');  //1.1.0    
    UnInstallOldVersion('{{3AAD5FAB-246D-4EF6-8A52-B177281C31F0}');  //1.2.0     
    UnInstallOldVersion('{{9A16BB52-0C41-498F-95B7-A1D81CBB3693}');  //1.3.0
    UnInstallOldVersion('{{61E6E348-06D4-42C3-8334-62ED500E6B73}');  //1.4.0
    UnInstallOldVersion('{{59D5E1A2-B8EF-4252-A0F4-8EE644C9ADC9}');  //1.5.0  
    UnInstallOldVersion('{{64D3EECC-2267-4AB2-A8A3-4530B38768C3}');  //1.5.1                                 
    UnInstallOldVersion('{{E15B8EBB-C04E-4537-8187-C626529EEBEA}');  //1.6.0                                     
    UnInstallOldVersion('{#ProjectGuid}');  //1.6.* and above
  end;
end;