# Weather Station V1

## Overview

A battery-powered, rechargeable IoT device built for reliable outdoor deployment within a durable, weatherproof enclosure.
It integrates sensors to monitor temperature, humidity, atmospheric pressure, and battery level, all managed by an ESP8266 microcontroller.
For ease of development, both firmware programming and debugging are streamlined through a single, shared interface port compatible with ESP8266 programmer.

## Schematic

TODO

## Power Supply

## Measurements

### Environmental Monitoring

Ambient temperature, relative humidity, and atmospheric pressure are measured using a high-precision BME280 environmental sensor module.
The sensor interfaces with the ESP8266 microcontroller over the I2C communication bus, enabling data acquisition with minimal pin usage.

### Batter Level

Battery voltage measurement is implemented using the ESP8266's internal analog-to-digital converter (ADC) via pin ADC0.
By sampling the LiPo battery voltage, the firmware estimates the remaining state of charge.

The ESP8266 ADC features a 10-bit resolution and an input voltage range of 0 to 1V.
Because a standard 1S LiPo battery operates across a higher voltage range (typically 3.0V to 4.2V),
a voltage divider comprising resistors R1 and R2 scales the voltage down to a safe operating range for the microcontroller (0-0.98V).

To eliminate parasitic current leakage through the voltage divider network when idle, a Q1 P-MOS transistor is integrated as a high-side load switch.
In its non-conducting (off) state, its extremely high off-state impedance (in the gigaohm range) prevents static power consumption.
Without this switching mechanism, the continuous parasitic current draw (7-10uA depending on battery level) would significantly reduce battery operational lifespan,
as it can be substantial compared to the ~20 uA current drawn by the ESP8266 in deep sleep mode.
The P-MOS transistor must be switched on (driven into conduction) by the firmware immediately prior to acquiring the ADC sample.

## Programming

## Safety features

## Prototype