using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static Socket client;
        static IPEndPoint endPoint;
        static string channel;
        static string name;
        static bool hasJoined = false;

        static void Main(string[] args)
        {
            try
            {
                string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
                string port = ConfigurationManager.AppSettings["Port"];

                if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(port))
                {
                    Console.WriteLine("Invalid configuration. Using default IP address and port.");
                    ipAddress = "127.0.0.1";
                    port = "4444";
                }

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress address;

                while (!IPAddress.TryParse(ipAddress, out address))
                {
                    Console.WriteLine("Wrong IP format");
                    ipAddress = Console.ReadLine();
                }

                int portNumber;
                while (!int.TryParse(port, out portNumber) || portNumber <= 0)
                {
                    Console.WriteLine("Wrong port number");
                    port = Console.ReadLine();
                }

                endPoint = new IPEndPoint(address, portNumber);
                ConnectClient();
                while (client.Connected)
                {
                    if (!hasJoined)
                    {
                        SendJoinMessage();
                        hasJoined = true;
                    }

                    WaitForMessages();
                }
            }
            catch
            {
                Console.WriteLine("Unable to connect to the server. Please try again later.");
            }
        }

        static void ConnectClient()
        {
            Console.WriteLine("Connecting to the server...");
            try
            {
                client.Connect(endPoint);
                Console.WriteLine("Connected to the server!");
                Console.Clear();

                // Receive the list of available channels from the server
                string availableChannels = ReceiveMessage();
                Console.WriteLine(availableChannels);

                // Prompt the user to enter a channel name
                Console.WriteLine("Enter a channel name:");
                channel = Console.ReadLine();

                // Prompt the user to enter their name
                Console.WriteLine("Enter your name:");
                name = Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Connection failed!");
                return;
            }

            Task.Run(() => WaitForMessages());
        }

        static void WaitForMessages()
        {
            try
            {
                while (client.Connected)
                {
                    if (client.Available > 0)
                    {
                        // Receive and display the received message
                        string receivedMessage = ReceiveMessage();
                        Console.WriteLine(receivedMessage);
                    }

                    // Check if the user has entered a message
                    if (Console.KeyAvailable)
                    {
                        // Read the message from the user
                        string userInput = Console.ReadLine();

                        // Send the message to the server
                        SendRegularMessage(userInput);
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Lost connection to the server. Retrying to reconnect...");

                // Reconnect to the server
                ConnectClient();
                if (!client.Connected)
                {
                    Console.WriteLine("Failed to reconnect to the server. Please try again later.");
                    return;
                }

                // Resend the join message
                if (!hasJoined)
                {
                    SendJoinMessage();
                    hasJoined = true;
                }

                // Continue listening for messages
                WaitForMessages();
            }
            catch
            {
                Console.WriteLine("An error occurred. Disconnecting from the server.");
                DisconnectClient();
            }
        }

        static void DisconnectClient()
        {
            if (client.Connected)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            Console.WriteLine("Disconnected from the server. Press any key to exit.");
            Console.ReadKey();
        }

        static void SendJoinMessage()
        {
            string joinMessage = $"JOIN:{channel}:{name}";
            SendMessage(joinMessage);
            Console.Clear();
            Console.WriteLine($"You have joined the channel {channel} as {name}.\n To leave write [/leave]");
        }

        static void SendRegularMessage(string message)
        {
            if (message.ToLower() == "/leave")
            {
                string leaveMessage = $"LEAVE:{channel}:{name}";
                SendMessage(leaveMessage);
            }
            else
            {
                string regularMessage = $"MESSAGE:{channel}:{name}:{message}";
                SendMessage(regularMessage);
            }
        }

        static void SendMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.Send(buffer);
        }

        static string ReceiveMessage()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = client.Receive(buffer);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}