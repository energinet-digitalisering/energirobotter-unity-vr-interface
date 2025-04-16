using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;

public class ZeroMQTrackingPublisher : MonoBehaviour
{
    public string ipAddress = "localhost";
    public int port = 5557;
    public Transform headTransform;
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    private PublisherSocket pubSocket;
    private Thread sendThread;
    private bool running = true;

    private Vector3 headPos, leftHandPos, rightHandPos;
    private Quaternion headRot, leftHandRot, rightHandRot;
    private readonly object dataLock = new object();  // Lock for thread safety

    void Start()
    {
        // Start the thread to publbish tracking data
        sendThread = new Thread(PublishTrackingData);
        sendThread.Start();
    }

    Vector3 Left2RightPos(Vector3 pos_left)
    {
        // Flip axis to convert left handed frame to right handed
        Vector3 pos_right = new Vector3(pos_left.x, pos_left.y, -pos_left.z);
        return pos_right;
    }

    Quaternion Left2RightRotation(Quaternion rot_quat)
    {
        return new Quaternion(-rot_quat.x, -rot_quat.y, rot_quat.z, rot_quat.w);
    }


    void PublishTrackingData()
    {
        Debug.Log($"IP Address: {ipAddress}:{port}");

        AsyncIO.ForceDotNet.Force();
        pubSocket = new PublisherSocket();

        try
        {
            pubSocket.Bind($"tcp://{ipAddress}:{port}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to bind socket: {e}");
            return;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long nextSendTime = stopwatch.ElapsedMilliseconds;

        // Loop
        while (running)
        {
            long now = stopwatch.ElapsedMilliseconds;

            if (now >= nextSendTime)
            {
                TrackingData data;
                lock (dataLock)  // Copy thread-safe data
                {
                    data = new TrackingData
                    {
                        headPos = Left2RightPos(headPos),
                        headRot = Left2RightRotation(headRot),
                        leftHandPos = Left2RightPos(leftHandPos),
                        leftHandRot = Left2RightRotation(leftHandRot),
                        rightHandPos = Left2RightPos(rightHandPos),
                        rightHandRot = Left2RightRotation(rightHandRot)
                    };
                }

                string jsonData = JsonUtility.ToJson(data);
                pubSocket.SendFrame(jsonData);

                Debug.Log($"data: {jsonData}");

                nextSendTime += 20;  // Schedule next send ~20ms later (50 Hz)
            }
            else
            {
                Thread.Sleep(1);  // Yield CPU briefly
            }

            pubSocket.Close();
        }
    }

    void Update()
    {
        lock (dataLock)
        {
            if (headTransform && leftHandTransform && rightHandTransform)
            {
                headPos = headTransform.position;
                headRot = headTransform.rotation;
                leftHandPos = leftHandTransform.position;
                leftHandRot = leftHandTransform.rotation;
                rightHandPos = rightHandTransform.position;
                rightHandRot = rightHandTransform.rotation;
            }
        }
    }

    void OnDestroy()
    {
        running = false;
        sendThread.Join();
        pubSocket?.Close();
        NetMQConfig.Cleanup(false);
    }

    [System.Serializable]
    private class TrackingData
    {
        public Vector3 headPos;
        public Quaternion headRot;
        public Vector3 leftHandPos;
        public Quaternion leftHandRot;
        public Vector3 rightHandPos;
        public Quaternion rightHandRot;
    }
}
