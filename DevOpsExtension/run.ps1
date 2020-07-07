$env:INPUT_ProjectFileName="TestApplication.csproj"
$env:INPUT_VulnerabilityAction="TestApplication.csproj"

Set-Location '..'
$env:BUILD_SOURCESDIRECTORY=Get-Location
Set-Location 'DevOpsExtension'

Set-Location '.\buildAndReleaseTask' 
tsc
node index.js
Set-Location '..'