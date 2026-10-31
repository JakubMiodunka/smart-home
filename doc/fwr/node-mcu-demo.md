# NodeMCU Demonstrative Firmware

## Introduction

Please refer to [`firmware-development.md`](./firmware-development.md) for the project's core firmware development philosophy and coding standards.

Demonstrative firmware baseline designed for initial system validation prior to deploying dedicated hardware peripherals.

## Features

The firmware implements a straightforward on/off utility configured to remotely control the state of the built-in LED.

## Hardware

No proprietary hardware is required.
A standard, bare *NodeMCU* development board featuring the *ESP8266-12E* IC is sufficient for running this firmware.

## Dependencies

**Board Manager Packages**

* [ESP8266 Arduino Core](https://github.com/esp8266/Arduino)

**Arduino Libraries**

* [ArduinoJson](https://arduinojson.org/?utm_source=meta&utm_medium=library.properties), version 7.4.3