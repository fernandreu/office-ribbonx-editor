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
Get-ChildItem "SharedAssemblyInfo.cs" |
ForEach-Object {
    $c = ($_ | Get-Content -encoding UTF8)
    if ($version -and -not ($c -match [Regex]::Escape("AssemblyVersion(`"$($version.Substring(1)).*`")"))) {
        $error = "Tag version $version does not coincide with assembly version"
        Write-Host "$("##vso[task.setvariable variable=ErrorMessage]") $error"
        exit 1
    }
    $c = $c -replace '(AssemblyVersion\(\"\d+.\d+.\d+)\.(\d)(\"\))', "`$1.$buildId`$3"
    $joined = $c -join "`r`n"
    Write-Host "Resulting assembly:`n$joined"
    [IO.File]::WriteAllText($_.FullName, $joined)
}