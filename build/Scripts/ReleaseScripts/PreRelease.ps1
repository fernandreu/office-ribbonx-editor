$sourcePath = $args[0]
$base64signer = $args[1]
$signerPassword = $args[2]

Write-Host "Source path: '$sourcePath'"
Write-Host "Signer: '$base64signer'"
Write-Host "Signer password: '$signerPassword'"

# Install the certificate as an authority in this agent to ensure the signature validation step is accurate
$signerPath = "signer.pfx"
$bytes = [System.Convert]::FromBase64String($base64Signer)
[System.IO.File]::WriteAllBytes($signerPath, $bytes)
$securePassword = ConvertTo-SecureString -String $SignerPassword -Force -AsPlainText
try {
    Import-PfxCertificate -FilePath $signerPath -Password $securePassword -CertStoreLocation "Cert:\LocalMachine\Root"
} finally {
    Remove-Item -Path $signerPath
}

# Do the signed artifacts exist?
$artifactsToCheck = @(
    'Framework Dependent - x64 - Binaries';
    'Framework Dependent - arm64 - Binaries';
    'Self-Contained - x64 - Binaries';
    'Self-Contained - arm64 - Binaries'
    'Framework Dependent - x64 - Installer';
    'Framework Dependent - arm64 - Installer';
    'Self-Contained - x64 - Installer';
    'Self-Contained - arm64 - Installer'
)

$path = $null

$missing = [System.Collections.Generic.List[string]]@()
foreach ($artifact in $artifactsToCheck) {
    $path = "$sourcePath/$artifact/OfficeRibbonXEditor.exe"
    if (-not (Test-Path $path -PathType Leaf)) {
        $missing.Add($artifact) | Out-Null
    }
}

if ($missing.Count -ne 0) {
    $message = "The following artifacts were not found: $($missing -join ', ')"
    Write-Host "##vso[task.LogIssue type=error;] $message"
    exit 1
}

$missing = [System.Collections.Generic.List[string]]@()
foreach ($artifact in $artifactsToCheck) {
    $path = "$sourcePath/$artifact/OfficeRibbonXEditor.exe"
    if ((Get-AuthenticodeSignature -FilePath $path).Status -ne 'Valid') {
        $missing.Add($artifact) | Out-Null
    }
}

if ($missing.Count -ne 0) {
    $message = "The following artifacts contained no signature or it was invalid: $($missing -join ', ')"
    Write-Host "##vso[task.LogIssue type=error;] $message"
    exit 1
}

# Find assembly version from any artifact (i.e. the last one tested above)
$version = (Get-Item $path | Select-Object -ExpandProperty VersionInfo).FileVersion
$versionParts = $version.Split(".")
$versionParts = $versionParts[0..($versionParts.Length - 2)]
$version = $versionParts -join "."

Write-Host "Resulting three-digit version: $version"
Write-Host "##vso[task.setvariable variable=ThreeDigitVersion;]$version"
