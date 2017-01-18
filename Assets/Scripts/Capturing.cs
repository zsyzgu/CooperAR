﻿using UnityEngine;
using System;
using System.IO;
using System.Text;

#if WINDOWS_UWP

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
                int len = sr.Read(buffer, 0, buffer.Length);
                if (len == 0) {
                    break;
                }
                images[0] = new byte[len];
                Array.Copy(buffer, images[0], len);
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