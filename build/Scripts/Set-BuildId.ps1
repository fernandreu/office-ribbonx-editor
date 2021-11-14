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
        Export-Variable -Lines $c -PropertyName 'AssemblyName'
        Export-Variable -Lines $c -PropertyName 'AssemblyTitle'
        Export-Variable -Lines $c -PropertyName 'Authors'
        Export-Variable -Lines $c -PropertyName 'Copyright'
        Export-Variable -Lines $c -PropertyName 'Description'
        Export-Variable -Lines $c -PropertyName 'PackageProjectUrl'
        Export-Variable -Lines $c -PropertyName 'VersionPrefix'
    }
}

Set-BuildId @args
