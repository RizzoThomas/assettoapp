"""
Main GUI Window V3 - Enhanced with all advanced features
- Multi-method connection (Process + UDP + SharedMemory + HTTP)
- Dual telemetry plots (Speed + Throttle/Brake)
- Track map visualization
- Flashing lockup indicators
- Debug console
- Strategic setup management
"""

import customtkinter as ctk
from telemetry.observer import TelemetryObserver
from telemetry.enhanced_ace_connector import EnhancedACEConnector
from gui.telemetry_plot import TelemetryPlot
from gui.lockup_indicator import LockupIndicator
from gui.track_map import TrackMap
from loguru import logger
import threading
import time


class MainWindowV3(ctk.CTk, TelemetryObserver):
    """Enhanced main application window with all advanced features"""
    
    def __init__(self, telemetry_manager, lock_detector, lap_comparison, setup_manager):
        super().__init__()
        
        # Create enhanced ACE connector
        self.ace_connector = EnhancedACEConnector()
        
        # Store references
        self.telemetry_manager = telemetry_manager
        self.lock_detector = lock_detector
        self.lap_comparison = lap_comparison
        self.setup_manager = setup_manager
        
        # Subscribe to telemetry updates
        self.telemetry_manager.add_observer(self)
        
        # Window configuration
        self.title("ACE Telemetry V3 - Advanced Edition")
        self.geometry("1600x1000")
        
        # State
        self.connected = False
        self.running = False
        self.last_lap_number = 0
        self.reconnect_thread = None
        self.update_counter = 0
        
        # Create UI
        self._create_widgets()
        
        # Start auto-reconnect thread
        self.after(1000, self._start_reconnect_thread)
    
    def _create_widgets(self):
        """Create all GUI widgets"""
        
        # Configure grid - 3 columns layout
        self.grid_columnconfigure(1, weight=2)  # Main content
        self.grid_columnconfigure(2, weight=1)  # Right panel
        self.grid_rowconfigure(0, weight=1)
        
        # Left Sidebar
        self._create_sidebar()
        
        # Center content area
        self.center_frame = ctk.CTkFrame(self, corner_radius=0)
        self.center_frame.grid(row=0, column=1, sticky="nsew", padx=5, pady=5)
        self.center_frame.grid_columnconfigure(0, weight=1)
        self.center_frame.grid_rowconfigure((0, 1, 2, 3), weight=1)
        
        # Telemetry display
        self._create_telemetry_panel()
        
        # Dual plot (Speed + Throttle/Brake)
        self._create_plot_panel()
        
        # Debug console
        self._create_debug_console()
        
        # Lockup indicators
        self._create_lockup_panel()
        
        # Right panel - Track Map
        self._create_right_panel()
    
    def _create_sidebar(self):
        """Create left sidebar with controls"""
        self.sidebar = ctk.CTkFrame(self, width=250, corner_radius=0)
        self.sidebar.grid(row=0, column=0, sticky="nsew")
        self.sidebar.grid_rowconfigure(12, weight=1)
        
        # Title
        title = ctk.CTkLabel(
            self.sidebar,
            text="ACE Telemetry V3",
            font=ctk.CTkFont(size=20, weight="bold")
        )
        title.grid(row=0, column=0, padx=20, pady=(20, 10))
        
        # Connection status
        self.status_label = ctk.CTkLabel(
            self.sidebar,
            text="⚠️ Connecting...",
            font=ctk.CTkFont(size=11),
            wraplength=200
        )
        self.status_label.grid(row=1, column=0, padx=20, pady=10)
        
        # Force reconnect button
        self.reconnect_btn = ctk.CTkButton(
            self.sidebar,
            text="🔄 Force Reconnect",
            command=self._force_reconnect,
            font=ctk.CTkFont(size=12)
        )
        self.reconnect_btn.grid(row=2, column=0, padx=20, pady=5)
        
        # Separator
        separator = ctk.CTkLabel(self.sidebar, text="────────────", text_color="gray")
        separator.grid(row=3, column=0, pady=10)
        
        # Setup Strategy Section
        strategy_label = ctk.CTkLabel(
            self.sidebar,
            text="⚙️ Setup Strategy",
            font=ctk.CTkFont(size=14, weight="bold")
        )
        strategy_label.grid(row=4, column=0, padx=20, pady=(10, 5))
        
        # Safe Setup Button
        self.safe_btn = ctk.CTkButton(
            self.sidebar,
            text="🛡️ Safe Setup",
            command=lambda: self._apply_setup("Safe"),
            fg_color="#2d5016",
            hover_color="#3d6826",
            font=ctk.CTkFont(size=13)
        )
        self.safe_btn.grid(row=5, column=0, padx=20, pady=5)
        
        # Balanced Setup Button
        self.balanced_btn = ctk.CTkButton(
            self.sidebar,
            text="⚖️ Balanced Setup",
            command=lambda: self._apply_setup("Balanced"),
            fg_color="#805000",
            hover_color="#906010",
            font=ctk.CTkFont(size=13)
        )
        self.balanced_btn.grid(row=6, column=0, padx=20, pady=5)
        
        # Aggressive Setup Button
        self.aggressive_btn = ctk.CTkButton(
            self.sidebar,
            text="🔥 Aggressive Setup",
            command=lambda: self._apply_setup("Aggressive"),
            fg_color="#8b0000",
            hover_color="#a01010",
            font=ctk.CTkFont(size=13)
        )
        self.aggressive_btn.grid(row=7, column=0, padx=20, pady=5)
        
        # Separator
        separator2 = ctk.CTkLabel(self.sidebar, text="────────────", text_color="gray")
        separator2.grid(row=8, column=0, pady=10)
        
        # Clear Data Button
        self.clear_btn = ctk.CTkButton(
            self.sidebar,
            text="🧹 Clear Data",
            command=self._clear_data,
            fg_color="#404040",
            hover_color="#505050",
            font=ctk.CTkFont(size=12)
        )
        self.clear_btn.grid(row=9, column=0, padx=20, pady=5)
        
        # Info
        info_label = ctk.CTkLabel(
            self.sidebar,
            text="Made for ACE Update 0.5",
            font=ctk.CTkFont(size=9),
            text_color="gray"
        )
        info_label.grid(row=11, column=0, padx=20, pady=(0, 20), sticky="s")
    
    def _create_telemetry_panel(self):
        """Create telemetry data display panel"""
        panel = ctk.CTkFrame(self.center_frame)
        panel.grid(row=0, column=0, padx=5, pady=5, sticky="nsew")
        panel.grid_columnconfigure((0, 1, 2, 3), weight=1)
        
        # Title
        title = ctk.CTkLabel(
            panel,
            text="📊 Live Telemetry Data",
            font=ctk.CTkFont(size=14, weight="bold")
        )
        title.grid(row=0, column=0, columnspan=4, pady=5)
        
        # Speed
        self.speed_label = ctk.CTkLabel(
            panel,
            text="Speed: 0 km/h",
            font=ctk.CTkFont(size=12)
        )
        self.speed_label.grid(row=1, column=0, padx=10, pady=5)
        
        # RPM
        self.rpm_label = ctk.CTkLabel(
            panel,
            text="RPM: 0",
            font=ctk.CTkFont(size=12)
        )
        self.rpm_label.grid(row=1, column=1, padx=10, pady=5)
        
        # Gear
        self.gear_label = ctk.CTkLabel(
            panel,
            text="Gear: N",
            font=ctk.CTkFont(size=12)
        )
        self.gear_label.grid(row=1, column=2, padx=10, pady=5)
        
        # G-Forces
        self.g_label = ctk.CTkLabel(
            panel,
            text="G: 0.0 | 0.0 | 0.0",
            font=ctk.CTkFont(size=12)
        )
        self.g_label.grid(row=1, column=3, padx=10, pady=5)
        
        # Throttle
        self.throttle_label = ctk.CTkLabel(
            panel,
            text="Throttle: 0%",
            font=ctk.CTkFont(size=11)
        )
        self.throttle_label.grid(row=2, column=0, padx=10, pady=5)
        
        # Brake
        self.brake_label = ctk.CTkLabel(
            panel,
            text="Brake: 0%",
            font=ctk.CTkFont(size=11)
        )
        self.brake_label.grid(row=2, column=1, padx=10, pady=5)
        
        # Lap Time
        self.lap_label = ctk.CTkLabel(
            panel,
            text="Lap: 0:00.000",
            font=ctk.CTkFont(size=11)
        )
        self.lap_label.grid(row=2, column=2, padx=10, pady=5)
        
        # Best Lap
        self.best_lap_label = ctk.CTkLabel(
            panel,
            text="Best: 0:00.000",
            font=ctk.CTkFont(size=11)
        )
        self.best_lap_label.grid(row=2, column=3, padx=10, pady=5)
    
    def _create_plot_panel(self):
        """Create dual telemetry plot panel"""
        panel = ctk.CTkFrame(self.center_frame)
        panel.grid(row=1, column=0, padx=5, pady=5, sticky="nsew")
        
        # Create plot
        self.telemetry_plot = TelemetryPlot(panel, max_points=500)
        self.telemetry_plot.get_widget().pack(fill='both', expand=True, padx=5, pady=5)
        
        # Schedule plot updates (5Hz for performance)
        self.after(200, self._update_plot_loop)
    
    def _create_debug_console(self):
        """Create debug console for error messages"""
        panel = ctk.CTkFrame(self.center_frame)
        panel.grid(row=2, column=0, padx=5, pady=5, sticky="nsew")
        
        # Title
        title = ctk.CTkLabel(
            panel,
            text="🔍 Debug Console",
            font=ctk.CTkFont(size=12, weight="bold")
        )
        title.pack(pady=5)
        
        # Text widget
        self.debug_text = ctk.CTkTextbox(
            panel,
            height=80,
            font=ctk.CTkFont(size=9, family="Courier New")
        )
        self.debug_text.pack(fill='both', expand=True, padx=5, pady=5)
        
        # Initial message
        self._log_debug("🔍 Cercando AssettoCorsaEVO.exe...")
    
    def _create_lockup_panel(self):
        """Create wheel lockup indicators"""
        self.lockup_indicator = LockupIndicator(self.center_frame)
        self.lockup_indicator.grid(row=3, column=0, padx=5, pady=5, sticky="nsew")
    
    def _create_right_panel(self):
        """Create right panel with track map"""
        panel = ctk.CTkFrame(self, corner_radius=0)
        panel.grid(row=0, column=2, sticky="nsew", padx=5, pady=5)
        panel.grid_rowconfigure(0, weight=1)
        panel.grid_columnconfigure(0, weight=1)
        
        # Track map
        self.track_map = TrackMap(panel)
        self.track_map.pack(fill='both', expand=True)
        
        # Schedule map updates (2Hz for performance)
        self.after(500, self._update_map_loop)
    
    def _start_reconnect_thread(self):
        """Start background thread for auto-reconnect"""
        if self.reconnect_thread is None or not self.reconnect_thread.is_alive():
            self.reconnect_thread = threading.Thread(target=self._reconnect_loop, daemon=True)
            self.reconnect_thread.start()
    
    def _reconnect_loop(self):
        """Background thread that continuously tries to connect"""
        while True:
            if not self.connected:
                success = self.ace_connector.try_connect()
                
                if success:
                    self.connected = True
                    conn_type = self.ace_connector.connection_type
                    self._update_status(f"✅ Connected ({conn_type})")
                    self._log_debug(f"✅ Connesso tramite {conn_type}")
                else:
                    error = self.ace_connector.last_error
                    self._update_status(f"❌ {error}")
                    if error:
                        self._log_debug(f"❌ {error}")
            
            time.sleep(5.0)  # Check every 5 seconds
    
    def _force_reconnect(self):
        """Force immediate reconnection attempt"""
        self._log_debug("🔄 Riconnessione forzata...")
        self.connected = False
        self.ace_connector.last_connection_attempt = 0.0  # Reset throttle
    
    def _apply_setup(self, setup_type: str):
        """Apply a setup strategy"""
        self._log_debug(f"⚙️ Applicando setup {setup_type}...")
        
        # Get current track/car from telemetry
        # For now, use defaults
        self.setup_manager.set_track_and_car("generic_track", "generic_car")
        
        success = self.setup_manager.apply_setup(setup_type)
        
        if success:
            self._log_debug(f"✅ Setup {setup_type} applicato con successo!")
        else:
            self._log_debug(f"❌ Errore nell'applicazione del setup {setup_type}")
    
    def _clear_data(self):
        """Clear all recorded data"""
        self._log_debug("🧹 Pulizia dati...")
        
        # Clear plot
        if hasattr(self, 'telemetry_plot'):
            self.telemetry_plot.clear()
        
        # Clear map
        if hasattr(self, 'track_map'):
            self.track_map.clear_data()
        
        # Clear lap comparison
        self.lap_comparison.clear()
        
        self._log_debug("✅ Dati puliti")
    
    def _update_status(self, status: str):
        """Update status label (thread-safe)"""
        try:
            self.after(0, lambda: self.status_label.configure(text=status))
        except:
            pass
    
    def _log_debug(self, message: str):
        """Add message to debug console (thread-safe)"""
        try:
            def update():
                self.debug_text.insert('end', f"{message}\n")
                self.debug_text.see('end')  # Auto-scroll
            self.after(0, update)
        except:
            pass
    
    def _update_plot_loop(self):
        """Update telemetry plot periodically"""
        if hasattr(self, 'telemetry_plot'):
            self.telemetry_plot.update_plot()
        self.after(200, self._update_plot_loop)  # 5Hz
    
    def _update_map_loop(self):
        """Update track map periodically"""
        if hasattr(self, 'track_map'):
            self.track_map.update_plot()
        self.after(500, self._update_map_loop)  # 2Hz
    
    def on_telemetry_update(self, data: dict):
        """Handle telemetry updates (Observer pattern)"""
        try:
            # Read from connector if connected
            if self.connected and self.ace_connector.read_telemetry():
                telemetry = self.ace_connector.get_telemetry_data()
                
                # Update displays
                self._update_telemetry_display(telemetry)
                
                # Update plot
                self._add_plot_data(telemetry)
                
                # Update map
                self._add_map_data(telemetry)
                
                # Update lockup indicator
                self._update_lockup_indicator(telemetry)
                
        except Exception as e:
            logger.error(f"Error in telemetry update: {e}")
    
    def _update_telemetry_display(self, data: dict):
        """Update telemetry display labels"""
        try:
            speed = data.get('speed_kmh', 0)
            rpm = data.get('rpm', 0)
            gear = data.get('gear', 0)
            throttle = data.get('gas', 0) * 100
            brake = data.get('brake', 0) * 100
            lap_time = data.get('lap_time', 0)
            best_lap = data.get('best_lap', 0)
            
            g_x = data.get('acc_g_x', 0)
            g_y = data.get('acc_g_y', 0)
            g_z = data.get('acc_g_z', 0)
            
            self.speed_label.configure(text=f"Speed: {int(speed)} km/h")
            self.rpm_label.configure(text=f"RPM: {int(rpm)}")
            self.gear_label.configure(text=f"Gear: {gear if gear > 0 else 'N'}")
            self.throttle_label.configure(text=f"Throttle: {int(throttle)}%")
            self.brake_label.configure(text=f"Brake: {int(brake)}%")
            self.g_label.configure(text=f"G: {g_x:.1f} | {g_y:.1f} | {g_z:.1f}")
            
            # Format lap times
            lap_str = self._format_lap_time(lap_time)
            best_str = self._format_lap_time(best_lap)
            self.lap_label.configure(text=f"Lap: {lap_str}")
            self.best_lap_label.configure(text=f"Best: {best_str}")
            
        except Exception as e:
            logger.error(f"Error updating display: {e}")
    
    def _add_plot_data(self, data: dict):
        """Add data point to telemetry plot"""
        try:
            speed = data.get('speed_kmh', 0)
            throttle = data.get('gas', 0)
            brake = data.get('brake', 0)
            wheel_slip = data.get('wheel_slip', [0, 0, 0, 0])
            
            # Check if any wheel has high slip
            max_slip = max([abs(s) for s in wheel_slip]) if wheel_slip else 0
            has_slip = max_slip > 0.15 and brake > 0.05
            
            # Add to plot
            self.telemetry_plot.add_data_point(
                time.time(),
                speed,
                throttle,
                brake,
                has_slip
            )
        except Exception as e:
            logger.error(f"Error adding plot data: {e}")
    
    def _add_map_data(self, data: dict):
        """Add data point to track map"""
        try:
            x = data.get('position_x', 0)
            z = data.get('position_z', 0)  # Use Z as Y for top-down view
            brake = data.get('brake', 0)
            wheel_slip = data.get('wheel_slip', [0, 0, 0, 0])
            
            # Check for lockup
            max_slip = max([abs(s) for s in wheel_slip]) if wheel_slip else 0
            is_lockup = max_slip > 0.2
            is_braking = brake > 0.5
            
            # Add to map
            self.track_map.add_point(x, z, is_braking, is_lockup)
        except Exception as e:
            logger.error(f"Error adding map data: {e}")
    
    def _update_lockup_indicator(self, data: dict):
        """Update lockup LED indicators"""
        try:
            wheel_slip = data.get('wheel_slip', [0, 0, 0, 0])
            self.lockup_indicator.update_slip(wheel_slip)
        except Exception as e:
            logger.error(f"Error updating lockup indicator: {e}")
    
    def _format_lap_time(self, seconds: float) -> str:
        """Format lap time as MM:SS.mmm"""
        if seconds <= 0:
            return "0:00.000"
        
        minutes = int(seconds // 60)
        secs = seconds % 60
        return f"{minutes}:{secs:06.3f}"
    
    def run(self):
        """Start the application"""
        self.running = True
        logger.info("ACE Telemetry V3 started")
        self.mainloop()
    
    def on_closing(self):
        """Handle window close"""
        self.running = False
        self.ace_connector.disconnect()
        self.destroy()
