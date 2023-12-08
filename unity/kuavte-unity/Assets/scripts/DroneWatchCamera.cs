using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneWatchCamera : MonoBehaviour
{
    public Transform droneBody;
    public Transform droneWatchCamera;
    private float interpolationFactor = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float droneBodyX = droneBody.localEulerAngles.x;
        float droneBodyZ= droneBody.localEulerAngles.z;

        //droneWatchCamera.localEulerAngles = new Vector3(-droneBodyX, 0.0f, -droneBodyZ);
        droneWatchCamera.localEulerAngles = Vector3.Lerp(droneWatchCamera.localEulerAngles, new Vector3(-droneBodyX, 0.0f, -droneBodyZ), interpolationFactor);
    }
}
