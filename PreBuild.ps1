Write-Host "Build number is: $($args[0])"
$parts = $args[0].Split(".")
Write-Host "Split into MSBuildNumber=$($parts[0]); MSRevision=$($parts[1])"
Write-Host "##vso[task.setvariable variable=MSBuildNumber;]$($parts[0])"
Write-Host "##vso[task.setvariable variable=MSRevision;]$($parts[1])"
Get-ChildItem "SharedAssemblyInfo.cs" |
ForEach-Object {
    $c = ($_ | Get-Content)
    $c = $c -replace '(AssemblyVersion\(\"\d?.\d?)\.(\*)(\"\))', "`$1.$($parts[0]).$($parts[1])`$3"
    [IO.File]::WriteAllText($_.FullName, ($c -join "`r`n"))
}