"""
Enhanced ACE Connector V2 - Multi-Method Connection
Supports:
1. Process monitoring via psutil
2. UDP (if active ports detected)
3. Shared Memory (Local\ACE_Physics or Global\AssettoCorsaEVO)
4. HTTP API (http://127.0.0.1:8080/telemetry)
"""

import socket
import struct
import time
from typing import Optional, Dict, Any, List
from loguru import logger

from telemetry.process_monitor import ProcessMonitor
from telemetry.http_connector import HTTPConnector

try:
    import mmap
    import win32file
    import win32con
    WINDOWS_AVAILABLE = True
except ImportError:
    WINDOWS_AVAILABLE = False
    logger.warning("Windows APIs not available")


class EnhancedACEConnector:
    """
    Enhanced connection handler that tries multiple methods:
    1. Check process (AssettoCorsaEVO.exe)
    2. Try UDP (if port detected)
    3. Try Shared Memory (multiple names)
    4. Try HTTP API
    """
    
    # Shared Memory names to try (in order)
    SHARED_MEMORY_NAMES = [
        "Local\\ACE_Physics",
        "Global\\AssettoCorsaEVO",
        "Local\\acememory",
        "Local\\acpmf_physics",
    ]
    
    # UDP Configuration
    UDP_PORT = 9000
    
    # HTTP Configuration
    HTTP_HOST = "127.0.0.1"
    HTTP_PORT = 8080
    
    # Connection retry settings
    RETRY_INTERVAL = 5.0  # seconds
    
    def __init__(self):
        self.connection_type = None  # 'shared_memory', 'udp', or 'http'
        self.shared_memory = None
        self.shared_memory_name = None
        self.udp_socket = None
        self.http_connector = HTTPConnector(self.HTTP_HOST, self.HTTP_PORT)
        self.process_monitor = ProcessMonitor()
        
        self.connected = False
        self.last_error = ""
        self.last_connection_attempt = 0.0
        
        # Flexible data structure (dictionary-based)
        self.telemetry_data = {}
        
        # Default offset mapping (can be updated)
        self.offsets = self._load_default_offsets()
    
    def _load_default_offsets(self) -> Dict[str, int]:
        """Load default memory offsets for ACE"""
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
            'position_x': 32,
            'position_y': 36,
            'position_z': 40,
            
            'velocity_x': 44,
            'velocity_y': 48,
            'velocity_z': 52,
            
            # G-Forces (vec3)
            'acc_g_x': 56,
            'acc_g_y': 60,
            'acc_g_z': 64,
            
            # Wheels (4 floats each)
            'wheel_slip': 68,  # 4 floats
            'wheel_load': 84,
            'wheel_pressure': 100,
            'wheel_angular_speed': 116,
            
            # Tire temperatures (4 floats each)
            'tire_temp_i': 132,
            'tire_temp_m': 148,
            'tire_temp_o': 164,
            'tire_temp_core': 180,
            
            # Track/session
            'lap_time': 200,
            'last_lap': 204,
            'best_lap': 208,
            'lap_count': 212,
        }
    
    def try_connect(self) -> bool:
        """
        Attempt connection using all available methods
        Returns True if connected
        """
        current_time = time.time()
        
        # Throttle connection attempts
        if current_time - self.last_connection_attempt < self.RETRY_INTERVAL:
            return self.connected
        
        self.last_connection_attempt = current_time
        
        # Step 1: Check if process is running
        logger.info("🔍 Tentativo di aggancio a AssettoCorsaEVO.exe...")
        
        if not self.process_monitor.is_running():
            self.last_error = "AssettoCorsaEVO.exe not running"
            logger.warning(f"❌ {self.last_error}")
            self.connected = False
            return False
        
        logger.info(f"✅ Process AssettoCorsaEVO.exe detected (PID: {self.process_monitor.pid})")
        
        # Step 2: Check for active UDP ports
        open_ports = self.process_monitor.get_open_ports()
        logger.debug(f"Open UDP ports: {open_ports}")
        
        # Step 3: Try connection methods in order
        
        # Try UDP first if port is open
        if self.UDP_PORT in open_ports:
            logger.info(f"🔍 Tentativo connessione UDP porta {self.UDP_PORT}...")
            if self._try_udp():
                self.connection_type = 'udp'
                self.connected = True
                self.last_error = ""
                logger.info(f"✅ Connesso via UDP (porta {self.UDP_PORT})")
                return True
        
        # Try Shared Memory (multiple names)
        for memory_name in self.SHARED_MEMORY_NAMES:
            logger.info(f"🔍 Tentativo di aggancio a Shared Memory '{memory_name}'...")
            if self._try_shared_memory(memory_name):
                self.connection_type = 'shared_memory'
                self.shared_memory_name = memory_name
                self.connected = True
                self.last_error = ""
                logger.info(f"✅ Connesso via Shared Memory '{memory_name}'")
                return True
        
        # Try HTTP API
        logger.info(f"🔍 Tentativo connessione HTTP API {self.HTTP_HOST}:{self.HTTP_PORT}...")
        if self.http_connector.connect():
            self.connection_type = 'http'
            self.connected = True
            self.last_error = ""
            logger.info(f"✅ Connesso via HTTP API")
            return True
        
        # All methods failed
        self.last_error = "Nessun metodo di connessione disponibile"
        logger.warning(f"❌ {self.last_error}")
        self.connected = False
        return False
    
    def _try_shared_memory(self, memory_name: str) -> bool:
        """Try to connect via Shared Memory with given name"""
        if not WINDOWS_AVAILABLE:
            self.last_error = "Windows APIs non disponibili"
            return False
        
        try:
            # Try to open shared memory with READ ONLY access (anti-cheat safe)
            handle = win32file.CreateFile(
                memory_name,
                win32con.GENERIC_READ,  # READ ONLY - Safe for anti-cheat
                win32con.FILE_SHARE_READ | win32con.FILE_SHARE_WRITE,
                None,
                win32con.OPEN_EXISTING,
                0,
                None
            )
            
            if handle == win32file.INVALID_HANDLE_VALUE:
                return False
            
            # Map memory with READ ONLY access
            self.shared_memory = mmap.mmap(handle, 0, access=mmap.ACCESS_READ)
            return True
            
        except PermissionError:
            self.last_error = "Permission Denied - Eseguire come Amministratore"
            return False
        except FileNotFoundError:
            # Silent fail - we're trying multiple names
            return False
        except Exception as e:
            logger.debug(f"Shared Memory '{memory_name}' error: {e}")
            return False
    
    def _try_udp(self) -> bool:
        """Try to connect via UDP"""
        try:
            if self.udp_socket is None:
                self.udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
                self.udp_socket.settimeout(0.5)
                self.udp_socket.bind(("127.0.0.1", self.UDP_PORT))
            
            # Try to receive a packet
            try:
                data, addr = self.udp_socket.recvfrom(4096)
                return len(data) > 0
            except socket.timeout:
                return False
                
        except Exception as e:
            logger.debug(f"UDP connection error: {e}")
            return False
    
    def read_telemetry(self) -> bool:
        """Read telemetry data from connected source"""
        if not self.connected:
            return False
        
        try:
            if self.connection_type == 'shared_memory':
                return self._read_shared_memory()
            elif self.connection_type == 'udp':
                return self._read_udp()
            elif self.connection_type == 'http':
                return self._read_http()
        except Exception as e:
            self.last_error = f"Errore lettura: {str(e)}"
            logger.error(self.last_error)
            self.connected = False
            return False
        
        return False
    
    def _read_shared_memory(self) -> bool:
        """Read from shared memory"""
        try:
            self.shared_memory.seek(0)
            data = self.shared_memory.read(512)
            
            # Parse using offset mapping
            self.telemetry_data = {}
            
            for key, offset in self.offsets.items():
                if offset + 4 <= len(data):
                    if key in ['gear', 'lap_count', 'packet_id']:
                        self.telemetry_data[key] = struct.unpack_from('i', data, offset)[0]
                    elif key.startswith('wheel_') or key.startswith('tire_temp_'):
                        values = struct.unpack_from('ffff', data, offset)
                        self.telemetry_data[key] = list(values)
                    else:
                        self.telemetry_data[key] = struct.unpack_from('f', data, offset)[0]
            
            return True
        except Exception as e:
            logger.error(f"Shared memory read error: {e}")
            return False
    
    def _read_udp(self) -> bool:
        """Read from UDP"""
        try:
            data, addr = self.udp_socket.recvfrom(4096)
            # Parse UDP data (format depends on ACE implementation)
            # Similar to shared memory parsing
            self.telemetry_data = self._parse_binary_data(data)
            return True
        except socket.timeout:
            return False
        except Exception as e:
            logger.error(f"UDP read error: {e}")
            return False
    
    def _read_http(self) -> bool:
        """Read from HTTP API"""
        data = self.http_connector.get_telemetry()
        if data:
            self.telemetry_data = data
            return True
        return False
    
    def _parse_binary_data(self, data: bytes) -> Dict[str, Any]:
        """Parse binary UDP/SharedMemory data"""
        result = {}
        for key, offset in self.offsets.items():
            if offset + 4 <= len(data):
                if key in ['gear', 'lap_count', 'packet_id']:
                    result[key] = struct.unpack_from('i', data, offset)[0]
                elif key.startswith('wheel_') or key.startswith('tire_temp_'):
                    values = struct.unpack_from('ffff', data, offset)
                    result[key] = list(values)
                else:
                    result[key] = struct.unpack_from('f', data, offset)[0]
        return result
    
    def get_telemetry_data(self) -> Dict[str, Any]:
        """Get current telemetry data"""
        return self.telemetry_data.copy()
    
    def update_offsets(self, new_offsets: Dict[str, int]):
        """Update offset mapping"""
        self.offsets.update(new_offsets)
        logger.info(f"Updated {len(new_offsets)} offsets")
    
    def disconnect(self):
        """Close all connections"""
        if self.shared_memory:
            self.shared_memory.close()
            self.shared_memory = None
        
        if self.udp_socket:
            self.udp_socket.close()
            self.udp_socket = None
        
        if self.http_connector:
            self.http_connector.disconnect()
        
        self.connected = False
        logger.info("Disconnected from ACE")
