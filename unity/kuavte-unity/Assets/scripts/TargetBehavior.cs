using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public enum TargetSizingType{
    StaticSize = 0,
    VaryingSize = 1
}

public enum TargetMovementType{
    NoMovement = 1,
    GroundLinear, // Obje kendi konumun x ekseninde -5 ve 5 aralığında sabit hızla hareket eder
    GroundCircular, // Obje başlangıç konumu merkez olacak şekilde 5m yarıçap sabit hızla çember çizer
    GroundRandom, // Obje kendi konumu merkez olacak şekilde x - y ekseninde (-5,-5) (5,5) karesi içinde rastgele hareket eder
    AirLinear, // Obje kendi konumu başlangıç olacak şekilde y ekseninde 0 ile 5 metre arasında sabit hızla yükselip alçalır
    AirCircular, // Obje kendi konumunun 5m üstü merkez olacak şekilde x - y ekseninde sabit hızla çember çizer
    AirRandom, // Obje kendi konumunun 5m üstü merkez olan 5m yarıçaplı bir küre üzerinde rastgele hareket eder
    Escape, // Obje drone kendisine 30m'den daha yakın olduğunda drone'dan 5m/s hızla aksi yönder kaçar
    GreatCircle, // Obje drone'un başlangıç merkezi merkez olacak şekilde 50m yarıçapta sabit hızda hareket eder
}

public class TargetBehavior : MonoBehaviour
{
    private Transform corner1, corner2, corner3, corner4, corner5, corner6, corner7, corner8;
    private Transform center;
    public Transform hunter;

    private TargetMovementType movementType = TargetMovementType.NoMovement;
    private TargetSizingType sizingType = TargetSizingType.StaticSize;

    private bool updateEnabled = false;

    // GROUND LINEAR PARAMETERS
    private float groundLinearSpeed = 1.5f;
    private UnityEngine.Vector3 groundLinearTargetPosition;

    // GROUND CIRCULAR PARAMETERS
    private float groundCircularAngularSpeed = 30.0f;
    private float groundCircularRadius = 5.0f;
    private float groundCircularAngle = 0.0f;

    // GROUND RANDOM PARAMETERS
    private float minSpeed = 1f;
    private float maxSpeed = 3f;
    private float currentSpeed;
    private UnityEngine.Vector3 groundRandomTargetPosition;

    // AIR LINEAR PARAMETERS
    private float airLinearSpeed = 1.5f;
    private UnityEngine.Vector3 airLinearTargetPosition;

    // AIR CIRCULAR PARAMETERS
    private float airCircularAngularSpeed = 30.0f;
    private float airCircularRadius = 5.0f;
    private float airCircularAngle = 0.0f;

    // AIR RANDOM PARAMETERS
    private float airRandomPsiAngularSpeed = 0.0f;
    private float airRandomThetaAngularSpeed = 0.0f;
    private float airRandomCurrentPsiAngle = 0.0f;
    private float airRandomCurrentThetaAngle = 0.0f;
    private float airRandomMovementTime = 0.0f;
    private float airRandomCurrentTime = 0.0f;

    // ESCAPE PARAMETERS
    private float escapeDistance = 30.0f;
    private float escapeSpeed = 4.0f;

    // GREAT CIRCLE PARAMETERS
    private float greatCircleAngularSpeed = 2.0f;
    private float greatCircleRadius = 50.0f;
    private float greatCircleAngle = 90.0f;

    // Start is called before the first frame update
    void Start()
    {
        updateEnabled = false;

        corner1 = TransformDeepChildExtension.FindDeepChild(transform, "Corner1");
        corner2 = TransformDeepChildExtension.FindDeepChild(transform, "Corner2");
        corner3 = TransformDeepChildExtension.FindDeepChild(transform, "Corner3");
        corner4 = TransformDeepChildExtension.FindDeepChild(transform, "Corner4");
        corner5 = TransformDeepChildExtension.FindDeepChild(transform, "Corner5");
        corner6 = TransformDeepChildExtension.FindDeepChild(transform, "Corner6");
        corner7 = TransformDeepChildExtension.FindDeepChild(transform, "Corner7");
        corner8 = TransformDeepChildExtension.FindDeepChild(transform, "Corner8");
        center = TransformDeepChildExtension.FindDeepChild(transform, "Center");

        Initialize(TargetMovementType.NoMovement);
    }

