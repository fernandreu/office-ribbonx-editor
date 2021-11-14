function Export-Variable {
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Lines,

        [Parameter(Mandatory = $true)]
        [string] $PropertyName
    )
    $value = ($Lines | Select-String -Pattern "<$PropertyName>(.*)</$PropertyName>").Matches.Groups[1].Value
    Write-Host "- $($PropertyName): $value"
    Write-Host "##vso[task.setvariable variable=Extracted$PropertyName;]$value"
}

function Set-BuildId {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ProjectFile,

        [Parameter(Mandatory = $true)]
        [int] $BuildId
    )

    Write-Host "Build ID is: $BuildId"
    Get-ChildItem $ProjectFile |
    ForEach-Object {
        $c = ($_ | Get-Content -encoding UTF8)
        $c = $c -replace '(<VersionPrefix>\d+\.\d+\.\d+)\.(\d)(</VersionPrefix>)', "`$1.$BuildId`$3"
        $joined = $c -join "`r`n"
        Write-Host "Resulting project:`n$joined"
        [IO.File]::WriteAllText($_.FullName, $joined)
    
        Write-Host "Extracted variables:"
        Export-Variable $c 'AssemblyName'
        Export-Variable $c 'AssemblyTitle'
        Export-Variable $c 'Authors'
        Export-Variable $c 'Copyright'
        Export-Variable $c 'Description'
        Export-Variable $c 'PackageProjectUrl'
        Export-Variable $c 'VersionPrefix'
    }
}

Set-BuildId @args
