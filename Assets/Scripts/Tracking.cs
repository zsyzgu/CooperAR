using UnityEngine;
using System.IO;

#if WINDOWS_UWP
using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
#else
using System.Threading;
using System.Net.Sockets;
#endif

public class Tracking : MonoBehaviour {
    //const string SERVER_IP = "192.168.1.167";
    const string SERVER_IP = "192.168.1.129";
    const int PORT = 8520;

    public class TrackingFrame {
        public bool[] exist;
        public Vector3[] pos;
        public Vector3[] rot;

        public TrackingFrame() {
            exist = new bool[10];
            pos = new Vector3[10];
            rot = new Vector3[10];
        }
    }

    static TrackingFrame currFrame = new TrackingFrame();
    static TrackingFrame frame;

    static public bool getTransform(int id, out Vector3 position, out Vector3 rotation) {
        if (0 <= id && id < 10 && currFrame.exist[id]) {
            position = currFrame.pos[id];
            rotation = currFrame.rot[id];
            return true;
        } else {
            position = Vector3.zero;
            rotation = Vector3.zero;
            return false;
        }
    }

    private bool recvMessage(string msg) {
        if (msg == null || msg == "exit") {
            return false;
        }
        if (msg == "begin") {
            frame = new TrackingFrame();
        }
        if (msg == "end") {
            currFrame = frame;
        }
        if (msg.Split(' ')[0] == "rb") {
            string[] tags = msg.Split(' ');
            int id = int.Parse(tags[1]);
            float x = float.Parse(tags[2]);
            float y = float.Parse(tags[3]);
            float z = float.Parse(tags[4]);
            float rx = float.Parse(tags[5]);
            float ry = float.Parse(tags[6]);
            float rz = float.Parse(tags[7]);
            frame.exist[id] = true;
            frame.pos[id] = new Vector3(x, y, z);
            frame.rot[id] = new Vector3(rx, ry, rz);
        }
        return true;
    }

#if WINDOWS_UWP
    private Task mainTask;

    void Awake() {
        mainTask = new Task(clientThread);
        mainTask.Start();
    }

    void OnApplicationQuit() {
        mainTask = null;
    }

    private async void clientThread() {
        StreamSocket socket = new StreamSocket();
        await socket.ConnectAsync(new HostName(SERVER_IP), "" + PORT);
        Stream stream = socket.InputStream.AsStreamForRead();
        StreamReader sr = new StreamReader(stream);
        
        while (mainTask != null) {
            string msg = await sr.ReadLineAsync();
            if (!recvMessage(msg)) {
                break;
            }
        }
    }
#else
    private Thread mainThread;

    void Awake() {
        mainThread = new Thread(clientThread);
        mainThread.Start();
    }

    void OnApplicationQuit() {
        mainThread = null;
    }

    private void clientThread() {
        TcpClient client = new TcpClient();
        client.Connect(SERVER_IP, PORT);
        
        StreamReader sr = new StreamReader(client.GetStream());
        
        while (mainThread != null) {
            string msg = sr.ReadLine();
            if (!recvMessage(msg)) {
                break;
            }
        }
        
        client.Close();
    }
#endif
}
