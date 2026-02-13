"""
Telemetry Data Acquisition Module
Handles Shared Memory and UDP communication with Assetto Corsa EVO
"""

import struct
import mmap
import ctypes
from typing import Optional, Dict, Any
from dataclasses import dataclass
from loguru import logger

try:
    import win32file
    import win32con
    WINDOWS_AVAILABLE = True
except ImportError:
    WINDOWS_AVAILABLE = False
    logger.warning("win32file not available - shared memory reading disabled")


@dataclass
class CarPhysics:
    """Car physics data"""
    speed_kmh: float = 0.0
    rpm: float = 0.0
    gear: int = 0
    throttle: float = 0.0
    brake: float = 0.0
    clutch: float = 0.0
    steering_angle: float = 0.0
    
    # Position
    position_x: float = 0.0
    position_y: float = 0.0
    position_z: float = 0.0
    
    # G-Forces
    g_force_lateral: float = 0.0
    g_force_longitudinal: float = 0.0
    g_force_vertical: float = 0.0


@dataclass
class WheelData:
    """Individual wheel data"""
    angular_velocity: float = 0.0  # rad/s
    slip_ratio: float = 0.0
    slip_angle: float = 0.0
    temperature_core: float = 0.0
    temperature_inner: float = 0.0
    temperature_middle: float = 0.0
    temperature_outer: float = 0.0
    pressure: float = 0.0
    wear: float = 0.0
    load: float = 0.0


@dataclass
class SessionData:
    """Session information"""
    track_name: str = ""
    car_name: str = ""
    current_lap: int = 0
    last_lap_time_ms: int = 0
    best_lap_time_ms: int = 0
    current_lap_time_ms: int = 0
    session_time_left: float = 0.0
    is_valid_lap: bool = True


