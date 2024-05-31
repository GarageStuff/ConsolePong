using System;
using System.Collections.Generic;
using System.Linq;
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
            int consoleSizeX = width + 3;
            int consoleSizeY = height + 6;
            ConsoleWriter.AniWrite(25, "Would you like to play a game? (N)ew game or (J)oin?", new Tuple<int, int>(1, 1));
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
                //GameManager gameManager = new GameManager(GameManager.Role.Client);
                //gameManager.currentRole = GameManager.Role.Client;
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
                GameManager gameManager = new GameManager(GameManager.Role.Client);
                gameManager.currentRole = GameManager.Role.Client;
            //}

        }
        public static void NewGame()
        {
            Console.Clear();
            //ConsoleWriter.AniWrite(25, "Ready? Hit enter to start or optional port number", new Tuple<int, int>(1, 1));
            //string response = Console.ReadLine();
            Console.Clear();
            if (true)
            {
                GameManager gameManager = new GameManager(GameManager.Role.Server);
            }
            else 
            {
                GameManager gameManager = new GameManager(GameManager.Role.Server);
            }
        }
        public static void DrawGameMenu()
        {
            ConsoleWriter.AniWrite(10, "(esc) to quit", new Tuple<int, int>(63, 4));
            ConsoleWriter.AniWrite(10, "(c) to chat", new Tuple<int, int>(63, 6));
        }

        public static void QuitMenu()
        {
            
        }
    }
}
