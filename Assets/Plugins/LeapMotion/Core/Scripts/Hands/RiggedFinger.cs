/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap;
using Leap.Unity.Attributes;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.IO;

namespace Leap.Unity {

    /// <summary>
    /// Manages the position and orientation of the bones in a model rigged for skeletal
    /// animation.
    ///  
    /// The class expects that the graphics model's bones that correspond to the bones in
    /// the Leap Motion hand model are in the same order in the bones array.
    /// </summary>
    public class RiggedFinger : FingerModel {

        /// <summary>
        /// Allows the mesh to be stretched to align with finger joint positions.
        /// Only set to true when mesh is not visible.
        /// </summary>
        [HideInInspector]
        public bool deformPosition = false;

        [HideInInspector]
        public bool scaleLastFingerBone = false;

        public Vector3 modelFingerPointing = Vector3.forward;
        public Vector3 modelPalmFacing = -Vector3.up;

        /*public bool animate = false;

        public Vector3 prox_position = new Vector3(0, 0, 0);
        public Vector3 inter_position = new Vector3(0, 0, 0);
        public Vector3 distal_position = new Vector3(0, 0, 0);
        public Quaternion prox_rotation = new Quaternion(0, 0, 0, 0);
        public Quaternion inter_rotation = new Quaternion(0, 0, 0, 0);
        public Quaternion distal_rotation = new Quaternion(0, 0, 0, 0);*/

        public Quaternion Reorientation() {
            return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
        }


        /// <summary>
        /// Fingertip lengths for the standard edit-time hand.
        /// </summary>
        private static float[] s_standardFingertipLengths = null;
        static RiggedFinger() {
            // Calculate standard fingertip lengths.
            s_standardFingertipLengths = new float[5];
            var testHand = TestHandFactory.MakeTestHand(isLeft: true,
                                 unitType: TestHandFactory.UnitType.UnityUnits);
            for (int i = 0; i < 5; i++) {
                var fingertipBone = testHand.Fingers[i].bones[3];
                s_standardFingertipLengths[i] = fingertipBone.Length;
            }
        }

        private RiggedHand _parentRiggedHand = null;
        /// <summary>
        /// Updates model bone positions and rotations based on tracked hand data.
        /// </summary>
        public override void UpdateFinger()
        {

            for (int i = 0; i < bones.Length; ++i)
            {

                /*if (!animate)
                {*/
                if (bones[i] != null)
                {

                    bones[i].rotation = GetBoneRotation(i) * Reorientation();
                    if (deformPosition)
                    {
                        var boneRootPos = GetJointPosition(i);
                        bones[i].position = boneRootPos;

                        if (i == 3 && scaleLastFingerBone)
                        {
                            // Set fingertip base bone scale to match the bone length to the fingertip.
                            // This will only scale correctly if the model was constructed to match
                            // the standard "test" edit-time hand model from the TestHandFactory.
                            var boneTipPos = GetJointPosition(i + 1);
                            var boneVec = boneTipPos - boneRootPos;

                            // If the rigged hand is scaled (due to a scaled rig), we'll need to divide
                            // out that scale from the bone length to get its normal length.
                            if (_parentRiggedHand == null)
                            {
                                _parentRiggedHand = GetComponentInParent<RiggedHand>();
                            }
                            if (_parentRiggedHand != null)
                            {
                                var parentRiggedHandScale = _parentRiggedHand.transform.lossyScale.x;
                                if (parentRiggedHandScale != 0f && parentRiggedHandScale != 1f)
                                {
                                    boneVec /= parentRiggedHandScale;
                                }
                            }

                            var boneLen = boneVec.magnitude;

                            var standardLen = s_standardFingertipLengths[(int)this.fingerType];
                            var newScale = bones[i].transform.localScale;
                            var lengthComponentIdx = getLargestComponentIndex(modelFingerPointing);
                            newScale[lengthComponentIdx] = boneLen / standardLen;
                            bones[i].transform.localScale = newScale;
                        }
                    }
                }
                /*}

                else
                {
                    if (bones[i] != null)
                    {
                        Vector3 position = new Vector3(0, 0, 0);
                        Quaternion rotation = new Quaternion(0, 0, 0, 0);

                        if (i == 1)
                        {
                            position = prox_position;
                            rotation = prox_rotation;
                        }
                        else if (i == 2)
                        {
                            position = inter_position;
                            rotation = inter_rotation;
                        }
                        else if (i == 3)
                        {
                            position = distal_position;
                            rotation = distal_rotation;
                        }


                        bones[i].rotation = rotation  * Reorientation();
                        
                        if (deformPosition)
                        {
                            var boneRootPos = position;
                            bones[i].position = boneRootPos;

                            if (i == 3 && scaleLastFingerBone)
                            {
                                // Set fingertip base bone scale to match the bone length to the fingertip.
                                // This will only scale correctly if the model was constructed to match
                                // the standard "test" edit-time hand model from the TestHandFactory.
                                var boneTipPos = GetJointPosition(i + 1);
                                var boneVec = boneTipPos - boneRootPos;

                                // If the rigged hand is scaled (due to a scaled rig), we'll need to divide
                                // out that scale from the bone length to get its normal length.
                                if (_parentRiggedHand == null)
                                {
                                    _parentRiggedHand = GetComponentInParent<RiggedHand>();
                                }
                                if (_parentRiggedHand != null)
                                {
                                    var parentRiggedHandScale = _parentRiggedHand.transform.lossyScale.x;
                                    if (parentRiggedHandScale != 0f && parentRiggedHandScale != 1f)
                                    {
                                        boneVec /= parentRiggedHandScale;
                                    }
                                }

                                var boneLen = boneVec.magnitude;

                                var standardLen = s_standardFingertipLengths[(int)this.fingerType];
                                var newScale = bones[i].transform.localScale;
                                var lengthComponentIdx = getLargestComponentIndex(modelFingerPointing);
                                newScale[lengthComponentIdx] = boneLen / standardLen;
                                bones[i].transform.localScale = newScale;
                            }
                        }
                    }
                }
            }*/

            }
        }

