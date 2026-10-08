dotnet build Expressif.sln -c Release --nologo 
dotnet test -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --no-build --nologo
dotnet reportgenerator "-reports:Expressif.Core.Testing\coverage.*.opencover.xml;Expressif.Library.Testing\coverage.*.opencover.xml" "-targetdir:.\.coverage"
start .coverage/index.html
