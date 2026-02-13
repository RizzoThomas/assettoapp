"""
Main GUI Window V2 - With ACE Connection, Live Plot, and Debug Console
CustomTkinter-based dark theme interface for ACE telemetry
"""

import customtkinter as ctk
from telemetry.observer import TelemetryObserver
from telemetry.ace_connector import ACEConnector
from gui.telemetry_plot import TelemetryPlot
from loguru import logger
import threading
import time


class MainWindow(ctk.CTk, TelemetryObserver):
    """Main application window with dark theme"""
    
    def __init__(self, telemetry_manager, lock_detector, lap_comparison, setup_manager):
        super().__init__()
        
        # Create ACE connector
        self.ace_connector = ACEConnector()
        
        # Store references
        self.telemetry_manager = telemetry_manager
        self.lock_detector = lock_detector
        self.lap_comparison = lap_comparison
        self.setup_manager = setup_manager
        
        # Subscribe to telemetry updates
        self.telemetry_manager.add_observer(self)
        
        # Window configuration
        self.title("ACE Telemetry - Assetto Corsa EVO (Fixed)")
        self.geometry("1400x900")
        
        # State
        self.connected = False
        self.running = False
        self.last_lap_number = 0
        self.reconnect_thread = None
        
        # Create UI
        self._create_widgets()
        
        # Start auto-reconnect thread
        self.after(1000, self._start_reconnect_thread)
    
    def _create_widgets(self):
        """Create all GUI widgets"""
        
        # Configure grid
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(0, weight=1)
        
        # Sidebar
        self._create_sidebar()
        
        # Main content area
        self.main_frame = ctk.CTkFrame(self, corner_radius=0)
        self.main_frame.grid(row=0, column=1, sticky="nsew", padx=10, pady=10)
        self.main_frame.grid_columnconfigure(0, weight=1)
        self.main_frame.grid_rowconfigure((0, 1, 2, 3), weight=1)
        
        # Telemetry display
        self._create_telemetry_panel()
        
        # Live plot
        self._create_plot_panel()
        
        # Debug console
        self._create_debug_console()
        
        # Lock-up display
        self._create_lockup_panel()
    
    def _create_sidebar(self):
        """Create sidebar with controls"""
        self.sidebar = ctk.CTkFrame(self, width=250, corner_radius=0)
        self.sidebar.grid(row=0, column=0, sticky="nsew")
        self.sidebar.grid_rowconfigure(10, weight=1)
        
        # Title
        title = ctk.CTkLabel(
            self.sidebar,
            text="ACE Telemetry V2",
            font=ctk.CTkFont(size=20, weight="bold")
        )
        title.grid(row=0, column=0, padx=20, pady=20)
        
        # Connection status
        self.status_label = ctk.CTkLabel(
            self.sidebar,
            text="⚠️ Connecting...",
            font=ctk.CTkFont(size=12),
            wraplength=200
        )
        self.status_label.grid(row=1, column=0, padx=20, pady=10)
        
        # Manual connect button
        self.connect_btn = ctk.CTkButton(
            self.sidebar,
            text="Force Reconnect",
            command=self._force_reconnect
        )
        self.connect_btn.grid(row=2, column=0, padx=20, pady=10)
        
        # Separator
        ctk.CTkLabel(self.sidebar, text="").grid(row=3, column=0, pady=10)
        
        # Setup Strategy Section
        strategy_label = ctk.CTkLabel(
            self.sidebar,
            text="Setup Strategy",
            font=ctk.CTkFont(size=16, weight="bold")
        )
        strategy_label.grid(row=4, column=0, padx=20, pady=(20, 10))
        
        # Setup buttons
        self.safe_btn = ctk.CTkButton(
            self.sidebar,
            text="🛡️ Safe Setup",
            command=lambda: self._apply_setup("Safe"),
            fg_color="green",
            hover_color="darkgreen"
        )
        self.safe_btn.grid(row=5, column=0, padx=20, pady=10)
        
        self.balanced_btn = ctk.CTkButton(
            self.sidebar,
            text="⚖️ Balanced Setup",
            command=lambda: self._apply_setup("Balanced"),
            fg_color="orange",
            hover_color="darkorange"
        )
        self.balanced_btn.grid(row=6, column=0, padx=20, pady=10)
        
        self.aggressive_btn = ctk.CTkButton(
            self.sidebar,
            text="🔥 Aggressive Setup",
            command=lambda: self._apply_setup("Aggressive"),
            fg_color="red",
            hover_color="darkred"
        )
        self.aggressive_btn.grid(row=7, column=0, padx=20, pady=10)
        
        # Clear Data Button
        self.clear_btn = ctk.CTkButton(
            self.sidebar,
            text="Clear Data & Plot",
            command=self._clear_data
        )
        self.clear_btn.grid(row=8, column=0, padx=20, pady=(30, 10))
    
    def _create_telemetry_panel(self):
        """Create main telemetry display panel"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=0, column=0, sticky="nsew", padx=10, pady=10)
        panel.grid_columnconfigure((0, 1, 2, 3), weight=1)
        
        # Title
        title = ctk.CTkLabel(panel, text="Real-Time Telemetry", 
                           font=ctk.CTkFont(size=18, weight="bold"))
        title.grid(row=0, column=0, columnspan=4, pady=10)
        
        # Speed, RPM, Gear, Inputs
        self.speed_label = ctk.CTkLabel(panel, text="Speed: 0 km/h",
                                       font=ctk.CTkFont(size=20))
        self.speed_label.grid(row=1, column=0, padx=10, pady=10)
        
        self.rpm_label = ctk.CTkLabel(panel, text="RPM: 0",
                                     font=ctk.CTkFont(size=20))
        self.rpm_label.grid(row=1, column=1, padx=10, pady=10)
        
        self.gear_label = ctk.CTkLabel(panel, text="Gear: N",
                                      font=ctk.CTkFont(size=20))
        self.gear_label.grid(row=1, column=2, padx=10, pady=10)
        
        self.input_label = ctk.CTkLabel(panel, text="T: 0% B: 0%",
                                       font=ctk.CTkFont(size=18))
        self.input_label.grid(row=1, column=3, padx=10, pady=10)
    
    def _create_plot_panel(self):
        """Create live telemetry plot panel"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=1, column=0, sticky="nsew", padx=10, pady=10)
        
        # Create telemetry plot
        self.telemetry_plot = TelemetryPlot(panel, max_points=500)
        plot_widget = self.telemetry_plot.get_widget()
        plot_widget.pack(fill="both", expand=True, padx=5, pady=5)
    
    def _create_debug_console(self):
        """Create debug console for connection errors"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=2, column=0, sticky="nsew", padx=10, pady=10)
        
        title = ctk.CTkLabel(panel, text="Debug Console", 
                           font=ctk.CTkFont(size=16, weight="bold"))
        title.pack(pady=5)
        
        # Debug text area
        self.debug_text = ctk.CTkTextbox(panel, height=120, font=ctk.CTkFont(size=10))
        self.debug_text.pack(fill="both", expand=True, padx=10, pady=10)
        
        # Add initial message
        self._log_debug("🔍 Waiting for connection to ACE...")
        self._log_debug(f"Looking for: {ACEConnector.SHARED_MEMORY_NAME}")
        self._log_debug(f"Or UDP on port: {ACEConnector.UDP_PORT}")
    
    def _create_lockup_panel(self):
        """Create lock-up detection display panel"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=3, column=0, sticky="nsew", padx=10, pady=10)
        
        title = ctk.CTkLabel(panel, text="Lock-Up Detection", 
                           font=ctk.CTkFont(size=16, weight="bold"))
        title.grid(row=0, column=0, columnspan=4, pady=5)
        
        # Lock-up counters
        self.lockup_labels = []
        for i, name in enumerate(["FL", "FR", "RL", "RR"]):
            frame = ctk.CTkFrame(panel)
            frame.grid(row=1, column=i, padx=10, pady=10)
            
            ctk.CTkLabel(frame, text=name, font=ctk.CTkFont(size=12)).pack(pady=2)
            label = ctk.CTkLabel(frame, text="0", font=ctk.CTkFont(size=18, weight="bold"))
            label.pack(pady=2)
            self.lockup_labels.append(label)
    
    def _start_reconnect_thread(self):
        """Start auto-reconnect background thread"""
        if self.reconnect_thread is None or not self.reconnect_thread.is_alive():
            self.reconnect_thread = threading.Thread(
                target=self._reconnect_loop, 
                daemon=True
            )
            self.reconnect_thread.start()
            logger.info("Auto-reconnect thread started")
    
    def _reconnect_loop(self):
        """Background loop that tries to reconnect every 5 seconds"""
        while True:
            try:
                if not self.connected:
                    success = self.ace_connector.try_connect()
                    
                    if success and not self.connected:
                        # Just connected
                        self.connected = True
                        self._log_debug(f"✅ {self.ace_connector.get_connection_status()}")
                        self.after(0, self._on_connected)
                    elif not success:
                        # Still not connected
                        error = self.ace_connector.last_error
                        if error:
                            self.after(0, lambda: self._log_debug(f"❌ {error}"))
                
                time.sleep(ACEConnector.RETRY_INTERVAL)
                
            except Exception as e:
                logger.error(f"Reconnect loop error: {e}")
                time.sleep(5.0)
    
    def _on_connected(self):
        """Called when connection is established"""
        self.status_label.configure(text=self.ace_connector.get_connection_status())
        self.connect_btn.configure(text="Disconnect", fg_color="red")
        self._start_telemetry()
    
    def _force_reconnect(self):
        """Force reconnection attempt"""
        if self.connected:
            # Disconnect
            self._stop_telemetry()
            self.ace_connector.disconnect()
            self.connected = False
            self.status_label.configure(text="⚠️ Disconnected")
            self.connect_btn.configure(text="Force Reconnect", fg_color=None)
            self._log_debug("🔌 Manually disconnected")
        else:
            # Try to connect immediately
            self.ace_connector.last_connection_attempt = 0.0
            self._log_debug("🔄 Forcing reconnection attempt...")
    
    def _start_telemetry(self):
        """Start telemetry reading"""
        if self.running:
            return
        
        self.running = True
        self.telemetry_manager.start()
        
        # Start update loop
        self.after(10, self._telemetry_update_loop)
        logger.info("Telemetry started")
    
    def _stop_telemetry(self):
        """Stop telemetry reading"""
        self.running = False
        self.telemetry_manager.stop()
        logger.info("Telemetry stopped")
    
    def _telemetry_update_loop(self):
        """Main telemetry update loop (runs in UI thread)"""
        if not self.running:
            return
        
        try:
            # Read telemetry from ACE
            if self.ace_connector.read_telemetry():
                # Update display
                self._update_telemetry_display()
                
                # Check for lock-ups
                self._check_lockups()
                
                # Update plot every few frames (to avoid UI lag)
                if int(time.time() * 10) % 2 == 0:  # Every 0.2 seconds
                    self.telemetry_plot.update_plot()
            else:
                # Lost connection
                if self.connected:
                    self.connected = False
                    self.status_label.configure(text="❌ Connection Lost")
                    self._log_debug("⚠️ Connection lost - will auto-reconnect")
                    self._stop_telemetry()
        
        except Exception as e:
            logger.error(f"Telemetry update error: {e}")
        
        # Schedule next update (100Hz)
        if self.running:
            self.after(10, self._telemetry_update_loop)
    
    def _update_telemetry_display(self):
        """Update telemetry display with current data"""
        try:
            # Get data from connector
            speed = self.ace_connector.get_value('speed_kmh', 0.0)
            rpm = self.ace_connector.get_value('rpm', 0.0)
            gear = self.ace_connector.get_value('gear', 0)
            throttle = self.ace_connector.get_value('gas', 0.0)
            brake = self.ace_connector.get_value('brake', 0.0)
            
            # Update labels
            self.speed_label.configure(text=f"Speed: {speed:.0f} km/h")
            self.rpm_label.configure(text=f"RPM: {rpm:.0f}")
            
            gear_text = "R" if gear < 0 else ("N" if gear == 0 else str(gear))
            self.gear_label.configure(text=f"Gear: {gear_text}")
            
            self.input_label.configure(
                text=f"T: {throttle*100:.0f}% B: {brake*100:.0f}%"
            )
            
            # Add data to plot
            current_time = time.time()
            wheel_slip = self.ace_connector.get_wheel_value('wheel_slip', 0, 0.0)
            has_slip = abs(wheel_slip) > 0.15 and brake > 0.05
            
            self.telemetry_plot.add_data_point(
                current_time,
                throttle,
                brake,
                has_slip
            )
            
        except Exception as e:
            logger.error(f"Display update error: {e}")
    
    def _check_lockups(self):
        """Check for wheel lock-ups"""
        try:
            # Create temporary physics object for compatibility
            class TempPhysics:
                def __init__(self, connector):
                    self.brake = connector.get_value('brake', 0.0)
                    self.position_x = connector.get_value('velocity_x', 0.0)
                    self.position_y = connector.get_value('velocity_y', 0.0)
                    self.position_z = connector.get_value('velocity_z', 0.0)
                    self.speed_kmh = connector.get_value('speed_kmh', 0.0)
            
            class TempWheel:
                def __init__(self, slip_ratio):
                    self.slip_ratio = slip_ratio
            
            physics = TempPhysics(self.ace_connector)
            wheels = [
                TempWheel(self.ace_connector.get_wheel_value('wheel_slip', i, 0.0))
                for i in range(4)
            ]
            
            # Check lock-ups
            events = self.lock_detector.check_lock_ups(physics, wheels)
            
            if events:
                # Update counters
                counts = self.lock_detector.get_lock_up_count_by_wheel()
                for i, count in enumerate(counts):
                    self.lockup_labels[i].configure(text=str(count))
                    
        except Exception as e:
            logger.error(f"Lock-up check error: {e}")
    
    def update(self, data):
        """Observer update method (compatibility)"""
        pass
    
    def _apply_setup(self, setup_type: str):
        """Apply a setup strategy"""
        try:
            # Get current track/car info (if available)
            # For now, use placeholder
            track = "generic"
            car = "generic"
            
            self.setup_manager.set_track_and_car(track, car)
            success = self.setup_manager.apply_setup(setup_type)
            
            if success:
                self._log_debug(f"✅ {setup_type} setup applied!")
            else:
                self._log_debug(f"❌ Failed to apply {setup_type} setup")
        except Exception as e:
            logger.error(f"Setup apply error: {e}")
            self._log_debug(f"❌ Error: {str(e)}")
    
    def _clear_data(self):
        """Clear all collected data"""
        self.lock_detector.clear_events()
        self.lap_comparison.clear_data()
        self.telemetry_plot.clear()
        
        # Reset lock-up counters
        for label in self.lockup_labels:
            label.configure(text="0")
        
        self._log_debug("✅ Data and plot cleared")
        logger.info("All data cleared")
    
    def _log_debug(self, message: str):
        """Add message to debug console"""
        try:
            self.debug_text.insert("end", f"{message}\n")
            self.debug_text.see("end")  # Scroll to bottom
        except:
            pass
    
    def destroy(self):
        """Clean up before closing"""
        self.running = False
        if self.reconnect_thread and self.reconnect_thread.is_alive():
            # Give thread time to stop
            time.sleep(0.5)
        if self.connected:
            self.ace_connector.disconnect()
        super().destroy()
