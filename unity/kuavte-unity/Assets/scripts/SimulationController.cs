using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

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
     * 0 : No Initialization
     * 1 : Ground Linear
     * 2 : Ground Circular
     * 3 : Ground Random
     * 4 : Air Linear
     * 5 : Air Circular
     * 6 : Air Random
     * 7 : Escape
     * 8 : Great Circle
    */
    public uint initialize;

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
    static IntPtr fdmModelLibrary;

    delegate IntPtr FDMCreate();
    delegate void FDMDelete(IntPtr model);
    delegate void FDMInit(IntPtr model);
    delegate void FDMDeinit(IntPtr model);
    delegate void FDMSendData(IntPtr model, uint data);

    void Awake()
    {
        if (fdmModelLibrary != IntPtr.Zero) return;
 
        fdmModelLibrary = Native.LoadLibrary("Assets/kuavte-fdm-model/KuavteFdmModel.dll");
        if (fdmModelLibrary == IntPtr.Zero)
        {
            Debug.LogError("Failed to load FDM Model library");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        IntPtr instance = Native.Invoke<IntPtr, FDMCreate>(fdmModelLibrary);
        Native.Invoke<FDMInit>(fdmModelLibrary, instance);
        Native.Invoke<FDMSendData>(fdmModelLibrary, instance, 7);
        Native.Invoke<FDMDeinit>(fdmModelLibrary, instance);
        Native.Invoke<FDMDelete>(fdmModelLibrary, instance);
        instance = IntPtr.Zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationQuit()
    {
        if (fdmModelLibrary == IntPtr.Zero) return;
 
        Debug.Log(Native.FreeLibrary(fdmModelLibrary)
                      ? "FDM Model library successfully unloaded."
                      : "FDM Model library could not be unloaded.");
    }
}
