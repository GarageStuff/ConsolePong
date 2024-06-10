using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleGame
{
    public static class GameServer
    {
        private static Timer _timer;
        private static readonly int _tickRate =  2000; // Tick rate in milliseconds
        private static readonly object _lock = new object();
        private static TcpListener server;
        public static List<TcpClient> seenClients= new List<TcpClient>();
        public static List<Stream> playerStreams = new List<Stream>();
        static GameServer()
        {

        }
        public static void Start(int _tickRate)
        {
            
            _timer = new Timer(Tick, null, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(0, _tickRate);
            StartTcpListener();
        }
        public static void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer
        }
        private static void Tick(object state)
        {
            lock (_lock)
            {
                if (GameManager.scored || GameManager.gameOver)
                {
                    return;
                }
                else 
                {
                    if (GameManager.playersReady)
                    {
                        UpdateState();
                    }                   
                }
            }
        }
        private static async void StartTcpListener()
        {       
            try
            {
                int port = 13000;
                server = new TcpListener(IPAddress.Any, port);
                server.Start();                
                await Task.Run(() => AcceptClientsAsync());
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
            }
            finally
            {
                Console.WriteLine("TCP listener stopped");
                server?.Stop();
            }
        }

        private static async Task AcceptClientsAsync()
        {
            GameManager.waitingForPlayerConnect = true;
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                seenClients.Add(client);
                
                ClientManager.clients.Add(client);
                playerStreams.Add(client.GetStream());
                await Task.Run(() => HandleClient(client));
            }
        }
        private static async void HandleClient(TcpClient client)
        {
            byte[] buffer = new byte[256];
            NetworkStream stream = client.GetStream();
            ConfirmConnect(stream);
            ClientManager.stream= stream;
            Pong.waitingOnPlayerConnect = false;
            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        //Disconnect
                        break;
                    }
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
                            ClientManager.ReceiveChat(data);
                            break;
                        case 's':
                            ClientManager.ReceiveState(data);
                            break;
                        case 'n':
                            ClientManager.ReceiveNetworkCommand(data);
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
                if (client != null)
                {
                    client.Close();
                    seenClients.Remove(client);
                }              
                Console.SetCursorPosition(12,20);
                Console.WriteLine("Lost Connection");
                Console.SetCursorPosition(8,21);
                Console.WriteLine("Press Any Key To Return To Menu...");
                Console.ReadLine();
                GameManager.QuitToMenu();
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
        public static async void UpdateState() 
        {
            if (!GameManager.playersReady)
            { return; }
            string ballPosition="";
            Console.CursorVisible = false;
            if (Pong.ball != null)
            {
                if (GameManager.scored || GameManager.gameOver)
                {
                    return;  
                }
                Pong.UpdateBall();
                ballPosition = Pong.ball.position.ToString();
                ballPosition = ballPosition.Trim('(', ')').Replace(" ", "");
                string[] parts = ballPosition.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                ballPosition = "b" + ballPosition;
                if (GameManager.currentRole == GameManager.Role.Server && playerStreams.Count>0)
                {
                    if (ClientManager.client.Connected || GameServer.playerStreams.Count > 0)
                    {
                        await ClientManager.SendDataAsync(ballPosition, GameServer.playerStreams[0]);
                    }                    
                }
                Pong.DrawBall();
            }           
            foreach (Stream client in playerStreams)
            {
                await ClientManager.SendDataAsync("b" + ballPosition, client);
            }
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

        public static async void ConfirmConnect(Stream clientStream)
        {
            await ClientManager.SendDataAsync("nConfirmConnect", clientStream);
        }

        static void ReceiveNetworkCommand(string message)
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
