# Weather Station V1

## Overview

A battery-powered, rechargeable IoT device built for reliable outdoor deployment within a durable, weather resistant enclosure.
It integrates sensors to monitor temperature, humidity, atmospheric pressure, and battery level, all managed by an ESP8266 microcontroller.

## Schematic

TODO: Add when it will be ready.

## Measurements

### Environmental Monitoring

Ambient temperature, relative humidity, and atmospheric pressure are measured using a BME280 environmental sensor module.
The sensor interfaces with the ESP8266 microcontroller over the I2C communication bus, enabling data acquisition with minimal pin usage.

### Batter Level

Battery voltage measurement is implemented using the ESP8266's internal analog-to-digital converter (ADC) via pin ADC0.
By sampling the LiPo battery voltage, the firmware estimates the remaining state of charge.

The ESP8266 ADC features a 10-bit resolution and an input voltage range of 0 to 1V.
Because a standard 1S LiPo battery operates across a higher voltage range (typically 3.0V to 4.2V),
a voltage divider comprising resistors R1 and R2 scales the voltage down to a safe operating range for the microcontroller (0-0.98V).

To eliminate parasitic current leakage through the voltage divider network when idle, a Q1 P-MOS transistor is integrated as a high-side load switch.
In its non-conducting state, its extremely high off-state impedance (in the gigaohm range) prevents static power consumption.
Without this switching mechanism, the continuous parasitic current draw (7-10uA depending on battery level) would significantly reduce battery operational lifespan,
as it can be substantial compared to the ~20 uA current drawn by the ESP8266 in deep sleep mode.
The P-MOS transistor must be switched on (driven into conduction) by the firmware immediately prior to acquiring the ADC sample.

For enhanced hardware safety, the voltage measurement section is isolated from the battery by an F1 PTC fuse, which provides short-circuit protection.
Additionally, a Q2 P-MOS transistor is integrated into this section to serve as reverse voltage protection, preventing damage in the event of an improper connection.
A dedicated female JST socket is provided for secure and convenient battery connection within the measurement circuit,
featuring physical polarization to make incorrect reverse insertion significantly less likely.

## Power Supply

A single-cell 18650 LiPo battery serves as the main power source for the board, connected via a dedicated female JST socket
that features physical polarization to make incorrect reverse insertion significantly less likely.

Charging, overcurrent protection, and overdischarge protection are managed by a dedicated module based on the TP4056 IC,
allowing the battery to be conveniently charged via an integrated USB-C port.

For enhanced hardware safety, an F1 750mA PTC fuse protects the main board from overcurrent events,
complemented by a Q3 P-MOS transistor configured for reverse polarity protection.

To regulate and normalize the battery voltage down to a stable 3.3V suitable for the ESP8266 and its surrounding components,
a U2 buck-boost converter is utilized. Additionally, a C5 capacitor is placed in the circuit to provide further voltage rectification and filtering.

## Firmware Upload and Debugging

For ease of development, both firmware programming and debugging are streamlined through a single, shared interface port combining UART,
FLASH, RESET, and GND lines into a single pin header compatible with the [ESP8266 programming board](./esp8266-programmer-v1.md).

Additionally, dedicated FLASH and RESET tactile switches are integrated directly onto the board.
These allow the ESP8266 IC to be manually forced into flashing mode by pulling the FLASH pin low while toggling the RESET button to trigger a low-level pulse.

## Status Indicators
To provide visual feedback on system state, the board features three distinct status LEDs:

- CONN: Indicates Wi-Fi network association status. It utilizes inverted logic—remaining illuminated upon boot and turning
off once the ESP8266 successfully associates with the configured Wi-Fi network.
- REG: Indicates successful registration with the central server. Like the CONN indicator,
it employs inverted logic, starting in an illuminated state and turning off once the station completes its registration process.
- BLD_IN: The native onboard status LED of the ESP8266 module, which pulses dynamically during firmware flashing cycles and hardware resets.

The inverted logic for the CONN and REG indicators is intentionally designed to minimize continuous power consumption.
By keeping the LEDs turned off during normal, steady-state operational modes, unnecessary quiescent current draw is avoided, maximizing battery longevity.

## Prototype

To ensure ease of manual soldering and prototyping, the board is designed entirely around through-hole technology (THT) components.

Because standard ESP8266 modules utilize surface-mount layouts, a 2.54 mm pitch adapter (raster board adapter) is incorporated to accommodate the module.
Note that the adapter board integrates essential pull-up/pull-down resistors for the CHIP_SELECT (GPIO15) and ENABLE pins, designated on the schematic as R5 and R9 respectively.

TODO: Add board photo when it will be ready
