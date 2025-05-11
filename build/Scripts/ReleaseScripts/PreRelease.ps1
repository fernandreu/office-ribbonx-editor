$sourcePath = $args[0]

# Do the signed artifacts exist?
$artifactsToCheck = @(
    'Framework Dependent - x64';
    'Framework Dependent - arm64';
    'Self-Contained - x64';
    'Self-Contained - arm64'
    'Framework Dependent - x64 - Installer';
    'Framework Dependent - arm64 - Installer';
    'Self-Contained - x64 - Installer';
    'Self-Contained - arm64 - Installer'
)
$missing = [System.Collections.Generic.List[string]]@()
$path = $null
foreach ($artifact in $artifactsToCheck) {
    $path = "$sourcePath/$artifact/OfficeRibbonXEditor.exe"
    if (-not (Test-Path $path -PathType Leaf) -or (Get-AuthenticodeSignature -FilePath $path).Status -ne 'Valid') {
        $missing.Add($path) | Out-Null
    }
}

if ($missing.Count -ne 0) {
    $message = "The following artifacts were not found or were not signed correctly: $($missing -join ', ')"
    Write-Host "##vso[task.LogIssue type=error;] $message"
    exit 1
}

# Find assembly version from any artifact (i.e. the last one tested above)
$version = (Get-Item $path | Select-Object -ExpandProperty VersionInfo).FileVersion
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
