using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody m_CarRigidbody;
    [SerializeField, Header("Car's Front Wheels"), Space(10)]
    public Transform[] m_FrontWheels;
    [SerializeField, Header("Car's Rear Wheels"), Space(10)]
    public Transform[] m_RearWheels;
    [SerializeField]//Reference to the steering wheel Transform
    private Transform m_SteeringWheel;

    // Controllers for input handling
    public OVRInput.Controller m_RightController;
    public OVRInput.Controller m_LeftController;

    [SerializeField]
    public AutoGrabSteeringWheel steeringWheelGrab;

    public float m_MotorForce = 1000f;
    public float m_MaxSteerAngle = 30f;
    public float m_MaxSpeed = 10f;
    public float m_RotationSpeed = 80f;
    private float m_OriginalRotationSpeed;
    private float m_SteeringInput;
    private float m_ThrottleInput;
    private float m_MinimumSteerInputThreshold = 0.2f; 
    public float m_DragCoefficient = 0.2f;
    public float m_GroundRayLength = 1.5f;

    public bool isDisabled = false;

    void Start()
    {
        m_OriginalRotationSpeed = m_RotationSpeed;
        //Set initial drag coefficient
        m_CarRigidbody.drag = m_DragCoefficient;
    }

    void Update()
    {
        if (isDisabled) return;
        HandleInput();
    }

    void FixedUpdate()
    {
        if (isDisabled) return;
        ApplyMotorForce();
        AdjustCarPitch();
        LimitSpeed();
    }

    private void HandleInput()
    {
        //Process input based on whether both hands are grabbing the steering wheel
        if (steeringWheelGrab.isLeftHandGrabbing && steeringWheelGrab.isRightHandGrabbing)
        {
            // Read joystick input for steering and triggers for throttle
            m_SteeringInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_RightController).x;
            m_ThrottleInput = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_RightController) - OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_LeftController);
        }
        else
        {
            //Set throttle to zero for natural deceleration when not actively controlled
            m_ThrottleInput = 0;
        }
    }
    private void ApplyMotorForce()
    {
        float motor = m_ThrottleInput * m_MotorForce;
        float steering = Mathf.Abs(m_SteeringInput) > m_MinimumSteerInputThreshold ? m_SteeringInput * m_MaxSteerAngle : 0;

        m_CarRigidbody.AddForce(transform.forward * motor);
        // Front wheels
        foreach (Transform wheel in m_FrontWheels)
        {
            wheel.Rotate(Vector3.right, -motor * Time.deltaTime);
            wheel.localEulerAngles = new Vector3(wheel.localEulerAngles.x, steering, wheel.localEulerAngles.z);
        }

        // Rear wheels
        foreach (Transform wheel in m_RearWheels)
        {
            wheel.Rotate(Vector3.right, -motor * Time.deltaTime);
        }

        //Rotate the steering wheel along the Z-axis in the reverse direction
        if (m_SteeringWheel != null)
        {
            //Scale and invert the steering to match the wheel limit
            float steeringWheelRotation = Mathf.Clamp(-steering * 1.666f, -50f, 50f);  
            m_SteeringWheel.localRotation = Quaternion.Euler(0f, 0f, steeringWheelRotation);
        }
        RotateCar(m_SteeringInput);

    }

    private void LimitSpeed()
    {        
        //Limit car's speed to the maximum allowable speed
        if (m_CarRigidbody.velocity.magnitude > m_MaxSpeed)
        {
            m_CarRigidbody.velocity = m_CarRigidbody.velocity.normalized * m_MaxSpeed;
        }
    }
    private void AdjustCarPitch()
    {
        //Use raycasting to detect ground below the car and adjust pitch accordingly
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, m_GroundRayLength))
        {
            Vector3 groundNormal = hit.normal;
            //Calculate angle between ground normal and vertical axis
            float angle = Vector3.Angle(groundNormal, Vector3.up);
            //Adjust pitch if the angle is less than 45 degrees
            if (angle < 45) 
            {
                Quaternion toRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
                m_CarRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, toRotation, Time.fixedDeltaTime * 5));
            }
        }
    }

    private void RotateCar(float horizontalInput)
    {
        //Dynamically adjust the rotation speed of the car based on its current speed
        float newRotationSpeed = Mathf.Lerp(0f, m_OriginalRotationSpeed, m_CarRigidbody.velocity.magnitude / m_MaxSpeed);
        float rotationAmount = Mathf.Abs(horizontalInput) > m_MinimumSteerInputThreshold ? horizontalInput * newRotationSpeed * Time.deltaTime : 0;

        //Apply a deceleration factor to reduce speed when turning sharply
        float decelerationFactor = Mathf.Clamp01(Mathf.Abs(horizontalInput) * 0.035f);  // 0.02f‚ÌŒW”‚ÍŒ¸‘¬‚Ì‹­‚³‚ð§Œä

        //Apply deceleration if the car is turning and moving above a certain speed threshold
        if (Mathf.Abs(rotationAmount) > 0 && m_CarRigidbody.velocity.magnitude > m_MaxSpeed * 0.65f)  // 0.75f ‚Ìè‡’l‚Í•K—v‚É‰ž‚¶‚Ä’²®
        {
            m_CarRigidbody.velocity *= (1f - decelerationFactor);
        }
        //Apply the calculated rotation to the car
        transform.Rotate(Vector3.up, rotationAmount);
        //Update the rotation speed based on current conditions
        m_RotationSpeed = m_CarRigidbody.velocity.magnitude >= m_MaxSpeed ? m_OriginalRotationSpeed : newRotationSpeed;
    }

}