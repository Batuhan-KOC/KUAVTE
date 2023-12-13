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
    public float simulation_time;
}

// AILERON -----> ROLL -----> Z -----> PHI

// ELEVATOR -----> PITCH -----> X -----> THETA

// RUDDER -----> YAW -----> Y -----> PSI

public class FdmController : MonoBehaviour
{
    UdpClient udpClient;
    IPEndPoint senderEndpoint;

    public SimulationController simulationController;

    public Transform DronePosition;
    public Transform DroneRPY;
    private float interpolationFactor = 0.1f;
    // ------------ ROTATION CALCULATION ALTERNATIVE 1 ------------
    //private float interpolationFactorRotation = 5.0f;
    // ------------ ROTATION CALCULATION ALTERNATIVE 2 ------------
    private float smoothingFactor = 2.5f;

    private byte[] data;
    private PositionData receivedData;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient(12345);
        senderEndpoint = new IPEndPoint(IPAddress.Any, 0);
    }

    void Update()
    {
        if(udpClient.Available > 0){
            data = udpClient.Receive(ref senderEndpoint);
            if(data.Length > 0){
                simulationController.fdmReady = true;
                
                receivedData = ByteArrayToStructure<PositionData>(data);

                // For a smooth movement
                Vector3 newPosition = new Vector3(receivedData.x, receivedData.z, receivedData.y);
                DronePosition.localPosition = Vector3.Lerp(DronePosition.localPosition, newPosition, interpolationFactor);
                

                // ------------ ROTATION CALCULATION ALTERNATIVE 1 ------------
                /*
                Vector3 newRotation = new Vector3(-receivedData.theta, receivedData.psi, -receivedData.phi); // Alternative 1
                DroneRPY.localEulerAngles = Vector3.Lerp(DroneRPY.localEulerAngles, newRotation, interpolationFactorRotation); // Alternative 1
                */

                // ------------ ROTATION CALCULATION ALTERNATIVE 2 ------------
                Vector3 targetRotation = new Vector3(-receivedData.theta, receivedData.psi, -receivedData.phi);

                // Exponential smoothing
                Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
                DroneRPY.rotation = Quaternion.Slerp(
                    DroneRPY.rotation,
                    targetQuaternion,
                    1 - Mathf.Exp(-smoothingFactor * Time.deltaTime)
                );
            }
        }
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
