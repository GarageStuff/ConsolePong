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
        static Queue<Tuple<int,int>> blankQueue = new Queue<Tuple<int,int>>();
        static readonly object lockObject = new object();
        public static bool writing = false;
        public static void AniWrite(int speed, string text, Tuple<int,int> position)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Thread.Sleep(speed);
                ConsoleWriter.Write(position.Item1+i, position.Item2, text[i]);
            }
        }
        public static void AniWriteReverse(int speed, string text, Tuple<int, int> position)
        {
            int length = text.Length;
            for (int i = text.Length-1; i > 0; i--)
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
                    Tuple<int,int> position= blankQueue.Dequeue();
                    Console.SetCursorPosition(position.Item1, position.Item2);
                    Console.Write(" ");
                }
                if (messageQueue.Count > 0)
                {
                    messageObject messageObject = messageQueue.Dequeue();
                    Console.SetCursorPosition(messageObject.position.Item1, messageObject.position.Item2);
                    Console.Write(messageObject.message);
                }
            }
        }
        public static void Clear(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(' ');
        }

        public static void Blank(int x, int y)
        {
            blankQueue.Enqueue(new Tuple<int, int>(x, y));
        }
    }
}
