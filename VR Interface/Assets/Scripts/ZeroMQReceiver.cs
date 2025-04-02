using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System;
using System.IO;

public class ZeroMQReceiver : MonoBehaviour
{
    public string ipAddress = "localhost";  // Default IP
    public int port = 5555;  // Default port 

    private SubscriberSocket subSocket;
    private Thread receiveThread;
    private Texture2D receivedTexture;
    private Renderer planeRenderer;
    private byte[] latestFrame;  // Store the latest frame
    private bool running = true;

    void Start()
    {
        // Create a Texture2D for received frames
        receivedTexture = new Texture2D(640, 480);

        // Get the Renderer component from the GameObject this script is attached to
        planeRenderer = GetComponent<Renderer>();

        // Start the thread to receive images
        receiveThread = new Thread(ReceiveFrames);
        receiveThread.Start();
    }

    void ReceiveFrames()
    {
        AsyncIO.ForceDotNet.Force();
        subSocket = new SubscriberSocket();
        subSocket.Connect($"tcp://{ipAddress}:{port}");
        subSocket.Subscribe("");  // Subscribe to all messages

        while (running)
        {
            latestFrame = subSocket.ReceiveFrameBytes();  // Store received frame
        }
    }

    void Update()
    {
        if (latestFrame != null)
        {
            receivedTexture.LoadImage(latestFrame);
            planeRenderer.material.mainTexture = receivedTexture;
            latestFrame = null;  // Reset after updating
        }
    }

    void OnDestroy()
    {
        running = false;
        receiveThread.Abort();
        subSocket.Close();
        NetMQConfig.Cleanup(false);
    }
}
