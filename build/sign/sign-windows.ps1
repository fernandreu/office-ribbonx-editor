$_SIGN_TOOL = 'C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe'

$_CERT_NAME = 'Open Source Developer, Fernando Andreu'

function Get-IsSuitable {
    param(
        [Object] $File,
        [string[]] $Filters
    )

    foreach ($filter in $Filters) {
        if ($File.Name -like $filter) {
            return $true
        }
    }

    return $false
}

function Get-Files {
    param(
        [string] $Source,
        [string[]] $Filters
    )

    $files = [System.Collections.Generic.List[string]]@()
    foreach ($file in (Get-ChildItem -Path $Source -File -Recurse)) {
        if (Get-IsSuitable -File $file -Filters $Filters) {
            $files.Add("`"$file`"") | Out-Null
        }
    }

    return $files
}

function Set-Signatures {
    param(
        [string] $Source
    )

    # Only performing sha256 signatures for now. Dlls might need to be left untouched to speed things up
    $files = Get-Files -Source $Source -Filters *.msi,*.exe,*.dll
    if ($files.Count -ne 0) {
        Write-Host "Found $($files.Count) files to single-sign"
        $fileList = $files -join ' '
        $arguments = "sign /n `"$_CERT_NAME`" /tr http://time.certum.pl /fd sha256 /a $fileList"
        Start-Process -FilePath "$_SIGN_TOOL" -NoNewWindow -Wait -ArgumentList $arguments
    } else {
        Write-Host 'Found no files to single-sign'
    }
}

If ($MyInvocation.InvocationName -ne '.') {
    Set-Signatures $args[0]
}