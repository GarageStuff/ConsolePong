using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using static ConsoleGame.GameManager;
using System.Text.RegularExpressions;

namespace ConsoleGame
{
    internal class GameServer
    {
        private Role currentRole;
        private Timer _timer;
        private readonly int _tickRate; // Tick rate in milliseconds
        private readonly object _lock = new object();
        private static GameManager ?gameManager;
        private static TcpListener server;
        public static List<TcpClient> seenClients= new List<TcpClient>();
        public List<Stream> playerStreams = new List<Stream>();
        public ClientManager clientManager = new ClientManager();
        public Pong pong;
        public GameServer(int tickRate, GameManager _gm)
        {
            currentRole = Role.Server;
            _tickRate = tickRate;
            _timer = new Timer(Tick, null, Timeout.Infinite, Timeout.Infinite);
            gameManager = _gm;
            clientManager.gameManager= gameManager;
        }
        public void Start()
        {
            _timer.Change(0, _tickRate);
            StartTcpListener();
        }
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer
        }
        private void Tick(object state)
        {
            lock (_lock)
            {
                //Console.WriteLine("Tick: " + DateTime.Now);
                if (gameManager.scored || gameManager.gameOver)
                {
                    return;
                }
                UpdateState();
            }
        }
        private async void StartTcpListener()
        {       
            try
            {
                int port = 13000;
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                
                await Task.Run(() => AcceptClientsAsync());
                //Console.SetCursorPosition(1, 29);
               //Console.WriteLine("Server started. Waiting for a connection...");
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
            }
            finally
            {
                server?.Stop();
            }
            //Console.WriteLine("\nHit enter to continue...");
            //Console.Read();
        }

        private async Task AcceptClientsAsync()
        {
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                seenClients.Add(client);
                playerStreams.Add(client.GetStream());
                //Console.SetCursorPosition(1, 23);
                //Console.WriteLine("Client connected!");
                await Task.Run(() => HandleClient(client));
            }
        }
        private async void HandleClient(TcpClient client)
        {
            byte[] buffer = new byte[256];
            NetworkStream stream = client.GetStream();

            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        //Console.SetCursorPosition(1, 38);
                        break;
                    }

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    switch (data[0])
                    {
                        case '1':
                            ParseNetCoord(gameManager.players[0], data);
                            break;
                        case '2':
                            ParseNetCoord(gameManager.players[1], data);
                            break;
                        case 'c':
                            clientManager.ReceiveChat(data);
                            break;
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
                seenClients.Remove(client);
                Console.SetCursorPosition(1, 33);
                Console.WriteLine("Client disconnected.");
                Console.Clear();

            }
        }
        static (char, (int, int)) ParseStringToTuple(string input)
        {
            char initialChar = input[0];
            string numericPart = input.Substring(1).Trim('(', ')').Replace(" ", "");
            string[] parts = numericPart.Split(',');
            int first = int.Parse(parts[0]);
            int second = int.Parse(parts[1]);
            return (initialChar, (first, second));
        }
       
        public async void UpdateState() 
        {
            string ballPosition="";
            Console.CursorVisible = false;
            if (pong?.ball != null)
            {
                if (gameManager.scored || gameManager.gameOver)
                {
                    return;  
                }
                pong.UpdateBall();
                ballPosition = pong.ball.position.ToString();
                ballPosition = ballPosition.Trim('(', ')').Replace(" ", "");
                string[] parts = ballPosition.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                ballPosition = "b" + ballPosition;
                if (gameManager.currentRole == GameManager.Role.Server && playerStreams.Count>0)
                {
                    if (clientManager.client.Connected || gameManager.gameServer.playerStreams.Count > 0)
                    {
                        await clientManager.SendDataAsync(ballPosition, gameManager.gameServer.playerStreams[0]);
                    }
                    
                }
                pong.DrawBall();
            }           
            foreach (Stream client in playerStreams)
            {
                clientManager.SendDataAsync("Tick", client);
                await clientManager.SendDataAsync("b" + ballPosition, client);
            }
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
    }

    
}
