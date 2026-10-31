# Weather Station V1

## Introduction

Please refer to [`firmware-development.md`](./firmware-development.md) for the project's core firmware development philosophy and coding standards.

This firmware is dedicated to a battery-powered, portable weather station designed to monitor environmental conditions.

## Features

The firmware integrates the *Bosch BME280* sensor to continuously monitor ambient temperature, relative humidity, and barometric pressure.
Additionally, it provides functionality for measuring system battery voltage levels.
Operational status is indicated via onboard LEDs, which signal Wi-Fi connection progress and server registration state.

## Hardware

This firmware utilizes the project's proprietary PCB.
For detailed circuit schematics, board design, and hardware specifications, refer to
[`weather-station-v1.md`](../hwr/weather-station-v1.md) located in the hardware documentation directory.

## Dependencies

**Board Manager Packages**

* [ESP8266 Arduino Core](https://github.com/esp8266/Arduino)

**Arduino Libraries**

* [ArduinoJson](https://arduinojson.org/?utm_source=meta&utm_medium=library.properties), version 7.4.3
* [Adafruit BME280 Library](https://github.com/adafruit/Adafruit_BME280_Library), version 2.3.0
