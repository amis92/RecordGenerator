## Creation

This project was created using the following steps:
* `dotnet new console -o QuickDemo`
* `cd QuickDemo`
* `dotnet add package Amadevus.RecordGenerator`
* Add `<DotNetCliToolReference Include="dotnet-codegen" Version="0.4.88" />` to `QuickDemo.csproj` in an `<ItemGroup>`
* Write `Program.cs`
* `dotnet run` should now print output similar to the following:

```
Pretty display: { Id = 1, Name = Adam Demo, Email = foo@bar.com, Birthday = 06.01.2020 23:17:06 }
Check equality: False
Check equality: True
```