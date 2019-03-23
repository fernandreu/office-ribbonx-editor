New-Item -Path "tmp" -ItemType "directory"
cd tmp
git clone --single-branch --branch info https://github.com/fernandreu/office-ribbonx-editor.git -q
cd office-ribbonx-editor
$version = [string]$args[0]
$version = $version.Substring(1)
$pat = [string]$args[1]
$name = [string]$args[2]
$email = [string]$args[3]
Write-Host "Version passed is: $version"
$version | Out-File -Encoding UTF8 -FilePath .\RELEASE-VERSION
git config --global user.email "$email"
git config --global user.name "$name"
git commit -a -m "Update release version to $version" -q
git push https://$($pat)@github.com/fernandreu/office-ribbonx-editor.git info -q
