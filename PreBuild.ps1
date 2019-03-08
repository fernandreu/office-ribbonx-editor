Write-Host "Build number is: $env:buildNumber"
$parts = $env:buildNumber.Split(".")
Write-Host "Split into MSBuildNumber=$parts[0]; MSRevision=$parts[1]"
Write-Host "##vso[task.setvariable variable=MSBuildNumber;]$parts[0]"
Write-Host "##vso[task.setvariable variable=MSBuildNumber;]$parts[1]"