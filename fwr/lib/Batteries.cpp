#include <Arduino.h>

#include <Batteries.h>

/// <summary>
/// Representation of a point on a Cartesian coordinate plane.
/// </summary>
/// <remarks>
/// Currently, this structure is embedded within the battery module, but it may be moved to a more 
/// generic module for broader reuse in the future.
/// </remarks>
struct Point {
    /// <summary>
    /// X coordinate of the represented point.
    /// </summary>
    float x;
    
    /// <summary>
    /// Y coordinate of the represented point.
    /// </summary>
    float y;
};

/// <summary>
/// Performs linear interpolation on a function approximation provided as a lookup table.
/// </summary>
/// <remarks>
/// Implementation assumes that argument values are valid - providing invalid arguments may lead to a crash.
/// Function approximation is usually a static, non-changing collection, so continuous validation
/// is an inefficient approach, therefore  no runtime validation is performed.
/// Currently, this function is embedded within the battery module, but it may be moved to a more 
/// generic module for broader reuse in the future.
/// </remarks>
/// <param name="value">
/// The input x-coordinate to interpolate.
/// </param>
/// <param name="functionApproximation">
/// Pointer to an array of points treated as a function approximation. 
/// Must contain strictly increasing X values with a minimum of 2 points.
/// </param>
/// <param name="approximationSize">
/// Number of elements in the <paramref cref="functionApproximation"/> array.
/// </param>
/// <returns>
/// The interpolated Y-value, or the clamped boundary value if <paramref cref="value"/> falls outside the approximation range. 
/// </returns>
static float linearInterpolation(float value, const Point* functionApproximation, size_t approximationSize) {
    const size_t lastIndex = approximationSize - 1;
    for (size_t index = 0; index < lastIndex; index++)
    {
        const Point& current = functionApproximation[index];
        const Point& next = functionApproximation[index + 1];
        
        if (value < current.x || next.x < value) continue;

        return current.y + ((next.y - current.y) * (value - current.x) / (next.x - current.x));
    }

    if (value < functionApproximation[0].x) return functionApproximation[0].y;

    return functionApproximation[lastIndex].y;
}

float computeLithiumBatteryChargeLevel(float batteryVoltage, uint8_t cells) {
    if (cells == 0) return 0.0F;

    static const Point BatteryDischargeApproximation[] = {
        {3.00F, 0.0F},
        {3.30F, 3.0F},
        {3.40F, 5.0F},
        {3.50F, 10.0F},
        {3.60F, 20.0F},
        {3.70F, 35.0F},
        {3.80F, 50.0F},
        {3.90F, 65.0F},
        {4.00F, 80.0F},
        {4.10F, 90.0F},
        {4.20F, 100.0F}
    };

    static constexpr size_t BatteryDischargeApproximationSize = 
        sizeof(BatteryDischargeApproximation) / sizeof(Point);

    float averageCellVoltage = batteryVoltage / cells;
    
    return linearInterpolation(
        averageCellVoltage, 
        BatteryDischargeApproximation, 
        BatteryDischargeApproximationSize
    );
}