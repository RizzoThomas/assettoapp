"""
WebSocket Connector for ACE Telemetry
Supports WebSocket connection to ACE (V8/RenoirCore based)
"""

import json
import threading
import time
from typing import Dict, Any, Optional, Callable
from loguru import logger

try:
    import websocket
    WEBSOCKET_AVAILABLE = True
except ImportError:
    WEBSOCKET_AVAILABLE = False
    logger.warning("websocket-client not installed. WebSocket support disabled.")


class WebSocketConnector:
    """WebSocket connector for ACE telemetry"""
    
    def __init__(self, host: str = "127.0.0.1", port: int = 8080, path: str = "/telemetry"):
        self.host = host
        self.port = port
        self.path = path
        self.ws: Optional[websocket.WebSocketApp] = None
        self.connected = False
        self.running = False
        self.thread: Optional[threading.Thread] = None
        self.data_callback: Optional[Callable] = None
        self.latest_data: Dict[str, Any] = {}
        self.reconnect_delay = 5.0
        
    def set_data_callback(self, callback: Callable[[Dict[str, Any]], None]):
        """Set callback function for telemetry data"""
        self.data_callback = callback
    
    def connect(self) -> bool:
        """Connect to WebSocket server"""
        if not WEBSOCKET_AVAILABLE:
            logger.error("Cannot connect: websocket-client not installed")
            return False
        
        if self.connected:
            logger.warning("Already connected")
            return True
        
        try:
            url = f"ws://{self.host}:{self.port}{self.path}"
            logger.info(f"Connecting to WebSocket: {url}")
            
            self.ws = websocket.WebSocketApp(
                url,
                on_message=self._on_message,
                on_error=self._on_error,
                on_close=self._on_close,
                on_open=self._on_open
            )
            
            # Run in separate thread
            self.running = True
            self.thread = threading.Thread(target=self._run_forever, daemon=True)
            self.thread.start()
            
            # Wait a bit for connection
            time.sleep(1.0)
            return self.connected
            
        except Exception as e:
            logger.error(f"WebSocket connection failed: {e}")
            return False
    
    def _run_forever(self):
        """Run WebSocket connection in loop with reconnection"""
        while self.running:
            try:
                if self.ws:
                    self.ws.run_forever()
                
                if self.running:
                    logger.info(f"WebSocket disconnected. Reconnecting in {self.reconnect_delay}s...")
                    time.sleep(self.reconnect_delay)
                    
            except Exception as e:
                logger.error(f"WebSocket error in run loop: {e}")
                if self.running:
                    time.sleep(self.reconnect_delay)
    
    def _on_open(self, ws):
        """Called when WebSocket connection opens"""
        self.connected = True
        logger.info("WebSocket connected successfully")
    
    def _on_message(self, ws, message):
        """Called when WebSocket receives a message"""
        try:
            # Parse JSON telemetry data
            data = json.loads(message)
            self.latest_data = data
            
            # Call user callback if set
            if self.data_callback:
                self.data_callback(data)
                
        except json.JSONDecodeError as e:
            logger.error(f"Failed to parse WebSocket message: {e}")
        except Exception as e:
            logger.error(f"Error processing WebSocket message: {e}")
    
    def _on_error(self, ws, error):
        """Called on WebSocket error"""
        logger.error(f"WebSocket error: {error}")
    
    def _on_close(self, ws, close_status_code, close_msg):
        """Called when WebSocket closes"""
        self.connected = False
        logger.info(f"WebSocket closed (code: {close_status_code}, msg: {close_msg})")
    
    def disconnect(self):
        """Disconnect from WebSocket"""
        logger.info("Disconnecting WebSocket...")
        self.running = False
        self.connected = False
        
        if self.ws:
            self.ws.close()
            self.ws = None
        
        if self.thread and self.thread.is_alive():
            self.thread.join(timeout=2.0)
    
    def get_telemetry(self) -> Dict[str, Any]:
        """Get latest telemetry data"""
        return self.latest_data.copy()
    
    def is_connected(self) -> bool:
        """Check if connected"""
        return self.connected


class SecureWebSocketConnector(WebSocketConnector):
    """WebSocket connector with SSL support (wss://)"""
    
    def connect(self) -> bool:
        """Connect to secure WebSocket server"""
        if not WEBSOCKET_AVAILABLE:
            logger.error("Cannot connect: websocket-client not installed")
            return False
        
        if self.connected:
            logger.warning("Already connected")
            return True
        
        try:
            url = f"wss://{self.host}:{self.port}{self.path}"
            logger.info(f"Connecting to Secure WebSocket: {url}")
            
            self.ws = websocket.WebSocketApp(
                url,
                on_message=self._on_message,
                on_error=self._on_error,
                on_close=self._on_close,
                on_open=self._on_open
            )
            
            # Run in separate thread
            self.running = True
            self.thread = threading.Thread(target=self._run_forever_ssl, daemon=True)
            self.thread.start()
            
            # Wait a bit for connection
            time.sleep(1.0)
            return self.connected
            
        except Exception as e:
            logger.error(f"Secure WebSocket connection failed: {e}")
            return False
    
    def _run_forever_ssl(self):
        """Run WebSocket with SSL in loop"""
        import ssl
        
        while self.running:
            try:
                if self.ws:
                    # Disable SSL verification for local connections
                    self.ws.run_forever(sslopt={"cert_reqs": ssl.CERT_NONE})
                
                if self.running:
                    logger.info(f"Secure WebSocket disconnected. Reconnecting in {self.reconnect_delay}s...")
                    time.sleep(self.reconnect_delay)
                    
            except Exception as e:
                logger.error(f"Secure WebSocket error in run loop: {e}")
                if self.running:
                    time.sleep(self.reconnect_delay)


def main():
    """Test WebSocket connector"""
    def on_data(data):
        print(f"Received telemetry: {list(data.keys())}")
    
    connector = WebSocketConnector(port=8080)
    connector.set_data_callback(on_data)
    
    if connector.connect():
        print("Connected! Receiving telemetry...")
        try:
            while True:
                time.sleep(1)
                if connector.is_connected():
                    data = connector.get_telemetry()
                    if data:
                        print(f"Latest data keys: {list(data.keys())}")
        except KeyboardInterrupt:
            print("\nStopping...")
    else:
        print("Failed to connect")
    
    connector.disconnect()


if __name__ == "__main__":
    main()
