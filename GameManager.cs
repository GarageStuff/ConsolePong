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
    public static class GameManager

    {
        public enum Role { Client, Server, Host }
        public static Role currentRole = Role.Client;
        public static int tickRate = 1;
        private static float _tick = 1;
        public static List<TcpClient> clients = new();
        public static List<int> playerIDs = new();
        public static int consoleWidth, consoleHeight;       
        public static List<Player> players = new();
        public static bool waitingForPlayerConnect = true;
        public static int readyPlayers = 0;
        public static bool playersReady = false;
        public static int player1Score, player2Score;
        public static Tuple<int, int> scores;
        public static Tuple<int, int> scoreLocation1;
        public static Tuple<int, int> scoreLocation2;
        public static bool scored = false;
        public static bool gameOver = false;
        
        static GameManager() 
        {

        }
        
        public static void SetRole(Role role)
        {
            currentRole = role;
            switch (role)
            {
                case Role.Client:
                    try
                    {                     
                        StartClient();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
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
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                    finally
                    {
                        Console.WriteLine("Application finished. Press Enter to exit...");
                        Console.ReadLine();
                    }
            }
        }
        public static void StartGameServer()
        {
            Task.Run(() =>
            {                
                GameServer.Start(100);
            });

        }
        public static async Task StartClient()
        {   
            waitingForPlayerConnect= true;           
            try
            {
                if (currentRole == Role.Client)
                {
                    ClientManager.ConnectAsync("127.0.0.1", 13000);
                }               
                StartGame();               
            }
            catch(Exception ex) 
            {
                ConsoleWriter.Write(20, 20, ex.ToString());
                Console.ReadLine() ;
            }
            finally
            {
                ClientManager.Disconnect();
                Console.WriteLine("Client Ended");
                //Reset();
                //Console.ReadLine();
            }

        }
        public static void AddClient(TcpClient client)
        {
            clients.Add(client);
        }
        public static void RemoveClient(TcpClient client)
        {
            clients.Remove(client);
        }

        public static void PlayerReady()
        {
            
        }
        /////GAMEPLAY STUFF
        public static void StartGame()
        {    
            Pong.BuildBoard(60, 20);
            Menus.StartWaiting();
            Pong.PlacementCountDown(5);
            InputController.listening= true;
            InputController.GetInput();
        }
        public static void QuitToMenu()
        {
            if (ClientManager.stream != null)
            {
                ClientManager.stream.Close();
                if (ClientManager.client != null)
                {
                    ClientManager.client.Close();
                }
            }
            InputController.listening= false;
            playersReady = false;
            readyPlayers = 0;
            Menus.MainMenu();
        }
        public static void Reset()
        {
            Console.Clear();
            if (ClientManager.client.Connected)
            {
                
                ClientManager.Disconnect();
                
            }
            InputController.listening = false;
            Menus.MainMenu();
        }
        public static void Rematch()
        {
            playersReady = false;
            readyPlayers = 0;
            scored = false;
            gameOver = false;
            InputController.chatting = false;
            InputController.menuOpen = false;
            Pong.PlacementCountDown(5);
            InputController.listening= true;
            Pong.PlaceBall();
            InputController.GetInput();            
            
            //Pong.Rematch();
        }
        public static void UpdateScore()
        {
            ConsoleWriter.Write(27, 2, player1Score.ToString());
            ConsoleWriter.Write(33, 2, player2Score.ToString());
        }
    }
    
}