class ACEDataAcquisition:
    """
    Assetto Corsa EVO Data Acquisition via Shared Memory
    Compatible with ACE Update 0.5
    """
    
    # Memory mapped file names (try multiple patterns)
    MMF_PHYSICS = "Local\\acpmf_physics"
    MMF_GRAPHICS = "Local\\acpmf_graphics"
    MMF_STATIC = "Local\\acpmf_static"
    
    def __init__(self):
        self.physics_mmf = None
        self.graphics_mmf = None
        self.static_mmf = None
        self.connected = False
        
        self.car_physics = CarPhysics()
        self.wheels = [WheelData() for _ in range(4)]  # FL, FR, RL, RR
        self.session_data = SessionData()
        
    def connect(self) -> bool:
        """Attempt to connect to ACE shared memory"""
        if not WINDOWS_AVAILABLE:
            logger.error("Windows APIs not available - cannot connect")
            return False
            
        try:
            # Try to open shared memory files
            self.physics_mmf = self._open_memory_mapped_file(self.MMF_PHYSICS)
            self.graphics_mmf = self._open_memory_mapped_file(self.MMF_GRAPHICS)
            self.static_mmf = self._open_memory_mapped_file(self.MMF_STATIC)
            
            if self.physics_mmf and self.graphics_mmf:
                self.connected = True
                logger.info("Successfully connected to ACE shared memory")
                return True
            else:
                logger.warning("Could not open all shared memory files")
                return False
                
        except Exception as e:
            logger.error(f"Failed to connect to shared memory: {e}")
            return False
    
    def _open_memory_mapped_file(self, name: str):
        """Open a memory mapped file"""
        try:
            handle = win32file.CreateFile(
                name,
                win32con.GENERIC_READ,
                win32con.FILE_SHARE_READ | win32con.FILE_SHARE_WRITE,
                None,
                win32con.OPEN_EXISTING,
                0,
                None
            )
            
            if handle == win32file.INVALID_HANDLE_VALUE:
                return None
                
            mmf = mmap.mmap(handle, 0, access=mmap.ACCESS_READ)
            return mmf
            
        except Exception as e:
            logger.debug(f"Could not open {name}: {e}")
            return None
    
    def read_telemetry(self) -> bool:
        """Read current telemetry data"""
        if not self.connected:
            return False
            
        try:
            # Read physics data
            if self.physics_mmf:
                self._read_physics_data()
            
            # Read graphics/session data
            if self.graphics_mmf:
                self._read_graphics_data()
                
            return True
            
        except Exception as e:
            logger.error(f"Error reading telemetry: {e}")
            return False
    
    def _read_physics_data(self):
        """Parse physics shared memory structure"""
        # This is a simplified version - actual structure needs reverse engineering
        # Based on AC original shared memory structure
        try:
            self.physics_mmf.seek(0)
            data = self.physics_mmf.read(1024)  # Read enough bytes
            
            # Unpack data (adjust offsets based on actual ACE structure)
            offset = 0
            
            # Packet ID
            offset += 4
            
            # Inputs
            self.car_physics.throttle = struct.unpack_from('f', data, offset)[0]; offset += 4
            self.car_physics.brake = struct.unpack_from('f', data, offset)[0]; offset += 4
            
            # Skip fuel
            offset += 4
            
            # Gear and RPM
            self.car_physics.gear = struct.unpack_from('i', data, offset)[0]; offset += 4
            self.car_physics.rpm = struct.unpack_from('i', data, offset)[0]; offset += 4
            
            # Steering
            self.car_physics.steering_angle = struct.unpack_from('f', data, offset)[0]; offset += 4
            
            # Speed
            self.car_physics.speed_kmh = struct.unpack_from('f', data, offset)[0]; offset += 4
            
            # Skip velocity vector
            offset += 12
            
            # G-Forces (AccG array)
            self.car_physics.g_force_lateral = struct.unpack_from('f', data, offset)[0]; offset += 4
            self.car_physics.g_force_longitudinal = struct.unpack_from('f', data, offset)[0]; offset += 4
            self.car_physics.g_force_vertical = struct.unpack_from('f', data, offset)[0]; offset += 4
            
            # Wheel slip (4 wheels)
            for i in range(4):
                self.wheels[i].slip_ratio = struct.unpack_from('f', data, offset)[0]
                offset += 4
            
            # Skip wheel load
            offset += 16
            
            # Wheel pressure
            for i in range(4):
                self.wheels[i].pressure = struct.unpack_from('f', data, offset)[0]
                offset += 4
            
            # Wheel angular speed
            for i in range(4):
                self.wheels[i].angular_velocity = struct.unpack_from('f', data, offset)[0]
                offset += 4
                
        except Exception as e:
            logger.debug(f"Error parsing physics data: {e}")
    
    def _read_graphics_data(self):
        """Parse graphics/session shared memory structure"""
        try:
            self.graphics_mmf.seek(0)
            data = self.graphics_mmf.read(1024)
            
            offset = 0
            
            # Packet ID
            offset += 4
            
            # Status
            offset += 4
            
            # Session type
            offset += 4
            
            # Skip current time string
            offset += 30
            
            # Skip last time string
            offset += 30
            
            # Skip best time string
            offset += 30
            
            # Skip split string
            offset += 30
            
            # Completed laps
            self.session_data.current_lap = struct.unpack_from('i', data, offset)[0]
            offset += 4
            
            # Skip position
            offset += 4
            
            # Current time (ms)
            self.session_data.current_lap_time_ms = struct.unpack_from('i', data, offset)[0]
            offset += 4
            
            # Last time (ms)
            self.session_data.last_lap_time_ms = struct.unpack_from('i', data, offset)[0]
            offset += 4
            
            # Best time (ms)
            self.session_data.best_lap_time_ms = struct.unpack_from('i', data, offset)[0]
            offset += 4
            
        except Exception as e:
            logger.debug(f"Error parsing graphics data: {e}")
    
    def disconnect(self):
        """Close shared memory connections"""
        if self.physics_mmf:
            self.physics_mmf.close()
        if self.graphics_mmf:
            self.graphics_mmf.close()
        if self.static_mmf:
            self.static_mmf.close()
            
        self.connected = False
        logger.info("Disconnected from shared memory")
    
    def get_car_physics(self) -> CarPhysics:
        """Get current car physics data"""
        return self.car_physics
    
    def get_wheel_data(self, wheel_index: int) -> WheelData:
        """Get data for specific wheel (0=FL, 1=FR, 2=RL, 3=RR)"""
        if 0 <= wheel_index < 4:
            return self.wheels[wheel_index]
        return WheelData()
    
    def get_all_wheels(self) -> list[WheelData]:
        """Get all wheels data"""
        return self.wheels
    
    def get_session_data(self) -> SessionData:
        """Get current session data"""
        return self.session_data
