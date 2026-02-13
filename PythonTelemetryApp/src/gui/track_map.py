"""
Track Map - Visualize circuit layout and mark brake/lockup points
"""

import customtkinter as ctk
from matplotlib.figure import Figure
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
import numpy as np
from typing import List, Tuple
from collections import deque


class TrackMap(ctk.CTkFrame):
    """
    Circuit map visualization showing:
    - Track layout from X,Y coordinates
    - Red markers for late braking
    - Red markers for wheel lockups
    """
    
    def __init__(self, master, **kwargs):
        super().__init__(master, **kwargs)
        
        # Data storage
        self.max_points = 5000  # Max points to keep
        self.coordinates = deque(maxlen=self.max_points)
        self.brake_points = []  # Points where braking was detected
        self.lockup_points = []  # Points where lockup occurred
        
        self._create_widgets()
    
    def _create_widgets(self):
        """Create matplotlib canvas for track map"""
        
        # Title
        title = ctk.CTkLabel(
            self,
            text="🗺️ Track Map & Telemetry",
            font=ctk.CTkFont(size=14, weight="bold")
        )
        title.pack(pady=5)
        
        # Create figure with dark background
        self.figure = Figure(figsize=(6, 4), facecolor='#1a1a1a')
        self.ax = self.figure.add_subplot(111, facecolor='#1a1a1a')
        
        # Style
        self.ax.set_xlabel('X Position (m)', color='white')
        self.ax.set_ylabel('Y Position (m)', color='white')
        self.ax.set_title('Circuit Layout', color='white', fontsize=10)
        self.ax.tick_params(colors='white', labelsize=8)
        self.ax.grid(True, alpha=0.2, color='gray')
        
        # Create canvas
        self.canvas = FigureCanvasTkAgg(self.figure, self)
        self.canvas.get_tk_widget().pack(fill='both', expand=True, padx=5, pady=5)
        
        # Initialize empty plots
        self.track_line, = self.ax.plot([], [], 'cyan', linewidth=1, alpha=0.7, label='Track')
        self.brake_scatter = self.ax.scatter([], [], c='yellow', s=30, alpha=0.6, marker='o', label='Brake')
        self.lockup_scatter = self.ax.scatter([], [], c='red', s=50, alpha=0.8, marker='X', label='Lock-up')
        
        self.ax.legend(loc='upper right', fontsize=8, facecolor='#2a2a2a', edgecolor='white')
        self.ax.set_aspect('equal', adjustable='datalim')
        
        self.canvas.draw()
    
    def add_point(self, x: float, y: float, is_braking: bool = False, is_lockup: bool = False):
        """
        Add a telemetry point to the map
        
        Args:
            x: X position
            y: Y position (often use Z in game coordinates)
            is_braking: True if braking (brake > 0.5)
            is_lockup: True if wheel lockup detected
        """
        self.coordinates.append((x, y))
        
        if is_braking and len(self.coordinates) % 10 == 0:  # Sample brake points
            self.brake_points.append((x, y))
            # Keep only last 200 brake points
            if len(self.brake_points) > 200:
                self.brake_points.pop(0)
        
        if is_lockup:
            self.lockup_points.append((x, y))
            # Keep only last 100 lockup points
            if len(self.lockup_points) > 100:
                self.lockup_points.pop(0)
    
    def update_plot(self):
        """Redraw the track map"""
        if len(self.coordinates) < 2:
            return
        
        try:
            # Extract coordinates
            x_coords = [p[0] for p in self.coordinates]
            y_coords = [p[1] for p in self.coordinates]
            
            # Update track line
            self.track_line.set_data(x_coords, y_coords)
            
            # Update brake points
            if self.brake_points:
                brake_x = [p[0] for p in self.brake_points]
                brake_y = [p[1] for p in self.brake_points]
                self.brake_scatter.set_offsets(np.c_[brake_x, brake_y])
            
            # Update lockup points
            if self.lockup_points:
                lockup_x = [p[0] for p in self.lockup_points]
                lockup_y = [p[1] for p in self.lockup_points]
                self.lockup_scatter.set_offsets(np.c_[lockup_x, lockup_y])
            
            # Auto-scale
            self.ax.relim()
            self.ax.autoscale_view()
            
            # Redraw
            self.canvas.draw_idle()
        except Exception as e:
            pass  # Silently fail to avoid UI crashes
    
    def clear_data(self):
        """Clear all recorded data"""
        self.coordinates.clear()
        self.brake_points.clear()
        self.lockup_points.clear()
        
        # Clear plots
        self.track_line.set_data([], [])
        self.brake_scatter.set_offsets(np.empty((0, 2)))
        self.lockup_scatter.set_offsets(np.empty((0, 2)))
        
        self.canvas.draw()
