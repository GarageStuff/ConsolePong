using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace ConsoleGame
{
    class GameListener
    {       
        
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Menus.MainMenu();
        }
        private static void StartTCPClient()
        {
            
            int port = 13000;
            TcpClient client = new();
            client.Connect("127.0.0.1", 13000);
            while (true)
            {
                
            }
        }       
        private static void HandleClient(TcpClient client)
        {
            byte[] bytes = new byte[256];
            string data = null;

            NetworkStream stream = client.GetStream();

            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine($"Received: {data}");
                data = data.ToUpper();
                byte[] msg = Encoding.ASCII.GetBytes(data);
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine($"Sent: {data}");
            }
            client.Close();
        }
    }
}

