# Signs a local file by connecting to a remote machine and running osslsigncode in there
function RemoteSign {
    [OutputType([bool])]
    param([string]$hostname, [string]$path, [string]$destination = '', [string]$pin = '')

    if ($pin.Length -eq 0) {
        $pin = $env:SIGNCODE_PIN
    }

    $fileInfo = Get-Item $path
    & scp -q "$path" "$($hostname):/tmp/$($fileInfo.Name)"
    if ($LASTEXITCODE -ne 0) {
        return $false
    }

    $resultingName = "$($fileInfo.BaseName)-Signed$($fileInfo.Extension)"
    $commonArgs = "-pkcs11engine /usr/lib/x86_64-linux-gnu/engines-1.1/pkcs11.so -pkcs11module /opt/proCertumCardManager/sc30pkcs11-2.0.0.39.r2-MS.so -certs ~/codesign.spc -t http://time.certum.pl -pass $pin"
    if ($fileInfo.Extension -eq ".exe") {
        # Perform a dual signature
        [string[]]$commands = @(
            "osslsigncode $commonArgs -h sha1 -in `"/tmp/$($fileInfo.Name)`" -out `"/tmp/$resultingName.tmp`"",
            "osslsigncode $commonArgs -nest -h sha2 -in `"/tmp/$resultingName.tmp`" -out `"/tmp/$resultingName`""
        )
        & ssh $hostname ($commands -join " && ")

        # Another option: shorter, but it shows a welcome message unless the server settings are changed 
        # $commands | & ssh $hostname

        # Another option
        # & ssh $hostname osslsigncode $commonArgs -h sha1 -in "/tmp/$($fileInfo.Name)" -out "/tmp/tmp-$resultingName"
        # & ssh $hostname osslsigncode $commonArgs -nest -h sha2 -in "/tmp/tmp-$resultingName" -out "/tmp/$resultingName"
    } else {
        # Use only SHA256 signature (as it might not be possible to dual-sign the file)
        & ssh $hostname osslsigncode $commonArgs -h sha2 -in "/tmp/$($fileInfo.Name)" -out "/tmp/$resultingName"
    }
    if ($LASTEXITCODE -ne 0) {
        return $false
    }
    if ($destination.Length -eq 0) {
        $destination = Join-Path $fileInfo.Directory $resultingName
    }
    & scp -q "$($hostname):/tmp/$resultingName" "$destination"
    if ($LASTEXITCODE -ne 0) {
        return $false
    }
    & ssh $hostname "rm -f `"/tmp/$($fileInfo.Name)`" && rm -f `"/tmp/$resultingName*`""
    return $true
}

# Attempts to produce a signed version of the given file
function ProcessSingleFile {
    param ([string]$path, [string]$suffix = '-Signed')
    $fileInfo = Get-Item $path
    Write-Output "File to be processed: $($fileInfo.Name)"
    # TODO
}

function ProcessAllFiles {
    param ([string]$folder)
    $files = Get-ChildItem $folder
    $files | ForEach-Object {
        ProcessSingleFile $_.FullName
    } 
}

# if (!(RemoteSign 'fernando@ferando-gigabyte' 'C:\Users\FernA\Downloads\OfficeRibbonXEditor.exe')) {
#     Write-Output 'No.'
# }
