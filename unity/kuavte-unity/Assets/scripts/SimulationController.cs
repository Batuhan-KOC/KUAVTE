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
using System.Diagnostics;

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

    public bool fdmReady = false;

    public bool acceptJumperTPROInputs = false;

    public TargetBehavior targetBehavior;

    delegate IntPtr FDMCreate();
    delegate void FDMDelete(IntPtr model);
    delegate void FDMStartEnvironment(IntPtr model, float frequency, bool windActive);
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
            UnityEngine.Debug.LogError("Failed to load FDM Model library");
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
        if(!acceptJumperTPROInputs) return;

        controlInputs.throttle = context.ReadValue<float>();
        JumperTPROChanged();
    }

    void JumperTPROYawChanged(InputAction.CallbackContext context){
        if(!acceptJumperTPROInputs) return;

        controlInputs.yaw = context.ReadValue<float>();
        JumperTPROChanged();
    }

    void JumperTPROPitchChanged(InputAction.CallbackContext context){
        if(!acceptJumperTPROInputs) return;

        controlInputs.pitch = context.ReadValue<float>();
        JumperTPROChanged();
    }

    void JumperTPRORollChanged(InputAction.CallbackContext context){
        if(!acceptJumperTPROInputs) return;
        
        controlInputs.roll = context.ReadValue<float>();
        JumperTPROChanged();
    }   

    void JumperTPROChanged(){
        if (instance != IntPtr.Zero){
            Native.Invoke<FDMSetTRPY>(fdmModelLibrary, instance, controlInputs.throttle, controlInputs.roll, controlInputs.pitch, controlInputs.yaw);
        }
    }

    public void InitializeRequested(uint initValue, float frequency, bool windActive, bool targetSizing){
        fdmReady = false;

        int targetSz = 0;

        if(targetSizing){
            targetSz = 1;
        }

        targetBehavior.Initialize((TargetMovementType)initValue, (TargetSizingType)targetSz);

        if(instance != IntPtr.Zero){
            Native.Invoke<FDMStopEnvironment>(fdmModelLibrary, instance);
            Native.Invoke<FDMDelete>(fdmModelLibrary, instance);

            instance = IntPtr.Zero;
        }

        controlInputs.throttle = 0.0f;
        controlInputs.roll = 0.0f;
        controlInputs.pitch = 0.0f;
        controlInputs.yaw = 0.0f;

        instance = Native.Invoke<IntPtr, FDMCreate>(fdmModelLibrary);

        Native.Invoke<FDMStartEnvironment>(fdmModelLibrary, instance, frequency, windActive);

        fdmReady = false;
    }

    public void SetFdmTrypValues(float throttle, float roll, float pitch, float yaw){
        controlInputs.throttle = throttle;
        controlInputs.roll = roll;
        controlInputs.pitch = pitch;
        controlInputs.yaw = yaw;

        if (instance != IntPtr.Zero){
            Native.Invoke<FDMSetTRPY>(fdmModelLibrary, instance, controlInputs.throttle, controlInputs.roll, controlInputs.pitch, controlInputs.yaw);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnApplicationQuit()
    {
        if(instance != IntPtr.Zero){
            Native.Invoke<FDMStopEnvironment>(fdmModelLibrary, instance);
            Native.Invoke<FDMDelete>(fdmModelLibrary, instance);
            instance = IntPtr.Zero;
        }

        if (fdmModelLibrary == IntPtr.Zero) return;
 
        UnityEngine.Debug.Log(Native.FreeLibrary(fdmModelLibrary)
                      ? "FDM Model library successfully unloaded."
                      : "FDM Model library could not be unloaded.");
    }
}
