#include "serial_logging.h"
#include <stdarg.h>

void log_to_serial(LoggingLevel level, const char* format, ...) {
  const char* level_names[] = {"UNKNOWN", "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"};
  const char* level_name = (0  <= level && level <= 5) ? level_names[level] : level_names[0];

  char message_buffer[256];
  va_list args;
  va_start(args, format);
  vsnprintf(message_buffer, sizeof(message_buffer), format, args);
  va_end(args);

  Serial.printf("[%s] %s\n", level_name, message_buffer);
}