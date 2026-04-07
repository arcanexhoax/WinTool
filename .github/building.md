# Build and Test WinTool

Use this when I need to build or test WinTool locally.

## Build

- Do not use `dotnet build`. SDK MSBuild fails here because the app project uses `COMReference` and hits `MSB4803`.
- Build the solution with the full Visual Studio MSBuild.

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe' WinTool.slnx /p:Configuration=Debug /p:Platform="Any CPU" /nologo /v:minimal
```

Do not pass `Platform=x64` for the solution. `WinTool.slnx` builds with `Any CPU`, not `Debug|x64`.

## Test

- Do not run plain `dotnet test` from the repository root. It rebuilds with SDK MSBuild and fails on `COMReference` with `MSB4803`.
- Build first with the full Visual Studio MSBuild, then run only the test project with `--no-build`.

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe' WinTool.slnx /t:Restore,Build /p:Configuration=Release /p:RuntimeIdentifier=win-x64 /nologo /v:minimal
dotnet test tests/WinTool.Tests/WinTool.Tests.csproj -c Release /p:RuntimeIdentifier=win-x64 --no-build
```