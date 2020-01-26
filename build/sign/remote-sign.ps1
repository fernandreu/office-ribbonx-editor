# Signs a local file by connecting to a remote machine and running osslsigncode in there

function Set-SignatureRemotely {
    [CmdletBinding()]
    [OutputType([bool])]
    param([string]$Path, [string]$HostName, [string]$Pin, [string]$Port, [string]$destination = '')

    $FileInfo = Get-Item -Path $Path
    & scp -P "$Port" -q "$Path" "$($HostName):/tmp/$($FileInfo.Name)" | Write-Host
    if ($LASTEXITCODE -ne 0) {
        return $false
    }

    $resultingName = "$($FileInfo.BaseName)-Signed$($FileInfo.Extension)"
    $commonArgs = "-pkcs11engine /usr/lib/x86_64-linux-gnu/engines-1.1/pkcs11.so -pkcs11module /opt/proCertumCardManager/sc30pkcs11-2.0.0.39.r2-MS.so -certs ~/codesign.spc -t http://time.certum.pl -pass $Pin"
    if ($FileInfo.Extension -eq ".exe") {
        # Perform a dual signature
        [string[]]$commands = @(
            "osslsigncode $commonArgs -h sha1 -in `"/tmp/$($FileInfo.Name)`" -out `"/tmp/$resultingName.tmp`"",
            "osslsigncode $commonArgs -nest -h sha2 -in `"/tmp/$resultingName.tmp`" -out `"/tmp/$resultingName`""
        )
        & ssh $HostName -p "$Port" ($commands -join " && ") | Write-Host

        # Another option: shorter, but it shows a welcome message unless the server settings are changed 
        # $commands | & ssh $HostName

        # Another option
        # & ssh $HostName osslsigncode $commonArgs -h sha1 -in "/tmp/$($FileInfo.Name)" -out "/tmp/tmp-$resultingName"
        # & ssh $HostName osslsigncode $commonArgs -nest -h sha2 -in "/tmp/tmp-$resultingName" -out "/tmp/$resultingName"
    } else {
        # Use only SHA256 signature (as it might not be possible to dual-sign the file)
        & ssh $HostName -p "$Port" osslsigncode $commonArgs -h sha2 -in "/tmp/$($FileInfo.Name)" -out "/tmp/$resultingName" | Write-Host
    }
    if ($LASTEXITCODE -ne 0) {
        return $false
    }
    
    if ($destination.Length -eq 0) {
        # Replace existing file
        $destination = $Path
    }

    & scp -P "$Port" -q "$($HostName):/tmp/$resultingName" "$destination" | Write-Host
    if ($LASTEXITCODE -ne 0) {
        return $false
    }

    & ssh $HostName -p $Port "rm -f `"/tmp/$($FileInfo.Name)`" && rm -f `"/tmp/$resultingName*`"" | Write-Host
    return $true
}

function Update-AllFiles {
    [CmdletBinding()]
    [OutputType([bool])]
    param ([string]$folder, [string]$HostName, [string]$Pin, [string]$Port, [int]$TimeoutSeconds = 30)
    $any = $false
    $files = Get-ChildItem $folder -Recurse -File
    foreach ($file in $files) {
        if ($file.Extension -ne '.exe' -and $file.Extension -ne '.msi') {
            continue
        }

        Write-Host "File to be processed: $($file.Name)"

        $job = Start-Job -ScriptBlock ${function:Set-SignatureRemotely} -ArgumentList @($file.FullName, $HostName, $Pin, $Port)
        if (Wait-Job $job -Timeout $TimeoutSeconds) {
            $result = Receive-Job $job
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
