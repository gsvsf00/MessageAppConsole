# Server-Client Chat Application

This is a simple server-client chat application implemented in C#. The application allows multiple clients to connect to a server and communicate with each other through different channels.

## Features

- Multiple clients can connect to the server simultaneously.
- Clients can join different channels and send messages within the channel.
- Clients can leave channels and disconnect from the server.
- Server broadcasts messages to all connected clients within the same channel.
- Server maintains a list of available channels.

## Server

The server application is responsible for accepting client connections, managing channels, and broadcasting messages.

### Running the Server

To run the server, follow these steps:

1. Open the `Server` solution in Visual Studio.
2. Build the solution to ensure all dependencies are resolved.
3. Run the `Server` project.

The server will start listening for client connections on the specified IP address and port.

## Client

The client application allows users to connect to the server, join channels, send messages, and disconnect.

### Running the Client

To run the client, follow these steps:

1. Open the `Client` solution in Visual Studio.
2. Build the solution to ensure all dependencies are resolved.
3. Run the `Client` project.

The client application will need you to edit the .config file to enter the server IP address and port. Once connected, you can join channels, send messages, and perform other chat operations.

## Dependencies

The server and client applications are built using C# and .NET Framework. The applications use the following dependencies:

- `System.Net.Sockets` for network communication.
- `System.Collections.Concurrent` for thread-safe collections.
- `System.Configuration` for reading server configuration from the `App.config` file.

## License

This project is licensed under the [MIT License](LICENSE).

Feel free to explore, modify, and use the code according to your needs.
