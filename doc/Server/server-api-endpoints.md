# Server API Endpoints

## API Division
The Server API is segmented into two primary components based on the target consumer:
* **Firmware API:** Dedicated to communication between the server and the IoT node firmware.
* **Clients API:** Dedicated to handling incoming requests from system users (hereafter referred to as clients).

## API Versioning
The Firmware and Clients APIs utilize independent versioning schemas. API versions are represented
by positive integers (e.g., *v1*, *v2*), allowing them to be seamlessly embedded directly into
the endpoint URL paths to prevent breaking changes across updates.

## Endpoints of the Firmware API
URLs for the Firmware API endpoints adhere to a standardized routing convention described by the following pattern:

`<BASE_URL>/api/firmware/v<API_VERSION>/<ENDPOINT>`

**Example:** *http://192.168.1.10/api/firmware/v1/stations*

## Endpoints of the Clients API
Clients API endpoints follow a similar routing convention tailored for user-facing interfaces:

`<BASE_URL>/api/clients/v<API_VERSION>/<ENDPOINT>`

**Example:** *http://192.168.1.10/api/clients/v1/switches*
