# Firmware Development

## Overview

The firmware is developed within the Arduino Environment targeting the ESP8266 microcontroller platform.
ESP8266 support is provided to the *Arduino IDE* via the community-maintained
Board Manager package ([ESP8266 Arduino Core](https://github.com/esp8266/Arduino)).

The system architecture is designed for modularity and maintainability, separating universal logic from hardware-specific implementations.
The firmware for each station model is split into two primary components:

* **Feature Libraries:** Self-contained, hardware-agnostic modules that handle server-driven logic and reusable features across different station types.
* **Station-specific File:** The model-specific entry point that acts as the central orchestrator, responsible for mapping physical hardware, binding hardware-agnostic libraries to physical components, managing local peripherals, and defining the operational workflow.

## Feature Libraries

Each type of feature that a station can have and that is handled by the central server must be developed as a dedicated,
universal library according to the following guidelines:

* Each library consists of a dedicated pair of `.h` and `.cpp` files.
* Libraries must not be specific to any hardware. They are designed to be fully abstract and shareable across multiple station types.
* To bridge the gap with physical components without sacrificing abstraction, libraries provide mechanisms (such as callback methods) for the user or programmer to configure hardware-specific behavior.

## Station-Specific File

Specific hardware is modeled and handled exclusively within the `.ino` file for a particular station model.
The `.ino` file should be developed according to the following guidelines:

* All universal feature libraries are configured, initialized, and used directly within this file.
* Pin definitions, low-level wiring, and communication with specific sensors are handled exclusively here either manually or by configuration of shared libraries.
* Minor peripherals (such as status LEDs) that are not controlled by the central server are managed here, along with the operation flow,
feature registration flow, and general execution loops.
* The hardware and firmware setup defined here is designed to be stable and set up once, requiring no further modification after initial deployment.
Any configurable parameters should be handled through the dedicated configuration files described in the [Configuration Management](#configuration-management) section below.

## Configuration Management

Configurable parameters of the firmware such as central server communication parameters, used protocols,
and logging are maintained in a dedicated `config.h` and `config.cpp` file pair. 
Preferably, the structure of these configuration files should remain uniform across all station types.

## Secrets Management

To ensure project security and prevent accidental credential leaks:

* Sensitive data such as Wi-Fi passwords, API keys, and access tokens must be stored in dedicated `secrets.h` and `secrets.cpp` files.
Preferably, the structure of these files should remain uniform across all station types.
* These files must be explicitly excluded from versioning via `.gitignore`.
* Firmware execution must never log, print, or expose sensitive secrets under any circumstances.
