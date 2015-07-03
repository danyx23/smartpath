#
# Script1.ps1
#
msbuild .\SmartPath.csproj /t:Build /p:Configuration=Release /p:TargetFramework=v4.0
nuget pack .\SmartPath.csproj -Prop Configuration=Release