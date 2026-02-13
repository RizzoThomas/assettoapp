"""
Live Telemetry Plot using Matplotlib
Shows Throttle, Brake, and Wheel Slip events over time
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
    Live telemetry plot showing throttle, brake, and wheel slip
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
        self.throttle_data = deque(maxlen=max_points)
        self.brake_data = deque(maxlen=max_points)
        self.slip_events = []  # List of (time, value) tuples for slip events
        
        self.start_time = 0.0
        self.current_time = 0.0
        
        # Create matplotlib figure
        self.figure = Figure(figsize=(8, 4), dpi=100, facecolor='#2b2b2b')
        self.ax = self.figure.add_subplot(111)
        
        # Configure plot appearance (dark theme)
        self.ax.set_facecolor('#1e1e1e')
        self.ax.set_xlabel('Time (seconds)', color='white')
        self.ax.set_ylabel('Input %', color='white')
        self.ax.set_title('Throttle & Brake Input (Blue dots = Wheel Slip)', 
                         color='white', fontsize=10)
        self.ax.tick_params(colors='white')
        self.ax.spines['bottom'].set_color('white')
        self.ax.spines['top'].set_color('white')
        self.ax.spines['left'].set_color('white')
        self.ax.spines['right'].set_color('white')
        self.ax.grid(True, alpha=0.3, color='gray')
        
        # Set y-axis limits
        self.ax.set_ylim(0, 105)
        
        # Initialize plot lines
        self.throttle_line, = self.ax.plot([], [], 'g-', linewidth=2, label='Throttle')
        self.brake_line, = self.ax.plot([], [], 'r-', linewidth=2, label='Brake')
        self.slip_scatter = self.ax.scatter([], [], c='blue', s=50, marker='o', 
                                           label='Wheel Slip', zorder=5)
        
        self.ax.legend(loc='upper right', facecolor='#2b2b2b', 
                      edgecolor='white', labelcolor='white')
        
        # Embed matplotlib in tkinter
        self.canvas = FigureCanvasTkAgg(self.figure, parent_frame)
        self.canvas.draw()
        self.canvas_widget = self.canvas.get_tk_widget()
        
        logger.info("Telemetry plot initialized")
    
    def get_widget(self):
        """Get the tkinter widget for packing"""
        return self.canvas_widget
    
    def add_data_point(self, time_value: float, throttle: float, brake: float, 
                      has_slip: bool = False):
        """
        Add a new data point to the plot
        
        Args:
            time_value: Time in seconds
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
            throttle_list = list(self.throttle_data)
            brake_list = list(self.brake_data)
            
            # Update throttle line
            self.throttle_line.set_data(time_list, throttle_list)
            
            # Update brake line
            self.brake_line.set_data(time_list, brake_list)
            
            # Update slip scatter
            if len(self.slip_events) > 0:
                slip_times = [e[0] for e in self.slip_events]
                slip_values = [e[1] for e in self.slip_events]
                self.slip_scatter.set_offsets(np.column_stack([slip_times, slip_values]))
            
            # Adjust x-axis limits to show last 30 seconds
            if self.current_time > 30:
                self.ax.set_xlim(self.current_time - 30, self.current_time + 1)
            else:
                self.ax.set_xlim(0, 30)
            
            # Redraw canvas
            self.canvas.draw_idle()
            
        except Exception as e:
            logger.error(f"Error updating plot: {e}")
    
    def clear(self):
        """Clear all data from the plot"""
        self.time_data.clear()
        self.throttle_data.clear()
        self.brake_data.clear()
        self.slip_events.clear()
        self.start_time = 0.0
        self.current_time = 0.0
        
        # Clear plot
        self.throttle_line.set_data([], [])
        self.brake_line.set_data([], [])
        self.slip_scatter.set_offsets(np.empty((0, 2)))
        
        self.canvas.draw()
        logger.info("Telemetry plot cleared")
