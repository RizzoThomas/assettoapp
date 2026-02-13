"""
Engineer-Style Telemetry Plot
Professional overlapping graph with dual Y-axis
Shows Throttle, Brake, and Speed on single time-based plot
"""

import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from matplotlib.figure import Figure
import numpy as np
from collections import deque
from typing import Optional, Tuple
import tkinter as tk


class EngineerPlot:
    """
    Professional engineer-style telemetry plot
    - Green line: Throttle (0-100%)
    - Red line: Brake (0-100%)
    - Blue line: Speed (km/h)
    - Yellow markers: LOCK events
    """
    
    def __init__(self, parent, figsize=(12, 5)):
        self.parent = parent
        
        # Data buffers (time-based, last 30 seconds)
        self.max_points = 600  # 30s at 20Hz
        self.time_data = deque(maxlen=self.max_points)
        self.throttle_data = deque(maxlen=self.max_points)
        self.brake_data = deque(maxlen=self.max_points)
        self.speed_data = deque(maxlen=self.max_points)
        self.lock_events = deque(maxlen=100)  # (time, y_pos, wheel_name)
        
        self.start_time = 0.0
        self.current_time = 0.0
        
        # Create figure with dark theme
        plt.style.use('dark_background')
        self.fig = Figure(figsize=figsize, facecolor='#1e1e1e')
        self.ax1 = self.fig.add_subplot(111)
        
        # Setup dual Y-axis
        self.ax2 = self.ax1.twinx()  # Second Y-axis for speed
        
        # Configure left Y-axis (Throttle/Brake %)
        self.ax1.set_ylabel('Throttle / Brake (%)', color='white', fontsize=11)
        self.ax1.set_ylim(0, 105)
        self.ax1.tick_params(axis='y', labelcolor='white')
        self.ax1.grid(True, alpha=0.2)
        
        # Configure right Y-axis (Speed km/h)
        self.ax2.set_ylabel('Speed (km/h)', color='cyan', fontsize=11)
        self.ax2.set_ylim(0, 300)
        self.ax2.tick_params(axis='y', labelcolor='cyan')
        
        # X-axis (Time)
        self.ax1.set_xlabel('Time (seconds)', color='white', fontsize=11)
        self.ax1.set_xlim(0, 30)
        self.ax1.tick_params(axis='x', labelcolor='white')
        
        # Title
        self.ax1.set_title('Engineer Telemetry Plot - Trail Braking Analysis', 
                          color='white', fontsize=12, fontweight='bold')
        
        # Initialize lines
        self.throttle_line, = self.ax1.plot([], [], 'g-', linewidth=2, label='Throttle', alpha=0.9)
        self.brake_line, = self.ax1.plot([], [], 'r-', linewidth=2, label='Brake', alpha=0.9)
        self.speed_line, = self.ax2.plot([], [], 'c--', linewidth=2, label='Speed', alpha=0.8)
        
        # Lock markers (yellow)
        self.lock_scatter = self.ax1.scatter([], [], c='yellow', marker='v', s=100, 
                                            label='LOCK', zorder=5, alpha=0.9)
        
        # Legend
        lines1, labels1 = self.ax1.get_legend_handles_labels()
        lines2, labels2 = self.ax2.get_legend_handles_labels()
        self.ax1.legend(lines1 + lines2, labels1 + labels2, 
                       loc='upper left', framealpha=0.8, fontsize=9)
        
        # Tight layout
        self.fig.tight_layout()
        
        # Create canvas
        self.canvas = FigureCanvasTkAgg(self.fig, master=parent)
        self.canvas_widget = self.canvas.get_tk_widget()
        
    def update(self, throttle: float, brake: float, speed: float, 
               lock_event: Optional[Tuple[str, float]] = None):
        """
        Update plot with new telemetry data
        
        Args:
            throttle: Throttle input 0-100%
            brake: Brake input 0-100%
            speed: Speed in km/h
            lock_event: Optional (wheel_name, severity) tuple if lock detected
        """
        # Update time
        if self.start_time == 0.0:
            import time
            self.start_time = time.time()
        
        import time
        self.current_time = time.time() - self.start_time
        
        # Add data to buffers
        self.time_data.append(self.current_time)
        self.throttle_data.append(throttle)
        self.brake_data.append(brake)
        self.speed_data.append(speed)
        
        # Add lock event if present
        if lock_event:
            wheel_name, severity = lock_event
            # Position marker at brake level
            y_pos = brake if brake > 0 else 10
            self.lock_events.append((self.current_time, y_pos, wheel_name))
        
        # Update line data
        times = list(self.time_data)
        self.throttle_line.set_data(times, list(self.throttle_data))
        self.brake_line.set_data(times, list(self.brake_data))
        self.speed_line.set_data(times, list(self.speed_data))
        
        # Update lock markers
        if self.lock_events:
            lock_times = [t for t, y, w in self.lock_events]
            lock_y = [y for t, y, w in self.lock_events]
            self.lock_scatter.set_offsets(np.c_[lock_times, lock_y])
        
        # Auto-scroll X-axis (show last 30 seconds)
        if self.current_time > 30:
            self.ax1.set_xlim(self.current_time - 30, self.current_time)
        else:
            self.ax1.set_xlim(0, 30)
        
        # Auto-scale speed Y-axis if needed
        if speed > 280:
            self.ax2.set_ylim(0, speed + 20)
        
        # Redraw canvas
        self.canvas.draw()
    
    def clear(self):
        """Clear all data"""
        self.time_data.clear()
        self.throttle_data.clear()
        self.brake_data.clear()
        self.speed_data.clear()
        self.lock_events.clear()
        
        self.throttle_line.set_data([], [])
        self.brake_line.set_data([], [])
        self.speed_line.set_data([], [])
        self.lock_scatter.set_offsets(np.empty((0, 2)))
        
        self.start_time = 0.0
        self.current_time = 0.0
        
        self.ax1.set_xlim(0, 30)
        self.ax2.set_ylim(0, 300)
        
        self.canvas.draw()
    
    def get_widget(self) -> tk.Widget:
        """Get the tkinter widget for embedding"""
        return self.canvas_widget
    
    def pack(self, **kwargs):
        """Pack the canvas widget"""
        self.canvas_widget.pack(**kwargs)
    
    def grid(self, **kwargs):
        """Grid the canvas widget"""
        self.canvas_widget.grid(**kwargs)


