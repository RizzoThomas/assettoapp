"""
Setup Injector for ACE
Injects selected setup into game, allowing instant reload from garage
"""

import os
import json
import shutil
from pathlib import Path
from typing import Optional, Dict, Any
from loguru import logger


class SetupInjector:
    """
    Injects setup files into ACE
    Path: AppData\Local\AssettoCorsaEVO\Saved\SaveGames\Setups\
    """
    
    def __init__(self):
        self.setup_base_path = self._find_setup_path()
        self.current_car = None
        self.current_track = None
        
    def _find_setup_path(self) -> Optional[Path]:
        """Find ACE setup directory"""
        # Try LocalAppData first
        localappdata = os.getenv('LOCALAPPDATA')
        if localappdata:
            path1 = Path(localappdata) / "AssettoCorsaEVO" / "Saved" / "SaveGames" / "Setups"
            if path1.exists():
                logger.info(f"Found setup path: {path1}")
                return path1
        
        # Try AppData\Roaming
        appdata = os.getenv('APPDATA')
        if appdata:
            path2 = Path(appdata) / "AssettoCorsaEVO" / "Setups"
            if path2.exists():
                logger.info(f"Found setup path: {path2}")
                return path2
        
        # Try Documents
        userprofile = os.getenv('USERPROFILE')
        if userprofile:
            path3 = Path(userprofile) / "Documents" / "AssettoCorsaEVO" / "Setups"
            if path3.exists():
                logger.info(f"Found setup path: {path3}")
                return path3
        
        logger.warning("Setup path not found. Setups may not be injectable.")
        return None
    
    def set_current_context(self, car: str, track: str):
        """Set current car and track for targeted injection"""
        self.current_car = car
        self.current_track = track
        logger.info(f"Setup context: {car} at {track}")
    
    def find_active_setup_file(self) -> Optional[Path]:
        """Find the currently active setup file"""
        if not self.setup_base_path:
            return None
        
        if not self.current_car or not self.current_track:
            logger.warning("Car or track not set. Cannot find active setup.")
            return None
        
        # Look for setup file: car_track.json or similar
        possible_paths = [
            self.setup_base_path / f"{self.current_car}_{self.current_track}.json",
            self.setup_base_path / self.current_car / f"{self.current_track}.json",
            self.setup_base_path / "current_setup.json",
        ]
        
        for path in possible_paths:
            if path.exists():
                logger.info(f"Found active setup: {path}")
                return path
        
        logger.warning(f"No active setup file found for {self.current_car} at {self.current_track}")
        return None
    
    def create_setup_preset(self, preset_type: str) -> Dict[str, Any]:
        """
        Create setup preset JSON
        
        Args:
            preset_type: 'safe', 'balanced', or 'aggressive'
        """
        base_setup = {
            "version": "1.0",
            "car": self.current_car or "default",
            "track": self.current_track or "default",
            "preset": preset_type,
            "tyres": {},
            "suspension": {},
            "aero": {},
            "differential": {},
            "brakes": {}
        }
        
        if preset_type == 'safe':
            # Safe setup: High pressures, more downforce, soft suspension
            base_setup["tyres"] = {
                "pressure_fl": 28.0,  # +1.5 psi
                "pressure_fr": 28.0,
                "pressure_rl": 27.5,
                "pressure_rr": 27.5,
                "camber_fl": -2.5,
                "camber_fr": -2.5,
                "camber_rl": -2.0,
                "camber_rr": -2.0
            }
            base_setup["aero"] = {
                "front_wing": 8,  # +3 clicks (more downforce)
                "rear_wing": 8
            }
            base_setup["suspension"] = {
                "front_spring_rate": 85000,  # Softer
                "rear_spring_rate": 90000,
                "front_damper_bump": 6,
                "rear_damper_bump": 6
            }
            base_setup["brakes"] = {
                "balance": 52.0  # Slightly forward (stable)
            }
            
        elif preset_type == 'aggressive':
            # Aggressive: Low pressures, less downforce, stiff suspension
            base_setup["tyres"] = {
                "pressure_fl": 26.0,  # -0.5 psi
                "pressure_fr": 26.0,
                "pressure_rl": 25.5,
                "pressure_rr": 25.5,
                "camber_fl": -4.5,  # Extreme camber
                "camber_fr": -4.5,
                "camber_rl": -3.5,
                "camber_rr": -3.5
            }
            base_setup["aero"] = {
                "front_wing": 2,  # -3 clicks (less drag)
                "rear_wing": 2
            }
            base_setup["suspension"] = {
                "front_spring_rate": 105000,  # Stiffer
                "rear_spring_rate": 110000,
                "front_damper_bump": 10,
                "rear_damper_bump": 10
            }
            base_setup["brakes"] = {
                "balance": 48.0  # Slightly rearward (rotation)
            }
            
        else:  # balanced
            # Balanced: Middle ground
            base_setup["tyres"] = {
                "pressure_fl": 27.0,
                "pressure_fr": 27.0,
                "pressure_rl": 26.5,
                "pressure_rr": 26.5,
                "camber_fl": -3.0,
                "camber_fr": -3.0,
                "camber_rl": -2.5,
                "camber_rr": -2.5
            }
            base_setup["aero"] = {
                "front_wing": 5,
                "rear_wing": 5
            }
            base_setup["suspension"] = {
                "front_spring_rate": 95000,
                "rear_spring_rate": 100000,
                "front_damper_bump": 8,
                "rear_damper_bump": 8
            }
            base_setup["brakes"] = {
                "balance": 50.0  # Neutral
            }
        
        return base_setup
    
    def inject_setup(self, preset_type: str, backup: bool = True) -> bool:
        """
        Inject setup into game
        
        Args:
            preset_type: 'safe', 'balanced', or 'aggressive'
            backup: Whether to backup current setup
            
        Returns:
            True if injection successful
        """
        logger.info(f"Attempting to inject '{preset_type}' setup...")
        
        # Find active setup file
        active_setup = self.find_active_setup_file()
        if not active_setup:
            logger.error("No active setup file found. Cannot inject.")
            return False
        
        try:
            # Backup current setup
            if backup and active_setup.exists():
                backup_path = active_setup.with_suffix('.backup')
                shutil.copy2(active_setup, backup_path)
                logger.info(f"Backed up setup to: {backup_path}")
            
            # Create new setup
            new_setup = self.create_setup_preset(preset_type)
            
            # Write to file
            with open(active_setup, 'w') as f:
                json.dump(new_setup, f, indent=2)
            
            logger.info(f"Successfully injected '{preset_type}' setup to: {active_setup}")
            logger.info("Go to garage and reload setup!")
            return True
            
        except Exception as e:
            logger.error(f"Failed to inject setup: {e}")
            return False
    
    def restore_backup(self) -> bool:
        """Restore backed up setup"""
        active_setup = self.find_active_setup_file()
        if not active_setup:
            return False
        
        backup_path = active_setup.with_suffix('.backup')
        if not backup_path.exists():
            logger.warning("No backup found")
            return False
        
        try:
            shutil.copy2(backup_path, active_setup)
            logger.info("Setup restored from backup")
            return True
        except Exception as e:
            logger.error(f"Failed to restore backup: {e}")
            return False


def demo():
    """Demo setup injector"""
    injector = SetupInjector()
    
    print("Setup Injector Demo")
    print("=" * 50)
    
    if injector.setup_base_path:
        print(f"Setup path: {injector.setup_base_path}")
        
        # Set context
        injector.set_current_context("porsche_911_gt3_r", "monza")
        
        # Show presets
        for preset in ['safe', 'balanced', 'aggressive']:
            print(f"\n{preset.upper()} Preset:")
            setup = injector.create_setup_preset(preset)
            print(f"  Tire Pressure FL: {setup['tyres']['pressure_fl']} psi")
            print(f"  Camber FL: {setup['tyres']['camber_fl']}")
            print(f"  Front Wing: {setup['aero']['front_wing']}")
            print(f"  Spring Rate Front: {setup['suspension']['front_spring_rate']}")
    else:
        print("Setup path not found")


if __name__ == "__main__":
    demo()
