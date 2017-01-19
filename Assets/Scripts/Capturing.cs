using UnityEngine;
using System;
using System.IO;
using System.Text;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
#else
using System.Threading;
using System.Net;
using System.Net.Sockets;
#endif

public class Capturing : MonoBehaviour {
    const int PORT = 8888;
    const int BUFFER_LEN = 100000;

    static byte[] buffer;
    static byte[][] images;

    static public bool getFrame(int id, out Texture2D texture) {
        if (images[id] != null) {
            texture = new Texture2D(640, 480, TextureFormat.RGB24, false, false);
            texture.LoadImage(images[id]);
            return true;
        } else {
            texture = null;
            return false;
        }
    }

    void Awake() {
        buffer = new byte[BUFFER_LEN];
        images = new byte[10][];
        for (int i = 0; i < 10; i++) {
            images[i] = null;
        }

        startServer();
    }

    void OnApplicationQuit() {
        endServer();
    }

#if WINDOWS_UWP
    private Task mainTask;

    private void startServer() {
        mainTask = new Task(serverThread);
        mainTask.Start();
    }

    private void endServer() {
        mainTask = null;
    }

    private string getIP() {
        foreach (HostName localHostName in NetworkInformation.GetHostNames()) {
            if (localHostName.IPInformation != null) {
                if (localHostName.Type == HostNameType.Ipv4) {
                    return localHostName.ToString();
                }
            }
        }
        return "127.0.0.1";
    }

    private async void serverThread() {
        StreamSocketListener listener = new StreamSocketListener();
        listener.ConnectionReceived += connectionReceived;
        HostName hostName = new HostName(getIP());
        await listener.BindEndpointAsync(hostName, "" + PORT);
    }

    private void connectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args) {
        Stream sr = args.Socket.InputStream.AsStreamForRead();
        Stream sw = args.Socket.OutputStream.AsStreamForWrite();

        while (mainTask != null) {
            try {
                byte[] info = new byte[4];
                sr.Read(info, 0, 4);
                int id = info[0];
                int len = (info[1] << 16) | (info[2] << 8) | info[3];
                int offset = 0;
                int left = len;
                while (mainTask != null && left > 0) {
                    int ret = sr.Read(buffer, offset, left);
                    if (ret > 0) {
                        left -= ret;
                        offset += ret;
                    } else if (ret == 0) {
                        Debug.Log("socket closed");
                    } else {
                        Debug.Log("socket error");
                    }
                }
                images[id] = new byte[len];
                Array.Copy(buffer, images[id], len);
                sw.WriteByte(0);
                sw.Flush();
            } catch {
                break;
            }
        }
    }

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
        Stream sr = new StreamReader(client.GetStream()).BaseStream;
        Stream sw = new StreamWriter(client.GetStream()).BaseStream;

        while (mainThread != null) {
            try {
                byte[] info = new byte[4];
                sr.Read(info, 0, 4);
                int id = info[0];
                int len = (info[1] << 16) | (info[2] << 8) | info[3];
                int offset = 0;
                int left = len;
                while (mainThread != null && left > 0) {
                    int ret = sr.Read(buffer, offset, left);
                    if (ret > 0) {
                        left -= ret;
                        offset += ret;
                    } else if (ret == 0) {
                        Debug.Log("socket closed");
                    } else {
                        Debug.Log("socket error");
                    }
                }
                images[id] = new byte[len];
                Array.Copy(buffer, images[id], len);
                sw.WriteByte(0);
                sw.Flush();
            } catch {
                break;
            }
        }
        
        client.Close();
    }
#endif
}
