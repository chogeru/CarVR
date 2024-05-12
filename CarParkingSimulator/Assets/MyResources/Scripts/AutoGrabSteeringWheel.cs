using UnityEngine;

public class AutoGrabSteeringWheel : MonoBehaviour
{
    [SerializeField]
    private GameObject m_LeftHand;
    [SerializeField]
    private GameObject m_RightHand;
    [SerializeField]
    private Transform m_SteeringWheel;
    public Transform m_LeftHandTarget;
    public Transform m_RightHandTarget;

    [SerializeField] //Distance threshold to determine if hands should grab the wheel
    private float m_GrabDistance = 0.2f;
    //Public properties to check if hands are grabbing the steering wheel
    public bool isLeftHandGrabbing { get; private set; }
    public bool isRightHandGrabbing { get; private set; }
    void Update()
    {
        //Calculate distances from hands to steering wheel
        float distanceToLeftHand = Vector3.Distance(m_LeftHand.transform.position, m_SteeringWheel.position);
        float distanceToRightHand = Vector3.Distance(m_RightHand.transform.position, m_SteeringWheel.position);

        //Check if left hand should grab the steering wheel
        if (distanceToLeftHand < m_GrabDistance)
        {
            //Match left hand position and rotation with the target
            m_LeftHand.transform.position = m_LeftHandTarget.position;
            m_LeftHand.transform.rotation = m_LeftHandTarget.rotation;
            isLeftHandGrabbing = true;

        }
        else
        {
            isLeftHandGrabbing = false;
        }
        //Check if right hand should grab the steering wheel
        if (distanceToRightHand < m_GrabDistance)
        {
            //Match right hand position and rotation with the target
            m_RightHand.transform.position = m_RightHandTarget.position;
            m_RightHand.transform.rotation = m_RightHandTarget.rotation;
            isRightHandGrabbing = true;
        }
        else
        {
            isRightHandGrabbing = false;
        }
    }
}
