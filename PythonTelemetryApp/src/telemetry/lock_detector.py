"""
Lock-up Detection Algorithm
Detects wheel lock-ups during braking
"""

from dataclasses import dataclass
from typing import List, Tuple
from loguru import logger
import time


@dataclass
class LockUpEvent:
    """Represents a detected lock-up event"""
    timestamp: float
    wheel_index: int  # 0=FL, 1=FR, 2=RL, 3=RR
    position_x: float
    position_y: float
    position_z: float
    slip_ratio: float
    brake_input: float
    speed_kmh: float


class LockUpDetector:
    """
    Detects wheel lock-ups by comparing vehicle speed with wheel angular velocity
    """
    
    WHEEL_NAMES = ["Front Left", "Front Right", "Rear Left", "Rear Right"]
    
    def __init__(self, slip_threshold: float = 0.15, brake_threshold: float = 0.05):
        """
        Initialize lock-up detector
        
        Args:
            slip_threshold: Wheel slip ratio threshold for lock-up detection (default: 0.15)
            brake_threshold: Minimum brake input to consider (default: 0.05 = 5%)
        """
        self.slip_threshold = slip_threshold
        self.brake_threshold = brake_threshold
        self.lock_up_events: List[LockUpEvent] = []
        self.last_detection_time = [0.0] * 4  # Cooldown per wheel
        self.detection_cooldown = 1.0  # 1 second cooldown between detections per wheel
    
    def check_lock_ups(self, car_physics, wheels) -> List[LockUpEvent]:
        """
        Check for lock-ups on all wheels
        
        Returns:
            List of detected lock-up events (if any)
        """
        detected_events = []
        current_time = time.time()
        
        # Only check if braking
        if car_physics.brake < self.brake_threshold:
            return detected_events
        
        # Check each wheel
        for i, wheel in enumerate(wheels):
            # Check if enough time has passed since last detection on this wheel
            if current_time - self.last_detection_time[i] < self.detection_cooldown:
                continue
            
            # Detect lock-up: high slip ratio while braking
            if abs(wheel.slip_ratio) > self.slip_threshold and car_physics.brake > self.brake_threshold:
                # Create lock-up event
                event = LockUpEvent(
                    timestamp=current_time,
                    wheel_index=i,
                    position_x=car_physics.position_x,
                    position_y=car_physics.position_y,
                    position_z=car_physics.position_z,
                    slip_ratio=wheel.slip_ratio,
                    brake_input=car_physics.brake,
                    speed_kmh=car_physics.speed_kmh
                )
                
                detected_events.append(event)
                self.lock_up_events.append(event)
                self.last_detection_time[i] = current_time
                
                logger.warning(
                    f"LOCK-UP DETECTED: {self.WHEEL_NAMES[i]} | "
                    f"Slip: {wheel.slip_ratio:.3f} | "
                    f"Brake: {car_physics.brake:.2f} | "
                    f"Speed: {car_physics.speed_kmh:.1f} km/h | "
                    f"Position: ({car_physics.position_x:.1f}, {car_physics.position_y:.1f}, {car_physics.position_z:.1f})"
                )
        
        return detected_events
    
    def get_all_lock_ups(self) -> List[LockUpEvent]:
        """Get all recorded lock-up events"""
        return self.lock_up_events
    
    def get_lock_ups_by_wheel(self, wheel_index: int) -> List[LockUpEvent]:
        """Get lock-up events for a specific wheel"""
        return [event for event in self.lock_up_events if event.wheel_index == wheel_index]
    
    def get_lock_up_count(self) -> int:
        """Get total number of lock-ups detected"""
        return len(self.lock_up_events)
    
    def get_lock_up_count_by_wheel(self) -> Tuple[int, int, int, int]:
        """Get lock-up count per wheel (FL, FR, RL, RR)"""
        counts = [0, 0, 0, 0]
        for event in self.lock_up_events:
            counts[event.wheel_index] += 1
        return tuple(counts)
    
    def clear_events(self):
        """Clear all recorded lock-up events"""
        self.lock_up_events.clear()
        logger.info("Lock-up events cleared")
    
    def set_thresholds(self, slip_threshold: float = None, brake_threshold: float = None):
        """Update detection thresholds"""
        if slip_threshold is not None:
            self.slip_threshold = slip_threshold
            logger.info(f"Slip threshold updated to: {slip_threshold}")
        
        if brake_threshold is not None:
            self.brake_threshold = brake_threshold
            logger.info(f"Brake threshold updated to: {brake_threshold}")
