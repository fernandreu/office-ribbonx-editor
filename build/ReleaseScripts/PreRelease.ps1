$sourcePath = $args[0]

# Do the signed artifacts exist?
$allSigned = $true;
if (-Not (Test-Path "$sourcePath/.NET Framework Executable/OfficeRibbonXEditor.exe")) {
    $allSigned = $false
} elseif (-Not (Test-Path "$sourcePath/.NET Framework Installer/OfficeRibbonXEditor.exe")) {
    $allSigned = $false
} elseif (-Not (Test-Path "$sourcePath/.NET Core Binaries/OfficeRibbonXEditor/OfficeRibbonXEditor.exe")) {
    $allSigned = $false
} elseif (-Not (Test-Path "$sourcePath/.NET Core Installer/OfficeRibbonXEditor.exe")) {
    $allSigned = $false
}

if (-Not $allSigned)
{
    $message = "Not all necessary signed artifacts were found"
    Write-Host "##vso[task.LogIssue type=error;] $message"
    exit 1
}

# Find assembly version from any artifact
$version = (Get-Item "$sourcePath/.NET Framework Executable/OfficeRibbonXEditor.exe" | Select-Object -ExpandProperty VersionInfo).FileVersion
$versionParts = $version.Split(".")
$versionParts = $versionParts[0..($versionParts.Length - 2)]
$version = $versionParts -join "."

# Find version of latest release
$uri = "https://api.github.com/repos/fernandreu/office-ribbonx-editor/releases/latest"
$previousVersion = (Invoke-RestMethod -Uri $uri).tag_name
$previousVersionParts = $previousVersion.Substring(1).Split(".")
while ($previousVersionParts.Count -lt 3) {
    $previousVersionParts += "0"
}

# Is new version more recent?
$moreRecent = $false
for ($i = 0; $i -lt 3; $i++) {
    if ([int]$versionParts[$i] -gt [int]$previousVersionParts[$i]) {
        $moreRecent = $true
    }
}

if (-Not $moreRecent) {
    $message = "Assembly verison $version is not more recent than latest GitHub release version $previousVersion"
    Write-Host "##vso[task.LogIssue type=error;] $message"
    exit 1
}

Write-Host "Resulting three-digit version: $version"
Write-Host "##vso[task.setvariable variable=ThreeDigitVersion;]$version"
