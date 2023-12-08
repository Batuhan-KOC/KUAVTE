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

public struct JumperTPROInputs{
    public float throttle;
    public float roll;
    public float pitch;
    public float yaw;
}

public class SimulationController : MonoBehaviour
{
    static IntPtr fdmModelLibrary;

    static IntPtr instance = IntPtr.Zero;

    delegate IntPtr FDMCreate();
    delegate void FDMDelete(IntPtr model);
    delegate void FDMStartEnvironment(IntPtr model, float deltaTime, bool realTime);
    delegate void FDMStopEnvironment(IntPtr model);
    delegate void FDMSetTRPY(IntPtr model, float throttle, float roll, float pitch, float yaw);

    JumperTPRO control;
    JumperTPROInputs controlInputs;

    void Awake()
    {
        if (fdmModelLibrary != IntPtr.Zero) return;
 
        fdmModelLibrary = Native.LoadLibrary("Assets/kuavte-fdm-model/KuavteFdmModel.dll");
        if (fdmModelLibrary == IntPtr.Zero)
        {
            Debug.LogError("Failed to load FDM Model library");
        }

        control = new JumperTPRO();

        control.JumperTPROAction.Throttle.performed += JumperTPROThrottleChanged;
        control.JumperTPROAction.Yaw.performed += JumperTPROYawChanged;
        control.JumperTPROAction.Pitch.performed += JumperTPROPitchChanged;
        control.JumperTPROAction.Roll.performed += JumperTPRORollChanged;
    }

    void OnEnable(){
        control.JumperTPROAction.Enable();
    }

    void OnDisable(){
        control.JumperTPROAction.Disable();
    }

    void JumperTPROThrottleChanged(InputAction.CallbackContext context){
        controlInputs.throttle = context.ReadValue<float>();
        JumperTPROChanged();
    }

    void JumperTPROYawChanged(InputAction.CallbackContext context){
        controlInputs.yaw = context.ReadValue<float>();
        JumperTPROChanged();
    }

    void JumperTPROPitchChanged(InputAction.CallbackContext context){
        controlInputs.pitch = context.ReadValue<float>();
        JumperTPROChanged();
    }

    void JumperTPRORollChanged(InputAction.CallbackContext context){
        controlInputs.roll = context.ReadValue<float>();
        JumperTPROChanged();
    }   

    void JumperTPROChanged(){
        if (instance != IntPtr.Zero){
            Native.Invoke<FDMSetTRPY>(fdmModelLibrary, instance, controlInputs.throttle, controlInputs.roll, controlInputs.pitch, controlInputs.yaw);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = Native.Invoke<IntPtr, FDMCreate>(fdmModelLibrary);

        Native.Invoke<FDMStartEnvironment>(fdmModelLibrary, instance, (float)0, false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnApplicationQuit()
    {
        Native.Invoke<FDMStopEnvironment>(fdmModelLibrary, instance);
        Native.Invoke<FDMDelete>(fdmModelLibrary, instance);
        instance = IntPtr.Zero;

        if (fdmModelLibrary == IntPtr.Zero) return;
 
        Debug.Log(Native.FreeLibrary(fdmModelLibrary)
                      ? "FDM Model library successfully unloaded."
                      : "FDM Model library could not be unloaded.");
    }
}
