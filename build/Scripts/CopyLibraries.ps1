$buildConfiguration = $args[0]
$targetFramework = $args[1]
# TODO: If possible, the following should also be passed via a build variable
$winPath = $env:WINDIR
$dllPath = "$winPath/Microsoft.NET/Framework64/v4.0.30319/WPF/PresentationFramework.dll"
if (Test-Path $dllPath -PathType Leaf) {
    Write-Host "Found dll to be copied in: $dllPath"
} else {
    $message = "Cannot find dll to be copied in: $dllPath"
    Write-Host "$("##vso[task.setvariable variable=ErrorMessage]") $message"
    exit 1
}
Get-ChildItem -Path "$PSScriptRoot/../../tests" -Directory | 
ForEach-Object { 
    $destination = Join-Path $_.FullName "bin/$buildConfiguration/$targetFramework/"
    Write-Host "Copying dll to folder: $destination"
    if (Test-Path $destination -PathType Container) {
        Copy-Item $dllPath $destination
    } else {
        Write-Output "Folder does not exist. Skipping..."
    }
}