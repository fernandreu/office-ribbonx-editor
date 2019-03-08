Write-Host "Build number is: $($args[0])"
$parts = $args[0].Split(".")
Write-Host "Split into MSBuildNumber=$($parts[0]); MSRevision=$($parts[1])"
Write-Host "##vso[task.setvariable variable=MSBuildNumber;]$($parts[0])"
Write-Host "##vso[task.setvariable variable=MSRevision;]$($parts[1])"
