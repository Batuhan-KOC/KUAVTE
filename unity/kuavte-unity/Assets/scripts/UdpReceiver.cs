using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct PositionData
{
    public float x, y, z;
    public float phi, theta, psi;
}

// AILERON -----> ROLL -----> Z -----> PHI

// ELEVATOR -----> PITCH -----> X -----> THETA

// RUDDER -----> YAW -----> Y -----> PSI

public class UdpReceiver : MonoBehaviour
{
    UdpClient udpClient;
    IPEndPoint senderEndpoint;

    public Transform DronePosition;
    public Transform DroneRPY;
    private float interpolationFactor = 0.1f;
    private float interpolationFactorRotation = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient(12345);
        senderEndpoint = new IPEndPoint(IPAddress.Any, 0);
    }

    // Update is called once per frame
    void Update()
    {
        byte[] data = udpClient.Receive(ref senderEndpoint);

        PositionData receivedData = ByteArrayToStructure<PositionData>(data);

        //DronePosition.localPosition = new Vector3(receivedData.x, receivedData.z, receivedData.y);

        // For a smooth movement
        Vector3 newPosition = new Vector3(receivedData.x, receivedData.z, receivedData.y);
        Vector3 newRotation = new Vector3(-receivedData.theta, receivedData.psi, -receivedData.phi);

        DronePosition.localPosition = Vector3.Lerp(DronePosition.localPosition, newPosition, interpolationFactor);
        DroneRPY.localEulerAngles = Vector3.Lerp(DroneRPY.localEulerAngles, newRotation, interpolationFactorRotation);
    }

    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }
    }
}
