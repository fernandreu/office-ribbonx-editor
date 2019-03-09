Write-Host "Build number is: $($args[0])"
# Assembly version has length limitations, so we need to recombine this differently
$parts = $args[0].Split(".")
$tmp = $parts[0]
$parts[0] = $parts[0].Substring(2, 4)
$parts[1] = "$($tmp.Substring(6,2))$($parts[1].PadLeft(3, '0'))"
Write-Host "Split into MSBuildNumber=$($parts[0]); MSRevision=$($parts[1])"
Write-Host "##vso[task.setvariable variable=MSBuildNumber;]$($parts[0])"
Write-Host "##vso[task.setvariable variable=MSRevision;]$($parts[1])"
Get-ChildItem "SharedAssemblyInfo.cs" |
ForEach-Object {
    $c = ($_ | Get-Content -encoding Unicode)
    $c = $c -replace '(AssemblyVersion\(\"\d?.\d?)\.(\*)(\"\))', "`$1.$($parts[0]).$($parts[1])`$3"
    $joined = $c -join "`r`n"
    # Write-Host "Resulting assembly:`n$joined"
    [IO.File]::WriteAllText($_.FullName, $joined)
}