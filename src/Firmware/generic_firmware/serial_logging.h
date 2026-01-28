#ifndef SERIAL_LOGGING_H
#define SERIAL_LOGGING_H

#include <Arduino.h>

enum LoggingLevel {
  UNKNOWN,
  DEBUG,
  INFO,
  WARNING,
  ERROR,
  CRITICAL
};

/// TODO: Add doc string.
void logToSerial(LoggingLevel level, const char* format, ...);

#endif