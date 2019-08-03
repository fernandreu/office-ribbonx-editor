if ($args.Count -lt 6) {
    Write-Host "##vso[task.LogIssue type=error;]Not enough arguments passed"
    exit 1
}

$branch = [string]$args[0]
$stagingDir = [string]$args[1]
$version = [string]$args[2]
$version = $version.Substring(1)
$pat = [string]$args[3]
$name = [string]$args[4]
$email = [string]$args[5]

# Check if installer and exe were correctly generated
if (-not (Test-Path "$stagingDir\OfficeRibbonXEditor.exe")) {
    Write-Host "##vso[task.LogIssue type=error;]Missing executable in staging directory"
    exit 1
} elseif (-not (Test-Path "$stagingDir\OfficeRibbonXEditor.msi")) {
    Write-Host "##vso[task.LogIssue type=error;]Missing installer in staging directory"
    exit 1
}

# TODO: The following steps are deprecated, as latest release version is now read directly 
# from the GitHub API. This is only kept for legacy purposes (i.e. until enough time 
# passes for users to update to a more recent tool). Once this is removed, the corresponding
# pipeline variables will no longer be needed.

if (-not $branch.StartsWith("refs/tags/v")) {
    Write-Host "Not a release build; skipping remaining steps"
    exit 0
}

# Check if all necessary variables were passed correctly
if ($version.Length -eq 0) {
    Write-Host "##vso[task.LogIssue type=error;]The version is missing"
    exit 1
} elseif ($pat.Length -eq 0) {
    Write-Host "##vso[task.LogIssue type=error;]The GitHub PAT is missing"
    exit 1
} elseif ($name.Length -eq 0) {
    Write-Host "##vso[task.LogIssue type=error;]The GitHub name is missing"
    exit 1
} elseif ($email.Length -eq 0) {
    Write-Host "##vso[task.LogIssue type=error;]The GitHub email is missing"
    exit 1
}

New-Item -Path "tmp" -ItemType "directory"
cd tmp
git clone --single-branch --branch info https://github.com/fernandreu/office-ribbonx-editor.git -q
cd office-ribbonx-editor

Write-Host "Version passed is: $version"
$version | Out-File -Encoding UTF8 -FilePath .\RELEASE-VERSION
git config --global user.email "$email"
git config --global user.name "$name"
git commit -a -m "Update release version to $version" -q
git push https://$($pat)@github.com/fernandreu/office-ribbonx-editor.git info -q
