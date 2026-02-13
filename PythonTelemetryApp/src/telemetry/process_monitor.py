"""
Process Monitor - Monitor AssettoCorsaEVO.exe process
Uses psutil to detect if the game is running
"""

import psutil
from typing import Optional, List
from loguru import logger


class ProcessMonitor:
    """
    Monitor AssettoCorsaEVO.exe process
    """
    
    PROCESS_NAME = "AssettoCorsaEVO.exe"
    
    def __init__(self):
        self.process = None
        self.pid = None
    
    def is_running(self) -> bool:
        """Check if AssettoCorsaEVO.exe is running"""
        try:
            for proc in psutil.process_iter(['pid', 'name']):
                if proc.info['name'] == self.PROCESS_NAME:
                    self.pid = proc.info['pid']
                    self.process = proc
                    return True
            return False
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            return False
    
    def get_process_info(self) -> Optional[dict]:
        """Get detailed process information"""
        if not self.is_running():
            return None
        
        try:
            proc = psutil.Process(self.pid)
            return {
                'pid': self.pid,
                'name': proc.name(),
                'exe': proc.exe(),
                'cwd': proc.cwd(),
                'cpu_percent': proc.cpu_percent(interval=0.1),
                'memory_mb': proc.memory_info().rss / 1024 / 1024,
                'num_threads': proc.num_threads(),
            }
        except (psutil.NoSuchProcess, psutil.AccessDenied) as e:
            logger.warning(f"Cannot get process info: {e}")
            return None
    
    def get_open_ports(self) -> List[int]:
        """Get list of UDP ports opened by the process"""
        if not self.is_running():
            return []
        
        try:
            proc = psutil.Process(self.pid)
            connections = proc.connections(kind='udp')
            ports = [conn.laddr.port for conn in connections if conn.laddr]
            return ports
        except (psutil.NoSuchProcess, psutil.AccessDenied) as e:
            logger.warning(f"Cannot get open ports: {e}")
            return []
    
    def wait_for_process(self, timeout: float = 10.0) -> bool:
        """
        Wait for process to start
        Returns True if process started within timeout
        """
        import time
        start_time = time.time()
        
        while time.time() - start_time < timeout:
            if self.is_running():
                logger.info(f"Process {self.PROCESS_NAME} detected (PID: {self.pid})")
                return True
            time.sleep(0.5)
        
        return False
