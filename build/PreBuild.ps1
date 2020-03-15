$buildId = $args[0]
Write-Host "Build ID is: $buildId"
Get-ChildItem "src/OfficeRibbonXEditor/OfficeRibbonXEditor.csproj" |
ForEach-Object {
    $c = ($_ | Get-Content -encoding UTF8)
    $c = $c -replace '(<VersionPrefix>\d+\.\d+\.\d+)\.(\d)(</VersionPrefix>)', "`$1.$buildId`$3"
    $joined = $c -join "`r`n"
    $match = $c | Select-String -Pattern '<VersionPrefix>(\d+\.\d+\.\d+\.\d+)</VersionPrefix>'
    Write-Host "Resulting project:`n$joined"
    $version = $match.Matches.Groups[1].Value
    Write-Host "Resulting version: $version"
    Write-Host "##vso[task.setvariable variable=VersionPrefix;]$version"
    [IO.File]::WriteAllText($_.FullName, $joined)
}