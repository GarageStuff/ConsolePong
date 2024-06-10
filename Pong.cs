using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Media;

namespace ConsoleGame
{
    public class Ball
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
    public static class Pong
    {
        public static Player player1 = new Player();
        static Paddle p1Paddle = new Paddle();
        public static Player player2 = new Player();
        static Paddle p2Paddle = new Paddle();
        public static bool waitingOnPlayerConnect = true;
        static int[,] layout;
        public static char paddleChar = '|';
        public static char leftPaddleChar = '}';
        public static char rightPaddleChar = '{';
        public static char ballChar = '•';
        static char horizontalBorder = '-';
        static char verticalBorderLeft = '(';
        static char verticalBorderRight = ')';
        public static int center;
        static int xOffset = 2;
        static int yOffset = 3;
        public static bool placing = false;
        public static Ball ball;
        static Pong()
        {
            ball = new Ball(0, 0, 0, 0);
        }
        public static void BuildBoard(int x, int y)
        {
            placing = true;
            layout = new int[x, y];
            AnimateBoard();
            PlacePlayers();        
        }
        public static void PlaceBall()
        {
            int startX = (layout.GetLength(0) / 2)+xOffset-2;
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
                ball.position = new Tuple<int, int>(startX, startY);
                ball.velocity = new Tuple<int, int>(randVelX, randVelY);
            }
            else 
            {
                ball = new Ball(startX, startY, randVelX, randVelY);
                ball.position = new Tuple<int, int>(startX, startY);
                ball.velocity = new Tuple<int, int>(randVelX, randVelY);
            }
            ConsoleWriter.Write(ball.position.Item1, ball.position.Item2, ballChar.ToString());
            ball.prevPosition = ball.position;
        }

        public static void PlacementCountDown(int timer)
        {
            while (waitingOnPlayerConnect)
            {
                Thread.Sleep(100);
            }
            ConsoleWriter.Write(9, 13,"                                        ");
            Menus.ReadyPrompt();
            while (!GameManager.playersReady)
            {
                Thread.Sleep(100);
            }
            PlaceBall();
            if (ball != null)
            {
                ConsoleWriter.Write(ball.position.Item1, ball.position.Item2, " ");
            }
            placing = true;
            int x = (layout.GetLength(0) / 2) + xOffset-2;
            int y = (layout.GetLength(1) / 2) + yOffset;
            ConsoleWriter.Write(x,y," ");
            ConsoleWriter.Write(x,y-2," ");
            while (true)
            {
                Thread.Sleep(1000);
                timer -= 1;
                ConsoleWriter.Write(x, y-1, timer.ToString());
                if (timer == 0)
                {
                    placing = false;
                    ConsoleWriter.Write(x, y-1," ");
                    break;
                }
            }
        }

        public static void PlacePlayers()
        {
            player1.paddle = p1Paddle;
            player2.paddle = p2Paddle;
            GameManager.players.Add(player1);
            GameManager.players.Add(player2);
            player1.Id = 0;player2.Id = 1;
            if (GameManager.currentRole == GameManager.Role.Client)
            {
                InputController.player = player2;
            }
            if (GameManager.currentRole == GameManager.Role.Server)
            {
                InputController.player = player1;
            }
            player1.position= new Tuple<int, int>(3, (layout.GetLength(1) / 2)+yOffset);
            player2.position = new Tuple<int, int>(layout.GetLength(0)-1, (layout.GetLength(1) / 2)+yOffset);
            foreach (Player player in GameManager.players)
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
            DrawPlayers();    
        }
        static void DrawBoard(int x, int y)
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

