"""
Lockup Indicator - Flashing red LED when wheel slip > 0.2
"""

import customtkinter as ctk
from typing import List


class LockupIndicator(ctk.CTkFrame):
    """
    Visual indicator for wheel lockup events
    Shows 4 LEDs (one per wheel) that flash red when slip > 0.2
    """
    
    SLIP_THRESHOLD = 0.2  # Wheel slip threshold
    FLASH_INTERVAL = 200  # ms between flashes
    
    def __init__(self, master, **kwargs):
        super().__init__(master, **kwargs)
        
        self.wheel_states = [False, False, False, False]  # FL, FR, RL, RR
        self.flash_state = False
        self.flash_job = None
        
        self._create_widgets()
    
    def _create_widgets(self):
        """Create LED indicators"""
        
        # Title
        title = ctk.CTkLabel(
            self,
            text="🚨 Wheel Lock-Up Indicators",
            font=ctk.CTkFont(size=14, weight="bold")
        )
        title.grid(row=0, column=0, columnspan=4, pady=5)
        
        # Wheel labels
        labels = ["FL", "FR", "RL", "RR"]
        self.leds = []
        
        for i, label in enumerate(labels):
            # Label
            lbl = ctk.CTkLabel(
                self,
                text=label,
                font=ctk.CTkFont(size=10)
            )
            lbl.grid(row=1, column=i, padx=5)
            
            # LED indicator
            led = ctk.CTkLabel(
                self,
                text="●",
                font=ctk.CTkFont(size=40),
                text_color="gray"
            )
            led.grid(row=2, column=i, padx=10, pady=5)
            self.leds.append(led)
        
        # Start flash animation
        self._flash_animation()
    
    def update_slip(self, wheel_slip: List[float]):
        """
        Update wheel slip values and trigger LED state
        wheel_slip: [FL, FR, RL, RR]
        """
        for i, slip in enumerate(wheel_slip):
            if i < len(self.wheel_states):
                self.wheel_states[i] = abs(slip) > self.SLIP_THRESHOLD
    
    def _flash_animation(self):
        """Animate flashing LEDs"""
        self.flash_state = not self.flash_state
        
        for i, led in enumerate(self.leds):
            if self.wheel_states[i]:
                # Locked - flash red
                color = "#FF0000" if self.flash_state else "#8B0000"
                led.configure(text_color=color)
            else:
                # Normal - green
                led.configure(text_color="#00FF00")
        
        # Schedule next flash
        self.flash_job = self.after(self.FLASH_INTERVAL, self._flash_animation)
    
    def destroy(self):
        """Clean up"""
        if self.flash_job:
            self.after_cancel(self.flash_job)
        super().destroy()
