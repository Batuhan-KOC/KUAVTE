using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct GymEnvironmentInput
{
    /* Name : Restart
     *
     * Description :
     * If gym environment wants to restart the environment from beginning it shall be set to the
     * given values. 
     *
     * Values :
     * 0 : Gym environment do not want to restart
     * 1 : Not moving target - constant size
     * 2 : Constant moving target - constant size
     * 3 : Circular moving target - constant size
     * 4 : Randomly moving target - constant size
     * 5 : Not moving target - varying size
     * 6 : Constant moving target - varying size
     * 7 : Circular moving target - varying size
     * 8 : Randomly moving target - varying size
    */
    public uint restart;

    /* Name : Throttle
     *
     * Description :
     * Throttle value send by gym environment
     *
     * Range : 
     * [0.0, 1.0]
     *
     * Notes :
     * If data is not send in given range it will be clipped to the nearest edge value
    */
    public float throttle;

    /* Name : Pitch
     *
     * Description :
     * Pitch value send by gym environment
     *
     * Range : 
     * [-1.0, 1.0]
     *
     * Notes :
     * If data is not send in given range it will be clipped to the nearest edge value
    */
    public float pitch;

    /* Name : Yaw
     *
     * Description :
     * Yaw value send by gym environment
     *
     * Range : 
     * [-1.0, 1.0]
     *
     * Notes :
     * If data is not send in given range it will be clipped to the nearest edge value
    */
    public float yaw;

    /* Name : Roll
     *
     * Description :
     * Roll value send by gym environment
     *
     * Range : 
     * [-1.0, 1.0]
     *
     * Notes :
     * If data is not send in given range it will be clipped to the nearest edge value
    */
    public float roll;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct FDMEnvironmentInput{
    // Position data
    public float x;
    public float y;
    public float z;

    // Attitude data
    public float phi;
    public float psi;
    public float theta;
}

public class SimulationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
