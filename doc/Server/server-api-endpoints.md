# Server API Endpoints

## API Division
The Server API is segmented into two primary components based on the target consumer:
* **Firmware API:** Dedicated to communication between the server and the IoT node firmware.
* **Clients API:** Dedicated to handling incoming requests from system users (hereafter referred to as clients).

## Endpoints of the Firmware API
URLs for the Firmware API endpoints adhere to a standardized routing convention described by the following pattern:

`<BASE_URL>/api/firmware/v<API_VERSION>/<ENDPOINT>`

**Example:** *http://192.168.1.10/api/firmware/v1/stations*

## Endpoints of the Clients API
Clients API endpoints follow a similar routing convention tailored for user-facing interfaces:

`<BASE_URL>/api/clients/v<API_VERSION>/<ENDPOINT>`

**Example:** *http://192.168.1.10/api/clients/v1/switches*

## API Versioning
The Firmware and Clients APIs utilize independent versioning schemas. API versions are represented
by positive integers (e.g., *v1*, *v2*), allowing them to be seamlessly embedded directly into
the endpoint URL paths to prevent breaking changes across updates.

## Data-Transfer-Objects Naming Convention
The following convention has been established within the server codebase for naming Data Transfer Objects (DTOs):

`<DTO_NAME><DTO_INSTANCE_PRODUCER><Request/Response>`

Where the `<DTO_INSTANCE_PRODUCER>` indicates the origin of the payload and can be one of the following: 
* *Server*
* *Client*
* *Station*

### Examples:
* *SwitchUpdateClientRequest* - A DTO modeling a request initiated by a Client to update the state of a specified switch.
* *SwitchRegistrationStationRequest* - A DTO modeling a request initiated by node firmware (Station) to register a switch on the server.
* *SwitchRegistrationServerResponse* -  A DTO modeling the server's response to a switch registration request received from node firmware.