        static void AnimateBoard()
        {
            Console.CursorVisible = false;
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
            ConsoleWriter.AniWrite(5, "===========================================================", new Tuple<int, int>(startx+1, 3));
            ConsoleWriter.AniWrite(5, "===========================================================", new Tuple<int, int>(startx + 1, 26));
            ConsoleWriter.AniWrite(5, "===========================================================", new Tuple<int, int>(startx+1, 28));
            ConsoleWriter.AniWrite(5, "(C) To Chat", new Tuple<int, int>(startx + 2, 27));           
            DrawNames();
            ConsoleWriter.Write(27,2,GameManager.player1Score.ToString());
            ConsoleWriter.Write(33,2,GameManager.player2Score.ToString());
            Menus.DrawGameMenu();
            
        }
        static void DrawNames()
        {
            ConsoleWriter.AniWrite(10, "Player 1", new Tuple<int, int>(3, 2));
            ConsoleWriter.AniWrite(10, "Player 2", new Tuple<int, int>(Menus.width-8, 2));
        }
        public static void UpdateBoard() 
        {
            if (placing)
            {
                return;
            }
            DrawBall();
        }
        public static void UpdateBall()
        {
            if (placing)
            { 
                return; 
            }
            ball.prevPosition = ball.position;
            int newX = ball.position.Item1 + ball.velocity.Item1;
            int newY = ball.position.Item2 + ball.velocity.Item2;
            if (newX >63 )
            {
                if (!GameManager.scored)
                {
                    Score(1);
                    ClientManager.SendDataAsync("sscored" + 1, ClientManager.stream);
                    return;
                }
                return;
            }
            if (newX < 1)
            {
                if (!GameManager.scored)
                {
                    Score(2);
                    ClientManager.SendDataAsync("sscored" + 2, ClientManager.stream);
                    return;
                }
                return;
            }
            Tuple<int, int> coord = new Tuple<int, int>(newX, newY);
            foreach (var player in GameManager.players)
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
                if (newY < 6 || newY > 17)
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
        public static void ReceiveBallPositon(int x, int y)
        {
            Console.CursorVisible = false;
            ball.prevPosition = ball.position;
            ball.position = new Tuple<int, int>(x, y);
            DrawBall();
;        }
        public static void DrawBall()
        {
            if (ball == null)
            { 
                return;
            }
                ConsoleWriter.Blank(ball.prevPosition.Item1, ball.prevPosition.Item2);
                ConsoleWriter.Write(ball.position.Item1, ball.position.Item2, ballChar.ToString());
                if (ball.prevPosition.Item2 == yOffset || ball.prevPosition.Item2 == layout.GetLength(1) + 1)
                {
                ConsoleWriter.Write(ball.prevPosition.Item1, ball.prevPosition.Item2, "=");
                }
            if (InputController.chatting)
            {
                //Console.SetCursorPosition(ConsoleWriter.chatStart.Item1 + GameManager.InputController.mess - 1, ConsoleWriter.chatStart.Item2);
            }
        }
        public static void DrawPlayers()
        {
            Console.CursorVisible = false;
            foreach (Player player in GameManager.players)
            {
                DrawPlayerPaddle(player);               
            }
        }
        static void DrawPlayerPaddle(Player player)
        {
            for (int i = 0; i < player.paddle.positions.Length; i++)
            {
                if (player.Id == 0)
                {
                    ConsoleWriter.Write(player.paddle.positions[i].Item1, player.paddle.positions[i].Item2, leftPaddleChar);
                }
                else if (player.Id == 1)
                {
                    ConsoleWriter.Write(player.paddle.positions[i].Item1, player.paddle.positions[i].Item2, rightPaddleChar);
                }
            }
            
                
        }
        static void DrawPaddle(Tuple<int,int> position)
        {
            for (int i = -1;i<2;i++)
            {
                ConsoleWriter.Write(position.Item1, position.Item2 + i, paddleChar);
            }
        }
        public static void ClearPaddle(Tuple<int,int> position)
        {
            for (int i=-1;i<=1;i++)
            {
                ConsoleWriter.Blank(position.Item1, position.Item2+i);
            }
        }
        public static void Score(int playerID)
        {
            if (GameManager.gameOver)
            {
                return;
            }
            GameManager.playersReady= false;
            //ClientManager.SendDataAsync("sscored"+playerID, ClientManager.stream);
            ball.velocity = new Tuple<int, int>(0, 0);
            string player = "";           
            GameManager.scored = true;            
            if (playerID == 1)
            {
                player = "Player 1 ";
                GameManager.player1Score += 1;
            }
            if (playerID == 2)
            {
                player = "Player 2 ";
                GameManager.player2Score += 1;
            }
            GameManager.UpdateScore();
            InputController.listening = false;
            Menus.OpenMatchOver(player); 
        }       
        public static void Rematch()
         {
            ConsoleWriter.Write(ball.position.Item1, ball.position.Item2, " ");
            ConsoleWriter.Write(ball.prevPosition.Item1, ball.prevPosition.Item2, " ");
            ConsoleWriter.AniWrite(0, "                   ", new Tuple<int, int>(22, 15));
            ConsoleWriter.AniWrite(0, "                                        ", new Tuple<int, int>(9, 16));
            GameManager.Rematch();
                   
        }
    }
    
}
