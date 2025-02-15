# PCRemote - Android-Based Computer Remote Control

## Overview

PCRemote is an Android-based application that enables users to remotely control their Windows computer. The system consists of:

- **Android Client:** A mobile app that acts as a remote control.
- **Windows Server:** A desktop application that receives and executes commands from the mobile app.

This project was originally developed as my **Bachelor's thesis** at **Eszterházy Károly University**.

⚠️ **This project is no longer maintained, and no further updates or features will be added.**

## Features

- **Mouse & Keyboard Control:** Use your phone as a wireless mouse and keyboard.
- **System Monitoring:** View real-time CPU and RAM usage.
- **Power Management:** Shut down, restart, or put the PC to sleep remotely.
- **Auto-Discovery:** Automatically detect and connect to the PC.
- **Volume Control:** Adjust or mute the computer’s volume.
- **Secure Communication:** TCP socket-based communication between client and server.

## Installation

### Server (Windows)
1. Download and install the **PCRemote Server** from the [GitHub repository](https://github.com/vakitm/PCRemote).
2. Run the server application on your PC.
3. Configure the server settings, such as TCP/UDP ports and auto-start preferences.

### Client (Android)
1. Install the **PCRemote** app on your Android device.
2. Ensure that both the phone and PC are connected to the same WiFi network.
3. Manually enter the server’s IP address and port or use the auto-discovery feature.

## Usage

1. Start the **PCRemote Server** on the computer.
2. Open the **PCRemote** app on the phone and connect to the server.
3. Navigate through the menu to access different control features:
    - **Home Screen:** Displays system status (CPU, RAM, etc.).
    - **Mouse & Keyboard:** Provides remote input control.
    - **Volume Control:** Adjust the computer’s sound settings.
    - **Power Management:** Shut down, restart, or sleep the PC.
    - **Settings:** Configure the server address and connection preferences.

## System Requirements

- **Server:** Windows 7/10/11 with .NET Framework installed.
- **Client:** Android 4.0.1+
- **Network:** Both devices must be on the same local network for auto-discovery.

## Future Enhancements

🚫 **No further updates or features will be added.**

## License

This project is **open-source** and licensed under the **MIT License**.

---
📌 *Originally developed as a Bachelor's thesis at Eszterházy Károly University.*  
⚠️ *This project is no longer maintained.*
