using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleGame
{
    public static class ClientManager
    {
        public static TcpClient client = new();
        public static NetworkStream stream;
        public static List<TcpClient> clients = new List<TcpClient>();       

        public static async Task ConnectAsync(string server, int port)
        {
            GameManager.waitingForPlayerConnect = true;
            if (GameManager.currentRole == GameManager.Role.Client)
            {
                try
                {
                    if (client.Connected == false)
                    {
                        await client.ConnectAsync(server, port);

                    }
                    stream = client.GetStream();
                    //ConsoleWriter.Write(1,26,"connected to server");
                    GameManager.clients.Add(client);
                    clients.Add(client);
                    GameManager.waitingForPlayerConnect = false;

                    //GameManager.StartGame();
                    await Task.Run(() => ReceiveDataAsync());
                    //GameManager.StartGame();
                    //nInputController.GetInput();
                    // Start receiving data

                }
                catch (Exception ex)
                {
                    Console.SetCursorPosition(2, 23);
                    Console.Write(ex);
                }
                finally 
                {
                    Console.SetCursorPosition(2, 23);
                    Console.Write("clientmanager connect async closed");
                }
            }
            
        }
        private static async Task ReceiveDataAsync()        
        {

            byte[] buffer = new byte[256];
            Pong.waitingOnPlayerConnect = false;
            try
            {
                while (true)
                {
                    
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Server disconnected

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    switch (data[0])
                    {
                        case '1':
                            ParseNetCoord(GameManager.players[0], data);
                            break;
                        case '2':
                            ParseNetCoord(GameManager.players[1], data);
                            break;
                        case 'c':
                            ReceiveChat(data);
                            break;
                        case 's':
                            ReceiveState(data);
                            break;
                        case 'n':
                            ReceiveNetworkCommand(data);
                            break;
                        case 'b':
                            if (data.Length > 4 && data.Length < 8)
                            {
                                var result = ParseStringToIntegers(data);
                                Pong.ReceiveBallPositon(result.Item1, result.Item2);
                                break;
                            }
                            else 
                            {
                                break;
                            }                            
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.SetCursorPosition(16, 10);
                Console.WriteLine("Lost Connection");
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client != null)
                {
                    
                    client.Close();
                }
                Console.SetCursorPosition(12, 11);
                Console.WriteLine("Disconnected from the server.");

                Menus.MainMenu();
            }
        }

        public  static void ReceiveState(string data)
        {
            switch (data)
            {
                case "sready":
                    GameManager.readyPlayers += 1;
                    if (GameManager.readyPlayers == 2)
                    {
                        SendDataAsync("sready", stream);
                        GameManager.playersReady = true;
                    }
                    break;
                case "sscored1":
                    GameManager.scored = true;
                    Pong.Score(1);
                    break;
                case "sscored2":
                    GameManager.scored = true;
                    Pong.Score(2);
                    break;
                case "srematch":
                    //GameManager.scored = false;
                    GameManager.Rematch();
                    break;
                default: break;
            }
         
        }

        public static async Task SendDataAsync(string message, Stream playerStream)
        {
            if (message != null)
            {
                try
                {
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    if (playerStream!=null)
                    {
                        await playerStream.WriteAsync(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    if (client != null)
                    {

                        client.Close();
                    }
                    Console.SetCursorPosition(12, 11);
                    Console.WriteLine("Disconnected from the server.");

                    Menus.MainMenu();
                }
            }                     
        }
        public static void Disconnect()
        {
            client.Close();
        }
        static (int, int) ParseStringToIntegers(string input)
        {
            var match = Regex.Match(input, @"[a-zA-Z]+(\d+),(\d+)");
            if (!match.Success)
            {
                throw new ArgumentException("Input string is not in the expected format.");
            }
            int x = int.Parse(match.Groups[1].Value);
            int y= int.Parse(match.Groups[2].Value);
            return (x, y);
        }

        public static void ParseNetCoord(Player player, string message)
        {
            string pattern = @"\((\d+),\s*(\d+)\)";
            Match match = Regex.Match(message, pattern);
            if (match.Success)
            {
                int number1 = int.Parse(match.Groups[1].Value);
                int number2 = int.Parse(match.Groups[2].Value);
                Tuple<int, int> coord = new Tuple<int, int>(number1, number2);
                InputController.ReceivePaddle(player, coord);
            }
            string capturedGroup = match.Groups[1].Value;
        }

        public static async void SendChat(string message)
        {
            if (client.Connected || GameServer.playerStreams.Count > 0)
            {                
                await SendDataAsync(message, null);
            }            
        }
        public static async void ReceiveChat(string response)
        {
            int index = response.IndexOf(':');
            string sender = response.Substring(0, index);
            string message = response.Substring(index + 1);
            if (sender == "c1")
            {
                message = "Player1:"+ message;
            }
            if (sender == "c2")
            {
                message = "Player2:"+ message;             
            }
            ConsoleWriter.WriteChat(message);
        }

        public static void ReceiveNetworkCommand(string message)
        {
            switch (message)
            {
                case "nConfirmConnect":
                    GameManager.waitingForPlayerConnect = false;
                    break;
            }
        }

    }

}
