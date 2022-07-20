using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        if (vrTarget != null && rigTarget != null)
        {
            rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
            rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
        }
    }
    
}

public class VRRig : MonoBehaviour
{
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Transform headConstraint;
    private Vector3 headBodyOffset;
    public float turnSmoothness = 0.0f;


    void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
    }

    void LateUpdate()
    {

        head.Map();
        leftHand.Map();
        rightHand.Map();

        transform.rotation = Quaternion.Euler(0, head.vrTarget.rotation.eulerAngles.y /*- avatarRotation*/, 0);
        //transform.position = headConstraint.position + headBodyOffset;

        Vector3 position = headConstraint.position;
        position.y = position.y + headBodyOffset.y;
        transform.position = position;

        //transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized, Time.deltaTime * turnSmoothness);
        //transform.forward = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;


    }
}