        /*public void UpdateFingerAnimate(Vector3 prox_pos, Vector3 inter_pos, Vector3 distal_pos, Quaternion prox_rot, Quaternion inter_rot, Quaternion distal_rot, Vector3 palmFacing)
        {
            animate = true;
            //modelPalmFacing = palmFacing;
            prox_position = prox_pos;
            inter_position = inter_pos;
            distal_position = distal_pos;
            prox_rotation = prox_rot;
            inter_rotation = inter_rot;
            distal_rotation = distal_rot;
            calulateModelFingerPointing();

        }*/

        private int getLargestComponentIndex(Vector3 pointingVector) {
            var largestValue = 0f;
            var largestIdx = 0;
            for (int i = 0; i < 3; i++) {
                var testValue = pointingVector[i];
                if (Mathf.Abs(testValue) > largestValue) {
                    largestIdx = i;
                    largestValue = Mathf.Abs(testValue);
                }
            }
            return largestIdx;
        }

        public void SetupRiggedFinger(bool useMetaCarpals) {
            findBoneTransforms(useMetaCarpals);
            modelFingerPointing = calulateModelFingerPointing();
        }

        private void findBoneTransforms(bool useMetaCarpals) {
            if (!useMetaCarpals || fingerType == Finger.FingerType.TYPE_THUMB) {
                bones[1] = transform;
                bones[2] = transform.GetChild(0).transform;
                bones[3] = transform.GetChild(0).transform.GetChild(0).transform;
            }
            else {
                bones[0] = transform;
                bones[1] = transform.GetChild(0).transform;
                bones[2] = transform.GetChild(0).transform.GetChild(0).transform;
                bones[3] = transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform;
            }
        }

        private Vector3 calulateModelFingerPointing() {

            Vector3 distance = transform.InverseTransformPoint(transform.position) - transform.InverseTransformPoint(transform.GetChild(0).transform.position);
            /*Scene m_Scene;
            string sceneName;
            m_Scene = SceneManager.GetActiveScene();
            sceneName = m_Scene.name;
            if (sceneName.Equals("Learn"))
            {
                distance = transform.InverseTransformPoint(prox_position) - transform.InverseTransformPoint(inter_position);
            }*/
            Vector3 zeroed = RiggedHand.CalculateZeroedVector(distance);
            return zeroed;
        }
    }
}
