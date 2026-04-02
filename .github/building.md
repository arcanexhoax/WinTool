# Building WinTool

Use the full Visual Studio MSBuild for this project:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe' WinTool.slnx /p:Configuration=Debug /p:Platform="Any CPU" /nologo /v:minimal
```

Do not use `dotnet build` here. The project uses `COMReference`, and SDK MSBuild fails with `MSB4803` because `ResolveComReference` is not supported there.

Do not pass `Platform=x64` for the solution. `WinTool.slnx` builds with `Any CPU`, not `Debug|x64`.