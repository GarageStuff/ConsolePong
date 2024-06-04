using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public class Player
    {
        public int Id;
        public string Name;
        TcpClient client;
        NetworkStream connection;
        public Tuple<int, int> position;
        public Paddle paddle;
        public List<string> chats = new List<string>();
    }
    internal class GameManager
    {
        public enum Role { Client, Server, Host }
        public Role currentRole = Role.Client;
        public int tickRate = 1;
        private float _tick = 1;
        public GameManager gm;
        public List<TcpClient> clients = new();
        public ClientManager clientManager = new ClientManager();
        public InputController controller = new();
        public GameServer gameServer;
        public int consoleWidth, consoleHeight;        
        public List<Player> players = new();
        public int player1Score, player2Score;
        public Tuple<int, int> scores;
        public Tuple<int, int> scoreLocation1;
        public Tuple<int, int> scoreLocation2;
        public Pong pong;
        public bool scored = false;
        public bool gameOver = false;
        public GameManager(Role role)
        {
            pong = new Pong();
            clientManager.gameManager = this;
            clientManager.gameServer = gameServer;
            controller.gameManager= this;           
            controller.clientManager = clientManager;
            pong.gameManager = this;
            controller.pong = pong;
            currentRole= role;
            switch (role) 
            {
                case Role.Client:
                    try
                    {
                        StartClient();
                        controller.listening= true;
                        break;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }
                    finally 
                    {
                        Console.WriteLine("Application finished. Press Enter to exit...");
                        Console.ReadLine();
                    }
                    
               case Role.Server:
                    try
                    {
                        StartGameServer();
                        StartClient();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }
                    finally
                    {
                        Console.WriteLine("Application finished. Press Enter to exit...");
                        Console.ReadLine();
                    }
            }
        }      
        void MainMenu()
        {
            Console.SetCursorPosition(1, 1);
        }
        public void StartGameServer()
        {
            Task.Run(() =>
            {
                GameServer server = new GameServer(100, this); // Tick every 1000 ms (1 second)
                server.Start();
                gameServer = server;
                gameServer.pong= pong;
                //Console.SetCursorPosition(1, 30);
                //Console.WriteLine("Starting server...");
                StartGame();
            });

        }

        public async Task StartClient()
        {           
            try
            {
                if (currentRole == Role.Client)
                {
                    clientManager.ConnectAsync("127.0.0.1", 13000);
                }
                controller.listening= true;
                controller.menuOpen = false;
                controller.GetInput();
                
            }
            catch(Exception ex) 
            {
                ConsoleWriter.Write(20, 20, ex.ToString());
                Console.ReadLine() ;
            }
            finally
            {
                clientManager.Disconnect();
                Console.WriteLine("Client Ended");
                Reset();

                //Console.ReadLine();
            }

        }
        public void AddClient(TcpClient client)
        {
            clients.Add(client);
        }
        public void RemoveClient(TcpClient client)
        {
            clients.Remove(client);
        }

        /////GAMEPLAY STUFF
        public void StartGame()
        {
            //ConsoleWriter.AniWrite(100, "this long sentence", new Tuple<int,int>(5,5));
            pong.BuildBoard(60, 20);
        }

        public void Reset()
        {
            Console.Clear();
            if (clientManager.client.Connected)
            {
                
                clientManager.Disconnect();
                
            }
            controller.listening = false;
            Menus.MainMenu();
        }
        public void Rematch()
        {
            ConsoleWriter.AniWrite(5, "                   ", new Tuple<int, int>(22, 15));
            ConsoleWriter.AniWrite(5, "                                        ", new Tuple<int, int>(20, 16));
            controller.listening = true;
            controller.chatting = false;
            pong.PlaceBall();
            
        }
        public void UpdateScore()
        {
            ConsoleWriter.Write(27, 2, player1Score.ToString());
            ConsoleWriter.Write(33, 2, player2Score.ToString());
        }
    }
    
}
