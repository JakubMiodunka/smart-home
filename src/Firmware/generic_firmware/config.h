#ifndef CONFIG_H
#define CONFIG_H

#include <Arduino.h>

// Adjust according to your needs.
extern const bool SERIAL_PORT_LOGGING;
extern const uint32_t SERIAL_PORT_BAUD_RATE;

extern const uint32_t REQUESTS_RETRY_INTERVAL; // Given in milliseconds.

// Adjust below constraints to your server configuration.
extern const char* SERVER_IP_ADDRESS;
extern const uint16_t SERVER_PORT;
extern const char* PROTOCOL; // For now only 'http' is supported.
extern const uint8_t SERVER_API_VERSION;

#endif