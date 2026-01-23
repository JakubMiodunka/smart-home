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

void log_to_serial(LoggingLevel level, const char* format, ...);

#endif