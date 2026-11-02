#ifndef STATION_H
#define STATION_H

// TODO: Add doc-string.
struct Point {
    float x,
    float y
}

/// <summary>
/// Performs linear interpolation on a function approximation provided as a lookup table.
/// </summary>
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
/// Returns 0.0F on error with additional diagnostic logging.
/// </returns>
float linearInterpolation(float value, const Point& function, size_t funcitonSize);