# Switches

## Concept

A fundamental utility of any smart home system is the ability to toggle bistable states, such as turning devices on and off.
In this system, this functionality is implemented through switches, which act as digital state controllers driving station GPIO output pins.
What a GPIO pin physically controls - whether a relay, a status LED, an active low driver, or a high-voltage load — is strictly
a hardware design decision, providing full operational flexibility across different applications.

The operational state of each switch is continuously synchronized with the central server,
allowing users to remotely trigger state changes at any time through the central server interface.

## Switch Variants

To accommodate diverse hardware circuits, the logical state of a switch (ON/OFF) is intentionally decoupled from the physical electrical state of its assigned GPIO pin (`HIGH`/`LOW`). Whether a switch is considered active or inactive is determined by its configured logic type:

* **Regular Logic:** The switch is active (ON) when the digital pin state is driven `HIGH`, and inactive (OFF) when set to `LOW`.
* **Reversed Logic:** The switch is active (ON) when the digital pin state is driven `LOW`, and inactive (OFF) when set to `HIGH`.
