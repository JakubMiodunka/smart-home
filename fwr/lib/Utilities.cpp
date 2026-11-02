

float linearInterpolation(float value, const Point* functionApproximation, size_t approximationSize) {
    if (approximationSize < 2) {
        logToSerial(ERROR, "At least 2 points required for function approximation: SIZE=[%lu]", (unsigned long)approximationSize);
        return 0.0F;
    }

    const size_t lastIndex = approximationSize - 1;
    for (size_t index = 0; index < lastIndex; index++)
    {
        const Point& current = functionApproximation[index];
        const Point& next = functionApproximation[index + 1];
        
        if (next.x <= current.x) {
            logToSerial(ERROR, "X values of function approximation not strictly increasing: INDEX=[%lu]", (unsigned long)index);
            return 0.0F;
        }

        if (value < current.x || next.x < value) continue;

        return current.y + ((next.y - current.y) * (value - current.x) / (next.x - current.x));
    }

    if (value < functionApproximation[0].x)
    {
        logToSerial(WARNING, "Value below interpolation range: VALUE=[%f], RANGE_MIN=[%f]", value, functionApproximation[0].x);
        return functionApproximation[0].y;
    }

    logToSerial(WARNING, "Value above interpolation range: VALUE=[%f], RANGE_MAX=[%f]", value, functionApproximation[lastIndex].x);
    return functionApproximation[lastIndex].y;
}