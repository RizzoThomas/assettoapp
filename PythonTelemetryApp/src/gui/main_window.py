"""
Main GUI Window
CustomTkinter-based dark theme interface for ACE telemetry
"""

import customtkinter as ctk
from telemetry.observer import TelemetryObserver
from loguru import logger
import threading
import time


class MainWindow(ctk.CTk, TelemetryObserver):
    """Main application window with dark theme"""
    
    def __init__(self, data_acquisition, telemetry_manager, lock_detector, 
                 lap_comparison, setup_manager):
        super().__init__()
        
        # Store references
        self.data_acquisition = data_acquisition
        self.telemetry_manager = telemetry_manager
        self.lock_detector = lock_detector
        self.lap_comparison = lap_comparison
        self.setup_manager = setup_manager
        
        # Subscribe to telemetry updates
        self.telemetry_manager.add_observer(self)
        
        # Window configuration
        self.title("ACE Telemetry - Assetto Corsa EVO")
        self.geometry("1200x800")
        
        # State
        self.connected = False
        self.running = False
        self.last_lap_number = 0
        
        # Create UI
        self._create_widgets()
        
        # Start update thread
        self.update_thread = None
        self.after(100, self._check_and_connect)
    
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
        
        # G-Force display
        self._create_gforce_panel()
        
        # Tire temperature display
        self._create_tire_panel()
        
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
            text="ACE Telemetry",
            font=ctk.CTkFont(size=20, weight="bold")
        )
        title.grid(row=0, column=0, padx=20, pady=20)
        
        # Connection status
        self.status_label = ctk.CTkLabel(
            self.sidebar,
            text="⚠️ Not Connected",
            font=ctk.CTkFont(size=14)
        )
        self.status_label.grid(row=1, column=0, padx=20, pady=10)
        
        # Connect button
        self.connect_btn = ctk.CTkButton(
            self.sidebar,
            text="Connect to ACE",
            command=self._toggle_connection
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
        
        # Safe Setup Button
        self.safe_btn = ctk.CTkButton(
            self.sidebar,
            text="🛡️ Safe Setup",
            command=lambda: self._apply_setup("Safe"),
            fg_color="green",
            hover_color="darkgreen"
        )
        self.safe_btn.grid(row=5, column=0, padx=20, pady=10)
        
        # Balanced Setup Button
        self.balanced_btn = ctk.CTkButton(
            self.sidebar,
            text="⚖️ Balanced Setup",
            command=lambda: self._apply_setup("Balanced"),
            fg_color="orange",
            hover_color="darkorange"
        )
        self.balanced_btn.grid(row=6, column=0, padx=20, pady=10)
        
        # Aggressive Setup Button
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
            text="Clear Data",
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
        
        # Speed
        self.speed_label = ctk.CTkLabel(panel, text="Speed: 0 km/h",
                                       font=ctk.CTkFont(size=24))
        self.speed_label.grid(row=1, column=0, padx=10, pady=10)
        
        # RPM
        self.rpm_label = ctk.CTkLabel(panel, text="RPM: 0",
                                     font=ctk.CTkFont(size=24))
        self.rpm_label.grid(row=1, column=1, padx=10, pady=10)
        
        # Gear
        self.gear_label = ctk.CTkLabel(panel, text="Gear: N",
                                      font=ctk.CTkFont(size=24))
        self.gear_label.grid(row=1, column=2, padx=10, pady=10)
        
        # Throttle/Brake
        self.input_label = ctk.CTkLabel(panel, text="T: 0% B: 0%",
                                       font=ctk.CTkFont(size=20))
        self.input_label.grid(row=1, column=3, padx=10, pady=10)
        
        # Lap info
        self.lap_label = ctk.CTkLabel(panel, text="Lap: 0 | Time: --:---.---",
                                     font=ctk.CTkFont(size=16))
        self.lap_label.grid(row=2, column=0, columnspan=2, pady=10)
        
        # Best lap
        self.best_label = ctk.CTkLabel(panel, text="Best: --:---.---",
                                      font=ctk.CTkFont(size=16))
        self.best_label.grid(row=2, column=2, columnspan=2, pady=10)
    
    def _create_gforce_panel(self):
        """Create G-Force display panel"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=1, column=0, sticky="nsew", padx=10, pady=10)
        
        title = ctk.CTkLabel(panel, text="G-Forces", 
                           font=ctk.CTkFont(size=18, weight="bold"))
        title.grid(row=0, column=0, columnspan=3, pady=10)
        
        # Lateral G
        ctk.CTkLabel(panel, text="Lateral:").grid(row=1, column=0, padx=10, pady=5)
        self.g_lateral_label = ctk.CTkLabel(panel, text="0.00 G",
                                           font=ctk.CTkFont(size=16))
        self.g_lateral_label.grid(row=1, column=1, padx=10, pady=5)
        
        # Longitudinal G
        ctk.CTkLabel(panel, text="Longitudinal:").grid(row=2, column=0, padx=10, pady=5)
        self.g_long_label = ctk.CTkLabel(panel, text="0.00 G",
                                        font=ctk.CTkFont(size=16))
        self.g_long_label.grid(row=2, column=1, padx=10, pady=5)
        
        # Vertical G
        ctk.CTkLabel(panel, text="Vertical:").grid(row=3, column=0, padx=10, pady=5)
        self.g_vert_label = ctk.CTkLabel(panel, text="0.00 G",
                                        font=ctk.CTkFont(size=16))
        self.g_vert_label.grid(row=3, column=1, padx=10, pady=5)
    
    def _create_tire_panel(self):
        """Create tire temperature display panel"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=2, column=0, sticky="nsew", padx=10, pady=10)
        
        title = ctk.CTkLabel(panel, text="Tire Temperatures", 
                           font=ctk.CTkFont(size=18, weight="bold"))
        title.grid(row=0, column=0, columnspan=4, pady=10)
        
        # Headers
        for i, name in enumerate(["Front Left", "Front Right", "Rear Left", "Rear Right"]):
            ctk.CTkLabel(panel, text=name).grid(row=1, column=i, padx=5)
        
        # Temperature labels
        self.tire_labels = []
        for i in range(4):
            label = ctk.CTkLabel(panel, text="0°C", font=ctk.CTkFont(size=16))
            label.grid(row=2, column=i, padx=5, pady=10)
            self.tire_labels.append(label)
    
    def _create_lockup_panel(self):
        """Create lock-up detection display panel"""
        panel = ctk.CTkFrame(self.main_frame)
        panel.grid(row=3, column=0, sticky="nsew", padx=10, pady=10)
        
        title = ctk.CTkLabel(panel, text="Lock-Up Detection", 
                           font=ctk.CTkFont(size=18, weight="bold"))
        title.grid(row=0, column=0, columnspan=4, pady=10)
        
        # Lock-up counters
        self.lockup_labels = []
        for i, name in enumerate(["FL", "FR", "RL", "RR"]):
            frame = ctk.CTkFrame(panel)
            frame.grid(row=1, column=i, padx=10, pady=10)
            
            ctk.CTkLabel(frame, text=name).pack(pady=5)
            label = ctk.CTkLabel(frame, text="0", font=ctk.CTkFont(size=20))
            label.pack(pady=5)
            self.lockup_labels.append(label)
    
    def _check_and_connect(self):
        """Try to connect to ACE"""
        if not self.connected:
            if self.data_acquisition.connect():
                self.connected = True
                self.status_label.configure(text="✅ Connected")
                self.connect_btn.configure(text="Disconnect")
                logger.info("Connected to ACE")
                
                # Start telemetry updates
                self._start_telemetry()
            else:
                # Retry in 2 seconds
                self.after(2000, self._check_and_connect)
    
    def _toggle_connection(self):
        """Toggle connection to ACE"""
        if self.connected:
            self._stop_telemetry()
            self.data_acquisition.disconnect()
            self.connected = False
            self.status_label.configure(text="⚠️ Not Connected")
            self.connect_btn.configure(text="Connect to ACE")
            logger.info("Disconnected from ACE")
        else:
            self._check_and_connect()
    
    def _start_telemetry(self):
        """Start telemetry reading thread"""
        if self.running:
            return
        
        self.running = True
        self.telemetry_manager.start()
        
        # Start update thread
        self.update_thread = threading.Thread(target=self._telemetry_loop, daemon=True)
        self.update_thread.start()
        logger.info("Telemetry thread started")
    
    def _stop_telemetry(self):
        """Stop telemetry reading"""
        self.running = False
        self.telemetry_manager.stop()
        logger.info("Telemetry stopped")
    
    def _telemetry_loop(self):
        """Background telemetry reading loop"""
        while self.running:
            try:
                self.telemetry_manager.update()
                time.sleep(0.01)  # 100Hz
            except Exception as e:
                logger.error(f"Error in telemetry loop: {e}")
                time.sleep(0.1)
    
    def update(self, data):
        """Observer update method - called when telemetry data changes"""
        try:
            car_physics = data['car_physics']
            wheels = data['wheels']
            session = data['session']
            
            # Update telemetry display
            self.after(0, lambda: self._update_telemetry_display(car_physics, wheels, session))
            
            # Check for lock-ups
            lock_events = self.lock_detector.check_lock_ups(car_physics, wheels)
            if lock_events:
                self.after(0, self._update_lockup_display)
            
            # Update lap comparison
            if session.current_lap != self.last_lap_number:
                if self.last_lap_number > 0:
                    self.lap_comparison.finish_lap(
                        session.last_lap_time_ms,
                        session.is_valid_lap
                    )
                self.lap_comparison.start_new_lap(session.current_lap)
                self.last_lap_number = session.current_lap
            
            self.lap_comparison.add_data_point(car_physics)
            
        except Exception as e:
            logger.error(f"Error in update: {e}")
    
    def _update_telemetry_display(self, car_physics, wheels, session):
        """Update telemetry display widgets"""
        # Speed, RPM, Gear
        self.speed_label.configure(text=f"Speed: {car_physics.speed_kmh:.0f} km/h")
        self.rpm_label.configure(text=f"RPM: {car_physics.rpm:.0f}")
        gear_text = "R" if car_physics.gear < 0 else ("N" if car_physics.gear == 0 else str(car_physics.gear))
        self.gear_label.configure(text=f"Gear: {gear_text}")
        
        # Inputs
        self.input_label.configure(
            text=f"T: {car_physics.throttle*100:.0f}% B: {car_physics.brake*100:.0f}%"
        )
        
        # Lap info
        current_time = session.current_lap_time_ms / 1000
        minutes = int(current_time // 60)
        seconds = current_time % 60
        self.lap_label.configure(
            text=f"Lap: {session.current_lap} | Time: {minutes:02d}:{seconds:06.3f}"
        )
        
        # Best lap
        if session.best_lap_time_ms > 0:
            best_time = session.best_lap_time_ms / 1000
            minutes = int(best_time // 60)
            seconds = best_time % 60
            self.best_label.configure(text=f"Best: {minutes:02d}:{seconds:06.3f}")
        
        # G-Forces
        self.g_lateral_label.configure(text=f"{car_physics.g_force_lateral:.2f} G")
        self.g_long_label.configure(text=f"{car_physics.g_force_longitudinal:.2f} G")
        self.g_vert_label.configure(text=f"{car_physics.g_force_vertical:.2f} G")
        
        # Tire temperatures
        for i, wheel in enumerate(wheels):
            temp = wheel.temperature_core
            self.tire_labels[i].configure(text=f"{temp:.0f}°C")
            
            # Color code based on temperature (example ranges)
            if temp < 60:
                color = "blue"
            elif temp < 80:
                color = "green"
            elif temp < 100:
                color = "orange"
            else:
                color = "red"
            self.tire_labels[i].configure(text_color=color)
    
    def _update_lockup_display(self):
        """Update lock-up counters"""
        counts = self.lock_detector.get_lock_up_count_by_wheel()
        for i, count in enumerate(counts):
            self.lockup_labels[i].configure(text=str(count))
    
    def _apply_setup(self, setup_type: str):
        """Apply a setup strategy"""
        try:
            # Get current track/car from session
            session = self.data_acquisition.get_session_data()
            if session.track_name and session.car_name:
                self.setup_manager.set_track_and_car(session.track_name, session.car_name)
                success = self.setup_manager.apply_setup(setup_type)
                
                if success:
                    self._show_message(f"✅ {setup_type} setup applied successfully!")
                else:
                    self._show_message(f"❌ Failed to apply {setup_type} setup")
            else:
                self._show_message("⚠️ Track/Car info not available. Start a session first.")
        except Exception as e:
            logger.error(f"Error applying setup: {e}")
            self._show_message(f"❌ Error: {str(e)}")
    
    def _clear_data(self):
        """Clear all collected data"""
        self.lock_detector.clear_events()
        self.lap_comparison.clear_data()
        self._update_lockup_display()
        self._show_message("✅ Data cleared")
        logger.info("All data cleared")
    
    def _show_message(self, message: str):
        """Show a temporary message"""
        # Create a temporary label
        msg_label = ctk.CTkLabel(self.sidebar, text=message, 
                                font=ctk.CTkFont(size=12))
        msg_label.grid(row=9, column=0, padx=20, pady=10)
        
        # Remove after 3 seconds
        self.after(3000, msg_label.destroy)
    
    def destroy(self):
        """Clean up before closing"""
        self.running = False
        if self.update_thread and self.update_thread.is_alive():
            self.update_thread.join(timeout=1.0)
        if self.connected:
            self.data_acquisition.disconnect()
        super().destroy()
