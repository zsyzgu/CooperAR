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
using System.Net.Sockets;
#endif

public class Capturing : MonoBehaviour {
    const string SERVER_IP = "192.168.1.129";
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
        mainTask = new Task(clientThread);
        mainTask.Start();
    }

    private void endServer() {
        mainTask = null;
    }

    private async void clientThread() {
        StreamSocket socket = new StreamSocket();
        await socket.ConnectAsync(new HostName(SERVER_IP), "" + PORT);
        Stream sr = socket.InputStream.AsStreamForRead();
        Stream sw = socket.OutputStream.AsStreamForWrite();

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
        mainThread = new Thread(clientThread);
        mainThread.Start();
    }

    private void endServer() {
        mainThread = null;
    }

    private void clientThread() {
        TcpClient client = new TcpClient();
        client.Connect(SERVER_IP, PORT);

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
                    }
                }
                images[id] = new byte[len];
                Array.Copy(buffer, images[id], len);
                sw.WriteByte(0);
                sw.Flush();
            }
            catch {
                break;
            }
        }

        client.Close();
    }
#endif
}
