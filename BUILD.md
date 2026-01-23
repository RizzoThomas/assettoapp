# Build Instructions for AssettoApp

## Prerequisites

### Required Software

1. **.NET 8.0 SDK** or later
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **Windows 10/11** (x64)
   - Required for WPF and memory-mapped file support

3. **(Optional) Visual Studio 2022** or **Visual Studio Code**
   - Visual Studio: Community edition is free
   - VS Code: Install C# extension

## Building from Command Line

### 1. Clone the Repository

```bash
git clone https://github.com/RizzoThomas/assettoapp.git
cd assettoapp
```

### 2. Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages.

### 3. Build Debug Version

```bash
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Debug
```

Output: `AssettoApp.UI/bin/Debug/net8.0-windows/`

### 4. Build Release Version

```bash
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Release
```

Output: `AssettoApp.UI/bin/Release/net8.0-windows/`

### 5. Run the Application

```bash
cd AssettoApp.UI/bin/Release/net8.0-windows
./AssettoApp.UI.exe
```

## Publishing as Standalone .EXE

### Option 1: Self-Contained (Includes .NET Runtime)

**Advantages**: Users don't need .NET installed
**Disadvantages**: Larger file size (~150MB)

```bash
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:EnableCompressionInSingleFile=true
```

Output: `AssettoApp.UI/bin/Release/net8.0-windows/win-x64/publish/AssettoApp.UI.exe`

### Option 2: Framework-Dependent (Requires .NET 8)

**Advantages**: Smaller file size (~5MB)
**Disadvantages**: Users must have .NET 8 installed

```bash
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj \
    -c Release \
    -r win-x64 \
    --self-contained false \
    -p:PublishSingleFile=true
```

Output: `AssettoApp.UI/bin/Release/net8.0-windows/win-x64/publish/AssettoApp.UI.exe`

### Option 3: Optimized Self-Contained

Enable ReadyToRun compilation for faster startup:

```bash
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:EnableCompressionInSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=false
```

**Note**: Trimming is disabled because WPF requires full assembly.

## Building with Visual Studio

### 1. Open Solution

1. Launch Visual Studio 2022
2. Open `AssettoApp.slnx`
3. Wait for project restoration

### 2. Set Startup Project

1. Right-click `AssettoApp.UI` in Solution Explorer
2. Select "Set as Startup Project"

### 3. Build

- **Debug**: Press `F5` or click "Start"
- **Release**: 
  1. Configuration dropdown → "Release"
  2. Build → Build Solution (`Ctrl+Shift+B`)

### 4. Publish

1. Right-click `AssettoApp.UI` → "Publish"
2. Create new publish profile
3. Target: Folder
4. Configuration: Release | Any CPU | win-x64
5. Deployment Mode: Self-contained
6. File publish options:
   - ☑ Produce single file
   - ☑ Enable ReadyToRun compilation
7. Click "Publish"

## Building with VS Code

### 1. Open Project

```bash
code assettoapp
```

### 2. Install Extensions

- C# (Microsoft)
- .NET Extension Pack

### 3. Build Tasks

Create `.vscode/tasks.json`:

```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/AssettoApp.UI/AssettoApp.UI.csproj",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "publish",
            "command": "dotnet",
            "type": "process",
            "args": [
                "publish",
                "${workspaceFolder}/AssettoApp.UI/AssettoApp.UI.csproj",
                "-c", "Release",
                "-r", "win-x64",
                "--self-contained", "true",
                "-p:PublishSingleFile=true"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

### 4. Run Tasks

- `Ctrl+Shift+B` → Select "build"
- `Ctrl+Shift+P` → "Tasks: Run Task" → "publish"

## Distribution

### Creating a ZIP Package

```bash
# Navigate to publish folder
cd AssettoApp.UI/bin/Release/net8.0-windows/win-x64/publish

# Create ZIP (PowerShell)
Compress-Archive -Path * -DestinationPath AssettoApp-v1.0.0-win-x64.zip
```

### Creating an Installer (Inno Setup)

1. Download Inno Setup: https://jrsoftware.org/isinfo.php
2. Create `installer.iss`:

```ini
[Setup]
AppName=AssettoApp
AppVersion=1.0.0
DefaultDirName={pf}\AssettoApp
DefaultGroupName=AssettoApp
OutputBaseFilename=AssettoApp-Setup
Compression=lzma2
SolidCompression=yes

[Files]
Source: "AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\AssettoApp"; Filename: "{app}\AssettoApp.UI.exe"
Name: "{commondesktop}\AssettoApp"; Filename: "{app}\AssettoApp.UI.exe"

[Run]
Filename: "{app}\AssettoApp.UI.exe"; Description: "Launch AssettoApp"; Flags: postinstall nowait skipifsilent
```

3. Compile with Inno Setup Compiler

## Troubleshooting Build Issues

### Issue: "SDK not found"

**Solution**: Install .NET 8.0 SDK
```bash
winget install Microsoft.DotNet.SDK.8
```

### Issue: "EnableWindowsTargeting error"

**Solution**: Building WPF on non-Windows requires special configuration. Build on Windows or use CI/CD.

### Issue: "NuGet restore failed"

**Solution**: Clear NuGet cache
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

### Issue: "Project file corrupted"

**Solution**: Regenerate project
```bash
dotnet new wpf -n AssettoApp.UI -f net8.0 --force
# Re-add project references
```

### Issue: "LiveCharts compatibility warning"

This is expected - LiveCharts 0.9.7 targets .NET Framework but works in .NET 8.0. Safe to ignore.

## Build Verification

After building, verify the executable:

```bash
# Check file exists
ls AssettoApp.UI/bin/Release/net8.0-windows/win-x64/publish/AssettoApp.UI.exe

# Check dependencies (PowerShell)
dumpbin /dependents AssettoApp.UI.exe

# Run smoke test
./AssettoApp.UI.exe
```

Expected behavior:
1. Window opens
2. Status shows "Ready"
3. Game selector populated
4. No crashes on startup

## Continuous Integration

### GitHub Actions Example

Create `.github/workflows/build.yml`:

```yaml
name: Build AssettoApp

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Release
    
    - name: Publish
      run: dotnet publish AssettoApp.UI/AssettoApp.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: AssettoApp-Windows
        path: AssettoApp.UI/bin/Release/net8.0-windows/win-x64/publish/
```

## Performance Optimization

### Build Optimizations

For fastest build times:
```bash
dotnet build -c Release /m:8 /p:UseSharedCompilation=true
```

Where `/m:8` uses 8 parallel build processes (adjust for your CPU).

### Runtime Optimizations

In `.csproj`:
```xml
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishTrimmed>false</PublishTrimmed>
  <TieredCompilation>true</TieredCompilation>
  <TieredCompilationQuickJit>true</TieredCompilationQuickJit>
</PropertyGroup>
```

## Version Management

Update version in `AssettoApp.UI.csproj`:

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <FileVersion>1.0.0.0</FileVersion>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
</PropertyGroup>
```

## Release Checklist

Before releasing:

- [ ] Update version numbers
- [ ] Build Release configuration
- [ ] Test on clean Windows install
- [ ] Verify all dependencies included
- [ ] Test with Assetto Corsa
- [ ] Create release notes
- [ ] Tag Git commit
- [ ] Create GitHub release
- [ ] Upload compiled binaries

## Getting Help

If you encounter build issues:

1. Check Prerequisites are met
2. Try clean rebuild: `dotnet clean && dotnet build`
3. Check GitHub Issues
4. Open new issue with:
   - OS version
   - .NET SDK version
   - Full error message
   - Build command used
