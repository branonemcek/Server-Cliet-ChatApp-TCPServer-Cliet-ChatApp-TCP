using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        TcpClient client = new TcpClient("127.0.0.1", 8888);

        Console.WriteLine("Enter your name:");
        string clientName = Console.ReadLine();
        await SendMessageAsync(clientName, client);

        _ = ReceiveMessagesAsync(client); //discard _ pouzitie ked navratova hodnota neni priradena ziadna hodnota??

        // Ak je na vtupe hodnota spusti sa metoda SendMessageAsync
        while (true) {
            string message = Console.ReadLine();
            await SendMessageAsync(message, client);
        }
    }

    static async Task ReceiveMessagesAsync(TcpClient tcpClient) //ReceiveMessagesAsync metoda pre prijímanie správ
    {
        NetworkStream clientStream = tcpClient.GetStream();

        byte[] message = new byte[4096];
        int bytesRead;

        while (true) { bytesRead = 0;

            try
            {
                bytesRead = await clientStream.ReadAsync(message, 0, 4096);
            }
            catch
            {
                break;
            }

            if (bytesRead == 0)
                break;

            string serverMessage = Encoding.ASCII.GetString(message, 0, bytesRead);
            Console.WriteLine(serverMessage);
        }

        tcpClient.Close();
    }

    static async Task SendMessageAsync(string message, TcpClient client) //Metoda na odosielanie sprav cez TCP 
    {
        NetworkStream clientStream = client.GetStream();
        byte[] data = Encoding.ASCII.GetBytes(message);
        await clientStream.WriteAsync(data, 0, data.Length);
    }
}
