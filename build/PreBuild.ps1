$buildId = $args[0]
Write-Host "Build ID is: $buildId"
Get-ChildItem "src/OfficeRibbonXEditor/OfficeRibbonXEditor.csproj" |
ForEach-Object {
    $c = ($_ | Get-Content -encoding UTF8)
    $c = $c -replace '(<VersionPrefix>\d+.\d+.\d+)\.(\d)(</VersionPrefix>)', "`$1.$buildId`$3"
    $joined = $c -join "`r`n"
    Write-Host "Resulting project:`n$joined"
    [IO.File]::WriteAllText($_.FullName, $joined)
}