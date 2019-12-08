[CmdletBinding(PositionalBinding=$false)]
param (

    [string]$organization = 'fernandreu',

    [string]$project = 'office-ribbonx-editor',

    [string]$release = '',

    [string]$Username = 'fernandreu',

    [string]$Password = ''
)

function Get-BasicAuthCreds {
    param([string]$Username,[string]$Password)
    $AuthString = "{0}:{1}" -f $Username,$Password
    $AuthBytes  = [System.Text.Encoding]::Ascii.GetBytes($AuthString)
    return [Convert]::ToBase64String($AuthBytes)
}

if ($Password -eq '') {
    $SecurePassword = Read-Host "Please enter the GitHub password for user $($Username)" -AsSecureString
    $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword))
}
$BasicCreds = Get-BasicAuthCreds -Username $Username -Password $Password
$Headers = @{Authorization="Basic $BasicCreds"}
Remove-Variable Password

if ($release -eq '') {
    # Find the latest draft release
    $releases = Invoke-RestMethod -Method Get -Uri "https://api.github.com/repos/$organization/$project/releases" -Headers $Headers
    ForEach ($item in $releases) {
        $isDraft = [System.Convert]::ToBoolean($item.draft)
        if ($isDraft) {
            $Response = $item
            break
        }
    }

    if ($null -eq $Response) {
        Write-Output "No suitable draft release found. Please specify one manually (draft or not)"
        exit
    }
} else {
    $URI = "https://api.github.com/repos/$organization/$project/releases/$release"
    $Response = Invoke-RestMethod -Method Get -Uri $URI -Headers $Headers
}

# Create the temporary folder where all files will be downloaded
$folderParent = [System.IO.Path]::GetTempPath()
[string] $folderName = [System.Guid]::NewGuid()
$folder = Join-Path $folderParent $folderName
New-Item -ItemType Directory -Path $folder | Out-Null

Write-Output "Temporary folder: $folder"

Add-Type -AssemblyName System.IO.Compression.FileSystem
try {

    $DownloadHeaders = @{Authorization="Basic $BasicCreds"; Accept='application/octet-stream'}
    # Download all assets and unzip them if necessary
    ForEach ($asset in $Response.assets) {
        $OutputPath = Join-Path $folder $asset.name
        Invoke-WebRequest "https://api.github.com/repos/$organization/$project/releases/assets/$($asset.id)" -OutFile $OutputPath -Headers $DownloadHeaders
        if ($asset.name -match '.zip') {
            $OutputFolder = Join-Path $folder ([System.IO.Path]::GetFileNameWithoutExtension($OutputPath))
            [System.IO.Compression.ZipFile]::ExtractToDIrectory($OutputPath, $OutputFolder)
            Remove-Item $OutputPath
        }
    }

    # Sign all .exe and .msi files found in the temp folder (recursively to capture anything unzipped)
    $exeTargets = @(Get-ChildItem -Path $folder -Filter *.exe -Recurse -ErrorAction SilentlyContinue -Force | ForEach-Object {$_.FullName})
    $msiTargets = @(Get-ChildItem -Path $folder -Filter *.msi -Recurse -ErrorAction SilentlyContinue -Force | ForEach-Object {$_.FullName})
    $targets = $exeTargets + $msiTargets
    if ($targets.Count -eq 0) {
        exit
    }
    & .\sign.ps1 -files $targets

    # Zip files back
    ForEach ($subFolder in (Get-ChildItem -Path $folder -Directory -ErrorAction SilentlyContinue -Force)) {
        [System.IO.Compression.ZipFile]::CreateFromDirectory($subFolder.FullName, "$($subFolder.FullName).zip")
        Remove-Item -Path $subFolder.FullName -Recurse
    }

    # Finally, replace the original assets with the new ones
    # TODO: Confirm first that the entire process went well
    ForEach ($asset in $Response.assets) {
        # TODO: Skip files that were not signed
        Invoke-RestMethod -Method Delete -Uri "https://api.github.com/repos/$organization/$project/releases/assets/$($asset.id)" -Headers $Headers
        $OutputPath = Join-Path $folder $asset.name
        $sanitized = [uri]::EscapeDataString($asset.name)
        $UploadHeaders = @{Authorization="Basic $BasicCreds"}
        $Uri = "https://uploads.github.com/repos/$organization/$project/releases/$($response.id)/assets?name=$sanitized"
        if ($asset.name -match '.zip') {
            $ContentType = 'application/zip'
        } elseif ($asset.name -match '.msi') {
            $ContentType = 'application/x-ole-storage'
        } elseif ($asset.name -match '.exe') {
            $ContentType = 'application/vnd.microsoft.portable-executable'
        } else {
            $ContentType = $asset.content_type
        }
        Invoke-WebRequest -Method Post -Uri $Uri -InFile $OutputPath -ContentType $ContentType -Headers $UploadHeaders | Out-Null
    }
} finally {
    Remove-Item -Path $folder -Recurse
}
