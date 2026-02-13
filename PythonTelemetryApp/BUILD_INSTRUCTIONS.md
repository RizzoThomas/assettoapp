# 🔨 Build Instructions - ACE Telemetry

## Creating Standalone Executable with PyInstaller

This guide explains how to compile the Python application into a single `.exe` file with administrator privileges.

---

## Prerequisites

1. **Python 3.10+** installed
2. **All dependencies** installed: `pip install -r requirements.txt`
3. **PyInstaller** installed: `pip install pyinstaller==6.3.0`

---

## Method 1: Using Build Script (Recommended)

### Windows

```bash
cd PythonTelemetryApp
python build.py
```

The script will:
- Clean previous builds
- Run PyInstaller with optimal settings
- Create `dist/ACETelemetry.exe`

---

## Method 2: Manual PyInstaller Command

### Basic Command

```bash
pyinstaller --onefile --windowed --name ACETelemetry src/main.py
```

### Full Command with All Options

```bash
pyinstaller \
    --onefile \
    --windowed \
    --name ACETelemetry \
    --add-data "src;src" \
    --hidden-import customtkinter \
    --hidden-import PIL \
    --hidden-import win32file \
    --hidden-import win32con \
    --hidden-import loguru \
    --uac-admin \
    --icon=assets/icon.ico \
    --clean \
    src/main.py
```

---

## PyInstaller Flags Explained

### Core Flags

| Flag | Purpose |
|------|---------|
| `--onefile` | Bundle everything into a single .exe file |
| `--windowed` | No console window (GUI only) |
| `--name` | Output executable name |
| `--clean` | Clean PyInstaller cache before building |

### Data and Imports

| Flag | Purpose |
|------|---------|
| `--add-data "src;src"` | Include source directory |
| `--hidden-import` | Force include module (fixes import errors) |

### Admin Privileges

| Flag | Purpose |
|------|---------|
| `--uac-admin` | Request administrator privileges on startup |

**Why Admin?**
- Shared memory access requires elevated permissions
- ACE process memory reading needs admin rights

### Optional Flags

| Flag | Purpose |
|------|---------|
| `--icon=icon.ico` | Custom application icon |
| `--noconsole` | Same as --windowed |
| `--onedir` | Create folder with dependencies (alternative to --onefile) |

---

## Build Process

### Step 1: Prepare Environment

```bash
cd PythonTelemetryApp

# Clean old builds
rmdir /s /q build dist
del ACETelemetry.spec

# Install dependencies
pip install -r requirements.txt
```

### Step 2: Run PyInstaller

```bash
pyinstaller --onefile --windowed --uac-admin --name ACETelemetry src/main.py
```

### Step 3: Locate Output

Output file: `dist/ACETelemetry.exe`

Size: ~20-30 MB (includes Python runtime and all dependencies)

### Step 4: Test

```bash
# Run the exe
dist\ACETelemetry.exe

# Check it requests admin privileges
# Verify it can connect to ACE
```

---

## Customizing the Build

### Custom Icon

1. Create or download `.ico` file (256x256 recommended)
2. Save as `assets/icon.ico`
3. Add flag: `--icon=assets/icon.ico`

### Include Additional Files

```bash
# Include assets directory
--add-data "assets;assets"

# Include config files
--add-data "config.ini;."
```

### Debug Build (with Console)

For troubleshooting:
```bash
pyinstaller --onefile --console --name ACETelemetry_Debug src/main.py
```

This shows console output and errors.

---

## Troubleshooting Build Issues

### Error: "Module not found"

**Solution**: Add hidden import
```bash
--hidden-import module_name
```

**Common hidden imports**:
- `customtkinter`
- `PIL._tkinter_finder`
- `win32file`
- `win32con`

### Error: "Failed to execute script"

**Causes**:
1. Missing dependency
2. Import error
3. File not found

**Solutions**:
1. Build with console to see error: `--console`
2. Add missing imports: `--hidden-import`
3. Include data files: `--add-data`

### Error: "Cannot access shared memory"

**Cause**: Not running as administrator

**Solution**: Ensure `--uac-admin` flag is used

### Large File Size (>50 MB)

**Normal**: Includes entire Python runtime

