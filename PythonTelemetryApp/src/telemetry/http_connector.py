"""
HTTP Connector - Connect to ACE via HTTP API
http://127.0.0.1:8080/telemetry
"""

import requests
import time
from typing import Optional, Dict, Any
from loguru import logger


class HTTPConnector:
    """
    Connect to Assetto Corsa EVO via HTTP API
    """
    
    def __init__(self, host: str = "127.0.0.1", port: int = 8080):
        self.host = host
        self.port = port
        self.url = f"http://{host}:{port}/telemetry"
        self.connected = False
        self.last_error = ""
        self.session = requests.Session()
        self.session.headers.update({'User-Agent': 'ACETelemetry/1.0'})
    
    def connect(self) -> bool:
        """
        Test connection to HTTP API
        """
        try:
            response = self.session.get(self.url, timeout=2)
            if response.status_code == 200:
                self.connected = True
                logger.info(f"Connected to HTTP API: {self.url}")
                return True
            else:
                self.last_error = f"HTTP {response.status_code}: {response.text}"
                self.connected = False
                return False
        except requests.exceptions.ConnectionError:
            self.last_error = "Connection refused - HTTP server not available"
            self.connected = False
            return False
        except requests.exceptions.Timeout:
            self.last_error = "HTTP request timeout"
            self.connected = False
            return False
        except Exception as e:
            self.last_error = f"HTTP error: {str(e)}"
            self.connected = False
            return False
    
    def get_telemetry(self) -> Optional[Dict[str, Any]]:
        """
        Fetch telemetry data from HTTP API
        Returns dictionary with telemetry data or None if failed
        """
        if not self.connected:
            return None
        
        try:
            response = self.session.get(self.url, timeout=1)
            if response.status_code == 200:
                data = response.json()
                return self._parse_telemetry(data)
            else:
                self.last_error = f"HTTP {response.status_code}"
                return None
        except Exception as e:
            self.last_error = f"HTTP fetch error: {str(e)}"
            self.connected = False
            return None
    
    def _parse_telemetry(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """
        Parse HTTP response into standardized telemetry format
        """
        # Adapt this based on actual ACE HTTP API format
        telemetry = {
            'speed_kmh': data.get('speed', 0),
            'rpm': data.get('rpm', 0),
            'gear': data.get('gear', 0),
            'gas': data.get('throttle', 0),
            'brake': data.get('brake', 0),
            
            # Wheels
            'wheel_slip': data.get('wheelSlip', [0, 0, 0, 0]),
            
            # G-Forces
            'acc_g_x': data.get('gForce', {}).get('x', 0),
            'acc_g_y': data.get('gForce', {}).get('y', 0),
            'acc_g_z': data.get('gForce', {}).get('z', 0),
            
            # Position
            'position_x': data.get('position', {}).get('x', 0),
            'position_y': data.get('position', {}).get('y', 0),
            'position_z': data.get('position', {}).get('z', 0),
            
            # Tires
            'tire_temp_fl': data.get('tireTemp', [0, 0, 0, 0])[0],
            'tire_temp_fr': data.get('tireTemp', [0, 0, 0, 0])[1],
            'tire_temp_rl': data.get('tireTemp', [0, 0, 0, 0])[2],
            'tire_temp_rr': data.get('tireTemp', [0, 0, 0, 0])[3],
            
            # Lap
            'lap_time': data.get('currentLapTime', 0),
            'last_lap': data.get('lastLapTime', 0),
            'best_lap': data.get('bestLapTime', 0),
        }
        
        return telemetry
    
    def disconnect(self):
        """Close HTTP connection"""
        self.session.close()
        self.connected = False
        logger.info("HTTP connector disconnected")