    public void Initialize(TargetMovementType mt = TargetMovementType.NoMovement, 
                    TargetSizingType st = TargetSizingType.StaticSize)
    {
        updateEnabled = false;

        movementType = mt;
        sizingType = st;

        ResetTarget();

        if(mt == TargetMovementType.Escape){
            center.AddComponent<Rigidbody>();
            center.GetComponent<Rigidbody>().mass = 1f;
        }

        updateEnabled = true;
    }

    void ResetTarget()
    {
        transform.position = new UnityEngine.Vector3(0.0f, 0.4455f, 50.0f);

        center.localPosition = new UnityEngine.Vector3(0, 0, 0);

        corner1.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner2.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner3.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner4.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner5.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner6.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner7.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);
        corner8.localScale = new UnityEngine.Vector3(0.05f, 0.05f, 0.05f);

        Rigidbody centerrb = center.GetComponent<Rigidbody>();

        if(centerrb){
            Destroy(centerrb);
        }

        groundCircularAngle = 0.0f;
        airCircularAngle = 0.0f;
        currentSpeed = 0f;
        groundRandomTargetPosition = UnityEngine.Vector3.zero;
        airLinearTargetPosition = new UnityEngine.Vector3(0, 5f, 0f);
        groundLinearTargetPosition = new UnityEngine.Vector3(5f, 0f, 0f);
        airRandomPsiAngularSpeed = 0.0f;
        airRandomThetaAngularSpeed = 0.0f;
        airRandomMovementTime = 0.0f;
        airRandomCurrentTime = 0.0f;
        airRandomCurrentPsiAngle = 0.0f;
        airRandomCurrentThetaAngle = 0.0f;
        greatCircleAngle = 90.0f;
    }

    void ScaleUpdate(){

    }

    void GroundLinearMove(){
        center.localPosition = UnityEngine.Vector3.MoveTowards(center.localPosition, groundLinearTargetPosition, groundLinearSpeed * Time.fixedDeltaTime);

        if (UnityEngine.Vector3.Distance(center.localPosition, groundLinearTargetPosition) < 0.1f){
            if(groundLinearTargetPosition.x > 0f){
                groundLinearTargetPosition = new UnityEngine.Vector3(-5f, 0f, 0f);
            }
            else{
                groundLinearTargetPosition = new UnityEngine.Vector3(5f, 0f, 0f);
            }
        }
    }

    void GroundCircularMove(){
        groundCircularAngle += groundCircularAngularSpeed * Time.deltaTime;

        if(groundCircularAngle > 360.0f){
            groundCircularAngle -= 360.0f;
        }

        float radianValue = 3.1415926535897931f * groundCircularAngle / 180.0f;

        float xPosition = (float)Math.Cos(radianValue) * groundCircularRadius;
        float zPosition = (float)Math.Sin(radianValue) * groundCircularRadius;

        center.localPosition = new UnityEngine.Vector3(xPosition, 0.0f, zPosition);
    }

    void GroundRandomMove(){
        // Check if the target position is not set
        if (groundRandomTargetPosition == UnityEngine.Vector3.zero)
        {
            // Set a new random target position within the specified range
            groundRandomTargetPosition = new UnityEngine.Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));

            // Set a random speed within the specified range
            currentSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }

        // Move towards the target position smoothly
        center.localPosition = UnityEngine.Vector3.MoveTowards(center.localPosition, groundRandomTargetPosition, currentSpeed * Time.fixedDeltaTime);

        // Check if the object has reached the target position
        if (UnityEngine.Vector3.Distance(center.localPosition, groundRandomTargetPosition) < 0.1f)
        {
            // Set a new random target position within the specified range
            groundRandomTargetPosition = new UnityEngine.Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));

            // Set a random speed within the specified range
            currentSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
    }

    void AirLinearMove(){
        center.localPosition = UnityEngine.Vector3.MoveTowards(center.localPosition, airLinearTargetPosition, airLinearSpeed * Time.fixedDeltaTime);

        if (UnityEngine.Vector3.Distance(center.localPosition, airLinearTargetPosition) < 0.1f){
            if(airLinearTargetPosition.y > 2.5f){
                airLinearTargetPosition = UnityEngine.Vector3.zero;
            }
            else{
                airLinearTargetPosition = new UnityEngine.Vector3(0, 5f, 0f);
            }
        }
    }

    void AirCircularMove(){
        airCircularAngle += airCircularAngularSpeed * Time.deltaTime;

        if(airCircularAngle > 360.0f){
            airCircularAngle -= 360.0f;
        }

        float radianValue = 3.1415926535897931f * airCircularAngle / 180.0f;

        float xPosition = (float)Math.Cos(radianValue) * airCircularRadius;
        float yPosition = (float)Math.Sin(radianValue) * airCircularRadius + 5f;

        center.localPosition = new UnityEngine.Vector3(xPosition, yPosition, 0f);
    }

    void AirRandomMove(){
        if(airRandomMovementTime - airRandomCurrentTime < 0.01f){
            airRandomMovementTime = UnityEngine.Random.Range(1f, 4f);

            float rand1 = UnityEngine.Random.Range(-1,1);
            float rand2 = UnityEngine.Random.Range(-1,1);

            airRandomPsiAngularSpeed = UnityEngine.Random.Range(8f, 20f);
            airRandomThetaAngularSpeed = UnityEngine.Random.Range(8f, 20f);

            if(rand1 < 0f){
                airRandomPsiAngularSpeed *= -1;
            }

            if(rand2 < 0f){
                airRandomThetaAngularSpeed *= -1;
            }

            airRandomCurrentTime = 0f;
        }
        else{
            airRandomCurrentTime += Time.deltaTime;

            airRandomCurrentPsiAngle += airRandomPsiAngularSpeed * Time.deltaTime;

            if(airRandomCurrentPsiAngle > 360.0f){
                airRandomCurrentPsiAngle -= 360.0f;
            }
            else if(airRandomCurrentPsiAngle < 0f){
                airRandomCurrentPsiAngle += 360.0f;
            }

            float psiRadianValue = 3.1415926535897931f * airRandomCurrentPsiAngle / 180.0f;

            airRandomCurrentThetaAngle += airRandomThetaAngularSpeed * Time.deltaTime;

            if(airRandomCurrentThetaAngle > 360.0f){
                airRandomCurrentThetaAngle -= 360.0f;
            }
            else if(airRandomCurrentThetaAngle < 0f){
                airRandomCurrentThetaAngle += 360.0f;
            }

            float thetaRadianValue = 3.1415926535897931f * airRandomCurrentThetaAngle / 180.0f;

            float xPosition = 5f * (float)Math.Cos(thetaRadianValue) * (float)Math.Cos(psiRadianValue);
            float yPosition = 5f * (float)Math.Cos(thetaRadianValue) * (float)Math.Sin(psiRadianValue);
            float zPosition = (5f * (float)Math.Sin(thetaRadianValue)) + 5f;

            center.localPosition = new UnityEngine.Vector3(xPosition, zPosition, yPosition);
        }
    }

    void EscapeMove(){
        float distance = UnityEngine.Vector3.Distance(center.position, hunter.position);

        if (distance > escapeDistance)
        {
            // If the distance is greater than 30 meters, make the Center object stationary
            // You might want to add additional behavior here if needed
            center.GetComponent<Rigidbody>().velocity = UnityEngine.Vector3.zero;
        }
        else
        {
            // If the distance is less than 30 meters, escape towards the hunter's approaching vector
            UnityEngine.Vector3 escapeDirection = hunter.position - center.position;
            escapeDirection *= -1;
            escapeDirection.y = 0; // Ensure movement only on x and z axes
            escapeDirection.Normalize();

            // Move the Center object using localPosition
            center.localPosition += escapeDirection * escapeSpeed * Time.fixedDeltaTime;
        }
    }

    void GreatCircleMove(){
        greatCircleAngle += greatCircleAngularSpeed * Time.deltaTime;

        if(greatCircleAngle > 360.0f){
            greatCircleAngle -= 360.0f;
        }

        float radianValue = 3.1415926535897931f * greatCircleAngle / 180.0f;

        float xPosition = (float)Math.Cos(radianValue) * greatCircleRadius;
        float zPosition = (float)Math.Sin(radianValue) * greatCircleRadius;

        center.position = new UnityEngine.Vector3(xPosition, 0.4455f, zPosition);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(updateEnabled){
            if(movementType == TargetMovementType.GroundLinear){
                GroundLinearMove();
            }
            else if(movementType == TargetMovementType.GroundCircular){
                GroundCircularMove();
            }
            else if(movementType == TargetMovementType.GroundRandom){
                GroundRandomMove();
            }
            else if(movementType == TargetMovementType.AirLinear){
                AirLinearMove();
            }
            else if(movementType == TargetMovementType.AirCircular){
                AirCircularMove();
            }
            else if(movementType == TargetMovementType.AirRandom){
                AirRandomMove();
            }
            else if(movementType == TargetMovementType.Escape){
                EscapeMove();
            }
            else if(movementType == TargetMovementType.GreatCircle){
                GreatCircleMove();
            }
        }
    }
}