def demo():
    """Demo the engineer plot"""
    import math
    import time as time_module
    
    root = tk.Tk()
    root.title("Engineer Plot Demo")
    root.geometry("1200x600")
    root.configure(bg='#1e1e1e')
    
    plot = EngineerPlot(root)
    plot.pack(fill=tk.BOTH, expand=True)
    
    # Simulate telemetry data
    t = [0]
    
    def update_plot():
        t[0] += 0.05  # 20Hz
        
        # Simulate trail braking scenario
        phase = t[0] % 10
        
        if phase < 3:  # Acceleration
            throttle = min(100, phase / 3 * 100)
            brake = 0
            speed = 50 + (phase / 3) * 150
        elif phase < 5:  # Braking with trail braking
            progress = (phase - 3) / 2
            throttle = 0
            brake = 90 - (progress * 40)  # Gradually release brake
            speed = 200 - (progress * 100)
            # Simulate occasional lock
            if progress < 0.3 and t[0] % 1 < 0.1:
                plot.update(throttle, brake, speed, lock_event=('FL', 'HIGH'))
                root.after(50, update_plot)
                return
        elif phase < 7:  # Exit with throttle
            progress = (phase - 5) / 2
            throttle = progress * 80
            brake = max(0, 40 - progress * 50)
            speed = 100 + (progress * 80)
        else:  # Full throttle
            throttle = 100
            brake = 0
            speed = 180 + ((phase - 7) / 3) * 70
        
        plot.update(throttle, brake, speed)
        root.after(50, update_plot)  # 20Hz
    
    # Start update loop
    root.after(100, update_plot)
    
    root.mainloop()


if __name__ == "__main__":
    demo()
