using UnityEngine;
using System;
using System.IO;

#if WINDOWS_UWP

#else
using System.Threading;
using System.Net;
using System.Net.Sockets;
#endif

public class Capturing : MonoBehaviour {
    static Capturing capturing = null;
    const int PORT = 8888;
    const int BUFFER_LEN = 50000;

    private byte[] buffer;
    private byte[][] images;

    static public bool getFrame(int id, out Texture2D texture) {
        if (capturing.images[id] != null) {
            texture = new Texture2D(640, 480, TextureFormat.RGB24, false, false);
            texture.LoadImage(capturing.images[id]);
            return true;
        } else {
            texture = null;
            return false;
        }
    }

    void Awake() {
        if (capturing == null) {
            capturing = this;
        }

        buffer = new byte[BUFFER_LEN];
        images = new byte[10][];

        startServer();
    }

    void OnApplicationQuit() {
        endServer();
    }

#if WINDOWS_UWP

#else
    private Thread mainThread;

    private void startServer() {
        string ipAddress = Network.player.ipAddress;
        mainThread = new Thread(() => serverThread(ipAddress));
        mainThread.Start();
    }

    private void endServer() {
        mainThread = null;
    }

    private void serverThread(string ipAddress) {
        TcpListener listener = new TcpListener(IPAddress.Parse(ipAddress), PORT);
        listener.Start();
        while (mainThread != null) {
            if (listener.Pending()) {
                TcpClient client = listener.AcceptTcpClient();
                Thread thread = new Thread(() => msgThread(client));
                thread.Start();
            }
            Thread.Sleep(10);
        }
        listener.Stop();
    }

    private void msgThread(TcpClient client) {
        NetworkStream networkStream = client.GetStream();
        client.ReceiveTimeout = 5000;

        while (mainThread != null) {
            int len = networkStream.Read(buffer, 0, buffer.Length);
            if (len == 0) {
                break;
            }
            images[0] = new byte[len];
            Array.Copy(buffer, images[0], len);
        }

        client.Close();
    }
#endif
}
