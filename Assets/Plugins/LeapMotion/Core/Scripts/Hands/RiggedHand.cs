/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.   
 * 
 * Edited by Forum Modi Computer Science Student TCNJ 2021
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

    /// <summary> A skinned and jointed 3D HandModel. </summary>
    public class RiggedHand : HandModel {

        //changes made in an attempt to animate the hand based off of coordinates
        /*
        public bool isLeft;
        public bool stop = false;
        List<string> stringSignArray = new List<string>();
        public string signName;
        public string signDynamic;
        public static List<Vector3> jointPositions = new List<Vector3>();
        public static List<Quaternion> jointRotations = new List<Quaternion>();
        public Scene m_Scene;
        public string sceneName;

        public Vector3 palmPosition = new Vector3(0, 0, 0);
        public Vector3 wristPosition = new Vector3(0,0,0);
        public Quaternion palmRotation = new Quaternion(0,0,0,0);
        public Quaternion wristRotation = new Quaternion(0,0,0,0);

        //object of other script
        public InputField iField;

        int PALM_INDEX = 0;
        int WRIST_INDEX = 1;
        int INDEX_PROX_INDEX = 2;
        int INDEX_INTER_INDEX = 3;
        int INDEX_DISTAL_INDEX = 4;
        int MIDDLE_PROX_INDEX = 5;
        int MIDDLE_INTER_INDEX = 6;
        int MIDDLE_DISTAL_INDEX = 7;
        int PINKY_PROX_INDEX = 8;
        int PINKY_INTER_INDEX = 9;
        int PINKY_DISTAL_INDEX = 10;
        int RING_PROX_INDEX = 11;
        int RING_INTER_INDEX = 12;
        int RING_DISTAL_INDEX = 13;
        int THUMB_PROX_INDEX = 14;
        int THUMB_INTER_INDEX = 15;
        int THUMB_DISTAL_INDEX = 16;
        int USER_BONE_INDEX = 17;

        RiggedFinger riggedFinger;
        HandEnableDisable handToggle;*/

        public override ModelType HandModelType { get { return ModelType.Graphics; } }
        public override bool SupportsEditorPersistence() { return true; }

        [SerializeField]
        [FormerlySerializedAs("DeformPositionsInFingers")]
        [OnEditorChange("deformPositionsInFingers")]
        private bool _deformPositionsInFingers = true;
        public bool deformPositionsInFingers {
            get { return _deformPositionsInFingers; }
            set {
                _deformPositionsInFingers = value;
                updateDeformPositionsInFingers();
            }
        }

        [Tooltip("Because bones only exist at their roots in model rigs, the length " +
          "of the last fingertip bone is lost when placing bones at positions in the " +
          "tracked hand. " +
          "This option scales the last bone along its X axis (length axis) to match " +
          "its bone length to the tracked bone length. This option only has an " +
          "effect if Deform Positions In Fingers is enabled.")]
        [DisableIf("_deformPositionsInFingers", isEqualTo: false)]
        [SerializeField]
        [OnEditorChange("scaleLastFingerBones")]
        private bool _scaleLastFingerBones = true;
        public bool scaleLastFingerBones {
            get { return _scaleLastFingerBones; }
            set {
                _scaleLastFingerBones = value;
                updateScaleLastFingerBoneInFingers();
            }
        }

        [Tooltip("Hands are typically rigged in 3D packages with the palm transform near the wrist. Uncheck this if your model's palm transform is at the center of the palm similar to Leap API hands.")]
        [FormerlySerializedAs("ModelPalmAtLeapWrist")]
        public bool modelPalmAtLeapWrist = true;

        [Tooltip("Set to True if each finger has an extra trasform between palm and base of the finger.")]
        [FormerlySerializedAs("UseMetaCarpals")]
        public bool useMetaCarpals;

#pragma warning disable 0414
        // TODO: DELETEME these
        [Header("Values for Stored Start Pose")]
        [SerializeField]
        private List<Transform> jointList = new List<Transform>();
        [SerializeField]
        public List<Quaternion> localRotations = new List<Quaternion>();
        [SerializeField]
        public List<Vector3> localPositions = new List<Vector3>();
#pragma warning restore 0414*

        [Tooltip("If non-zero, this vector and the modelPalmFacing vector " +
          "will be used to re-orient the Transform bones in the hand rig, to " +
          "compensate for bone axis discrepancies between Leap Bones and model " +
          "bones.")]
        public Vector3 modelFingerPointing = new Vector3(0, 0, 0);
        [Tooltip("If non-zero, this vector and the modelFingerPointing vector " +
          "will be used to re-orient the Transform bones in the hand rig, to " +
          "compensate for bone axis discrepancies between Leap Bones and model " +
          "bones.")]
        public Vector3 modelPalmFacing = new Vector3(0, 0, 0);

        /*public void getValues()
        {
            m_Scene = SceneManager.GetActiveScene();
            sceneName = m_Scene.name;
            if (sceneName.Equals("Learn"))
            {
                Debug.Log("Here");
                stop = true;
                //allows us to call methods from the currentSign script
                List<string> signStrings = new List<string>();

                signStrings = readTextFile("Assets/Scripts/currentsign.txt");

                string signString = signStrings[0];

                Debug.Log(signString);
                
                int firstSpaceIndex = signString.IndexOf(" ");
                signName = signString.Substring(0, firstSpaceIndex);

                firstSpaceIndex = signString.IndexOf(" ");
                char signIsDynamic = signString[firstSpaceIndex + 1];
                signDynamic = signIsDynamic.ToString();

                createLocalPos(signName, signDynamic);
            }
        }*/

        /*public void createLocalPos(string signName, string signDynamic)
        {

            Debug.Log("Here!");

            List<string> coordinates = new List<string>();

            if (isLeft == true) coordinates = readTextFile("Assets\\Scripts\\" + signName + "_left.csv");
            else coordinates = readTextFile("Assets\\Scripts\\" + signName + "_right.csv");

            Debug.Log(coordinates);

            string coord = coordinates[1];

            Debug.Log(coord);

            int num = 122;

            float x_pos = 0;
            float y_pos = 0;
            float z_pos = 0;
            float x_rot = 0;
            float y_rot = 0;
            float z_rot = 0;
            float w_rot = 0;

            for (int i = 0; i < num; i++)
            {
                int index = coord.IndexOf(",");
                string coordinate = "";

                if (!(i == num - 1)) coordinate = coord.Substring(0, index);
                else coordinate = coord;

                if (!(i == num - 1)) coord = coord.Substring(index + 2);

                if (i % 7 == 0 || i == 119) { x_pos = float.Parse(coordinate); }
                else if (i % 7 == 1 || i == 120) { y_pos = float.Parse(coordinate); }
                else if (i % 7 == 2 || i == 121)
                {
                    z_pos = float.Parse(coordinate);
                    Vector3 vec = new Vector3(x_pos, y_pos, z_pos);
                    jointPositions.Add(vec);
                }

                else if (i % 7 == 3) { x_rot = float.Parse(coordinate); }
                else if (i % 7 == 4) { y_rot = float.Parse(coordinate); }
                else if (i % 7 == 5) { z_rot = float.Parse(coordinate); }
                else if (i % 7 == 6)
                {
                    w_rot = float.Parse(coordinate);
                    Quaternion qua = new Quaternion(x_rot, y_rot, z_rot, w_rot);
                    jointRotations.Add(qua);
                }
            }
        }*/

        /*
        //adds each line of the textfile list
        List<string> readTextFile(string filePath)
        {
            List<string> signArray = new List<string>();
            StreamReader signsFile = new StreamReader(filePath);

            while (!signsFile.EndOfStream) signArray.Add(signsFile.ReadLine());

            signsFile.Close();
            return signArray;
        }*/

        /// <summary> Rotation derived from the `modelFingerPointing` and
        /// `modelPalmFacing` vectors in the RiggedHand inspector. </summary>
        public Quaternion userBoneRotation { get {
                if (modelFingerPointing == Vector3.zero || modelPalmFacing == Vector3.zero) {
                    return Quaternion.identity;
                }
                return Quaternion.Inverse(
                  Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
            } }

        public override void InitHand()
        {
            UpdateHand();
            calculateModelFingerPointing();
            updateDeformPositionsInFingers();
            updateScaleLastFingerBoneInFingers();

        }

        public override void UpdateHand() {
            /*if (!stop)
            {*/
                if (palm != null) {
                    if (modelPalmAtLeapWrist) {
                        palm.position = GetWristPosition();
                    }
                    else {
                        palm.position = GetPalmPosition();
                        if (wristJoint) {
                            wristJoint.position = GetWristPosition();
                        }
                    }
                    palm.rotation = getRiggedPalmRotation() * userBoneRotation;
                }

                if (forearm != null) {
                    forearm.rotation = GetArmRotation() * userBoneRotation;
                }

                for (int i = 0; i < fingers.Length; ++i) {
                    if (fingers[i] != null) {
                        fingers[i].fingerType = (Finger.FingerType)i;
                        fingers[i].UpdateFinger();
                    }
                }
            /*}
            else
            {

                if (modelPalmAtLeapWrist)
                {
                    palm.position = jointPositions[WRIST_INDEX];
                }
                else
                {
                    palm.position = jointPositions[PALM_INDEX];
                    if (wristJoint)
                    {
                        wristJoint.position = jointPositions[WRIST_INDEX];
                    }
                }

                palm.rotation = jointRotations[PALM_INDEX] * userBoneRotation;

                if (forearm != null)
                {
                    forearm.rotation = jointRotations[WRIST_INDEX] * userBoneRotation;
                }

                for (int i = 0; i < fingers.Length; ++i)
                {
                    if (fingers[i] != null)
                    {
                        fingers[i].fingerType = (Finger.FingerType)i;

                        //allows us to call methods from the currentSign script
                        riggedFinger = fingers[i].GetComponent<RiggedFinger>();

                        if (fingers[i].fingerType == Finger.FingerType.TYPE_INDEX) { riggedFinger.UpdateFingerAnimate(jointPositions[INDEX_PROX_INDEX], jointPositions[INDEX_INTER_INDEX], jointPositions[INDEX_DISTAL_INDEX], jointRotations[INDEX_PROX_INDEX], jointRotations[INDEX_INTER_INDEX], jointRotations[INDEX_DISTAL_INDEX], modelPalmFacing); }
                        else if (fingers[i].fingerType == Finger.FingerType.TYPE_MIDDLE) { riggedFinger.UpdateFingerAnimate(jointPositions[MIDDLE_PROX_INDEX], jointPositions[MIDDLE_INTER_INDEX], jointPositions[MIDDLE_DISTAL_INDEX], jointRotations[MIDDLE_PROX_INDEX], jointRotations[MIDDLE_INTER_INDEX], jointRotations[MIDDLE_DISTAL_INDEX], modelPalmFacing); }
                        else if (fingers[i].fingerType == Finger.FingerType.TYPE_PINKY) { riggedFinger.UpdateFingerAnimate(jointPositions[PINKY_PROX_INDEX], jointPositions[PINKY_INTER_INDEX], jointPositions[PINKY_DISTAL_INDEX], jointRotations[PINKY_PROX_INDEX], jointRotations[PINKY_INTER_INDEX], jointRotations[PINKY_DISTAL_INDEX], modelPalmFacing); }
                        else if (fingers[i].fingerType == Finger.FingerType.TYPE_RING) { riggedFinger.UpdateFingerAnimate(jointPositions[RING_PROX_INDEX], jointPositions[RING_INTER_INDEX], jointPositions[RING_DISTAL_INDEX], jointRotations[RING_PROX_INDEX], jointRotations[RING_INTER_INDEX], jointRotations[RING_DISTAL_INDEX], modelPalmFacing); }
                        else if (fingers[i].fingerType == Finger.FingerType.TYPE_THUMB) { riggedFinger.UpdateFingerAnimate(jointPositions[THUMB_PROX_INDEX], jointPositions[THUMB_INTER_INDEX], jointPositions[THUMB_DISTAL_INDEX], jointRotations[THUMB_PROX_INDEX], jointRotations[THUMB_INTER_INDEX], jointRotations[THUMB_DISTAL_INDEX], modelPalmFacing); }
                        riggedFinger.UpdateFinger();
                    }
                        
            
                }
            }*/
        }

        /**Sets up the rigged hand by finding base of each finger by name */
        [ContextMenu("Setup Rigged Hand")]
        public void SetupRiggedHand() {
            Debug.Log("Using transform naming to setup RiggedHand on " + transform.name);
            modelFingerPointing = new Vector3(0, 0, 0);
            modelPalmFacing = new Vector3(0, 0, 0);
            assignRiggedFingersByName();
            setupRiggedFingers();
            modelPalmFacing = calculateModelPalmFacing(palm, fingers[2].transform, fingers[1].transform);
            modelFingerPointing = calculateModelFingerPointing();
            setFingerPalmFacing();
        }

        /**Sets up the rigged hand if RiggedFinger scripts have already been assigned using Mecanim values.*/
        public void AutoRigRiggedHand(Transform palm, Transform finger1, Transform finger2) {
            Debug.Log("Using Mecanim mapping to setup RiggedHand on " + transform.name);
            modelFingerPointing = new Vector3(0, 0, 0);
            modelPalmFacing = new Vector3(0, 0, 0);
            setupRiggedFingers();
            modelPalmFacing = calculateModelPalmFacing(palm, finger1, finger2);
            modelFingerPointing = calculateModelFingerPointing();
            setFingerPalmFacing();
        }

        /**Finds palm and finds root of each finger by name and assigns RiggedFinger scripts */
        private void assignRiggedFingersByName() {
            List<string> palmStrings = new List<string> { "palm" };
            List<string> thumbStrings = new List<string> { "thumb", "tmb" };
            List<string> indexStrings = new List<string> { "index", "idx" };
            List<string> middleStrings = new List<string> { "middle", "mid" };
            List<string> ringStrings = new List<string> { "ring" };
            List<string> pinkyStrings = new List<string> { "pinky", "pin" };
            //find palm by name
            //Transform palm = null;
            Transform thumb = null;
            Transform index = null;
            Transform middle = null;
            Transform ring = null;
            Transform pinky = null;
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            if (palmStrings.Any(w => transform.name.ToLower().Contains(w))) {
                base.palm = transform;
            }
            else {
                foreach (Transform t in children) {
                    if (palmStrings.Any(w => t.name.ToLower().Contains(w)) == true) {
                        base.palm = t;

                    }
                }
            }
            if (!palm) {
                palm = transform;
            }
            if (palm) {
                foreach (Transform t in children) {
                    RiggedFinger preExistingRiggedFinger;
                    preExistingRiggedFinger = t.GetComponent<RiggedFinger>();
                    string lowercaseName = t.name.ToLower();
                    if (!preExistingRiggedFinger) {
                        if (thumbStrings.Any(w => lowercaseName.Contains(w)) && t.parent == palm) {
                            thumb = t;
                            RiggedFinger newRiggedFinger = thumb.gameObject.AddComponent<RiggedFinger>();
                            newRiggedFinger.fingerType = Finger.FingerType.TYPE_THUMB;
                        }
                        if (indexStrings.Any(w => lowercaseName.Contains(w)) && t.parent == palm) {
                            index = t;
                            RiggedFinger newRiggedFinger = index.gameObject.AddComponent<RiggedFinger>();
                            newRiggedFinger.fingerType = Finger.FingerType.TYPE_INDEX;
                        }
                        if (middleStrings.Any(w => lowercaseName.Contains(w)) && t.parent == palm) {
                            middle = t;
                            RiggedFinger newRiggedFinger = middle.gameObject.AddComponent<RiggedFinger>();
                            newRiggedFinger.fingerType = Finger.FingerType.TYPE_MIDDLE;
                        }
                        if (ringStrings.Any(w => lowercaseName.Contains(w)) && t.parent == palm) {
                            ring = t;
                            RiggedFinger newRiggedFinger = ring.gameObject.AddComponent<RiggedFinger>();
                            newRiggedFinger.fingerType = Finger.FingerType.TYPE_RING;
                        }
                        if (pinkyStrings.Any(w => lowercaseName.Contains(w)) && t.parent == palm) {
                            pinky = t;
                            RiggedFinger newRiggedFinger = pinky.gameObject.AddComponent<RiggedFinger>();
                            newRiggedFinger.fingerType = Finger.FingerType.TYPE_PINKY;
                        }
                    }
                }
            }
        }

        /**Triggers SetupRiggedFinger() in each RiggedFinger script for this RiggedHand */
        private void setupRiggedFingers() {
            RiggedFinger[] fingerModelList = GetComponentsInChildren<RiggedFinger>();
            for (int i = 0; i < 5; i++) {
                int fingersIndex = fingerModelList[i].fingerType.indexOf();
                fingers[fingersIndex] = fingerModelList[i];
                fingerModelList[i].SetupRiggedFinger(useMetaCarpals);
            }
        }

        /**Sets the modelPalmFacing vector in each RiggedFinger to match this RiggedHand */
        private void setFingerPalmFacing() {
            RiggedFinger[] fingerModelList = GetComponentsInChildren<RiggedFinger>();
            for (int i = 0; i < 5; i++) {
                int fingersIndex = fingerModelList[i].fingerType.indexOf();
                fingers[fingersIndex] = fingerModelList[i];
                fingerModelList[i].modelPalmFacing = modelPalmFacing;
            }
        }

        /**Calculates the palm facing direction by finding the vector perpendicular to the palm and two fingers  */
        private Vector3 calculateModelPalmFacing(Transform palm, Transform finger1,
          Transform finger2)
        {
            Vector3 a = palm.transform.InverseTransformPoint(palm.position);
            Vector3 b = palm.transform.InverseTransformPoint(finger1.position);
            Vector3 c = palm.transform.InverseTransformPoint(finger2.position);

            Vector3 side1 = b - a;
            Vector3 side2 = c - a;
            Vector3 perpendicular;

            if (Handedness == Chirality.Left) {
                perpendicular = Vector3.Cross(side1, side2);
            }
            else perpendicular = Vector3.Cross(side2, side1);
            //flip perpendicular if it is above palm
            Vector3 calculatedPalmFacing = CalculateZeroedVector(perpendicular);
            return calculatedPalmFacing;
        }

        /**Find finger direction by finding distance vector from palm to middle finger */
        private Vector3 calculateModelFingerPointing() {
            Vector3 distance = palm.transform.InverseTransformPoint(fingers[2].transform.GetChild(0).transform.position) - palm.transform.InverseTransformPoint(palm.position);
            Vector3 calculatedFingerPointing = CalculateZeroedVector(distance);
            return calculatedFingerPointing * -1f;
        }

        /**Finds nearest cardinal vector to a vector */
        public static Vector3 CalculateZeroedVector(Vector3 vectorToZero) {
            var zeroed = new Vector3();
            float max = Mathf.Max(Mathf.Abs(vectorToZero.x), Mathf.Abs(vectorToZero.y), Mathf.Abs(vectorToZero.z));
            if (Mathf.Abs(vectorToZero.x) == max) {
                zeroed = (vectorToZero.x < 0) ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
            }
            if (Mathf.Abs(vectorToZero.y) == max) {
                zeroed = (vectorToZero.y < 0) ? new Vector3(0, 1, 0) : new Vector3(0, -1, 0);
            }
            if (Mathf.Abs(vectorToZero.z) == max) {
                zeroed = (vectorToZero.z < 0) ? new Vector3(0, 0, 1) : new Vector3(0, 0, -1);
            }
            return zeroed;
        }

        //Stores a snapshot of original joint positions 
        [ContextMenu("StoreJointsStartPose")]
        public void StoreJointsStartPose() {
            foreach (Transform t in palm.parent.GetComponentsInChildren<Transform>()) {
                jointList.Add(t);
                localRotations.Add(t.localRotation);
                localPositions.Add(t.localPosition);
            }
        }

        //Restores original joint positions, particularly after model has been placed in Leap's editor pose 
        [ContextMenu("RestoreJointsStartPose")]
        public void RestoreJointsStartPose() {
            Debug.Log("RestoreJointsStartPose()");
            for (int i = 0; i < jointList.Count; i++) {
                Transform jointTrans = jointList[i];
                jointTrans.localRotation = localRotations[i];
                jointTrans.localPosition = localPositions[i];
            }
        }

        private void updateDeformPositionsInFingers() {
            var riggedFingers = GetComponentsInChildren<RiggedFinger>();
            foreach (var finger in riggedFingers) {
                finger.deformPosition = deformPositionsInFingers;
            }
        }

        private void updateScaleLastFingerBoneInFingers() {
            var riggedFingers = GetComponentsInChildren<RiggedFinger>();
            foreach (var finger in riggedFingers) {
                finger.scaleLastFingerBone = scaleLastFingerBones;
            }
        }

        // These versions of GetPalmRotation & CalculateRotation return the opposite
        // vector compared to LeapUnityExtension.CalculateRotation.
        // This will be deprecated once LeapUnityExtension.CalculateRotation is
        // flipped in the next release of LeapMotion Core Assets.
        private Quaternion getRiggedPalmRotation() {
            if (hand_ != null) {
                LeapTransform trs = hand_.Basis;
                return calculateRotation(trs);
            }
            if (palm) {
                return palm.rotation;
            }
            return Quaternion.identity;
        }

        private Quaternion calculateRotation(LeapTransform trs) {
            Vector3 up = trs.yBasis.ToVector3();
            Vector3 forward = trs.zBasis.ToVector3();
            return Quaternion.LookRotation(forward, up);
        }

        [Tooltip("When true, hands will be put into a Leap editor pose near the LeapServiceProvider's transform.  When False, the hands will be returned to their Start Pose if it has been saved.")]
        [SerializeField]
        private bool setEditorLeapPose = true;

        public bool SetEditorLeapPose {
            get { return setEditorLeapPose; }
            set {
                if (value == false) {
                    RestoreJointsStartPose();
                }
                setEditorLeapPose = value;
            }
        }

        [Tooltip("When True, hands will be put into a Leap editor pose near the LeapServiceProvider's transform.  When False, the hands will be returned to their Start Pose if it has been saved.")]
        [SerializeField]
        [HideInInspector]
        private bool deformPositionsState = false;

    }
}


