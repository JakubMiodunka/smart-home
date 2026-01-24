#include "serial_logging.h"
#include <stdarg.h>

void logToSerial(LoggingLevel level, const char* format, ...) {
  const char* levelNames[] = {"UNKNOWN", "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"};
  const char* levelName = (0  <= level && level <= 5) ? levelNames[level] : levelNames[0];

  char messageBuffer[256];
  va_list args;
  va_start(args, format);
  vsnprintf(messageBuffer, sizeof(messageBuffer), format, args);
  va_end(args);

  Serial.printf("[%s] %s\n", levelName, messageBuffer);
}