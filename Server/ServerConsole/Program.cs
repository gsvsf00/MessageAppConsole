using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static ConcurrentDictionary<string, List<Socket>> channelClients;

        static void Main(string[] args)
        {
            channelClients = new ConcurrentDictionary<string, List<Socket>>();

            Console.WriteLine("Starting the server...");

            string ipAddress = "127.0.0.1";
            int port = 4444;

            IPAddress address;
            if (!IPAddress.TryParse(ipAddress, out address))
            {
                Console.WriteLine("Invalid IP address.");
                return;
            }

            IPEndPoint endPoint = new IPEndPoint(address, port);

            try
            {
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(10);

                Console.WriteLine("Server started and listening for connections...");

                while (true)
                {
                    Socket clientSocket = listener.Accept();
                    Task.Run(() => HandleClient(clientSocket));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        static void HandleClient(Socket client)
        {
            try
            {
                string clientAddress = client.RemoteEndPoint.ToString();
                Console.WriteLine($"Client connected: {clientAddress}");

                string availableChannels = GetAvailableChannels();
                SendMessage(client, availableChannels);

                string channel = string.Empty;
                string name = string.Empty;

                while (true)
                {
                    string receivedMessage = ReceiveMessage(client);
                    if (!string.IsNullOrWhiteSpace(receivedMessage))
                    {
                        string[] messageParts = receivedMessage.Split(':');
                        if (messageParts.Length >= 2)
                        {
                            string command = messageParts[0];
                            channel = messageParts[1];

                            if (command == "JOIN")
                            {
                                name = messageParts.Length >= 3 ? messageParts[2] : "Unknown";
                                JoinChannel(client, channel, name);
                            }
                            else if (command == "LEAVE")
                            {
                                name = messageParts.Length >= 3 ? messageParts[2] : "Unknown";
                                LeaveChannel(client, channel, name);
                                break; // Exit the loop and disconnect the client
                            }
                            else if (command == "MESSAGE")
                            {
                                name = messageParts.Length >= 3 ? messageParts[2] : string.Empty;
                                string message = messageParts.Length >= 4 ? messageParts[3] : string.Empty;
                                BroadcastMessage(client, channel, name, message);
                            }
                        }
                    }
                    else
                    {
                        // Abrupt disconnection
                        break; // Exit the loop and disconnect the client
                    }
                }

                // Check if it was an abrupt disconnection
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }

                Console.WriteLine($"Client disconnected: {clientAddress}");
                if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(name))
                {
                    string leaveMessage = $"{name} has left the channel.";
                    BroadcastMessage(client, channel, "Server", leaveMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        static void JoinChannel(Socket client, string channel, string name)
        {
            if (channelClients.TryGetValue(channel, out List<Socket> clients))
            {
                clients.Add(client);
            }
            else
            {
                clients = new List<Socket> { client };
                channelClients.TryAdd(channel, clients);
            }

            string joinMessage = $"{name} has joined the channel.";
            BroadcastMessage(client, channel, "Server", joinMessage);
        }

        static void LeaveChannel(Socket client, string channel, string name)
        {
            if (channelClients.TryGetValue(channel, out List<Socket> clients))
            {
                clients.Remove(client);

                string leaveMessage = $"{name} has left the channel.";
                BroadcastMessage(client, channel, "Server", leaveMessage);

                // Remove the channel from the dictionary if there are no clients left
                if (clients.Count == 0)
                {
                    channelClients.TryRemove(channel, out _);
                }
            }
        }

        static void BroadcastMessage(Socket sender, string channel, string name, string message)
        {
            if (channelClients.TryGetValue(channel, out List<Socket> clients))
            {
                string fullMessage = $"[{channel}]{name}: {message}";
                byte[] buffer = Encoding.UTF8.GetBytes(fullMessage);

                foreach (Socket client in clients)
                {
                    if (client != sender && client.Connected)
                    {
                        client.Send(buffer);
                    }
                }

                Console.WriteLine(fullMessage); // Display the message on the server console
            }
        }

        static string ReceiveMessage(Socket client)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = client.Receive(buffer);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        static void SendMessage(Socket client, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.Send(buffer);
        }

        static string GetAvailableChannels()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available channels:");
            foreach (string channel in channelClients.Keys)
            {
                sb.AppendLine(channel);
            }
            return sb.ToString();
        }
    }
}