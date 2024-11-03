param(
    [Parameter(Mandatory=$true)]
    [string]$version, 

    [Parameter(Mandatory=$true)]
    [string]$nugetApiKey
)

$PSNativeCommandUseErrorActionPreference = $true
$ErrorActionPreference = 'Stop'

dotnet build -c Release -p:Version=$version

dotnet paket pack "nuget" --version $version

foreach($file in (Get-ChildItem "nuget" -Recurse -Include *.nupkg)) {
	dotnet nuget push $file --api-key $nugetApiKey --source https://api.nuget.org/v3/index.json
}
