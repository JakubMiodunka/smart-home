#ifndef BATTERIES_H
#define BATTERIES_H

#include <Arduino.h>

/// <summary>
/// Computes the state-of-charge percentage for a lithium battery based on its voltage and cell count.
/// </summary>
/// <param name="batteryVoltage">
/// The total measured voltage of the battery pack.
/// </param>
/// <param name="cells">
/// The number of cells connected in series within the battery pack.
/// </param>
/// <returns>
/// The estimated battery charge level as a percentage.
/// Returns 0.0F if the cell count is zero.
/// </returns>
float computeLithiumBatteryChargeLevel(float batteryVoltage, uint8_t cells);

#endif