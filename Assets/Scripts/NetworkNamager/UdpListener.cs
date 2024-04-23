using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UdpListener : MonoBehaviour
{
    public const int listenPort = 12000;

    private static void StartListener()
    {
        UdpClient listener = new UdpClient(listenPort);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for broadcast");
                byte[] bytes = listener.Receive(ref groupEP);

                Console.WriteLine($"Received broadcast from {groupEP} :");
                Console.WriteLine($"Data: {Encoding.UTF8.GetString(bytes, 0, bytes.Length)}");

                // Process the data as needed
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            listener.Close();
        }
    }

    public static void Main()
    {
        Task.Run(() => StartListener());
        Console.WriteLine("Listening on port 12000. Press a key to quit.");
        Console.ReadKey();
    }
}
