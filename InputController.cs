using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public static class InputController
    {
        public static bool menuOpen = false;
        public static bool listening = false;
        public static bool chatting = false;
        public static Player player;
        public static List<string> chats= new List<string>();

        
        public static void Listen()
        {
            listening = true;
        }
        public static async void GetInput()
        {           
            while (true)
            {
                //Thread.Sleep(10);
                if (GameManager.scored)
                {
                    while (true)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                        if (keyInfo.Key == ConsoleKey.Spacebar)
                        {
                            GameManager.scored = false;
                            Pong.Rematch();
                            break;
                        }
                    }
                }
                if (menuOpen)
                {
                    continue;
                }
                if (GameManager.scored || GameManager.gameOver)
                {
                    continue;
                }
                if (listening)
                { 
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                        if (keyInfo.Key == ConsoleKey.DownArrow)
                        {
                            SendPaddleDown(player);
                        }
                        if (keyInfo.Key == ConsoleKey.UpArrow)
                        {
                            SendPaddleUp(player);
                        }
                        if (keyInfo.Key == ConsoleKey.Escape)
                        {
                            Menus.QuitMenu();
                        }
                        if (keyInfo.Key == ConsoleKey.C)
                        {
                            StartChatting();
                        }
                        if (keyInfo.Key == ConsoleKey.Spacebar)
                        {
                            if (GameManager.scored || GameManager.gameOver)
                            {
                                Pong.Rematch();                    
                            }
                        }
                    Pong.UpdateBoard();
                }
                if (chatting)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        Console.CursorVisible = false;
                        chatting = false;
                        listening = true;
                    }
                }
                //Pong.UpdateBoard();               
            }
        }
        public static async void MovePaddle(Player player,(int,int) position)
        {
            if ((player.position.Item2 >= 20 || player.position.Item2 <= 2))
            {
                return;
            }
            else
            {
                Pong.ClearPaddle(player.position);

                string message = " + player.position.ToString()";
               

                {
                    await ClientManager.SendDataAsync(message, null);
                }
                if (GameManager.currentRole == GameManager.Role.Server)
                {
                    await ClientManager.SendDataAsync(message, null);
                }

            }
        }
        public static async void SendPaddleDown(Player player)
        {
            if (player.position.Item2 >= 19)
            {
                return;
            }
            else
            {
                player.paddle.prevPosition = player.position;
                player.position = new Tuple<int, int>(player.position.Item1, player.position.Item2 + 1);                
                for (int i = -1; i < 2; i++)
                {
                    Console.SetCursorPosition(player.paddle.prevPosition.Item1, player.paddle.prevPosition.Item2 + i);
                    Console.Write(" ");
                }
                string message = "";
                
                for (int i = 0; i < 3; i++)
                {
                    player.paddle.positions[i] = new Tuple<int, int>(player.position.Item1, player.position.Item2 + (i + -1));
                }
                if (GameManager.currentRole == GameManager.Role.Server)
                {
                    message = "1" + player.position.ToString();
                }
                if (GameManager.currentRole == GameManager.Role.Client)
                {
                    message = "2" + player.position.ToString();
                }
                if (GameManager.currentRole == GameManager.Role.Client)
                {
                    if (ClientManager.stream!=null)
                    {
                        await ClientManager.SendDataAsync(message, ClientManager.stream);
                    }
                }
                if (GameManager.currentRole == GameManager.Role.Server)
                {
                    if (ClientManager.stream != null || GameServer.playerStreams.Count > 0)
                    {
                        await ClientManager.SendDataAsync(message, ClientManager.stream);
                    }                   
                }
            }
            Pong.DrawPlayers();
        }
        public static async void SendPaddleUp(Player player)
        {
            if (player.position.Item2 <= 5)
            {
                return;
            }
            else
            {
                
                Pong.ClearPaddle(player.position);
                string message = "";
                player.position = new Tuple<int, int>(player.position.Item1, player.position.Item2 - 1);
                for (int i = 0; i < 3; i++)
                {
                    player.paddle.positions[i] = new Tuple<int, int>(player.position.Item1, player.position.Item2 +(i + -1));
                }
                if (GameManager.currentRole == GameManager.Role.Server)
                {
                    message = "1"+player.position.ToString();
                }
                if (GameManager.currentRole == GameManager.Role.Client)
                {
                    message = "2" + player.position.ToString();
                }
                if (GameManager.currentRole == GameManager.Role.Client)
                {
                    if (ClientManager.stream!=null)
                    {
                        await ClientManager.SendDataAsync(message, ClientManager.stream);
                    }                                     
                }
                if (GameManager.currentRole == GameManager.Role.Server)
                {
                    if (ClientManager.stream != null || GameServer.playerStreams.Count > 0)
                    {
                        await ClientManager.SendDataAsync(message, ClientManager.stream);
                    }            
                }
            }
            Pong.DrawPlayers();
        }
        public static void ReceivePaddle(Player _player, Tuple<int,int> coord)
        {
            _player.paddle.prevPosition = _player.position;
            for (int i = -1; i < 2; i++)
            {
                Console.SetCursorPosition(_player.paddle.prevPosition.Item1, _player.paddle.prevPosition.Item2+i);
                Console.Write(" ");
                
            }
            _player.position = coord;
            for (int i = 0; i < 3; i++)
            {
                _player.paddle.positions[i] = new Tuple<int, int>(_player.position.Item1, _player.position.Item2 + (i - 1));
            }
            Pong.DrawPlayers();
        }
        public static async void StartChatting()
        {
            ConsoleWriter.Write(3,27, "Say:                                                  ");
            chatting = true;
            listening = false;
            string message = "";
            Console.CursorVisible = true;
        CAPTURE:
            Console.SetCursorPosition(ConsoleWriter.chatStart.Item1 + message.Length - 1, ConsoleWriter.chatStart.Item2);
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            if(keyInfo.Key == ConsoleKey.Escape) 
            {
                StopChatting();
                return;
            }
            if (keyInfo.Key == ConsoleKey.Enter)
            {                   
                ConsoleWriter.WriteChat("Me: "+message);                              
                for (int i = 0; i < message.Length; i++)
                {
                    Console.SetCursorPosition(ConsoleWriter.chatStart.Item1 + i, ConsoleWriter.chatStart.Item2);
                    Console.Write(" ");
                }
                if (GameManager.currentRole == GameManager.Role.Server)
                {
                    message = "c1:" + message;
                }
                else
                {
                    message = "c2:" + message;
                }
                if (ClientManager.stream!=null)
                {
                    await ClientManager.SendDataAsync(message, ClientManager.stream);
                }
                message = "";
            }
            else
            {
                if (message.Length <50)
                {
                    message += keyInfo.KeyChar.ToString();
                    ConsoleWriter.Write(ConsoleWriter.chatStart.Item1 + message.Length - 1, ConsoleWriter.chatStart.Item2, keyInfo.KeyChar);
                }
            }                
            goto CAPTURE;
        }
        public static void StopChatting()
        {
            ConsoleWriter.Write(3,27, "(C) To Chat                                            ");
            Console.CursorVisible= false;
            listening = true;
            chatting = false;
        }
    }
}
