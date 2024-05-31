﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    internal class InputController
    {
        public ClientManager clientManager;
        public GameManager gameManager;
        public Pong pong;
        public bool listening = false;
        public bool chatting = false;
        public Player player;
        public InputController() 
        {
            listening = false;
        }
        public async void GetInput()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                {
                    if (listening)
                    {
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
                    }
                    if (chatting)
                    {
                        if (keyInfo.Key == ConsoleKey.Escape)
                        {
                            Console.CursorVisible= false;
                            listening = true;
                        }
                    }
                    pong.UpdateBoard();
                }
            }
        }
        public async void MovePaddle(Player player,(int,int) position)
        {
            if ((player.position.Item2 >= 20 || player.position.Item2 <= 2))
            {
                return;
            }
            else
            {
                pong.ClearPaddle(player.position);

                string message = " + player.position.ToString()";
                if (gameManager.currentRole == GameManager.Role.Client)
                {
                    await clientManager.SendDataAsync(message, null);
                }
                if (gameManager.currentRole == GameManager.Role.Server)
                {
                    await clientManager.SendDataAsync(message, null);
                }

            }
            //pong.DrawPlayers();
        }
        public async void SendPaddleDown(Player player)
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
                if (gameManager.currentRole == GameManager.Role.Server)
                {
                    message = "1" + player.position.ToString();
                }
                if (gameManager.currentRole == GameManager.Role.Client)
                {
                    message = "2" + player.position.ToString();
                }
                if (gameManager.currentRole == GameManager.Role.Client)
                {
                    await clientManager.SendDataAsync(message, null);
                }
                if (gameManager.currentRole == GameManager.Role.Server)
                {
                    await clientManager.SendDataAsync(message, gameManager.gameServer.playerStreams[0]);
                }
            }
            //Thread.Sleep(100);
            pong.DrawPlayers();
        }
        public async void SendPaddleUp(Player player)
        {
            if (player.position.Item2 <= 5)
            {
                return;
            }
            else
            {
                pong.ClearPaddle(player.position);
                string message = "";
                player.position = new Tuple<int, int>(player.position.Item1, player.position.Item2 - 1);
                for (int i = 0; i < 3; i++)
                {
                    player.paddle.positions[i] = new Tuple<int, int>(player.position.Item1, player.position.Item2 +(i + -1));
                }
                if (gameManager.currentRole == GameManager.Role.Server)
                {
                    message = "1"+player.position.ToString();
                }
                if (gameManager.currentRole == GameManager.Role.Client)
                {
                    message = "2" + player.position.ToString();
                }
                if (gameManager.currentRole == GameManager.Role.Client)
                {
                        await clientManager.SendDataAsync(message, null);                    
                }
                if (gameManager.currentRole == GameManager.Role.Server)
                {
                        await clientManager.SendDataAsync(message, gameManager.gameServer.playerStreams[0]);                  
                }
            }
            pong.DrawPlayers();
        }

        public void ReceivePaddle(Player _player, Tuple<int,int> coord)
        {
            _player.paddle.prevPosition = _player.position;
            for (int i = -1; i < 2; i++)
            {
                Console.SetCursorPosition(_player.paddle.prevPosition.Item1, _player.paddle.prevPosition.Item2+i);
                Console.Write(" ");
                
            }
            //pong.ClearPaddle(_player.paddle.prevPosition);
            _player.position = coord;
            for (int i = 0; i < 3; i++)
            {
                _player.paddle.positions[i] = new Tuple<int, int>(_player.position.Item1, _player.position.Item2 + (i - 1));
            }
            pong.DrawPlayers();
        }
        public void StartChatting()
        {
            BEGIN:
            chatting = true;
            listening = false;
            Console.SetCursorPosition(6, 27);
            Console.CursorVisible = true;
            string chatMessage = Console.ReadLine();
            goto BEGIN;
        }

        public void StopChatting()
        {
            
        }
    }
}