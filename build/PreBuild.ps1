function ExtractVariable([string[]]$lines, [string]$propertyName) {
    $value = ($lines | Select-String -Pattern "<$propertyName>(.*)</$propertyName>").Matches.Groups[1].Value
    Write-Host "- ${$propertyName}: $value"
    Write-Host "##vso[task.setvariable variable=$propertyName;]$value"
}

$buildId = $args[0]
Write-Host "Build ID is: $buildId"
Get-ChildItem "src/OfficeRibbonXEditor/OfficeRibbonXEditor.csproj" |
ForEach-Object {
    $c = ($_ | Get-Content -encoding UTF8)
    $c = $c -replace '(<VersionPrefix>\d+\.\d+\.\d+)\.(\d)(</VersionPrefix>)', "`$1.$buildId`$3"
    $joined = $c -join "`r`n"
    Write-Host "Resulting project:`n$joined"
    [IO.File]::WriteAllText($_.FullName, $joined)

    Write-Host "Extracted variables:"
    ExtractVariable $c 'AssemblyName'
    ExtractVariable $c 'AssemblyTitle'
    ExtractVariable $c 'Authors'
    ExtractVariable $c 'Copyright'
    ExtractVariable $c 'Description'
    ExtractVariable $c 'PackageProjectUrl'
}