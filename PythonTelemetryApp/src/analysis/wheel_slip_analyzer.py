"""
Advanced Wheel Slip Analyzer
Detects wheel lock-ups using 25% threshold
Analyzes difference between car speed and wheel speed
"""

from typing import Optional, Tuple, List
from dataclasses import dataclass
from loguru import logger


@dataclass
class LockEvent:
    """Represents a wheel lock event"""
    wheel: str  # FL, FR, RL, RR
    timestamp: float
    slip_ratio: float  # How much the wheel slowed down (0-1)
    severity: str  # LOW, MEDIUM, HIGH
    car_speed: float  # km/h
    wheel_speed: float  # km/h


class WheelSlipAnalyzer:
    """
    Analyzes wheel slip and detects lock-ups
    
    Lock-up detection:
    - During braking (brake > threshold)
    - If wheel slows down >25% compared to car speed
    - Formula: slip_ratio = (car_speed - wheel_speed) / car_speed
    """
    
    LOCK_THRESHOLD = 0.25  # 25% slip = lock-up
    BRAKE_THRESHOLD = 0.1   # Must be braking (10% brake input)
    
    def __init__(self, lock_threshold: float = 0.25, brake_threshold: float = 0.1):
        """
        Initialize analyzer
        
        Args:
            lock_threshold: Slip ratio threshold for lock detection (default 0.25 = 25%)
            brake_threshold: Minimum brake input to consider (default 0.1 = 10%)
        """
        self.lock_threshold = lock_threshold
        self.brake_threshold = brake_threshold
        self.recent_locks: List[LockEvent] = []
        self.lock_cooldown = {}  # Prevent duplicate detections
        self.cooldown_time = 0.5  # seconds
        
    def analyze(self, 
                car_speed_kmh: float,
                wheel_speeds_kmh: dict,  # {'FL': speed, 'FR': speed, 'RL': speed, 'RR': speed}
                brake_input: float,  # 0.0 - 1.0
                timestamp: float) -> List[LockEvent]:
        """
        Analyze telemetry and detect lock-ups
        
        Args:
            car_speed_kmh: Car speed in km/h
            wheel_speeds_kmh: Dict with wheel speeds {'FL': 120.5, 'FR': 120.3, ...}
            brake_input: Brake input 0.0-1.0
            timestamp: Current timestamp
            
        Returns:
            List of LockEvent objects (empty if no locks detected)
        """
        locks = []
        
        # Must be braking to lock
        if brake_input < self.brake_threshold:
            return locks
        
        # Must be moving
        if car_speed_kmh < 10.0:
            return locks
        
        # Check each wheel
        for wheel_name in ['FL', 'FR', 'RL', 'RR']:
            # Check cooldown
            if wheel_name in self.lock_cooldown:
                if timestamp - self.lock_cooldown[wheel_name] < self.cooldown_time:
                    continue
            
            # Get wheel speed
            wheel_speed = wheel_speeds_kmh.get(wheel_name, car_speed_kmh)
            
            # Calculate slip ratio
            # slip_ratio = (car_speed - wheel_speed) / car_speed
            # If wheel is 25% slower than car, it's locking
            if car_speed_kmh > 0:
                slip_ratio = (car_speed_kmh - wheel_speed) / car_speed_kmh
            else:
                slip_ratio = 0.0
            
            # Detect lock
            if slip_ratio >= self.lock_threshold:
                severity = self._calculate_severity(slip_ratio)
                
                lock_event = LockEvent(
                    wheel=wheel_name,
                    timestamp=timestamp,
                    slip_ratio=slip_ratio,
                    severity=severity,
                    car_speed=car_speed_kmh,
                    wheel_speed=wheel_speed
                )
                
                locks.append(lock_event)
                self.lock_cooldown[wheel_name] = timestamp
                self.recent_locks.append(lock_event)
                
                logger.warning(
                    f"[LOCK] {wheel_name} at {car_speed_kmh:.1f} km/h "
                    f"(slip: {slip_ratio*100:.1f}%, severity: {severity})"
                )
        
        # Cleanup old locks (keep last 100)
        if len(self.recent_locks) > 100:
            self.recent_locks = self.recent_locks[-100:]
        
        return locks
    
    def _calculate_severity(self, slip_ratio: float) -> str:
        """Calculate lock severity based on slip ratio"""
        if slip_ratio >= 0.5:
            return 'HIGH'  # 50%+ slip = heavy lock
        elif slip_ratio >= 0.35:
            return 'MEDIUM'  # 35-50% = medium lock
        else:
            return 'LOW'  # 25-35% = light lock
    
    def analyze_simple(self,
                      car_speed_kmh: float,
                      wheel_angular_velocities: dict,  # rad/s
                      wheel_radius: float,  # meters
                      brake_input: float,
                      timestamp: float) -> List[LockEvent]:
        """
        Simplified analysis when only angular velocities are available
        
        Args:
            car_speed_kmh: Car speed in km/h
            wheel_angular_velocities: Angular velocities in rad/s
            wheel_radius: Wheel radius in meters (typically 0.3-0.35m)
            brake_input: Brake input 0.0-1.0
            timestamp: Current timestamp
        """
        # Convert car speed to m/s
        car_speed_ms = car_speed_kmh / 3.6
        
        # Calculate wheel speeds from angular velocities
        # v = ω * r
        wheel_speeds_kmh = {}
        for wheel_name, angular_vel in wheel_angular_velocities.items():
            wheel_speed_ms = abs(angular_vel) * wheel_radius
            wheel_speeds_kmh[wheel_name] = wheel_speed_ms * 3.6  # Convert to km/h
        
        return self.analyze(car_speed_kmh, wheel_speeds_kmh, brake_input, timestamp)
    
    def get_recent_locks(self, time_window: float = 5.0) -> List[LockEvent]:
        """Get lock events from recent time window"""
        if not self.recent_locks:
            return []
        
        current_time = self.recent_locks[-1].timestamp
        cutoff_time = current_time - time_window
        
        return [lock for lock in self.recent_locks if lock.timestamp >= cutoff_time]
    
    def get_lock_count_by_wheel(self) -> dict:
        """Get count of locks per wheel"""
        counts = {'FL': 0, 'FR': 0, 'RL': 0, 'RR': 0}
        for lock in self.recent_locks:
            counts[lock.wheel] += 1
        return counts
    
    def clear_history(self):
        """Clear lock history"""
        self.recent_locks.clear()
        self.lock_cooldown.clear()


