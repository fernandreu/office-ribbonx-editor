function New-Certificate {
    [Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "")]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Subject,

        [Parameter(Mandatory = $true)]
        [string] $FriendlyName,

        [Parameter(Mandatory = $true)]
        [string] $VariableName,

        [Parameter(Mandatory = $true)]
        [string] $CertificatePassword,

        [Parameter()]
        [string] $PublicKeyPath,

        [Parameter()]
        [string] $Base64Signer,

        [Parameter()]
        [string] $SignerPassword
    )

    $signer = $null
    if (-not [string]::IsNullOrEmpty($Base64Signer)) {
        $signerPath = "signer.pfx"
        $bytes = [System.Convert]::FromBase64String($Base64Signer)
        [System.IO.File]::WriteAllBytes($signerPath, $bytes)
        $securePassword = ConvertTo-SecureString -String $SignerPassword -Force -AsPlainText
        try {
            $signer = Get-PfxCertificate -FilePath $signerPath -Password $securePassword -NoPromptForPassword
            Import-PfxCertificate -FilePath $signerPath -Password $securePassword -CertStoreLocation "Cert:\CurrentUser\My" -Exportable
        } finally {
            Remove-Item -Path $signerPath
        }
    }

    try {
        # Pipelines should take around ~5 min, so a 60min lifespan is more than enough. It does no matter
        # if the certificate has expired by the time the user checks it, as long as it is timestamped    
        $cert = New-SelfSignedCertificate `
            -Type CodeSigningCert `
            -Subject "CN=$Subject" `
            -FriendlyName $FriendlyName `
            -KeyAlgorithm RSA `
            -KeyLength 2048 `
            -KeyUsage DigitalSignature `
            -KeyExportPolicy ExportableEncrypted `
            -NotAfter (Get-Date).AddMinutes(60) `
            -CertStoreLocation "Cert:\CurrentUser\My" `
            -Signer $signer
    } finally {
        if ($null -ne $signer) {
            Get-ChildItem Cert:\CurrentUser\My\$($signer.Thumbprint) | Remove-Item
        }

        # We don't actually need the certificate in the store, at least for this job. Remove it from there
        Get-ChildItem Cert:\CurrentUser\My\$($cert.Thumbprint) | Remove-Item
    }
    
    # Export it as PFX
    $certPath = 'cert.pfx'
    $securePassword = ConvertTo-SecureString -String $CertificatePassword -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $securePassword | Out-Null

    $bytes = Get-Content $certPath -AsByteStream -Raw
    $base64 = [System.Convert]::ToBase64String($bytes)
    Remove-Item -Path $certPath

    Write-Host "##vso[task.setvariable variable=$VariableName;issecret=true]$base64"

    if (-not [string]::IsNullOrEmpty($PublicKeyPath)) {
        Export-Certificate -Cert $cert -FilePath $PublicKeyPath -Type CERT | Out-Null
    }
}

New-Certificate @args
