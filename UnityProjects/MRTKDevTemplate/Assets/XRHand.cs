//based on 'FeatureUsageHandJointsManager.cs' https://github.com/microsoft/OpenXR-Unity-MixedReality-Samples/blob/main/BasicSample/Assets/HandTracking/Scripts/FeatureUsageHandJointsManager.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

public class XRHand : MonoBehaviour
{
    //[SerializeField] public GameObject FingerUIPrefab;
    [SerializeField]  public XRHand otherHand;

    public enum Handedness
    {
        Left,
        Right,
    }

    public GameObject HMDCamera;
    public GameObject indexTip;
    private GameObject uiJoint;

    [SerializeField] private Handedness handedness;
    [SerializeField] private bool visualizeJoints = false;


    private static readonly HandFinger[] HandFingers = System.Enum.GetValues(typeof(HandFinger)) as HandFinger[];

    private readonly Dictionary<HandFinger, GameObject[]> handFingerGameObjects = new Dictionary<HandFinger, GameObject[]>(); 
    private readonly List<Bone> fingerBones = new List<Bone>();
    private GameObject palmGameObject = null;

    [SerializeField] private ParentConstraint [] constraints;
    [SerializeField] private List<GameObject> joints;
    private bool handsareConnected = false; 

    private void Update()
    {
        UpdateHandJoints(handedness);
    }

    private void UpdateHandJoints(Handedness handedness)
    {
        InputDeviceCharacteristics flag;
        switch (handedness)
        {
            default:
            case Handedness.Left:
                flag = InputDeviceCharacteristics.Left;
                break;
            case Handedness.Right:
                flag = InputDeviceCharacteristics.Right;
                break;
        }
        List<InputDevice> inputDeviceList = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | flag, inputDeviceList);
        Hand hand = default;
        foreach (InputDevice device in inputDeviceList)
        {
            if (device.TryGetFeatureValue(CommonUsages.handData, out hand))
            {
                break;
            }
        }

        if (hand != default)
        {
            //EnableHand();

            if (hand.TryGetRootBone(out Bone palm))
            {
                if (palmGameObject == null)
                {
                    palmGameObject = InstantiateJoint();
                    palmGameObject.name = "wrist";
                }
                if (palm.TryGetPosition(out Vector3 position))
                {
                    palmGameObject.transform.localPosition = position;
                }
                if (palm.TryGetRotation(out Quaternion rotation))
                {
                    palmGameObject.transform.localRotation = rotation;
                }
                palmGameObject.GetComponent<MeshRenderer>().enabled = visualizeJoints;
                //attach to wrist constraint
            }

            foreach (HandFinger finger in HandFingers)
            {
                if (hand.TryGetFingerBones(finger, fingerBones))
                {
                    if (!handFingerGameObjects.ContainsKey(finger))
                    {
                        GameObject[] jointArray = new GameObject[fingerBones.Count];
                        for (int i = 0; i < fingerBones.Count; i++)
                        {
                            jointArray[i] = InstantiateJoint();
                            jointArray[i].name = finger.ToString() + i;
                            if (finger == HandFinger.Thumb || i != 0 || finger == HandFinger.Pinky)
                            {
                                joints.Add(jointArray[i]);
                            }
                                
                        }
                        handFingerGameObjects[finger] = jointArray;
                    }

                    GameObject[] fingerJointGameObjects = handFingerGameObjects[finger];

                    for (int i = 0; i < fingerBones.Count; i++)
                    {
                        Bone bone = fingerBones[i];
                        if (bone.TryGetPosition(out Vector3 position))
                        {
                            fingerJointGameObjects[i].transform.localPosition = position;
                        }
                        if (bone.TryGetRotation(out Quaternion rotation))
                        {
                            fingerJointGameObjects[i].transform.localRotation = rotation;
                        }
                        fingerJointGameObjects[i].GetComponent<MeshRenderer>().enabled = visualizeJoints;
                    }
                }
            }

            if (!handsareConnected)
            {
                for (int i = 0; i < constraints.Length; i++)
                {
                    ConstraintSource source = new ConstraintSource();
                    source.sourceTransform = joints[i].transform;
                    source.weight = 1;
                    constraints[i].AddSource(source);
                    if(handedness == Handedness.Left)
                    {
                        if (i == 1 || i == 2 || i == 3)
                            constraints[i].SetRotationOffset(0, new Vector3(270, 90, 0));
                        else
                            constraints[i].SetRotationOffset(0, new Vector3(-180, 90, 0));
                    }
                    else
                    {
                        if (i == 1 || i == 2 || i == 3)
                            constraints[i].SetRotationOffset(0, new Vector3(90, 270, 0));
                        else
                            constraints[i].SetRotationOffset(0, new Vector3(0, 270, 0));
                    }
                    
                }

                handsareConnected = true;
            }
        }
        else
        {
            //DisableHand();
        }

        
    }

    private GameObject InstantiateJoint()
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(gameObject.GetComponent<Collider>());
        gameObject.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        gameObject.transform.parent = transform;
        return gameObject;
    }

    public void AddFingertipScript(HandFinger finger, GameObject[] fingerObjects, XRHand hand)
    {
        int fingerTip = fingerObjects.Length - 1;

        if (finger == HandFinger.Index)
        {
            indexTip = fingerObjects[fingerTip];
        }
    }

    public void AddHandUIScript(GameObject[] fingerObjects, XRHand hand)
    {
        int fingerRoot = 0;
    }
}
