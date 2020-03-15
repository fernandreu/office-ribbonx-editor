function ExtractVariable([string[]]$Lines, [string]$PropertyName) {
    $value = ($Lines | Select-String -Pattern "<$PropertyName>(.*)</$PropertyName>").Matches.Groups[1].Value
    Write-Host "- ${$PropertyName}: $value"
    Write-Host "##vso[task.setvariable variable=$PropertyName;]$value"
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
    ExtractVariable -Lines $c -PropertyName 'AssemblyName'
    ExtractVariable -Lines $c -PropertyName 'AssemblyTitle'
    ExtractVariable -Lines $c -PropertyName 'Authors'
    ExtractVariable -Lines $c -PropertyName 'Copyright'
    ExtractVariable -Lines $c -PropertyName 'Description'
    ExtractVariable -Lines $c -PropertyName 'PackageProjectUrl'
}