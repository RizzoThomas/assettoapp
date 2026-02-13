"""
ACE Connector - Assetto Corsa EVO Specific Connection Handler
Supports both Shared Memory (Local\acememory) and UDP (port 9000)
"""

import socket
import struct
import time
from typing import Optional, Dict, Any
from loguru import logger

try:
    import mmap
    import win32file
    import win32con
    WINDOWS_AVAILABLE = True
except ImportError:
    WINDOWS_AVAILABLE = False
    logger.warning("Windows APIs not available")


class ACEConnector:
    """
    Handles connection to Assetto Corsa EVO via Shared Memory or UDP
    """
    
    # ACE Shared Memory name (updated for ACE)
    SHARED_MEMORY_NAME = "Local\\acememory"
    
    # UDP Configuration
    UDP_IP = "127.0.0.1"
    UDP_PORT = 9000
    
    # Connection retry settings
    RETRY_INTERVAL = 5.0  # seconds
    
    def __init__(self):
        self.connection_type = None  # 'shared_memory' or 'udp'
        self.shared_memory = None
        self.udp_socket = None
        self.connected = False
        self.last_error = ""
        self.last_connection_attempt = 0.0
        
        # Flexible data structure (dictionary-based)
        self.telemetry_data = {}
        
        # Default offset mapping (can be updated)
        self.offsets = self._load_default_offsets()
    
    def _load_default_offsets(self) -> Dict[str, int]:
        """
        Load default memory offsets for ACE
        These can be overridden by loading from config file
        """
        return {
            # Basic info
            'packet_id': 0,
            'gas': 4,
            'brake': 8,
            'fuel': 12,
            'gear': 16,
            'rpm': 20,
            'steer_angle': 24,
            'speed_kmh': 28,
            
            # Position (vec3)
            'velocity_x': 32,
            'velocity_y': 36,
            'velocity_z': 40,
            
            # G-Forces (vec3)
            'acc_g_x': 44,
            'acc_g_y': 48,
            'acc_g_z': 52,
            
            # Wheels (4 floats each)
            'wheel_slip': 56,  # 4 floats starting here
            'wheel_load': 72,
            'wheel_pressure': 88,
            'wheel_angular_speed': 104,
            
            # Tire temperatures
            'tire_temp_i': 120,  # Inner
            'tire_temp_m': 136,  # Middle
            'tire_temp_o': 152,  # Outer
            'tire_temp_core': 168,
            
            # Track/session
            'lap_time': 200,
            'last_lap': 204,
            'best_lap': 208,
            'lap_count': 212,
        }
    
    def update_offsets(self, new_offsets: Dict[str, int]):
        """Update offset mapping (for easy configuration)"""
        self.offsets.update(new_offsets)
        logger.info(f"Updated {len(new_offsets)} offsets")
    
    def try_connect(self) -> bool:
        """
        Attempt to connect to ACE via Shared Memory or UDP
        Returns True if connected, False otherwise
        """
        current_time = time.time()
        
        # Throttle connection attempts
        if current_time - self.last_connection_attempt < self.RETRY_INTERVAL:
            return self.connected
        
        self.last_connection_attempt = current_time
        
        # Try Shared Memory first
        if self._try_shared_memory():
            self.connection_type = 'shared_memory'
            self.connected = True
            self.last_error = ""
            logger.info("✅ Connected via Shared Memory")
            return True
        
        # Fallback to UDP
        if self._try_udp():
            self.connection_type = 'udp'
            self.connected = True
            self.last_error = ""
            logger.info("✅ Connected via UDP")
            return True
        
        # Connection failed
        self.connected = False
        return False
    
    def _try_shared_memory(self) -> bool:
        """Try to connect via Shared Memory"""
        if not WINDOWS_AVAILABLE:
            self.last_error = "Windows APIs not available"
            return False
        
        try:
            # Try to open shared memory with READ ONLY access (anti-cheat safe)
            handle = win32file.CreateFile(
                self.SHARED_MEMORY_NAME,
                win32con.GENERIC_READ,  # READ ONLY
                win32con.FILE_SHARE_READ | win32con.FILE_SHARE_WRITE,
                None,
                win32con.OPEN_EXISTING,
                0,
                None
            )
            
            if handle == win32file.INVALID_HANDLE_VALUE:
                self.last_error = f"Shared Memory '{self.SHARED_MEMORY_NAME}' not found"
                return False
            
            # Map memory with READ ONLY access
            self.shared_memory = mmap.mmap(handle, 0, access=mmap.ACCESS_READ)
            return True
            
        except PermissionError:
            self.last_error = "Permission Denied - Run as Administrator"
            logger.error(self.last_error)
            return False
        except FileNotFoundError:
            self.last_error = f"Memory Page '{self.SHARED_MEMORY_NAME}' Not Found - Is ACE running?"
            logger.debug(self.last_error)
            return False
        except Exception as e:
            self.last_error = f"Shared Memory Error: {str(e)}"
            logger.error(self.last_error)
            return False
    
    def _try_udp(self) -> bool:
        """Try to connect via UDP"""
        try:
            if self.udp_socket is None:
                self.udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
                self.udp_socket.settimeout(0.1)  # 100ms timeout
                self.udp_socket.bind((self.UDP_IP, self.UDP_PORT))
            
            # Try to receive a packet to verify connection
            try:
                data, addr = self.udp_socket.recvfrom(4096)
                if len(data) > 0:
                    return True
            except socket.timeout:
                self.last_error = f"UDP Timeout - No data on port {self.UDP_PORT}"
                return False
                
        except PermissionError:
            self.last_error = f"UDP Permission Denied on port {self.UDP_PORT}"
            logger.error(self.last_error)
            return False
        except OSError as e:
            self.last_error = f"UDP Error: {str(e)}"
            logger.error(self.last_error)
            return False
        except Exception as e:
            self.last_error = f"UDP Connection Error: {str(e)}"
            logger.error(self.last_error)
            return False
        
        return False
    
    def read_telemetry(self) -> bool:
        """
        Read telemetry data from connected source
        Returns True if data was read successfully
        """
        if not self.connected:
            return False
        
        try:
            if self.connection_type == 'shared_memory':
                return self._read_shared_memory()
            elif self.connection_type == 'udp':
                return self._read_udp()
        except Exception as e:
            self.last_error = f"Read Error: {str(e)}"
            logger.error(self.last_error)
            self.connected = False
            return False
        
        return False
    
    def _read_shared_memory(self) -> bool:
        """Read from shared memory using flexible offset mapping"""
        try:
            self.shared_memory.seek(0)
            data = self.shared_memory.read(512)  # Read enough bytes
            
            # Parse using offset mapping
            self.telemetry_data = {}
            
            for key, offset in self.offsets.items():
                if offset + 4 <= len(data):  # Ensure we have enough data
                    # Most values are floats
                    if key in ['gear', 'lap_count', 'packet_id']:
                        self.telemetry_data[key] = struct.unpack_from('i', data, offset)[0]
                    elif key.startswith('wheel_'):
                        # Wheel data: read 4 values (one per wheel)
                        values = struct.unpack_from('ffff', data, offset)
                        self.telemetry_data[key] = list(values)
                    elif key.startswith('tire_temp_'):
                        # Tire temps: 4 values per wheel
                        values = struct.unpack_from('ffff', data, offset)
                        self.telemetry_data[key] = list(values)
                    else:
                        # Single float value
                        self.telemetry_data[key] = struct.unpack_from('f', data, offset)[0]
            
            return True
            
        except Exception as e:
            self.last_error = f"Parse Error: {str(e)}"
            logger.error(self.last_error)
            return False
    
    def _read_udp(self) -> bool:
        """Read from UDP socket"""
        try:
            data, addr = self.udp_socket.recvfrom(4096)
            
            # Parse UDP packet (format depends on ACE implementation)
            # This is a placeholder - actual format needs to be determined
            if len(data) >= 100:
                # Basic parsing example
                offset = 0
                self.telemetry_data['speed_kmh'] = struct.unpack_from('f', data, offset)[0]
                offset += 4
                self.telemetry_data['rpm'] = struct.unpack_from('f', data, offset)[0]
                offset += 4
                self.telemetry_data['gas'] = struct.unpack_from('f', data, offset)[0]
                offset += 4
                self.telemetry_data['brake'] = struct.unpack_from('f', data, offset)[0]
                
                return True
                
        except socket.timeout:
            return False
        except Exception as e:
            self.last_error = f"UDP Read Error: {str(e)}"
            logger.error(self.last_error)
            return False
        
        return False
    
    def get_value(self, key: str, default: Any = 0.0) -> Any:
        """Get a telemetry value safely"""
        return self.telemetry_data.get(key, default)
    
    def get_wheel_value(self, key: str, wheel_index: int, default: float = 0.0) -> float:
        """Get a wheel-specific value (0=FL, 1=FR, 2=RL, 3=RR)"""
        values = self.telemetry_data.get(key, [default] * 4)
        if isinstance(values, list) and 0 <= wheel_index < len(values):
            return values[wheel_index]
        return default
    
    def disconnect(self):
        """Close connections"""
        if self.shared_memory:
            self.shared_memory.close()
            self.shared_memory = None
        
        if self.udp_socket:
            self.udp_socket.close()
            self.udp_socket = None
        
        self.connected = False
        self.connection_type = None
        logger.info("Disconnected from ACE")
    
    def get_connection_status(self) -> str:
        """Get human-readable connection status"""
        if self.connected:
            return f"✅ Connected ({self.connection_type})"
        else:
            return f"❌ Not Connected - {self.last_error}"
