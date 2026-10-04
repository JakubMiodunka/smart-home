# Sensors and Measurements

## Concept

The primary utility of a smart home system is the ability to measure various physical parameters and detect specific events across the household.
In our system, these responsibilities are fulfilled by sensors. Sensors act as data providers, monitoring the physical environment,
transforming physical phenomena into digital telemetry and transmitting these state updates back to the central server.

## Variants of Measuring Methods

### Measurement on Demand

In this model, data acquisition is strictly passive until explicitly initiated by an external trigger or system user.
The sensor remains in a standby state and performs physical parameter evaluation only upon receiving an explicit request from the central server.
This approach minimizes network traffic and processing overhead, making it ideal for non-critical telemetry and scenarios where continuous monitoring is unnecessary.

### Measurement on Change

In this event-driven model, the sensor independently detects changes in the physical environment, such as a door opening or a state transition.
Upon identifying an event occurrence, the sensor immediately transmits an updated state report to the central server.
This approach provides immediate responsiveness to real-time events while preserving bandwidth during static conditions.

Currently not implemented.

### Scheduled Measurement

In this periodic model, the sensor automatically measures physical parameters and reports telemetry to the central server at configured time intervals.
This approach guarantees consistent, time-series data collection independent of external triggers or state changes,
making it essential for the continuous monitoring of fluctuating environmental conditions, such as outdoor temperature.

Currently not implemented.

## Measurement Execution Sequence

Different hardware sensors often demand specific operational sequences to guarantee accurate data collection and preserve component longevity.
To maximize flexibility and support a wide variety of hardware configurations, the firmware divides the sensor lifecycle
and measurement execution flow into four distinct, fully customizable steps:

1. **Sensor Initialization**
   Executes a one-time setup routine when the device boots or registers a new sensor.
   This step includes actions such as configuring GPIO pin modes, initializing communication protocols like I2C or SPI,
   setting default sampling resolutions, or verifying hardware connectivity.

2. **Pre-Measurement Action**
   Executes necessary preparation tasks immediately before taking a reading.
   This step includes actions such as powering up peripheral lines via MOSFETs, warming up sensitive environmental sensors,
   or sending wake-up commands to sleeping chips.

3. **Measurement Action**
   Executes the primary data acquisition logic.
   This step includes actions such as reading analog signals or digital registers,
   applying calibration formulas, and returning the final numerical telemetry value.

4. **Post-Measurement Action**
   Performs teardown and power-management tasks immediately following data retrieval.
   This step includes actions such as cutting power to external modules, putting sensors back into low-power sleep modes,
   or releasing bus locks to optimize energy efficiency.
   