#ifndef SERIAL_LOGGING_H
#define SERIAL_LOGGING_H

#include <Arduino.h>

// TODO: Add doc-string.
enum LoggingLevel {
  UNKNOWN,
  DEBUG,
  INFO,
  WARNING,
  ERROR,
  CRITICAL
};

/// TODO: Add doc string.
void logToSerial(const LoggingLevel level, const char* format, ...);

#endif