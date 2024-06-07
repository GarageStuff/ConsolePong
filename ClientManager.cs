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
    internal class ClientManager
    {
        public  TcpClient client = new();
        private NetworkStream stream;
        public List<TcpClient> clients= new List<TcpClient>();
        public GameManager gameManager;
        public GameServer gameServer;
        //public InputController controller;
        public ClientManager()
        {
            //client = new TcpClient();
        }
        public async Task ConnectAsync(string server, int port)
        {
            if (gameManager.currentRole == GameManager.Role.Client)
            {
                try
                {
                    if (client.Connected == false)
                    {
                        await client.ConnectAsync(server, port);

                    }
                    stream = client.GetStream();
                    //ConsoleWriter.Write(1,26,"connected to server");
                    gameManager.clients.Add(client);

                    gameManager.StartGame();
                    await Task.Run(() => ReceiveDataAsync()); // Start receiving data

                }
                catch (Exception ex)
                {
                    Console.SetCursorPosition(2,23);
                    Console.Write(ex);
                }
            }
            
        }
        private async Task ReceiveDataAsync()        
        {

            byte[] buffer = new byte[256];
            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Server disconnected

                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    switch (response[0])
                    {
                        case '1':
                            ParseNetCoord(gameManager.players[0], response);
                            break;
                        case '2':
                            ParseNetCoord(gameManager.players[1], response);
                            break;
                        case 'c':
                            ReceiveChat(response);
                            break;
                    }
                    if (response[0].ToString()=='b'.ToString())
                    {
                        var result = ParseStringToIntegers(response);
                        gameManager.pong.ReceiveBallPositon(result.Item1, result.Item2);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Disconnected from the server.");
            }
        }
        public async Task SendDataAsync(string message, Stream playerStream)
        {
            if (message != null)
            {
                try
                {
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    if (client.Connected)
                    {
                        
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                    if (gameManager?.currentRole == GameManager.Role.Server)
                    {
                        if (client.Connected || gameManager.gameServer.playerStreams.Count > 0)
                        {
                            playerStream = gameManager.gameServer.playerStreams[0];
                            await playerStream.WriteAsync(data, 0, data.Length);
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }                     
        }
        public void Disconnect()
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

        public void ParseNetCoord(Player player, string message)
        {
            string pattern = @"\((\d+),\s*(\d+)\)";
            Match match = Regex.Match(message, pattern);
            if (match.Success)
            {
                int number1 = int.Parse(match.Groups[1].Value);
                int number2 = int.Parse(match.Groups[2].Value);
                Tuple<int, int> coord = new Tuple<int, int>(number1, number2);
                gameManager.controller.ReceivePaddle(player, coord);
            }
            string capturedGroup = match.Groups[1].Value;
        }

        public async void SendChat(string message)
        {
            if (client.Connected || gameManager.gameServer.playerStreams.Count > 0)
            {
                
                await SendDataAsync(message, null);
            }
            
        }
        public async void ReceiveChat(string response)
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
    }
}
