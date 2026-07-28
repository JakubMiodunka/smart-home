#ifndef SERIAL_LOGGING_H
#define SERIAL_LOGGING_H

#include <Arduino.h>

/// <summary>
/// Describes the severity level of a log message.
/// </summary>
enum LoggingLevel {
  UNKNOWN,
  DEBUG,
  INFO,
  WARNING,
  ERROR,
  CRITICAL
};

/// <summary>
/// Logs a formatted message to the serial interface at the specified logging level.
/// </summary>
/// <param name="level">
/// The severity level of the log message.
/// </param>
/// <param name="format">
/// A C-style format string (supports printf-style specifiers like %s, %d, %u).
/// </param>
/// <param name="...">
/// Additional arguments matching the format specifiers provided in <paramref name="format"/>.
/// </param>
void logToSerial(const LoggingLevel level, const char* format, ...);

#endif