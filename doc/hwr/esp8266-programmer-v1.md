# ESP8266 Programmer

## Overview

A dedicated programming board based on the NodeMCU 1.0 layout,
engineered to simplify firmware flashing and debugging for standalone ESP8266-based hardware modules and custom boards.

## Schematic

TODO: Add when it will be ready.

## How to connect

TODO: Add wiring diagram when it will be ready.

## Operation Principle

By pulling the ENABLE pin of the on-board NodeMCU into a low state, the integrated ESP8266 microcontroller is disabled,
while its onboard USB-to-UART bridge remains fully functional for data transmission.

By connecting the UART, FLASH, and RESET lines from the programmer board to a target, standalone ESP8266 circuit,
firmware can be flashed seamlessly without manual intervention.
The automatic programming circuit inherited from the NodeMCU layout handles the handshake sequence
automatically pulling the GPIO0/FLASH pin low and pulsing the RESET pin to enter flashing mode,
then automatically resetting the target upon successful completion.

When connecting to a target, standalone ESP8266 circuit, its ENABLE pin must be pulled high to prevent the target microcontroller from entering a shutdown state
and to ensure it remains operational for programming. Additionally, a separate power supply for the target module is necessary.

Because the on-board microcontroller is disabled, its built-in status LED remains permanently turned off.
To compensate for the lack of a native power indicator on the NodeMCU layout, a discrete POWER LED has been added
to the programming board to serve as a power status indicator.

The NodeMCU module is mounted onto the programming board using female header sockets, allowing the module to be easily removed and reused.

## Prototype

TODO: Add photo when it will be ready.
