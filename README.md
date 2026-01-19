# 🧠 NeuroGeometry: BCI Visualizer & RPG

**NeuroGeometry** is a C# WPF application that transforms raw brainwave data into a real-time "Neuro-RPG" experience. 

Using a NeuroSky MindWave headset, the application gamifies your mental states—rewarding Focus (Attention) with combos and Relaxation (Meditation) with special abilities—while visualizing your neural activity through dynamic geometry.

---

## 🎮 Features

### The Neuro-RPG System
*   **XP & Leveling:** Gain experience points based on the intensity of your mental focus. Level up to increase difficulty.
*   **Combo System:** Maintain your Attention levels above 60 to build a `x2`, `x3`, `x4` score multiplier. Distraction breaks the combo!
*   **Overdrive Mechanic:** Meditate to fill your "Overdrive" meter. When full, the interface turns Red and all point gains are doubled.
*   **Critical Hits:** Sudden spikes in Gamma waves (moments of insight) trigger massive bonus points.

### Visualization & Analysis
*   **Dynamic Geometry:** A polygon that morphs in real-time based on 8 EEG frequency bands (Delta, Theta, Alpha, Beta, Gamma).
*   **Raw Waveform Plot:** Visualizes the raw 512Hz electrical signal from the biosensor.
*   **Impedance Map:** Visual feedback on signal quality and headset connectivity.
*   **Arcade HUD:** A sci-fi inspired interface with neon aesthetics and data gauges.

### Utilities
*   **CSV Data Logger:** Record your brainwave sessions to a local file for analysis.
*   **Session Timer:** Track your training duration.

---

## 🛠 Prerequisites

To run this project, you need the following hardware and software:

### Hardware
*   **NeuroSky MindWave Mobile** (or original MindWave) headset.
*   A Bluetooth-capable PC.

### Software
*   **Windows OS** (Required for WPF).
*   **Visual Studio 2019/2022** (with .NET Desktop Development workload).
*   **[ThinkGear Connector (TGC)](http://developer.neurosky.com/):** The official driver that translates Bluetooth signals into a local TCP socket.
    *   *> **Note:** This app does not connect directly to Bluetooth. It connects to the TGC driver via TCP/IP.*

---

## 🚀 Getting Started

### 1. Installation
1.  Clone this repository.
2.  Open the solution (`.sln`) in Visual Studio.
3.  Right-click the Solution in Solution Explorer and select **Restore NuGet Packages**.
    *   *Required Package:* `Newtonsoft.Json` (Used for parsing the ThinkGear Socket Protocol).

### 2. Hardware Setup
1.  Turn on your MindWave headset (ensure it is in pairing mode).
2.  Pair it with Windows via your system Bluetooth settings.
3.  **Run the ThinkGear Connector (TGC.exe)**. You should see the NeuroSky "Brain" icon in your System Tray.

### 3. Run the App
1.  Press **F5** in Visual Studio to build and run.
2.  Put the headset on your head.
3.  Click the **INITIALIZE LINK** button in the center of the app.

---

## 🏗 Architecture

This application follows the **MVVM (Model-View-ViewModel)** design pattern.

*   **Abstractions (`IBiosensor`):** Defines the contract for data reception, decoupling the UI from the specific hardware driver.
*   **Services (`ThinkGearService`):** Implements the **ThinkGear Socket Protocol**. It connects to `127.0.0.1:13854`, authorizes via JSON configuration, and parses the incoming stream.
*   **ViewModels (`MainViewModel`):** The core "Game Loop." It processes 1Hz data packets to calculate XP, combos, and geometry math.
*   **Views:** Pure XAML user controls. The `AnalysisView` uses a `WrapPanel` to dynamically display electrode status.

---

## 🐛 Troubleshooting

| Error Message | Cause | Solution |
| :--- | :--- | :--- |
| **"Connection Failed: Is TGC Running?"** | The app cannot find the local server. | Ensure **ThinkGear Connector** is running in the system tray. Ensure no other app is currently holding the connection. |
| **"SEARCHING FOR SIGNAL..."** | App is connected to driver, but driver cannot find headset. | Check Bluetooth connection. Replace headset battery. Ensure the sensor is touching your forehead. |
| **Flat Charts / No Response** | Signal quality is too poor. | Check the **Signal Integrity** map on the left. If it is not Green, adjust the headset fit. |

---

## 📄 License

This project is open source. Feel free to fork, modify, and use it to learn about Brain-Computer Interfaces.

*NeuroSky, MindWave, and ThinkGear are trademarks of NeuroSky, Inc.*