def demo():
    """Demo the wheel slip analyzer"""
    import time
    
    analyzer = WheelSlipAnalyzer(lock_threshold=0.25)
    
    print("Wheel Slip Analyzer Demo")
    print("=" * 50)
    
    # Simulate a braking scenario with front left lock
    scenarios = [
        # (car_speed, FL, FR, RL, RR, brake)
        (200.0, 200.0, 200.0, 200.0, 200.0, 0.0),  # Cruising
        (190.0, 190.0, 190.0, 190.0, 190.0, 0.3),  # Light braking
        (180.0, 170.0, 180.0, 180.0, 180.0, 0.5),  # FL starting to lock
        (170.0, 120.0, 170.0, 170.0, 170.0, 0.7),  # FL LOCKED (30% slip)
        (160.0, 110.0, 160.0, 160.0, 160.0, 0.8),  # FL still locked
        (150.0, 140.0, 150.0, 150.0, 150.0, 0.6),  # FL recovered
        (140.0, 140.0, 140.0, 140.0, 140.0, 0.4),  # Normal braking
    ]
    
    timestamp = 0.0
    for car_speed, fl, fr, rl, rr, brake in scenarios:
        timestamp += 0.1
        
        wheel_speeds = {
            'FL': fl,
            'FR': fr,
            'RL': rl,
            'RR': rr
        }
        
        locks = analyzer.analyze(car_speed, wheel_speeds, brake, timestamp)
        
        print(f"\nTime: {timestamp:.1f}s | Speed: {car_speed:.0f} km/h | Brake: {brake*100:.0f}%")
        print(f"  Wheel speeds: FL={fl:.0f} FR={fr:.0f} RL={rl:.0f} RR={rr:.0f}")
        
        if locks:
            for lock in locks:
                print(f"  [LOCK DETECTED] {lock.wheel}: {lock.slip_ratio*100:.1f}% slip ({lock.severity})")
        else:
            print("  No locks")
    
    print("\n" + "=" * 50)
    print("Lock Summary:")
    counts = analyzer.get_lock_count_by_wheel()
    for wheel, count in counts.items():
        print(f"  {wheel}: {count} locks")


if __name__ == "__main__":
    demo()
