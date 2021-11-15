function New-RootCertificate {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Subject,

        [Parameter(Mandatory = $true)]
        [string] $FriendlyName,

        [Parameter(Mandatory = $true)]
        [securestring] $CertificatePassword,

        [Parameter()]
        [int] $MonthLifespan = 120
    )

    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject "CN=$Subject" `
        -FriendlyName $FriendlyName `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -KeyUsage CertSign,CRLSign,DigitalSignature `
        -KeyExportPolicy ExportableEncrypted `
        -NotAfter (Get-Date).AddMonths($MonthLifespan) `
        -CertStoreLocation "Cert:\CurrentUser\My"

    # Unlike for New-Certificate.ps1, we will leave the certificate in the store as a backup

    # Export it as PFX
    $certPath = 'cert.pfx'
    Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $CertificatePassword | Out-Null

    $bytes = Get-Content $certPath -AsByteStream -Raw
    $base64 = [System.Convert]::ToBase64String($bytes)
    Remove-Item -Path $certPath

    return $base64
}
