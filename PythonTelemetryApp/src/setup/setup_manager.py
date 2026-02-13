"""
Setup Manager
Manages car setup profiles (Safe/Balanced/Aggressive)
"""

import json
import os
from pathlib import Path
from typing import Dict, Any, Optional
from loguru import logger


class SetupManager:
    """
    Manages car setup profiles for Assetto Corsa EVO
    """
    
    SETUP_TYPES = ["Safe", "Balanced", "Aggressive"]
    
    def __init__(self):
        self.setup_path = self._find_setup_directory()
        self.current_track = ""
        self.current_car = ""
    
    def _find_setup_directory(self) -> Optional[Path]:
        """Find ACE setup directory"""
        # Try standard Windows Documents location
        documents = Path.home() / "Documents"
        
        possible_paths = [
            documents / "Assetto Corsa EVO" / "savedata" / "setups",
            documents / "Assetto Corsa Evo" / "savedata" / "setups",
            documents / "AssettoCorساEVO" / "savedata" / "setups",
        ]
        
        for path in possible_paths:
            if path.exists():
                logger.info(f"Found ACE setup directory: {path}")
                return path
        
        # If not found, use first path anyway (will be created)
        logger.warning(f"ACE setup directory not found, will use: {possible_paths[0]}")
        return possible_paths[0]
    
    def set_track_and_car(self, track: str, car: str):
        """Set current track and car for setup operations"""
        self.current_track = track
        self.current_car = car
        logger.info(f"Setup context: Track={track}, Car={car}")
    
    def apply_setup(self, setup_type: str) -> bool:
        """
        Apply a setup profile to the current track/car
        
        Args:
            setup_type: One of "Safe", "Balanced", "Aggressive"
        
        Returns:
            True if successful, False otherwise
        """
        if setup_type not in self.SETUP_TYPES:
            logger.error(f"Invalid setup type: {setup_type}")
            return False
        
        if not self.current_track or not self.current_car:
            logger.error("Track and car must be set before applying setup")
            return False
        
        try:
            # Generate setup based on type
            setup_data = self._generate_setup(setup_type)
            
            # Save to file
            success = self._save_setup(setup_type, setup_data)
            
            if success:
                logger.info(f"Applied {setup_type} setup for {self.current_car} @ {self.current_track}")
            
            return success
            
        except Exception as e:
            logger.error(f"Failed to apply setup: {e}")
            return False
    
    def _generate_setup(self, setup_type: str) -> Dict[str, Any]:
        """Generate setup parameters based on type"""
        
        # Base template (these are example values - adjust for ACE)
        base_setup = {
            "car": self.current_car,
            "track": self.current_track,
            "setup_type": setup_type,
            "tyres": {
                "front_left_pressure": 26.0,
                "front_right_pressure": 26.0,
                "rear_left_pressure": 26.0,
                "rear_right_pressure": 26.0,
            },
            "suspension": {
                "front_spring_rate": 80000,
                "rear_spring_rate": 75000,
                "front_ride_height": 50,
                "rear_ride_height": 60,
                "front_damper_bump": 5,
                "front_damper_rebound": 7,
                "rear_damper_bump": 5,
                "rear_damper_rebound": 7,
            },
            "alignment": {
                "front_camber": -2.5,
                "rear_camber": -2.0,
                "front_toe": 0.0,
                "rear_toe": 0.2,
            },
            "aero": {
                "front_wing": 2,
                "rear_wing": 3,
            },
            "differential": {
                "preload": 50,
                "power_ramp": 60,
                "coast_ramp": 40,
            },
            "brake_balance": 58,
            "fuel": 50,
        }
        
        # Modify based on setup type
        if setup_type == "Safe":
            # Conservative setup for stability
            base_setup["suspension"]["front_ride_height"] = 55
            base_setup["suspension"]["rear_ride_height"] = 65
            base_setup["aero"]["front_wing"] = 3
            base_setup["aero"]["rear_wing"] = 4
            base_setup["differential"]["power_ramp"] = 50
            base_setup["brake_balance"] = 60
            
        elif setup_type == "Balanced":
            # Already set as base
            pass
            
        elif setup_type == "Aggressive":
            # Performance-oriented setup
            base_setup["tyres"]["front_left_pressure"] = 25.0
            base_setup["tyres"]["front_right_pressure"] = 25.0
            base_setup["suspension"]["front_ride_height"] = 45
            base_setup["suspension"]["rear_ride_height"] = 55
            base_setup["suspension"]["front_spring_rate"] = 85000
            base_setup["aero"]["front_wing"] = 1
            base_setup["aero"]["rear_wing"] = 2
            base_setup["differential"]["power_ramp"] = 70
            base_setup["brake_balance"] = 56
        
        return base_setup
    
    def _save_setup(self, setup_type: str, setup_data: Dict[str, Any]) -> bool:
        """Save setup to file"""
        try:
            if not self.setup_path:
                logger.error("Setup path not available")
                return False
            
            # Create directory structure if needed
            car_dir = self.setup_path / self.current_car
            car_dir.mkdir(parents=True, exist_ok=True)
            
            # Setup filename
            filename = f"{self.current_track}_{setup_type}.json"
            filepath = car_dir / filename
            
            # Write JSON file
            with open(filepath, 'w') as f:
                json.dump(setup_data, f, indent=2)
            
            logger.info(f"Setup saved to: {filepath}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to save setup file: {e}")
            return False
    
    def get_available_setups(self, car: str = None, track: str = None) -> list:
        """Get list of available setup files"""
        try:
            if not self.setup_path:
                return []
            
            car = car or self.current_car
            if not car:
                return []
            
            car_dir = self.setup_path / car
            if not car_dir.exists():
                return []
            
            setups = []
            for file in car_dir.glob("*.json"):
                setups.append(file.stem)
            
            return sorted(setups)
            
        except Exception as e:
            logger.error(f"Failed to list setups: {e}")
            return []
    
    def load_setup(self, setup_name: str, car: str = None) -> Optional[Dict[str, Any]]:
        """Load a setup from file"""
        try:
            if not self.setup_path:
                return None
            
            car = car or self.current_car
            if not car:
                return None
            
            filepath = self.setup_path / car / f"{setup_name}.json"
            if not filepath.exists():
                logger.warning(f"Setup file not found: {filepath}")
                return None
            
            with open(filepath, 'r') as f:
                setup_data = json.load(f)
            
            logger.info(f"Loaded setup: {setup_name}")
            return setup_data
            
        except Exception as e:
            logger.error(f"Failed to load setup: {e}")
            return None
