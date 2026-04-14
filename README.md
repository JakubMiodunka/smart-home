# smart-home

## Description

This repository provides an [IoT](https://en.wikipedia.org/wiki/Internet_of_things "Wikipedia article") ecosystem
designed to transition standard residential spaces into modern smart-home environments. The system architecture is 
centered around a single server instance that orchestrates a distributed network of stations within a single household.
Communication is established over the local Wi-Fi network, leveraging the infrastructure existing nowadays in most
households to facilitate data exchange between the central controller and the individual hardware nodes.

A core principle of the system is its feature-oriented model. Rather than maintaining a library of supported devices,
the server treats hardware nodes as unknown entities until the point of registration. Upon connecting, each station 
dynamically registers itself within the system along with all utilities that it implements. By abstracting
functionality into these standardized features, the server interacts with functional capabilities directly,
independent of the underlying hardware implementation.

While this approach is notoriously difficult to maintain in commercial markets, where fragmented manufacturer standards
often hinder interoperability. This private side project provides a unique opportunity to implement the system in this manner.
By leveraging the advantages of full-stack vertical integration, where the hardware, firmware, and server software are
developed by a single individual, the system bypasses traditional compatibility constraints. This creates a highly
optimized environment where the benefits of a modular, capability-based architecture are fully realized through
a unified, custom-tailored ecosystem.

### Project Motivation

The primary objective of developing this repository was to gain hands-on educational value in the following fields:

* Understand the interactions between each application layer, from low-level firmware through the backend to the frontend interface.
* Understanding of database design and the effective use of [SQL](https://en.wikipedia.org/wiki/SQL "Wikipedia article") and 
[SQL Server](https://learn.microsoft.com/en-us/sql/sql-server/what-is-sql-server?view=sql-server-ver17 "Microsoft Learn article").
* Refreshing and refining skills in firmware development.
* Practical implementation of [ADO.NET](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/ "Microsoft Learn article") basics.
* Architecture and design of end-to-end IoT systems.

Beyond the technical milestones, this project is driven by the enjoyment of the engineering craft.
There is immense satisfaction in building a system from the ground up that reflects personal philosophy of software development.

### Current state

The system currently features a functional foundational architecture with established bidirectional communication between
the central server and hardware nodes. The core database schema is implemented for persistent data management,
and the server-side logic is integrated with modular firmware components capable of data exchange.
At this stage, the system supports primary home automation functionality through on/off switching,
allowing the server to control binary states within the household while maintaining
a stable connection across the local network infrastructure.

### Future Directions

The project is designed for continuous expansion across all layers of the stack.
Future functional updates will extend the system's capabilities beyond binary switching to include linear control modules,
such as light dimmers, and the integration of environmental telemetry to track measurements like temperature
and humidity across rooms within the household. To move beyond the current reliance on manual HTTP requests via tools like
cURL or Postman, a dedicated graphical client application is planned to provide a more intuitive and fluid interaction with the system.
On the hardware side, the project will expand to include specific station models accompanied by electronic schematics and 3D models.
These assets will cover protective enclosures and also functional mechanical components required for specialized station operations.
Furthermore, development will focus on iterative improvements to code quality and the systematic strengthening of cybersecurity
to transition the system from a foundational prototype to a hardened, production-ready smart home solution.

## Repository Structure

* */src/firmware* - Source code for the station's firmware. Implemented in the C language.
* */src/server* - Source code for the central server responsible for managing the system and aggregating data from all components.
Implemented in ADO.NET.
* */doc* - General project documentation. 
* */doc/server/html* - HTML-based server code documentation, automatically generated using
[DoxyGen](https://www.doxygen.nl/ "DoxyGen website"). View the content by opening *index.html* in Your browser of choice.

## Getting Started 

TODO: Refine this section

### Server Setup
The backend infrastructure requires *SQL Server 2022*. For a streamlined deployment, it is recommended to use Docker and [Official SQL Server Docker Image](https://hub.docker.com/_/microsoft-mssql-server).

* Initialize your instance by deploying the schema located in the `Database` project.
* Update the connection string within `Program.cs` to match your SQL Server credentials.
* he application is configured to bind to `localhost` by default. To enable communication with external hardware nodes, update the network interface settings in the `launchsettings.json` file to match your host's local IP address.

Note: As this project is currently in development, file-oriented configuration management (such as `.json` or `.env` files) has not yet been implemented.
All configurations are handled directly within the source code.

### Node Deployment
Hardware stations are built using the *NodeMCU (ESP8266)* platform. You will need the *Arduino IDE* to flash the firmware.

* Open the firmware source code in the Arduino IDE.
* Compile and upload the code to your NodeMCU board.
* The current firmware implementation includes a single **Binary Switch** utility, which is configured to control the state of the onboard LED.

### Interactions with the System

As the frontend interface is currently under development, all interactions with the server must be performed via direct API calls. You can use tools such as **cURL**, **Postman**, or **Insomnia** to communicate with the system.

* **API Discovery:** Once a node registers, the server exposes its utilities via RESTful endpoints.
* **Control Flow:** Commands are sent to the server, which then coordinates the specific action with the registered hardware node.

*Note:** Detailed API documentation and a collection of sample requests will be added as the system expands.


## Project versioning

Versioning for this project follows the established guidelines
of [semantic versioning](https://en.m.wikipedia.org/wiki/Software_versioning#Semantic_versioning "Wikipedia article").

Current version: 1.0.0

## Project management

Project development is tracked and managed using GitHub project available under following link:
[smart-home](https://github.com/users/JakubMiodunka/projects/7 "Link to smart-home project")

If You spotted some bug or have a suggestion feel free to create corresponding issue there.

## Used Tools

* Server development IDE: [Visual Studio 2022/2026](https://visualstudio.microsoft.com/vs/ "Visual Studio website")
* Firmware development IDE: [Arduino IDE](https://www.arduino.cc/en/software/ "Arduino website")
* Documentation generator: [DoxyGen 1.12.0](https://www.doxygen.nl/ "DoxyGen website")
* AI model used during development: [Google Gemini](https://gemini.google.com/app "Google Gemini chat")

## AI Disclaimer

The code in this repository was developed with a focus on deep technical understanding and the enjoyment of the engineering process.
No part of the system was [vibe-coded](https://en.wikipedia.org/wiki/Vibe_coding "Wikipedia article"),
and no [LLM model](https://en.wikipedia.org/wiki/Large_language_model "Wikipedia article") had direct access to
the files stored in this repository. All AI-assisted tasks were conducted through manual chat interactions to maintain
full human oversight. No code was directly copy-pasted from the chat, instead all implementation was authored manually
based on refined concepts to uphold high quality and best practices. AI tools were used strictly for research,
code refinement, and documentation assistance to ensure high quality and best practices.

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")

## License

This project is licensed under the MIT License - see the [*LICENSE.md*](./LICENSE "Licence") file for details.