@echo off
cls

echo Creating the module zip file..
powershell -command "Compress-Archive -Path .\ClientResources, .\Views\, .\module.config -DestinationPath .\modules\_protected\Verndale.LogViewer\Verndale.LogViewer.zip -CompressionLevel Optimal -Force"
echo.

echo Creating NuGet package..
echo.
nuget pack LogViewer.csproj -Build -Properties Configuration=Release -Verbosity detailed

echo.
echo.
echo Done