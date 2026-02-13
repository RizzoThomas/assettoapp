"""
Build Script for ACE Telemetry Application
Compiles the Python app into a standalone .exe using PyInstaller
"""

import os
import sys
import shutil
import subprocess
from pathlib import Path


def clean_build():
    """Remove previous build artifacts"""
    print("Cleaning previous builds...")
    
    dirs_to_remove = ['build', 'dist', '__pycache__']
    for dir_name in dirs_to_remove:
        if os.path.exists(dir_name):
            shutil.rmtree(dir_name)
            print(f"  Removed: {dir_name}/")
    
    # Remove .spec file
    spec_file = "ACETelemetry.spec"
    if os.path.exists(spec_file):
        os.remove(spec_file)
        print(f"  Removed: {spec_file}")


def build_exe():
    """Build the executable with PyInstaller"""
    print("\nBuilding ACETelemetry.exe...")
    
    # PyInstaller command
    cmd = [
        "pyinstaller",
        "--onefile",
        "--windowed",
        "--name", "ACETelemetry",
        "--add-data", "src;src",
        "--hidden-import", "customtkinter",
        "--hidden-import", "PIL",
        "--hidden-import", "PIL._tkinter_finder",
        "--hidden-import", "win32file",
        "--hidden-import", "win32con",
        "--hidden-import", "loguru",
        "--uac-admin",
        "--clean",
        "src/main.py"
    ]
    
    # Add icon if it exists
    icon_path = Path("assets/icon.ico")
    if icon_path.exists():
        cmd.extend(["--icon", str(icon_path)])
    
    # Run PyInstaller
    result = subprocess.run(cmd, capture_output=True, text=True)
    
    if result.returncode == 0:
        print("✅ Build successful!")
        print(f"\nExecutable location: dist/ACETelemetry.exe")
        
        # Check file size
        exe_path = Path("dist/ACETelemetry.exe")
        if exe_path.exists():
            size_mb = exe_path.stat().st_size / (1024 * 1024)
            print(f"File size: {size_mb:.1f} MB")
    else:
        print("❌ Build failed!")
        print("\nError output:")
        print(result.stderr)
        return False
    
    return True


def main():
    """Main build process"""
    print("="*60)
    print("ACE Telemetry - Build Script")
    print("="*60)
    
    # Check if PyInstaller is installed
    try:
        import PyInstaller
        print(f"PyInstaller version: {PyInstaller.__version__}")
    except ImportError:
        print("❌ PyInstaller not found!")
        print("Install with: pip install pyinstaller")
        sys.exit(1)
    
    # Clean previous builds
    clean_build()
    
    # Build executable
    success = build_exe()
    
    if success:
        print("\n" + "="*60)
        print("Build complete! 🎉")
        print("="*60)
        print("\nTo run the application:")
        print("  dist\\ACETelemetry.exe")
        print("\nTo test:")
        print("  1. Launch Assetto Corsa EVO")
        print("  2. Enter a session")
        print("  3. Run ACETelemetry.exe")
        print("  4. Click 'Connect to ACE'")
    else:
        print("\nBuild failed. Check error messages above.")
        sys.exit(1)


if __name__ == "__main__":
    main()
