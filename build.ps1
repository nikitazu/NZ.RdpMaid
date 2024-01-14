# Build
#

[CmdletBinding()]
Param (
    [Switch]
    $Publish
)

# Settings
#

$AppName = "NZ.RdpMaid.App"
$SolutionPath = "./src/NZ.RdpMaid.sln"
$ProjectPath = "./src/NZ.RdpMaid.App/NZ.RdpMaid.App.csproj"
$PublishPath = "./publish"
$PublishCurrentPath = "$PublishPath/current"

# Run
#

if ($Publish)
{
    # Detect Version

    $Version = (Select-Xml -Path $ProjectPath -XPath '/Project/PropertyGroup/Version').Node.InnerXml

    if (-not $Version)
    {
        throw 'В файле проекта отсутствует тег <Version>'
    }
    
    $PackageName = "$($AppName)-v$($Version).zip"

    Write-Host "Version = $Version"
    Write-Host "[Build]"
    dotnet build $SolutionPath

    Write-Host "[Prepare publish directory]"
    if (Test-Path -Path $PublishCurrentPath)
    {
        Remove-Item $PublishCurrentPath -Recurse -Force
    }

    Write-Host "[Copy publish files]"
    Copy-Item "./src/$AppName/bin/Debug" $PublishCurrentPath -Recurse
    Copy-Item "./README.md" $PublishCurrentPath
    Copy-Item "./CHANGELOG.md" $PublishCurrentPath
    Copy-Item "./doc/*.*" $PublishCurrentPath

    Write-Host "[Create package]"
    Compress-Archive -Path "$PublishCurrentPath/*" -DestinationPath "$PublishPath/$PackageName" -Force
}
