dotnet build
dotnet test .\test\Dapper.FluentMap.Tests\Dapper.FluentMap.Tests.csproj
dotnet pack -c Release -o ..\..\artifacts
