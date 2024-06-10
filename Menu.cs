using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public static class Menus
    {
        public static int width =60;
        public static int height = 20;
        public static void MainMenu()
        {
            Console.Clear();
            int consoleSizeX = width + 3;
            int consoleSizeY = height + 6;
            ConsoleWriter.AniWrite(25, "Would you like to play a game? (N)ew game or (J)oin?", new Tuple<int, int>(5,5));
        PROMPT:
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key == ConsoleKey.N)
            {
                NewGame();
                return;
            }
            else if (keyInfo.Key == ConsoleKey.J)
            {
                JoinMenu();
                return;
            }
            else
            {
                ConsoleWriter.AniWrite(50, "Try again...", new Tuple<int, int>(1, 3));
                Console.Write("");
                goto PROMPT;
            }
        }

        public static void JoinMenu() 
        {
            Console.Clear();
            //ConsoleWriter.AniWrite(10, "Enter host and port or leave blank for local game", new Tuple<int, int>(1, 1));
            //string host = Console.ReadLine();
            //if (host == "")
            //{
                Console.Clear();
            GameManager.SetRole(GameManager.Role.Client);
                //GameManager.currentRole = GameManager.Role.Client;
            //}

        }
        public static void NewGame()
        {
            //ConsoleWriter.AniWrite(25, "Ready? Hit enter to start or optional port number", new Tuple<int, int>(1, 1));
            //string response = Console.ReadLine();
            Console.Clear();
            GameManager.SetRole(GameManager.Role.Server);
        }
        public static void DrawGameMenu()
        {
            ConsoleWriter.AniWrite(10, "(esc) to quit", new Tuple<int, int>(63, 2));
        }
        public static void OpenMatchOver(string player)
        {           
            ConsoleWriter.AniWrite(10, player + "scored!", new Tuple<int, int>(22, 15));
            ConsoleWriter.AniWrite(10, "Press SPACE to rematch or ESC to quit.", new Tuple<int, int>(10, 16));
        }
        static void SendRematch()
        {
            GameManager.Rematch();
        }
        public static void QuitToMenu()
        {
            
        }
        public static void QuitMenu()
        {
            
        }
        public static void StartWaiting()
        {
            if (GameManager.waitingForPlayerConnect)
            {
            ConsoleWriter.AniWrite(10, "Waiting For Player...", new Tuple<int, int>(18, 13));
            }
        }
        public static async void ReadyPrompt()
        {
            ConsoleWriter.AniWrite(1, "Players Connected", new Tuple<int, int>(20, 12));
            ConsoleWriter.AniWrite(1, "READY?: Y/N", new Tuple<int, int>(24, 13));
        RETRY:
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.Y:
                    ConsoleWriter.AniWrite(10, "                  ", new Tuple<int, int>(20, 12));
                    ConsoleWriter.AniWrite(10, "           ", new Tuple<int, int>(24, 13));
                    string message = "sready";
                    GameManager.readyPlayers += 1;
                    if (GameManager.readyPlayers == 2)
                    {
                        GameManager.playersReady = true;
                    }
                    if (ClientManager.clients[0] != null)
                    {
                        await ClientManager.SendDataAsync(message, ClientManager.stream);
                    }
                    break;
                case ConsoleKey.C:
                    InputController.StartChatting();
                    goto RETRY;                    
                case ConsoleKey.Escape:
                    GameManager.QuitToMenu();
                    break;
                default:
                    goto RETRY;

            }
            
        }
    }
}
