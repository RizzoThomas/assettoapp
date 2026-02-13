"""
Main Application Entry Point
Assetto Corsa EVO - Python Telemetry Application (Fixed Version)
"""

import sys
import customtkinter as ctk
from loguru import logger

# Configure logger
logger.remove()  # Remove default handler
logger.add(
    "ace_telemetry.log",
    rotation="10 MB",
    retention="7 days",
    level="DEBUG",
    format="{time:YYYY-MM-DD HH:mm:ss} | {level: <8} | {name}:{function}:{line} - {message}"
)
logger.add(sys.stderr, level="INFO")

# Import modules
from telemetry.observer import TelemetryManager
from telemetry.lock_detector import LockUpDetector
from analysis.lap_comparison import LapComparison
from setup.setup_manager import SetupManager
from gui.main_window import MainWindow


def main():
    """Main application entry point"""
    logger.info("="*60)
    logger.info("ACE Telemetry Application Starting (Fixed Version)")
    logger.info("="*60)
    
    try:
        # NOTE: ACEConnector is now created inside MainWindow
        # This allows for better connection management
        
        # Initialize telemetry manager (Observer pattern)
        # We'll pass None as data_acquisition since MainWindow creates ACEConnector
        telemetry_manager = TelemetryManager(None)
        
        # Initialize lock-up detector
        lock_detector = LockUpDetector(slip_threshold=0.15, brake_threshold=0.05)
        
        # Initialize lap comparison
        lap_comparison = LapComparison(num_sectors=3)
        
        # Initialize setup manager
        setup_manager = SetupManager()
        
        # Set CustomTkinter appearance
        ctk.set_appearance_mode("dark")
        ctk.set_default_color_theme("blue")
        
        # Create and run GUI (with new ACE connector integrated)
        app = MainWindow(
            telemetry_manager=telemetry_manager,
            lock_detector=lock_detector,
            lap_comparison=lap_comparison,
            setup_manager=setup_manager
        )
        
        logger.info("GUI initialized with ACE connector, starting main loop")
        app.mainloop()
        
    except Exception as e:
        logger.exception(f"Fatal error in main application: {e}")
        sys.exit(1)
    
    finally:
        logger.info("Application shutting down")


if __name__ == "__main__":
    main()
