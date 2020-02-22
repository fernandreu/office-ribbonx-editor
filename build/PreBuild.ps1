$buildId = $args[0]
Write-Host "Build ID is: $buildId"
$version = [string]$args[1]
if ($version.StartsWith("refs/tags/v")) {
    $parts = $version.Substring(10).Split(".")
    while ($parts.Count -lt 3) {
        $parts += "0"
    }
    $version = $parts -join "."
    Write-Host "Resulting version: $version"
} else {
    Write-Host "No version detected"
    $version = ""
}
Write-Host "##vso[task.setvariable variable=ThreeDigitVersion;]$version"
Get-ChildItem "src/OfficeRibbonXEditor/OfficeRibbonXEditor.csproj" |
ForEach-Object {
    $c = ($_ | Get-Content -encoding UTF8)
    if ($version -and -not ($c -match [Regex]::Escape("<VersionPrefix>$($version.Substring(1)).0</VersionPrefix>"))) {
        $message = "Tag version $version does not coincide with project version"
        Write-Host "$("##vso[task.setvariable variable=ErrorMessage]") $message"
        exit 1
    }
    $c = $c -replace '(<VersionPrefix>\d+.\d+.\d+)\.(\d)(</VersionPrefix>)', "`$1.$buildId`$3"
    $joined = $c -join "`r`n"
    Write-Host "Resulting project:`n$joined"
    [IO.File]::WriteAllText($_.FullName, $joined)
}