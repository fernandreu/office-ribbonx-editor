$buildConfiguration = $args[0]
$targetFramework = $args[1]

$runtimes = @(dotnet --list-runtimes)

# The command above should produce an output in the following format:
# Microsoft.AspNetCore.All 2.1.26 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
# ...
# Microsoft.WindowsDesktop.App 5.0.4 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
# Microsoft.WindowsDesktop.App 6.0.0 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]

# We will search for the highest version of Microsoft.WindowsDesktop.App
$runtimeVersion = "0.0.0"
$runtimePath = $null

foreach ($line in $runtimes) {
    $collection = ($line | Select-String -Pattern "Microsoft\.WindowsDesktop\.App (.*?) \[(.*?)\]")
    if ($null -eq $collection -or $collection.Matches.Length -eq 0) {
        continue
    }

    $match = $collection.Matches[0]
    if ($match.Groups[1].Value -lt $runtimeVersion) {
        continue
    }

    $runtimeVersion = $match.Groups[1].Value
    $runtimePath = $match.Groups[2].Value
}

if ($null -eq $runtimePath) {
    $message = "Cannot find a suitable Microsoft.WindowsDesktop.App runtime"
    Write-Host "$("##vso[task.setvariable variable=ErrorMessage]") $message"
    exit 1
}

$runtimePath = Join-Path $runtimePath $runtimeVersion
Write-Host "Found target runtime: $runtimePath"

$fileNames = @('PresentationFramework.dll', 'WindowsBase.dll')
foreach ($fileName in $fileNames) {
    $dllPath = Join-Path $runtimePath $fileName

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
}
