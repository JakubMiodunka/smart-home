# smart-home

## Description

This repository provides an [IoT](https://en.wikipedia.org/wiki/Internet_of_things "Wikipedia article") ecosystem
designed to transition standard residential spaces into modern 
[smart-home](https://en.wikipedia.org/wiki/Home_automation "Wikipedia article") environments. The system architecture
is centered around a single server instance that orchestrates a distributed network of stations within a single household.
Communication is established over the local [Wi-Fi](https://en.wikipedia.org/wiki/Wi-Fi "Wikipedia article") network,
leveraging the infrastructure existing nowadays in most households to facilitate data exchange between the central
controller and the individual hardware nodes. Primary communication within the system is driven by
[RESTful](https://en.wikipedia.org/wiki/REST "Wikipedia article") [HTTP](https://en.wikipedia.org/wiki/HTTP "Wikipedia article")
[APIs]( https://en.wikipedia.org/wiki/API "Wikipedia article") exposed and utilized by all parts of the system.

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

* Understanding the interactions between each application layer, from low-level firmware through the backend to the frontend interface.
* Understanding of database design and the effective use of [SQL](https://en.wikipedia.org/wiki/SQL "Wikipedia article") and 
[SQL Server](https://learn.microsoft.com/en-us/sql/sql-server/what-is-sql-server?view=sql-server-ver17 "Microsoft Learn article").
* Refining proficiency in [C language](https://en.wikipedia.org/wiki/C_(programming_language) "Wikipedia article")
in context of firmware development.
* Practical implementation of
[ADO.NET](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/ "Microsoft Learn article") basics.
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
[cURL](https://curl.se/ "cURL website"), [Postman](https://www.postman.com/ "Postman website")
or [Insomnia](https://insomnia.rest/ "Insomnia website"), a dedicated graphical client application is planned to provide
a more intuitive and fluid interaction with the system. On the hardware side, the project will expand to include comprehensive
designs for specific station models, accompanied by electronic schematics and 3D CAD models. These assets will cover protective
enclosures and also functional mechanical components required for specialized station operations.
Furthermore, development will focus on iterative improvements to code quality and the systematic strengthening
of cybersecurity to transition the system from a foundational prototype to a hardened, production-ready smart-home solution.

## Repository Structure

* [*/src/Firmware*](./src/Firmware/) - Source code for the station's firmware.
Implemented in [Arduino](https://www.arduino.cc/ "Arduino website") ecosystem.
* [*/src/Server*](./src/Server/) - Source code for the central server responsible for managing the system
and aggregating data from all components.
Implemented in [C#](https://en.wikipedia.org/wiki/C_Sharp_(programming_language) "Wikipedia article") and ADO.NET.
* [*/doc/Server/html*](./doc/Server/html) - HTML-based server code documentation, automatically generated using
[DoxyGen](https://www.doxygen.nl/ "DoxyGen website"). View the content by opening
[*index.html*](./doc/Server/html/index.html) in Your browser of choice.

## Getting Started 

### Server Setup

The backend infrastructure requires instance of SQL Server 2022. For a streamlined deployment, it is recommended
to use [Docker](https://www.docker.com/ "Docker website") and [Official SQL Server Docker Image](https://hub.docker.com/_/microsoft-mssql-server).

* Initialize your instance by deploying the schema located in the [*Database.sqlproj*](./src/Server/Database/Database.sqlproj).
* Update the connection string within [*Program.cs*](./src/Server/Server/Program.cs) to match your SQL Server credentials.
* The application is configured to bind to *localhost* by default. To enable communication with external hardware nodes,
update the network interface settings in the [*launchSettings.json*](./src/Server/Server/Properties/launchSettings.json)
file to match your host's local IP address.

Note: As this project is currently in development, file-oriented configuration management has not yet been implemented.
All configurations are handled directly within the source code.

### Node Deployment

Hardware stations are built using the NodeMCU (ESP8266) platform. You will need the Arduino IDE to flash the firmware.

* Open the Arduino IDE and configure it to support NodeMCU by
following the [official tutorial](https://projecthub.arduino.cc/PatelDarshil/getting-started-with-nodemcu-esp8266-on-arduino-ide-b193c3)
* Open the [*generic_firmware.ino*](./src/Firmware/generic_firmware/generic_firmware.ino) in the Arduino IDE,
compile and upload the code to your NodeMCU board.
* The current firmware implementation includes a single on/off switch utility, which is configured to control the state of the build-in LED.

### Interactions With The System

As the frontend interface is currently not available, all interactions with the server must be performed via direct API calls.
You can use tools such as cURL, Postman, Insomnia or the built-in Visual Studio HTTP Client to communicate with the system.
For specific request structures and endpoint examples, please inspect 
the [*Server.http*](./src/Server/Server/Server.http) file located in the repository.

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