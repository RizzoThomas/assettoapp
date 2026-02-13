"""
Observer Pattern Implementation for Telemetry Updates
"""

from abc import ABC, abstractmethod
from typing import List, Any
from loguru import logger


class TelemetryObserver(ABC):
    """Abstract observer for telemetry updates"""
    
    @abstractmethod
    def update(self, data: Any):
        """Called when telemetry data is updated"""
        pass


class TelemetrySubject:
    """Subject that notifies observers of telemetry changes"""
    
    def __init__(self):
        self._observers: List[TelemetryObserver] = []
        self._data = None
    
    def attach(self, observer: TelemetryObserver):
        """Attach an observer"""
        if observer not in self._observers:
            self._observers.append(observer)
            logger.info(f"Observer {observer.__class__.__name__} attached")
    
    def detach(self, observer: TelemetryObserver):
        """Detach an observer"""
        if observer in self._observers:
            self._observers.remove(observer)
            logger.info(f"Observer {observer.__class__.__name__} detached")
    
    def notify(self, data: Any):
        """Notify all observers of data change"""
        self._data = data
        for observer in self._observers:
            try:
                observer.update(data)
            except Exception as e:
                logger.error(f"Error notifying observer {observer.__class__.__name__}: {e}")
    
    def get_data(self) -> Any:
        """Get current data"""
        return self._data


class TelemetryManager:
    """
    Manages telemetry data acquisition and observer notifications
    """
    
    def __init__(self, data_acquisition):
        self.data_acquisition = data_acquisition
        self.subject = TelemetrySubject()
        self.running = False
    
    def add_observer(self, observer: TelemetryObserver):
        """Add an observer to receive telemetry updates"""
        self.subject.attach(observer)
    
    def remove_observer(self, observer: TelemetryObserver):
        """Remove an observer"""
        self.subject.detach(observer)
    
    def start(self):
        """Start telemetry reading"""
        self.running = True
        logger.info("Telemetry manager started")
    
    def stop(self):
        """Stop telemetry reading"""
        self.running = False
        logger.info("Telemetry manager stopped")
    
    def update(self):
        """Read telemetry and notify observers (call this in main loop)"""
        if not self.running:
            return
        
        if self.data_acquisition.read_telemetry():
            telemetry_data = {
                'car_physics': self.data_acquisition.get_car_physics(),
                'wheels': self.data_acquisition.get_all_wheels(),
                'session': self.data_acquisition.get_session_data()
            }
            self.subject.notify(telemetry_data)
