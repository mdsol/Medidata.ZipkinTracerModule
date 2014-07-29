"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe" "Medidata.ZipkinTracerModule.csproj" /p:Configuration=Release /t:Clean
"..\.nuget\nuget.exe" pack Medidata.ZipkinTracerModule.nuspec
"..\.nuget\nuget.exe" pack Medidata.ZipkinTracerModule.csproj -Build -Properties Configuration=Release