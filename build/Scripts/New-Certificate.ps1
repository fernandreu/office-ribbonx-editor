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
        [string] $PublicKeyPath
    )

    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject "CN=$Subject" `
        -FriendlyName $FriendlyName `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -KeyUsage DigitalSignature `
        -NotAfter (Get-Date).AddYears(5) `
        -CertStoreLocation "Cert:\CurrentUser\My"

    # We don't actually need the certificate in the store, at least for this job. Remove it from there
    Get-ChildItem Cert:\CurrentUser\My\$($cert.Thumbprint) | Remove-Item

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
