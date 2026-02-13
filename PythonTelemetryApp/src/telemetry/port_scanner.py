"""
Port Scanner for ACE Telemetry Discovery Mode
Scans open ports from AssettoCorsaEVO.exe process
"""

import psutil
import socket
import time
from typing import List, Tuple, Optional
from loguru import logger


class PortScanner:
    """Scanner for discovering ACE telemetry ports"""
    
    # Target ports to scan
    TARGET_PORTS = [8080, 8081, 9000, 9996]
    PROCESS_NAME = "AssettoCorsaEVO.exe"
    
    def __init__(self):
        self.process_pid: Optional[int] = None
        
    def find_ace_process(self) -> Optional[int]:
        """Find AssettoCorsaEVO.exe process and return PID"""
        try:
            for proc in psutil.process_iter(['pid', 'name']):
                if proc.info['name'] == self.PROCESS_NAME:
                    self.process_pid = proc.info['pid']
                    logger.info(f"Found {self.PROCESS_NAME} with PID: {self.process_pid}")
                    return self.process_pid
            
            logger.warning(f"{self.PROCESS_NAME} not found")
            return None
            
        except Exception as e:
            logger.error(f"Error finding process: {e}")
            return None
    
    def get_process_connections(self) -> List[Tuple[int, str]]:
        """Get all TCP/UDP connections from ACE process"""
        connections = []
        
        if not self.process_pid:
            logger.warning("No process PID set")
            return connections
        
        try:
            process = psutil.Process(self.process_pid)
            conns = process.connections(kind='all')
            
            for conn in conns:
                if conn.laddr:
                    port = conn.laddr.port
                    conn_type = 'TCP' if conn.type == socket.SOCK_STREAM else 'UDP'
                    connections.append((port, conn_type))
            
            logger.info(f"Found {len(connections)} connections from process")
            return connections
            
        except (psutil.NoSuchProcess, psutil.AccessDenied) as e:
            logger.error(f"Cannot access process connections: {e}")
            return connections
    
    def scan_target_ports(self) -> List[Tuple[int, str, str]]:
        """
        Scan target ports and detect protocol type
        Returns: List of (port, connection_type, protocol_guess)
        """
        results = []
        
        # First, get ports opened by the process
        process_connections = self.get_process_connections()
        process_ports = {port for port, _ in process_connections}
        
        logger.info(f"Scanning target ports: {self.TARGET_PORTS}")
        
        for port in self.TARGET_PORTS:
            # Check if this port is opened by ACE process
            if port in process_ports:
                # Determine connection type from process connections
                conn_type = next((ct for p, ct in process_connections if p == port), 'TCP')
                
                # Guess protocol based on port
                protocol = self._guess_protocol(port, conn_type)
                
                results.append((port, conn_type, protocol))
                logger.info(f"[OPEN] Port {port} ({conn_type}) - Protocol: {protocol}")
            else:
                # Try to connect anyway (might be listening but not in connections list)
                if self._test_port(port):
                    protocol = self._guess_protocol(port, 'TCP')
                    results.append((port, 'TCP', protocol))
                    logger.info(f"[CONNECTABLE] Port {port} (TCP) - Protocol: {protocol}")
        
        return results
    
    def _test_port(self, port: int, timeout: float = 0.5) -> bool:
        """Test if a port is connectable"""
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(timeout)
            result = sock.connect_ex(('127.0.0.1', port))
            sock.close()
            return result == 0
        except:
            return False
    
    def _guess_protocol(self, port: int, conn_type: str) -> str:
        """Guess protocol based on port number and connection type"""
        if conn_type == 'UDP':
            if port in [9000, 9996]:
                return 'UDP'
            return 'UDP'
        
        # TCP ports
        if port in [8080, 8081]:
            # Could be HTTP or WebSocket
            # WebSocket uses HTTP upgrade, so both are possible
            return 'HTTP/WebSocket'
        elif port == 9000:
            return 'HTTP'
        elif port == 9996:
            return 'Custom'
        
        return 'Unknown'
    
    def scan_ace_ports(self) -> List[Tuple[int, str, str]]:
        """
        Main scanning function
        Returns: List of (port, connection_type, protocol)
        """
        logger.info("=" * 50)
        logger.info("Starting ACE Telemetry Port Discovery")
        logger.info("=" * 50)
        
        # Step 1: Find process
        pid = self.find_ace_process()
        if not pid:
            logger.warning(f"Cannot scan ports: {self.PROCESS_NAME} not running")
            return []
        
        # Step 2: Scan target ports
        open_ports = self.scan_target_ports()
        
        if open_ports:
            logger.info(f"Discovery complete: Found {len(open_ports)} open ports")
            for port, conn_type, protocol in open_ports:
                logger.info(f"  -> Port {port}: {conn_type} ({protocol})")
        else:
            logger.warning("No telemetry ports detected")
            logger.info("Possible reasons:")
            logger.info("  - ACE not in a session (menu/not driving)")
            logger.info("  - Telemetry not enabled in game settings")
            logger.info("  - Insufficient permissions (try running as Admin)")
        
        return open_ports


def main():
    """Test the port scanner"""
    scanner = PortScanner()
    results = scanner.scan_ace_ports()
    
    if results:
        print("\nDetected Telemetry Ports:")
        for port, conn_type, protocol in results:
            print(f"  Port {port}: {conn_type} - {protocol}")
    else:
        print("\nNo telemetry ports found")


if __name__ == "__main__":
    main()
