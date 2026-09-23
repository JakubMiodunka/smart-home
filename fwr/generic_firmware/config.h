#ifndef CONFIG_H
#define CONFIG_H

#include <Arduino.h>

#include "serial_logging.h"

// Remote server properties:
extern const char* REMOTE_SERVER_IP_ADDRESS;
extern const uint16_t REMOTE_SERVER_PORT;
extern const char* REMOTE_SERVER_PROTOCOL; // For now only 'http' is supported.
extern const uint8_t REMOTE_SERVER_API_VERSION;

// Local server configuration:
extern const uint16_t LOCAL_SERVER_PORT;
extern const uint8_t LOCAL_SERVER_API_VERSION;
extern const uint32_t LOCAL_SERVER_API_POLL_INTERVAL; // Given in milliseconds.
extern const bool IP_RESTRICTION; // Process only requests from the remote server IP.

// HTTP data transmission:
extern const uint32_t REQUESTS_RETRY_INTERVAL; // Given in milliseconds.
extern const uint32_t HEARTBEAT_INTERVAL; // Given in milliseconds.

// Serial port configuration:
extern const uint32_t SERIAL_PORT_BAUD_RATE;
extern const bool SERIAL_PORT_LOGGING; 
extern const LoggingLevel SERIAL_PORT_LOGGING_LEVEL;
  
#endif