**Reduce size**:
1. Use `--onedir` instead (creates folder with DLLs)
2. Use UPX compression (PyInstaller option)
3. Remove unused imports

---

## Advanced Options

### UPX Compression

Compress executable with UPX:

1. Download UPX: https://upx.github.io/
2. Place `upx.exe` in PATH
3. Build with: `--upx-dir=path/to/upx`

Result: 30-50% size reduction

### Spec File Customization

After first build, edit `ACETelemetry.spec`:

```python
# ACETelemetry.spec
a = Analysis(
    ['src/main.py'],
    pathex=[],
    binaries=[],
    datas=[('src', 'src')],
    hiddenimports=['customtkinter', 'win32file'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=None,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=None)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='ACETelemetry',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    uac_admin=True,  # Request admin
)
```

Build from spec:
```bash
pyinstaller ACETelemetry.spec
```

---

## Distribution

### Single File Distribution

1. Copy `dist/ACETelemetry.exe` to target PC
2. Run as administrator
3. No Python installation required

### Installer Package

Create installer with:
- **Inno Setup** (Windows)
- **NSIS** (Nullsoft Scriptable Install System)

Benefits:
- Professional installation experience
- Start menu shortcuts
- Uninstaller
- Version management

---

## Testing the Build

### Basic Tests

1. **Launch Test**
   ```
   Double-click ACETelemetry.exe
   Should request admin privileges
   Should open GUI window
   ```

2. **Connection Test**
   ```
   Launch ACE
   Enter session
   Click "Connect to ACE"
   Should show "Connected"
   ```

3. **Telemetry Test**
   ```
   Drive around
   Verify speed, RPM update
   Check G-forces display
   Monitor tire temperatures
   ```

4. **Setup Test**
   ```
   Click setup button (Safe/Balanced/Aggressive)
   Check Documents/Assetto Corsa EVO/savedata/setups/
   Verify .json file created
   ```

5. **Lock-Up Test**
   ```
   Brake hard to lock wheels
   Verify lock-up counter increments
   Check log for lock-up events
   ```

### Advanced Tests

- **Multi-lap**: Drive 10+ laps, verify memory doesn't leak
- **Session change**: Change track, verify data resets
- **Disconnect/reconnect**: Test connection stability

---

## Continuous Integration

For automated builds:

```yaml
# .github/workflows/build.yml
name: Build EXE

on: [push]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-python@v2
        with:
          python-version: '3.10'
      - run: pip install -r requirements.txt
      - run: pip install pyinstaller
      - run: pyinstaller --onefile --windowed --uac-admin src/main.py
      - uses: actions/upload-artifact@v2
        with:
          name: ACETelemetry
          path: dist/ACETelemetry.exe
```

---

## Version Management

Include version in build:

1. Update `src/__init__.py`:
   ```python
   __version__ = "1.0.0"
   ```

2. Add to PyInstaller:
   ```bash
   --version-file=version.txt
   ```

3. Create `version.txt`:
   ```
   VSVersionInfo(
     ffi=FixedFileInfo(
       filevers=(1, 0, 0, 0),
       prodvers=(1, 0, 0, 0),
     ),
     StringFileInfo([
       StringTable(
         '040904b0',
         [StringStruct('CompanyName', 'AssettoApp'),
          StringStruct('FileDescription', 'ACE Telemetry'),
          StringStruct('FileVersion', '1.0.0'),
          StringStruct('ProductName', 'ACE Telemetry'),
          StringStruct('ProductVersion', '1.0.0')]
       )
     ]),
     VarFileInfo([VarStruct('Translation', [1033, 1200])])
   )
   ```

---

## Final Checklist

Before distributing:

- [ ] Build completes without errors
- [ ] Exe runs without console
- [ ] Requests admin privileges
- [ ] Connects to ACE successfully
- [ ] All features work
- [ ] Logs are written properly
- [ ] Setup files save correctly
- [ ] No crashes during 30-minute test
- [ ] Tested on clean Windows install
- [ ] README.md is included

---

## Support

For build issues:
1. Check PyInstaller logs in `build/` directory
2. Try debug build with `--console`
3. Verify all dependencies in requirements.txt
4. Check Python version compatibility

---

**Happy Building! 🔨**
