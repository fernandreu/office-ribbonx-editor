# Signs a local file by connecting to a remote machine and running osslsigncode in there

function Set-SignatureLocally {
    [CmdletBinding()]
    [OutputType([bool])]
    param([string]$Path, [string]$Pin, [string]$destination = '')

    $FileInfo = Get-Item -Path $Path
    Copy-Item -Path $Path -Destination "/tmp/$($FileInfo.Name)"
    if ($LASTEXITCODE -ne 0) {
        return $false
    }

    $resultingName = "$($FileInfo.BaseName)-Signed$($FileInfo.Extension)"
    $commonArgs = "-pkcs11engine /usr/lib/x86_64-linux-gnu/engines-1.1/pkcs11.so -pkcs11module /opt/proCertumCardManager/sc30pkcs11-2.0.0.39.r2-MS.so -certs ~/codesign.spc -t http://time.certum.pl -pass $Pin"
    if ($FileInfo.Extension -eq ".exe") {
        # Perform a dual signature
        . osslsigncode $commonArgs -h sha1 -in "/tmp/$($FileInfo.Name)" -out "/tmp/$resultingName.tmp" | Write-Host
        . osslsigncode $commonArgs -nest -h sha2 -in "/tmp/$resultingName.tmp" -out "/tmp/$resultingName" | Write-Host
    } else {
        # Use only SHA256 signature (as it might not be possible to dual-sign the file)
        . osslsigncode $commonArgs -h sha2 -in "/tmp/$($FileInfo.Name)" -out "/tmp/$resultingName" | Write-Host
    }
    if ($LASTEXITCODE -ne 0) {
        return $false
    }
    
    if ($destination.Length -eq 0) {
        # Replace existing file
        $destination = $Path
    }

    Move-Item -Path "/tmp/$resultingName" -Destination "$destination"
    if ($LASTEXITCODE -ne 0) {
        return $false
    }

    return $true
}

function Update-AllFiles {
    [CmdletBinding()]
    [OutputType([bool])]
    param ([string]$folder, [string]$Pin, [int]$TimeoutSeconds = 30)
    Write-Host "Folder passed: $folder"
    $any = $false
    $files = Get-ChildItem $folder -Recurse -File
    foreach ($file in $files) {
        Write-Host "File: $($file.Name)"
        if ($file.Extension -ne '.exe' -and $file.Extension -ne '.msi') {
            continue
        }

        Write-Host "File to be processed: $($file.Name)"

        $job = Start-Job -ScriptBlock ${function:Set-SignatureLocally} -ArgumentList @($file.FullName, $Pin)
        if (Wait-Job $job -Timeout $TimeoutSeconds) {
            try {
                $result = Receive-Job $job -ErrorAction Stop
            } catch {
                $result = $false
            }
        }
        else {
            $result = $false
        }
        Remove-Job $job -Force

        if (-not $result) {
            return $false
        }
        
        Write-Host "Setting result to $true"
        $any = $true
    }

    return $any
}
