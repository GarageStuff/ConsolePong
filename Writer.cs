using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public static class ConsoleWriter
    {
        class messageObject
        {
            public Tuple<int, int> position;
            public object message;
        }
        static Queue<messageObject> messageQueue = new Queue<messageObject>();
        static Queue<Tuple<int, int>> blankQueue = new Queue<Tuple<int, int>>();
        public static Tuple<int, int> chatStart = new Tuple<int, int>(7, 27);
        public static int chatStartX = 2, chatStartY = 22;
        public static List<string> chats = new List<string>();
        public static string incompleteChat = "";
        static readonly object lockObject = new object();
        public static bool writing = false;
        public static async Task AniWrite(int speed, string text, Tuple<int, int> position)
        {
            for (int i = 0; i < text.Length; i++)
            {
                await Task.Delay(speed);
                ConsoleWriter.Write(position.Item1 + i, position.Item2, text[i]);
            }
        }
        public static void AniWriteReverse(int speed, string text, Tuple<int, int> position)
        {
            int length = text.Length;
            for (int i = text.Length - 1; i > 0; i--)
            {
                Thread.Sleep(speed);
                ConsoleWriter.Write(position.Item1 + i, position.Item2, text[i]);
            }
        }
        public static void Write(int x, int y, object _message)
        {
            messageObject message = new messageObject();
            message.position = new Tuple<int, int>(x, y);
            message.message = _message;
            lock (lockObject)
            {
                messageQueue.Enqueue(message);
                WriteQueue();
            }
        }
        static void WriteQueue()
        {
            while (true)
            {
                lock (lockObject)
                    if (messageQueue.Count == 0)
                    {
                        break;
                    }
                if (blankQueue.Count > 0)
                {
                    Tuple<int, int> position = blankQueue.Dequeue();
                    Console.SetCursorPosition(position.Item1, position.Item2);
                    Console.Write(" ");
                }
                if (messageQueue.Count > 0)
                {
                    messageObject messageObject = messageQueue.Dequeue();
                    if (messageObject != null)
                    {
                        Console.SetCursorPosition(messageObject.position.Item1, messageObject.position.Item2);
                        Console.Write(messageObject.message);
                    }
                }
            }
        }
        public static void Clear(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(' ');
        }
        public static async Task Blank(int x, int y)
        {
            blankQueue.Enqueue(new Tuple<int, int>(x, y));
        }
        public static async Task WriteChat(string message)
        {
            chats.Add(message);
            if (chats.Count == 5)
            {
                chats.Remove(chats[0]);
            }
            for (int i = 0; i < chats.Count; i++)
            {
                int length = chats[i].Length;
                int start = chatStartX + length - 1;
                int dif = 60 - start;
                for (int j = start; j < dif + length + 1; j++)
                {
                    Write(start + j - chats[i].Length, chatStart.Item2 - 5 + i, " ");
                }
                Write(chatStart.Item1 - 4, chatStart.Item2 - 5 + i, chats[i]);
            }
        }

        public static void WriteError(string message)
        {
            Console.SetCursorPosition(1, 30);
            Console.WriteLine("                                                            ");
            Console.WriteLine("ERROR:"+ message);
        }
    }
}
