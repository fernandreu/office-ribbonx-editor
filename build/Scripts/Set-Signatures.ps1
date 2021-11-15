$_SIGN_TOOL = 'C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe'

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

    foreach ($file in (Get-ChildItem -Path $Source -File -Recurse)) {
        if (Get-IsSuitable -File $file -Filters $Filters) {
            Write-Output $file
        }
    }
}

function Set-Signatures {
    [Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "")]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Source,

        [Parameter(Mandatory = $true)]
        [string] $Base64Certificate,

        [Parameter(Mandatory = $true)]
        [string] $CertificatePassword
    )

    $certPath = 'cert.pfx'
    $bytes = [System.Convert]::FromBase64String($Base64Certificate)
    [System.IO.File]::WriteAllBytes($certPath, $bytes)

    try {
        # Validate the certificate and password are correct
        $securePassword = ConvertTo-SecureString -String $CertificatePassword -AsPlainText -Force
        $cert = Get-PfxCertificate -FilePath $certPath -Password $securePassword -NoPromptForPassword
        
        # Only performing sha256 signatures for now. Dlls might need to be left untouched to speed things up
        $files = Get-Files -Source $Source -Filters *.msi,*.exe,*.dll
        Write-Host "Found $($files.Count) files to sign"
        foreach ($file in $files) {
            $signature = Get-AuthenticodeSignature -FilePath $file.FullName
            if ($signature.Status -eq 'Valid') {
                Write-Host "Skipping as it is already signed: $($file.FullName)"
                continue
            }
    
            & $_SIGN_TOOL sign /f $certPath /p $CertificatePassword /tr 'http://timestamp.digicert.com' /td sha256 /fd sha256 "$($file.FullName)" | Out-Null
            if ($LASTEXITCODE -ne 0) {
                $message = "signtool returned $LASTEXITCODE for file $($file.FullName)"
                Write-Host "##vso[task.setvariable variable=ErrorMessage]$message"
                throw $message
            }

            Write-Host "Signed: $($file.FullName)"
        }
    } finally {
        Remove-Item -Path $certPath
    }
}

Set-Signatures @args
