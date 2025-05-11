<h1 align="center">
  <br>
  <img src="https://secure-asset-delivery.gameforge.com/partnersite_live_product/81854f0b-0698-4507-bcae-59b909e2f1f0/11EZuBbI9Ss_big.jpg" alt="Nostale" width="500">
  <br>
  Nostale Clientless WebController
  <br>
</h1>

**Nostale Clientless WebController** is an application built with **.NET Core 8**. The main objective is to offer a flexible and scalable way to interact with Nostale game servers without needing a traditional game client, leveraging modern backend technologies and container orchestration.

---

![Login and Nosbazar items search](https://ntdev.tech/Assets/Projects/NostaleClientLessWebController/Poc_Login.gif)

---

## Solution Structure

The solution consists of several projects:

- **NosCore.Worker**:  
  A background service implementing a clientless Nostale interaction logic.

- **NosCore.ClientApi**:  
  Allows sending actions to the Worker using MassTransit via RabbitMQ.

- **NosCore.GameAuthenticationApi**:  
  Handles authentication with GameForge and retrieves the necessary authentication token to access Nostale servers.

- **NosCore.Contracts**:  
  Contains communication contracts between the Worker and ClientApi.

- **NosCore.Tcp**:  
  A TCP client with socks5 proxy support.

- **NosCore.Packet**:  
  Houses all game packets for Nostale.

- **NosCore.Sdk**:  
  Contains shared elements used by the Worker.

- **NosCore.AppHost**:  
  Deploys the whole solution with initial setup. RabbitMQ is started and managed via Docker.

---

## Internal Project Workflow

- The project relies on **.NET Aspire** and **Docker** for orchestration.
- Aspire automatically deploys all projects and spins up a RabbitMQ Docker machine for inter-service communication.

---

## How to Run the Project

To get Nostale Clientless WebController running:

1. **Update Your Client Information**  
   - In `NosCore.Worker.Services.Client.ClientSession.SendLoginCredentials()`, update:
     - `versionNostaleClientX` with the current Nostale client version.
     - `hashNostaleClients` with the latest client hash.
     - *Note*: GameForge occasionally changes some fields, such as adding extra spaces, so keep an eye on those!

2. **Get Your GameForge Authentication Token**  
   - Use a packet logger to intercept the authentication packet sent when switching servers.
   - Extract the token from this packet.

3. **Login with the Token**  
   - Use the token with the `ClientApi` (Web API) using the `/LOGIN` endpoint.

---

## Technologies Used

- [.NET Core 8](https://dotnet.microsoft.com/)
- [MassTransit](https://masstransit-project.com/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [Docker](https://www.docker.com/)
- [Aspire](https://github.com/dotnet/aspire)

---

## Conclusion

Nostale Clientless WebController offers a robust, scalable way to automate Nostale interactions, API-driven and container-ready for modern development workflows. Perfect for developers looking to build bots, automation, or backend services for Nostale.

*Feel free to contribute or ask questions on the [GitHub repository](#)!*
