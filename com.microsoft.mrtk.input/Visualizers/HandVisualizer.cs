// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Subsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Basic hand joint visualizer that draws an instanced mesh on each hand joint.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Hand Visualizer")]
    public class HandVisualizer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The XRNode on which this hand is located.")]
        private XRNode handNode = XRNode.LeftHand;

        /// <summary> The XRNode on which this hand is located. </summary>
        public XRNode HandNode { get => handNode; set => handNode = value; }

        [SerializeField]
        [Tooltip("Joint visualization mesh.")]
        private Mesh jointMesh;

        /// <summary> Joint visualization mesh. </summary>
        public Mesh JointMesh { get => jointMesh; set => jointMesh = value; }

        [SerializeField]
        [Tooltip("Joint visualization material.")]
        private Material jointMaterial;

        /// <summary> Joint visualization material. </summary>
        public Material JointMaterial { get => jointMaterial; set => jointMaterial = value; }

        private HandsAggregatorSubsystem handsSubsystem;

        // Transformation matrix for each joint.
        private List<Matrix4x4> jointMatrices = new List<Matrix4x4>();

        //Test by Me--------------------------------
        [SerializeField] private Transform[] constraints;
        [SerializeField] private GameObject[] spheres = new GameObject[26];
        public Transform constRotations;
        private bool handsareConnected = false;

        protected void OnEnable()
        {
            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand, $"HandVisualizer has an invalid XRNode ({handNode})!");

            handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            if (handsSubsystem == null)
            {
                StartCoroutine(EnableWhenSubsystemAvailable());
            }
            else
            {
                for (int i = 0; i < (int)TrackedHandJoint.TotalJoints; i++)
                {
                    jointMatrices.Add(new Matrix4x4());
                }
            }
        }

        /// <summary>
        /// Coroutine to wait until subsystem becomes available.
        /// </summary>
        private IEnumerator EnableWhenSubsystemAvailable()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            OnEnable();
        }

        private void LateUpdate()
        {
            if (handsSubsystem == null)
            {
                return;
            }

            // Query all joints in the hand.
            if (!handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
            {
                return;
            }

            for (int i = 1; i < joints.Count; i++)
            {
                // Skip joints with uninitialized quaternions.
                // This is temporary; eventually the HandsSubsystem will
                // be robust enough to never give us broken joints.
                if (joints[i].Rotation.Equals(new Quaternion(0, 0, 0, 0)))
                {
                    continue;
                }

                // Fill the matrices list with TRSs from the joint poses.
                jointMatrices[i] = Matrix4x4.TRS(joints[i].Position, joints[i].Rotation.normalized, Vector3.one * joints[i].Radius);
                Debug.Log(joints[i]);

                if (!handsareConnected && handNode == XRNode.LeftHand)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    sphere.transform.position = joints[i].Position;
                    spheres[i] = sphere;

                    //constraints[i].transform.rotation = joints[i].Rotation;
                }
                spheres[i].transform.position = joints[i].Position;
                spheres[i].transform.rotation = joints[i].Rotation;
            }
            //constraints[0].transform.position = joints[0].Position;
            
            for (int i = 0; i < constraints.Length; i++)
            {
                constraints[i].transform.position = joints[i+1].Position;
                if (i == 0)
                {
                    constraints[i].rotation = joints[i+1].Rotation * constRotations.transform.rotation;
                }
                else
                {
                    constraints[i].rotation = joints[i + 1].Rotation * Quaternion.Euler(180,90,0);
                }
                
            }
            // Draw the joints.
            //Graphics.DrawMeshInstanced(jointMesh, 0, jointMaterial, jointMatrices);


            if (!handsareConnected && handNode == XRNode.LeftHand)
            {
                for (int i = 0; i < constraints.Length; i++)
                {
                    //ConstraintSource source = new ConstraintSource();
                    //constraints[i].localPosition = joints[i].Position;
                    //constraints[i].localRotation = joints[i].Rotation.normalized;
                    //source.weight = 1;
                    //constraints[i].AddSource(source);
                    //if (handNode == XRNode.LeftHand)
                    //{
                    //    if (i == 1 || i == 2 || i == 3)
                    //        constraints[i].SetRotationOffset(0, new Vector3(270, 90, 0));
                    //    else
                    //        constraints[i].SetRotationOffset(0, new Vector3(-180, 90, 0));
                    //}
                    //else
                    //{
                    //    if (i == 1 || i == 2 || i == 3)
                    //        constraints[i].SetRotationOffset(0, new Vector3(90, 270, 0));
                    //    else
                    //        constraints[i].SetRotationOffset(0, new Vector3(0, 270, 0));
                    //}
                }

                handsareConnected = true;
            }
        }
    }
}
