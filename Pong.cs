using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    class Ball
    {
        public Tuple<int, int> position;
        public Tuple<int, int> prevPosition;
        public Tuple<int, int> velocity;
        public Ball(int x, int y, int vx, int vy)
        {
            position = new Tuple<int, int>(x, y);
            prevPosition = new Tuple<int, int>(x, y);
            velocity = new Tuple<int, int>(vx, vy);
        }
    }
    public class Paddle
    {
        public Tuple<int, int> position;
        public Tuple<int, int> prevPosition;
        public Tuple<int, int>[] positions = new Tuple<int, int>[3];
        public Paddle() 
        {
            
        } 
        
    }
    internal class Pong
    {
        public Player player1 = new Player();
        Paddle p1Paddle = new Paddle();
        
        public Player player2 = new Player();
        Paddle p2Paddle = new Paddle();
        int[,] layout;
        public char paddleChar = '|';
        //public char p2Paddle = '[';
        public char ballChar = 'O';
        char horizontalBorder = '-';
        char verticalBorderLeft = '(';
        char verticalBorderRight = ')';
        public int center;
        int xOffset = 2;
        int yOffset = 3;
        public GameManager gameManager;
        public Ball ball;
        public void BuildBoard(int x, int y)
        {
            layout = new int[x, y];
            AnimateBoard();
            PlacePlayers();
            PlaceBall();
        }
        public void PlaceBall()
        {
            int startX = (layout.GetLength(0) / 2)+xOffset;
            int startY = (layout.GetLength(1) / 2) + yOffset;
            int randVelX;
            int randVelY;
            Random random= new Random();
            if (random.Next(0, 2) == 0)
            {
                randVelX = -1;
            }
            else
            {
                randVelX= 1;
            }
            randVelY = random.Next(-1, 2);
            if (ball != null)
            {
                ball.velocity = new Tuple<int, int>(randVelX, randVelY);               
            }
            else 
            {
                ball = new Ball(startX, startY, randVelX, randVelY);
            }
            ConsoleWriter.Write(ball.position.Item1, ball.position.Item2, ballChar);
            ball.prevPosition = ball.position;
        }

        public void PlacePlayers()
        {
            player1.paddle = p1Paddle;
            player2.paddle = p2Paddle;
            gameManager.players.Add(player1);
            gameManager.players.Add(player2);
            player1.Id = 0;player2.Id = 1;
            if (gameManager.currentRole == GameManager.Role.Client)
            {
                gameManager.controller.player = player2;
            }
            if (gameManager.currentRole == GameManager.Role.Server)
            {
                gameManager.controller.player = player1;
            }
            player1.position= new Tuple<int, int>(3, (layout.GetLength(1) / 2)+yOffset);
            player2.position = new Tuple<int, int>(layout.GetLength(0)-2, (layout.GetLength(1) / 2)+yOffset);
            foreach (Player player in gameManager.players)
            {
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        player.paddle.positions[i] = new Tuple<int, int>(player.position.Item1, player.position.Item2 + (i-1));
                    }
                }
                catch (Exception ex)
                {
                    Console.SetCursorPosition(1, 25);
                    Console.WriteLine(ex.ToString());
                }               
            }            
            DrawPaddle(player1.position);
            DrawPaddle(player2.position);      
        }
        void DrawBoard(int x, int y)
        {
            for (int i = 0; i < layout.GetLength(0); i++)
            {
                for (int j = 0; j < layout.GetLength(1); j++)
                {
                    if (i == 0)
                    {
                        Console.SetCursorPosition(i, j);
                        Console.Write(verticalBorderLeft.ToString());
                    }
                    if (i == layout.GetLength(0)-1)
                    {
                        Console.SetCursorPosition(i, j);
                        Console.Write(verticalBorderRight.ToString());
                    }
                    if (j == 0 || j == layout.GetLength(1)-1)
                    {
                        Console.SetCursorPosition(i, j);
                        Console.Write(horizontalBorder.ToString());
                    }
                    Thread.Sleep(2);
                }
            }
        }

        void AnimateBoard()
        {
            int startx = 1;
            int starty = 1;
            ConsoleWriter.AniWrite(5, "============================================================", new Tuple<int, int>(startx, starty));
            for (int i = 0; i < Menus.height + 1+7; i++)
            {
                if (starty + i < 7 || starty + i > 17)
                {
                    ConsoleWriter.Write(startx + Menus.width, starty + i, ")");
                    Thread.Sleep(10);
                }               
            }
            ConsoleWriter.AniWriteReverse(5, "============================================================", new Tuple<int, int>(startx, starty+Menus.height));
            for (int i = Menus.height+7; i >-1; i--)
            {
                if (starty + i < 7 || starty + i > 17)
                {
                    ConsoleWriter.Write(startx, starty + i, "(");
                    Thread.Sleep(10);
                }
                
            }            
            center = Menus.width / 2;
            for (int i=1;i<Menus.height;i++)
            {
                ConsoleWriter.Write(center, starty + i, "|");
            }
            ConsoleWriter.AniWrite(5, "===========================================================", new Tuple<int, int>(startx+1, 3));
            ConsoleWriter.AniWrite(5, "===========================================================", new Tuple<int, int>(startx + 1, 26));
            ConsoleWriter.AniWrite(5, "===========================================================", new Tuple<int, int>(startx+1, 28));
            ConsoleWriter.AniWrite(5, "Say:", new Tuple<int, int>(startx + 2, 27));           
            DrawNames();
            ConsoleWriter.Write(27,2,gameManager.player1Score.ToString());
            ConsoleWriter.Write(33,2,gameManager.player2Score.ToString());
            Menus.DrawGameMenu();
        }
        void DrawNames()
        {
            ConsoleWriter.AniWrite(10, "Player 1", new Tuple<int, int>(3, 2));
            ConsoleWriter.AniWrite(10, "Player 2", new Tuple<int, int>(Menus.width-8, 2));
        }
        public void UpdateBoard() 
        {
            DrawBall();
        }

        public void UpdateBall()
        {
            ball.prevPosition = ball.position;
            int newX = ball.position.Item1 + ball.velocity.Item1;
            int newY = ball.position.Item2 + ball.velocity.Item2;
            if (newX >63 )
            {
                if (gameManager.scored)
                {
                    return;
                }
                Score(1);
            }
            if (newX < 1)
            {
                if (gameManager.scored)
                {
                    return;
                }
                Score(2);
            }
            Tuple<int, int> coord = new Tuple<int, int>(newX, newY);
            foreach (var player in gameManager.players)
            {
                if (Array.Exists(player.paddle.positions, pos => pos.Equals(coord)))
                {
                    int index = Array.FindIndex(player.paddle.positions, pos => pos.Equals(coord));
                    switch (index)
                    {
                        case 0: // Hit top
                            if (ball.velocity.Item1 < 0)
                            {
                                ball.velocity = Tuple.Create(1, -1);
                            }
                            else if (ball.velocity.Item1 > 0)
                            {
                                ball.velocity = Tuple.Create(-1, -1);
                            }
                            break;

                        case 1: // Hit center
                            if (ball.velocity.Item1 < 0)
                            {
                                ball.velocity = Tuple.Create(1, ball.velocity.Item2);
                            }
                            else if (ball.velocity.Item1 > 0)
                            {
                                ball.velocity = Tuple.Create(-1, ball.velocity.Item2);
                            }
                            break;

                        case 2: // Hit bottom
                            if (ball.velocity.Item1 < 0)
                            {
                                ball.velocity = Tuple.Create(1, 1);
                            }
                            else if (ball.velocity.Item1 > 0)
                            {
                                ball.velocity = Tuple.Create(-1, 1);
                            }
                            break;

                        default:
                            break;
                    }
                    newX = ball.position.Item1 + ball.velocity.Item1;
                    newY = ball.position.Item2 + ball.velocity.Item2;
                    break;
                }
            }
            if (newX <= 0+xOffset-1 || newX >= layout.GetLength(0) - 1+xOffset)
            {
                if (newY < 7 || newY > 17)
                {
                    ball.velocity = new Tuple<int, int>(-ball.velocity.Item1, ball.velocity.Item2);
                    newX = ball.position.Item1 + ball.velocity.Item1;
                }                
            }
            if (newY <= 0+yOffset-1 || newY >= layout.GetLength(1) - 1+yOffset)
            {
                ball.velocity = new Tuple<int, int>(ball.velocity.Item1, -ball.velocity.Item2);
                newY = ball.position.Item2 + ball.velocity.Item2;
            }            
            ball.position = new Tuple<int, int>(newX, newY);
        }
        public void ReceiveBallPositon(int x, int y)
        {
            Console.CursorVisible = false;
            ball.prevPosition = ball.position;
            ball.position = new Tuple<int, int>(x, y);
            DrawBall();
;        }

        public void DrawBall()
        {
                ConsoleWriter.Blank(ball.prevPosition.Item1, ball.prevPosition.Item2);
                ConsoleWriter.Write(ball.position.Item1, ball.position.Item2, ballChar);
                if (ball.prevPosition.Item1 == center)
                {
                    ConsoleWriter.Write(ball.prevPosition.Item1, ball.prevPosition.Item2, "|");
                }
                if (ball.prevPosition.Item2 == yOffset || ball.prevPosition.Item2 == layout.GetLength(1) + 1)
                {
                ConsoleWriter.Write(ball.prevPosition.Item1, ball.prevPosition.Item2, "=");
                }
            if (gameManager.controller.chatting)
            {
                //Console.SetCursorPosition(ConsoleWriter.chatStart.Item1 + gameManager.controller.mess - 1, ConsoleWriter.chatStart.Item2);
            }
        }
        public void DrawPlayers()
        {
            Console.CursorVisible = false;
            foreach (Player player in gameManager.players)
            {
                DrawPlayerPaddle(player);               
            }
        }
        void DrawPlayerPaddle(Player player)
        {
            for (int i = 0; i < player.paddle.positions.Length; i++)
            {
                ConsoleWriter.Write(player.paddle.positions[i].Item1, player.paddle.positions[i].Item2, paddleChar);
            }
        }
        void DrawPaddle(Tuple<int,int> position)
        {
            for (int i = -1;i<2;i++)
            {
                ConsoleWriter.Write(position.Item1, position.Item2 + i, paddleChar);
            }
        }
        public void ClearPaddle(Tuple<int,int> position)
        {
            for (int i=-1;i<=1;i++)
            {
                ConsoleWriter.Blank(position.Item1, position.Item2+i);
            }
        }
        public void Score(int playerID)
        {
            
            string player = "";
            if (gameManager.gameOver || gameManager.scored)
            {
                return;
            }
            gameManager.scored = true;
            ball.velocity = new Tuple<int, int>(0, 0);
            if (playerID == 1)
            {
                player = "Player 1 ";
                gameManager.player1Score += 1;
            }
            if (playerID == 2)
            {
                player = "Player 2 ";
                gameManager.player2Score += 1;
            }
            gameManager.UpdateScore();
            gameManager.controller.listening = false;

            Menus.OpenMatchOver(player);
            
            LOOP:
            
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    Rematch();
                goto EXIT;
                }
            goto LOOP;
        EXIT:;
                //return;
                
                
        }
        
        public void Rematch()
        {
            ConsoleWriter.AniWrite(5, "                   ", new Tuple<int, int>(22, 15));
            ConsoleWriter.AniWrite(5, "                                        ", new Tuple<int, int>(20, 16));
            gameManager.scored = false;
            gameManager.gameOver = false;
            gameManager.controller.Listen();
            gameManager.controller.chatting = false;
            gameManager.controller.menuOpen = false;
            
            PlaceBall();
            
        }
    }
    
}
