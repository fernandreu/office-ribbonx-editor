$_SIGN_TOOL = 'C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe'

$_CERT_NAME = 'Open Source Developer, Fernando Andreu'

function Set-Signatures {
    param(
        [string] $Sources
    )

    $fileList = @(Get-ChildItem -Path $Source -Include *.exe,*.dll | ForEach-Object { "`"$($_.FullName)`"" }) -join ' '
    $arguments = "sign /n `"$_CERT_NAME`" /t http://time.certum.pl/ /fd sha1 /v $fileList"
    Start-Process -FilePath "$_SIGN_TOOL" -NoNewWindow -Wait -ArgumentList $arguments
    $arguments = "sign /n `"$_CERT_NAME`" /tr http://time.certum.pl/ /fd sha256 /as /v $fileList"
    Start-Process -FilePath "$_SIGN_TOOL" -NoNewWindow -Wait -ArgumentList $arguments

    $fileList = @(Get-ChildItem -Path $Source -Include *.msi | ForEach-Object { "`"$($_.FullName)`"" }) -join ' '
    $arguments = "sign /n `"$_CERT_NAME`" /tr http://time.certum.pl/ /fd sha256 /as /v $fileList"
    Start-Process -FilePath "$_SIGN_TOOL" -NoNewWindow -Wait -ArgumentList $arguments
}

If ($MyInvocation.InvocationName -ne '.') {
    Set-Signatures $args[0]
}