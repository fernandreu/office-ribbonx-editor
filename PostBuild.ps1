New-Item -Path "tmp" -ItemType "directory"
cd tmp
git clone --single-branch --branch info https://github.com/fernandreu/wpf-custom-ui-editor.git -q
cd wpf-custom-ui-editor
$version = [string]$args[0]
$pat = [string]$args[1]
Write-Host "Version passed is: $version"
$version | Out-File -Encoding UTF8 -FilePath .\RELEASE-VERSION
git commit -a -m "Update release version to $version" -q
git push https://$($pat)@github.com/fernandreu/wpf-custom-ui-editor.git info -q