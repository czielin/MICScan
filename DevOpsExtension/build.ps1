# Update package build number
$extensionJson = Get-Content '.\vss-extension.json' -Raw | ConvertFrom-Json
$version = $extensionJson.version
$versionParts = $version.Split('.')
$buildNumber = [int]$versionParts[$versionParts.Length - 1]
$buildNumber = $buildNumber + 1
$versionParts[$versionParts.Length - 1] = $buildNumber.ToString()
$version = [string]::Join('.', $versionParts)
$extensionJson.version = $version
$extensionJson | ConvertTo-Json -Depth 100 | Set-Content '.\vss-extension.json'

# Update task build number
$taskJson = Get-Content '.\buildAndReleaseTask\task.json' -Raw | ConvertFrom-Json
$taskJson.version.Patch = $buildNumber
$taskJson | ConvertTo-Json -Depth 100 | Set-Content '.\buildAndReleaseTask\task.json'

# Build task
Set-Location .\buildAndReleaseTask
tsc 
Set-Location ..

# Package
tfx extension create --manifest-globs vss-extension.json