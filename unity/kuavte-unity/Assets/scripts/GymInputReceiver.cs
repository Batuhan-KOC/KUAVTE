using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GymEnvironmentInput
{
    // 0 : No Initialization Request [1-9] : Initialization Request
    public uint initialize;
    // If initialize is not equal to 0 and windActive is 1 : FDM environment will calculate random wind conditions
    public bool windActive;
    // If initialize is not equal to 0, frequency will be set to environment;
    public float frequency;
    // If initialize is not equal to 0, target will be changing its size in time if targetSizing is true
    public bool targetSizing;

    // Throttle and RPY values send by reinforcement model
    public float throttle;
    public float pitch;
    public float yaw;
    public float roll;
}

public class GymInputReceiver : MonoBehaviour
{
    public SimulationController simulationController;

    UdpClient udpClient;
    IPEndPoint senderEndpoint;

    uint p_initialize;

    private byte[] data;
    private GymEnvironmentInput receivedData;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient(12346);
        senderEndpoint = new IPEndPoint(IPAddress.Any, 0);
        p_initialize = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(udpClient.Available > 0){
            data = udpClient.Receive(ref senderEndpoint);

            if(data.Length > 0){
                receivedData = ByteArrayToStructure<GymEnvironmentInput>(data);

                if(p_initialize == 0 && receivedData.initialize > 0){
                    UnityEngine.Debug.Log("Initialization Requested");
                    simulationController.InitializeRequested(receivedData.initialize, receivedData.frequency, receivedData.windActive, receivedData.targetSizing);
                }
                else{
                    if(simulationController.fdmReady){
                        simulationController.SetFdmTrypValues(receivedData.throttle, 
                                                            receivedData.roll,
                                                            receivedData.pitch,
                                                            receivedData.yaw);
                    }
                }

                p_initialize = receivedData.initialize;
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
