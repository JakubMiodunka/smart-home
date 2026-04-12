# smart-home

## Description

This repository provides an IoT ecosystem designed to transition standard residential spaces into modern smart-home environments.
The system architecture is centered around a single server instance that orchestrates a distributed network of stations within a single household.
Communication is established over the local Wi-Fi network, leveraging the infrastructure existing nowadays in most households to facilitate data exchange between the central controller and the individual hardware nodes.

The primary objective of developing this repository was to gain hands-on educational value in the following fields:

* Understand the interactions between each application layer, from low-level firmware through the backend to the frontend interface.
* Understanding of database design and the effective use of SQL and SQL Server.
* Refreshing and refining skills in firmware development.
* Practical implementation of ADO.NET basics.
* Architecture and design of end-to-end IoT systems.

### Current state

The system currently features a functional foundational architecture with established bidirectional communication between the central server and hardware nodes.
The core database schema is implemented for persistent data management, and the server-side logic is integrated with modular firmware components capable of data exchange.
At this stage, the system supports primary home automation functionality through on/off switching, allowing the server to control binary states within the household while maintaining a stable connection across the local network infrastructure.

### Future Directions

The project is designed for continuous expansion across all layers of the stack.
Future functional updates will extend the system's capabilities beyond binary switching to include linear control modules, such as light dimmers, and the integration of environmental telemetry to track measurements like temperature and humidity across rooms within the household. To move beyond the current reliance on manual HTTP requests via tools like cURL or Postman, a dedicated graphical client application is planned to provide a more intuitive and fluid interaction with the system.
On the hardware side, the project will expand to include specific station models accompanied by electronic schematics and 3D models.
These assets will cover protective enclosures and also functional mechanical components required for specialized station operations.
Furthermore, development will focus on iterative improvements to code quality and the systematic strengthening of cybersecurity to transition the system from a foundational prototype to a hardened, production-ready smart home solution.

## Repository Structure

* */src/firmware* - Source code for the station's firmware. Implemented in the C language.
* */src/server* - Source code for the central server responsible for managing the system and aggregating data from all components. Implemented in ADO.NET.
* */doc* - General project documentation. 
* */doc/server/html* - HTML-based server code documentation, automatically generated using [DoxyGen](https://www.doxygen.nl/ "DoxyGen website"). View the content by opening *index.html* in Your browser of choice.

## Project versioning

Versioning for this project follows the established guidelines of [semantic versioning](https://en.m.wikipedia.org/wiki/Software_versioning#Semantic_versioning "Wikipedia article").

Current version: 1.0.0

## Project management

Project development is tracked and managed using GitHub project available under following link: [smart-home](https://github.com/users/JakubMiodunka/projects/7 "Link to smart-home project").
If You spotted some bug or have a suggestion feel free to create corresponding issue there.

## Used Tools

* Server development IDE: [Visual Studio 2022/2026](https://visualstudio.microsoft.com/vs/ "Visual Studio website")
* Firmware development IDE: Arduino IDE [https://www.arduino.cc/en/software/]("Arduino website")
* Documentation generator: [DoxyGen 1.12.0](https://www.doxygen.nl/ "DoxyGen website")
* AI model used during development: [Google Gemini](https://gemini.google.com/app "Google Gemini chat")

## AI Disclaimer

The code in this repository was developed with a focus on deep technical understanding and the enjoyment of the engineering process.
No part of the system was [vibe-coded](https://en.wikipedia.org/wiki/Vibe_coding "Wikipedia article"), and no AI model had direct access to the files stored in this repository.
All AI-assisted tasks were conducted through manual chat interactions to maintain full human oversight. No code was directly copy-pasted from the chat, instead all implementation was authored manually based on refined concepts to uphold high quality and best practices. AI tools were used strictly for research, code refinement, and documentation assistance to ensure high quality and best practices.

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")

## License

This project is licensed under the MIT License - see the [*LICENSE.md*](./LICENSE "Licence") file for details.