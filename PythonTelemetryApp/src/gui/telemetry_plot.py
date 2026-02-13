"""
Live Telemetry Plot using Matplotlib V2
Shows:
- Upper graph: Speed (km/h) real-time
- Lower graph: Throttle (green) and Brake (red) bars/lines
"""

import matplotlib
matplotlib.use('TkAgg')  # Use TkAgg backend for CustomTkinter compatibility

from matplotlib.figure import Figure
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
import numpy as np
from collections import deque
from loguru import logger


class TelemetryPlot:
    """
    Live telemetry plot with two subplots:
    1. Speed graph (upper)
    2. Throttle/Brake graph (lower)
    """
    
    def __init__(self, parent_frame, max_points=500):
        """
        Initialize telemetry plot
        
        Args:
            parent_frame: Parent tkinter frame
            max_points: Maximum number of data points to display
        """
        self.max_points = max_points
        
        # Data buffers (use deque for efficient append/pop)
        self.time_data = deque(maxlen=max_points)
        self.speed_data = deque(maxlen=max_points)
        self.throttle_data = deque(maxlen=max_points)
        self.brake_data = deque(maxlen=max_points)
        self.slip_events = []  # List of (time, value) tuples for slip events
        
        self.start_time = 0.0
        self.current_time = 0.0
        
        # Create matplotlib figure with 2 subplots
        self.figure = Figure(figsize=(8, 5), dpi=100, facecolor='#2b2b2b')
        self.ax_speed = self.figure.add_subplot(211)  # Upper subplot for speed
        self.ax_inputs = self.figure.add_subplot(212)  # Lower subplot for throttle/brake
        
        # Configure SPEED subplot (upper)
        self.ax_speed.set_facecolor('#1e1e1e')
        self.ax_speed.set_ylabel('Speed (km/h)', color='white', fontsize=10)
        self.ax_speed.set_title('🏎️ Speed Real-Time', color='white', fontsize=10)
        self.ax_speed.tick_params(colors='white', labelsize=8)
        self.ax_speed.spines['bottom'].set_color('white')
        self.ax_speed.spines['top'].set_color('white')
        self.ax_speed.spines['left'].set_color('white')
        self.ax_speed.spines['right'].set_color('white')
        self.ax_speed.grid(True, alpha=0.3, color='gray')
        self.ax_speed.set_xticklabels([])  # Hide x-labels for upper plot
        
        # Initialize speed line (cyan/blue)
        self.speed_line, = self.ax_speed.plot([], [], 'cyan', linewidth=2, label='Speed')
        self.ax_speed.legend(loc='upper right', facecolor='#2b2b2b', 
                            edgecolor='white', labelcolor='white', fontsize=8)
        
        # Configure INPUTS subplot (lower) 
        self.ax_inputs.set_facecolor('#1e1e1e')
        self.ax_inputs.set_xlabel('Time (seconds)', color='white', fontsize=10)
        self.ax_inputs.set_ylabel('Input %', color='white', fontsize=10)
        self.ax_inputs.set_title('🎮 Throttle & Brake Input', color='white', fontsize=10)
        self.ax_inputs.tick_params(colors='white', labelsize=8)
        self.ax_inputs.spines['bottom'].set_color('white')
        self.ax_inputs.spines['top'].set_color('white')
        self.ax_inputs.spines['left'].set_color('white')
        self.ax_inputs.spines['right'].set_color('white')
        self.ax_inputs.grid(True, alpha=0.3, color='gray')
        self.ax_inputs.set_ylim(0, 105)
        
        # Initialize input lines
        self.throttle_line, = self.ax_inputs.plot([], [], 'g-', linewidth=2, label='Throttle')
        self.brake_line, = self.ax_inputs.plot([], [], 'r-', linewidth=2, label='Brake')
        self.slip_scatter = self.ax_inputs.scatter([], [], c='blue', s=30, marker='o', 
                                           label='Wheel Slip', zorder=5, alpha=0.8)
        
        self.ax_inputs.legend(loc='upper right', facecolor='#2b2b2b', 
                      edgecolor='white', labelcolor='white', fontsize=8)
        
        # Adjust spacing between subplots
        self.figure.subplots_adjust(hspace=0.3, left=0.1, right=0.95, top=0.95, bottom=0.1)
        
        # Embed matplotlib in tkinter
        self.canvas = FigureCanvasTkAgg(self.figure, parent_frame)
        self.canvas.draw()
        self.canvas_widget = self.canvas.get_tk_widget()
        
        logger.info("Telemetry plot initialized")
    
    def get_widget(self):
        """Get the tkinter widget for packing"""
        return self.canvas_widget
    
    def add_data_point(self, time_value: float, speed: float, throttle: float, brake: float, 
                      has_slip: bool = False):
        """
        Add a new data point to the plot
        
        Args:
            time_value: Time in seconds
            speed: Speed in km/h
            throttle: Throttle input (0.0 to 1.0)
            brake: Brake input (0.0 to 1.0)
            has_slip: Whether wheel slip is detected at this point
        """
        # Initialize start time on first data point
        if len(self.time_data) == 0:
            self.start_time = time_value
        
        # Relative time from start
        relative_time = time_value - self.start_time
        self.current_time = relative_time
        
        # Add data
        self.time_data.append(relative_time)
        self.speed_data.append(speed)
        self.throttle_data.append(throttle * 100)  # Convert to percentage
        self.brake_data.append(brake * 100)
        
        # Add slip event marker
        if has_slip:
            self.slip_events.append((relative_time, 50))  # Put marker at 50% height
            
            # Limit slip events to keep performance reasonable
            if len(self.slip_events) > 100:
                self.slip_events.pop(0)
    
    def update_plot(self):
        """Update the plot with current data"""
        try:
            if len(self.time_data) == 0:
                return
            
            # Convert deques to lists for plotting
            time_list = list(self.time_data)
            speed_list = list(self.speed_data)
            throttle_list = list(self.throttle_data)
            brake_list = list(self.brake_data)
            
            # Update SPEED line (upper plot)
            self.speed_line.set_data(time_list, speed_list)
            
            # Update THROTTLE line (lower plot)
            self.throttle_line.set_data(time_list, throttle_list)
            
            # Update BRAKE line (lower plot)
            self.brake_line.set_data(time_list, brake_list)
            
            # Update slip scatter (lower plot)
            if len(self.slip_events) > 0:
                slip_times = [e[0] for e in self.slip_events]
                slip_values = [e[1] for e in self.slip_events]
                self.slip_scatter.set_offsets(np.column_stack([slip_times, slip_values]))
            
            # Adjust x-axis limits to show last 30 seconds
            xlim_min = max(0, self.current_time - 30)
            xlim_max = max(30, self.current_time + 1)
            
            self.ax_speed.set_xlim(xlim_min, xlim_max)
            self.ax_inputs.set_xlim(xlim_min, xlim_max)
            
            # Auto-scale y-axis for speed
            if len(speed_list) > 0:
                max_speed = max(speed_list) if speed_list else 300
                self.ax_speed.set_ylim(0, max(100, max_speed + 20))
            
            # Redraw canvas
            self.canvas.draw_idle()
            
        except Exception as e:
            logger.error(f"Error updating plot: {e}")
    
    def clear(self):
        """Clear all data from the plot"""
        self.time_data.clear()
        self.speed_data.clear()
        self.throttle_data.clear()
        self.brake_data.clear()
        self.slip_events.clear()
        self.start_time = 0.0
        self.current_time = 0.0
        
        # Clear plots
        self.speed_line.set_data([], [])
        self.throttle_line.set_data([], [])
        self.brake_line.set_data([], [])
        self.slip_scatter.set_offsets(np.empty((0, 2)))
        
        self.canvas.draw()
        logger.info("Telemetry plot cleared")
