"""
Lap Comparison System
Compares current lap with best lap and calculates sector deltas
"""

from dataclasses import dataclass, field
from typing import List, Optional, Dict
from loguru import logger
import time


@dataclass
class LapDataPoint:
    """Single data point during a lap"""
    timestamp: float
    distance: float  # Estimated distance along track
    speed_kmh: float
    position_x: float
    position_y: float
    position_z: float
    throttle: float
    brake: float


@dataclass
class LapData:
    """Complete lap data"""
    lap_number: int
    lap_time_ms: int
    is_valid: bool
    data_points: List[LapDataPoint] = field(default_factory=list)
    sector_times: List[int] = field(default_factory=list)  # Sector times in ms


class LapComparison:
    """
    Manages lap data storage and comparison
    """
    
    def __init__(self, num_sectors: int = 3):
        self.num_sectors = num_sectors
        self.current_lap_data: List[LapDataPoint] = []
        self.last_lap: Optional[LapData] = None
        self.best_lap: Optional[LapData] = None
        self.all_laps: List[LapData] = []
        
        self.current_lap_number = 0
        self.lap_start_time = 0.0
        self.recording = False
    
    def start_new_lap(self, lap_number: int):
        """Start recording a new lap"""
        self.current_lap_number = lap_number
        self.lap_start_time = time.time()
        self.current_lap_data = []
        self.recording = True
        logger.info(f"Started recording lap {lap_number}")
    
    def add_data_point(self, car_physics):
        """Add a data point to current lap"""
        if not self.recording:
            return
        
        point = LapDataPoint(
            timestamp=time.time() - self.lap_start_time,
            distance=0.0,  # Would need track position to calculate
            speed_kmh=car_physics.speed_kmh,
            position_x=car_physics.position_x,
            position_y=car_physics.position_y,
            position_z=car_physics.position_z,
            throttle=car_physics.throttle,
            brake=car_physics.brake
        )
        self.current_lap_data.append(point)
    
    def finish_lap(self, lap_time_ms: int, is_valid: bool, sector_times: List[int] = None):
        """Finish current lap and store data"""
        if not self.recording:
            return
        
        lap = LapData(
            lap_number=self.current_lap_number,
            lap_time_ms=lap_time_ms,
            is_valid=is_valid,
            data_points=self.current_lap_data.copy(),
            sector_times=sector_times or []
        )
        
        self.all_laps.append(lap)
        self.last_lap = lap
        
        # Update best lap if this is faster
        if is_valid and (self.best_lap is None or lap_time_ms < self.best_lap.lap_time_ms):
            self.best_lap = lap
            logger.info(f"New best lap! Time: {lap_time_ms/1000:.3f}s")
        
        self.recording = False
        logger.info(f"Lap {self.current_lap_number} completed: {lap_time_ms/1000:.3f}s (Valid: {is_valid})")
    
    def get_speed_delta_vs_best(self, current_speed: float, track_position: float) -> Optional[float]:
        """
        Calculate speed delta vs best lap at current track position
        Returns None if no best lap available
        """
        if not self.best_lap or len(self.best_lap.data_points) == 0:
            return None
        
        # Find closest point in best lap (simplified - should use track distance)
        # This is a placeholder - actual implementation would need proper track mapping
        if len(self.best_lap.data_points) > 0:
            best_speed = sum(p.speed_kmh for p in self.best_lap.data_points) / len(self.best_lap.data_points)
            return current_speed - best_speed
        
        return None
    
    def get_sector_delta(self, sector: int) -> Optional[int]:
        """
        Get delta in ms for a specific sector vs best lap
        Returns None if data not available
        """
        if not self.last_lap or not self.best_lap:
            return None
        
        if (sector >= len(self.last_lap.sector_times) or 
            sector >= len(self.best_lap.sector_times)):
            return None
        
        return self.last_lap.sector_times[sector] - self.best_lap.sector_times[sector]
    
    def get_lap_delta(self) -> Optional[int]:
        """Get total lap time delta vs best lap in ms"""
        if not self.last_lap or not self.best_lap:
            return None
        
        return self.last_lap.lap_time_ms - self.best_lap.lap_time_ms
    
    def get_best_lap_time(self) -> Optional[int]:
        """Get best lap time in ms"""
        return self.best_lap.lap_time_ms if self.best_lap else None
    
    def get_last_lap_time(self) -> Optional[int]:
        """Get last lap time in ms"""
        return self.last_lap.lap_time_ms if self.last_lap else None
    
    def get_lap_count(self) -> int:
        """Get total number of completed laps"""
        return len(self.all_laps)
    
    def get_valid_lap_count(self) -> int:
        """Get number of valid laps"""
        return sum(1 for lap in self.all_laps if lap.is_valid)
    
    def clear_data(self):
        """Clear all lap data"""
        self.current_lap_data = []
        self.last_lap = None
        self.best_lap = None
        self.all_laps = []
        logger.info("All lap data cleared")
