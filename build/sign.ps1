[CmdletBinding(PositionalBinding=$false)]
param (

    [string]$signTool = 'C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe',

    [string]$certName = 'Open Source Developer, Fernando Andreu',

    [Parameter(Mandatory=$true, ValueFromRemainingArguments=$true)]
    [string[]]$files
)

# sha256 signing

# Dual signing only for supported files
$filtered = @($files.where{ $_ -match '.exe' })
if ($filtered.Count -gt 0) {
    $fileList = @($filtered | ForEach-Object { "`"$_`"" }) -join ' '
    $arguments = "sign /n `"$certName`" /t http://time.certum.pl/ /fd sha1 /v $fileList"
    Start-Process -FilePath "$signTool" -NoNewWindow -Wait -ArgumentList $arguments

    $fileList = @($filtered | ForEach-Object { "`"$_`"" }) -join ' '
    $arguments = "sign /n `"$certName`" /tr http://time.certum.pl/ /fd sha256 /as /v $fileList"
    Start-Process -FilePath "$signTool" -NoNewWindow -Wait -ArgumentList $arguments
}

# sha256 signing for everything else
$filtered = @($files.where{ -not ($_ -match '.exe') })
if ($filtered.Count -gt 0) {
    $fileList = @($filtered | ForEach-Object { "`"$_`"" }) -join ' '
    $arguments = "sign /n `"$certName`" /tr http://time.certum.pl/ /fd sha256 /v $fileList"
    Start-Process -FilePath "$signTool" -NoNewWindow -Wait -ArgumentList $arguments
}
