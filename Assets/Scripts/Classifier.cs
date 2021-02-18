using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class Classifier : MonoBehaviour
{
    //Bone Types
    int TYPE_METACARPAL = 0;
    int TYPE_PROXIMAL = 1;
    int TYPE_INTERMEDIATE = 2;
    int TYPE_DISTAL = 3;

    //creates controller variable
    Controller controller;

    //creates list of feature ID variable
    List<int> featureIDs;

    //creates array of Hand list frames variable
    List<Hand> rightHandFrames;
    List<Hand> leftHandFrames;

    //creates counter variable
    int handFrameCount;

    //creates max count variable (max array size)
    int maxFrameCount;

    //creates boolean variable for when recording
    bool recording;

    //creates boolean variable for dynamic signs
    bool isDynamic;

    // Start is called before the first frame update
    void Start()
    {
        //creates controller for leap
        controller = new Controller();

        //creates new list for feature IDs
        featureIDs = new List<int>();

        //sets count to 0 initially
        handFrameCount = 0;

        //sets max frame count to 1000
        maxFrameCount = 2000;

        //creates new array for hand lists of size maxFrameCount
        rightHandFrames = new List<Hand>();
        leftHandFrames = new List<Hand>();

        recording = false;

        isDynamic = false;
    
    }

    //prints out featureID to unity console
    void printFeatureID(List<int> featureIDs)
    {
        string featureIDString = string.Join(",", featureIDs);
        Debug.Log(featureIDString);
    }

    //Method to check for left hand.
    //if true returns factor of 1 to add to featureID list b/c all left hand features are even 
    int leftHandFactor(Hand hand)
    {
        int leftHandID = 1;
        if (hand.IsRight) leftHandID = 0;
        return leftHandID;
    }


    //Method to check if hand is moving or paused
    //if hand is paused for 50 consecutive frames, then end the recording
    bool checkPaused(List<Hand> rightHandFrames, List<Hand> leftHandFrames)
    {

        //code to check if hand is paused
        //check if there was movement in the last 50 frames 

        //check if hand is paused
        return true;
    }

    bool isBent(Finger finger)
    {
        //find intermediate bone
        Bone intermediateBone = finger.Bone((Bone.BoneType)(TYPE_PROXIMAL));
        
        //find proximal bone
        Bone proximalBone = finger.Bone((Bone.BoneType)(TYPE_INTERMEDIATE));

        //find angle between direction of both bones
        float angle = (intermediateBone.Direction).AngleTo(proximalBone.Direction);

        //if above a threshold of .8 radians, then feature is present
        if (angle >= .8) return true;

        //if feature is not present return false
        return false;
    }

    List<int> checkFingersStatic(Hand hand)
    {
        List<int> featureIDList = new List<int>();

        //gets the value if lefthand (makes featureID even)
        int leftHandID = leftHandFactor(hand);

        //creates variable for featureID number
        int featureID = 0;

        //bent feature for each finger is always the extended feature + 2
        int bentFeatureFactor = 2;

        //iterates through list of fingers
        List<Finger> fingers = hand.Fingers;
        foreach (Finger finger in fingers)
        {

            //assigns feature ID number based on finger
            if (finger.Type == Finger.FingerType.TYPE_PINKY) featureID = 3;
            if (finger.Type == Finger.FingerType.TYPE_RING) featureID = 7;
            if (finger.Type == Finger.FingerType.TYPE_MIDDLE) featureID = 11;
            if (finger.Type == Finger.FingerType.TYPE_INDEX) featureID = 15;
            if (finger.Type == Finger.FingerType.TYPE_THUMB) featureID = 19;

            //adds feature ID numbers for corresponding fingers based upon if they are bent or extended
            if (finger.IsExtended) featureIDList.Add(featureID + leftHandID);
            if (isBent(finger)) featureIDList.Add(featureID + bentFeatureFactor + leftHandID);

        }

        return featureIDList;
    }


    //method to check if the Palm is Facing the leap
    //returns present featureIDs
    List<int> checkPalmFacingLeapStatic(Hand hand)
    {
        
        //create featureID list
        List<int> featureIDList = new List<int>();

        //find palm normal vector
        Vector palmNormal = hand.PalmNormal;

        //find Y value of palm normal vector
        float palmNormalY = palmNormal[1];

        //if palm normal Y value is less than 0, the palm is facing the leap, add to featureID list
        if (palmNormalY < 0) featureIDList.Add(1 + leftHandFactor(hand));

        //return featureID list
        return featureIDList;
    }


    //Method to organize hands 
    //if there are two hands, puts right hand at index 0 and left hand at index 1
    List<Hand> twoHandsStatic(List<Hand> hands)
    {
        if (hands.Count < 2) return hands;
        
        Hand leftHand = new Hand();
        Hand rightHand = new Hand();

        if (hands[0].IsLeft)
        {
            leftHand = hands[0];
            rightHand = hands[1];
            hands[0] = rightHand;
            hands[1] = leftHand;
        }

        return hands;
    }

    //Checks for all static feature IDs
    //Returns list of feature IDs that classify the sign
    List<int> checkStaticFeatureID(List<Hand> hands)
    {
        List<int> featureIDList = new List<int>();

        hands = twoHandsStatic(hands);
        int handNum = hands.Count;

        //finding feature IDs for right hand (or 1 hand)
        featureIDList.AddRange(checkPalmFacingLeapStatic(hands[0]));
        featureIDList.AddRange(checkFingersStatic(hands[0]));

        //finding feature IDs for left hand
        if (handNum == 2) {
            //featureID for twohands being present
            featureIDList.Add(0);

            //adds feature IDs for palm facing leap if present
            featureIDList.AddRange(checkPalmFacingLeapStatic(hands[1]));

            //adds finger feature IDs
            featureIDList.AddRange(checkFingersStatic(hands[1]));
        }

        //returns full list of featureIDs
        return featureIDList;
        
    }

    //Method that returns feature IDs related to dynamic palm features
    List<int> checkPalmDynamic(List<Hand> handFrames, int handFrameCount)
    {
        //creates featureIDlist
        List<int> featureIDList = new List<int>();

        //initial x and z displacement values
        float xDisplacement = 0;
        float zDisplacement = 0;

        //intial palm position values
        float xPrev = (handFrames[0].PalmPosition)[0];
        float zPrev = (handFrames[0].PalmPosition)[2];

        //intializes x and y mean values
        float xMean = 0;
        float zMean = 0;

        for (int i=1; i<handFrameCount; i++)
        {
            //gets Hand object from current frame
            Hand hand = handFrames[i];

            //gets vector of plam position
            float xCurr = hand.PalmPosition[0];
            float zCurr = hand.PalmPosition[2];

            //gets X,Z coordinates and adds to total
            xDisplacement += Math.Abs(xPrev - xCurr);
            zDisplacement += Math.Abs(zPrev - zCurr);

            //set current values to previous values
            xPrev = xCurr;
            zPrev = zCurr;

        }

        //finds avg displacement for each feature
        xMean = xDisplacement / handFrameCount;
        zMean = zDisplacement / handFrameCount;

        //if the features are present than there was more than 2 avg movement in the frames
        //if they are present, add them to the feature list and return it
        if (xMean > 2) featureIDList.Add(31 + leftHandFactor(handFrames[0]));
        if (zMean > 2) featureIDList.Add(33 + leftHandFactor(handFrames[0]));

        return featureIDList;

    }

    List <int> checkBoneDynamic(List<Hand> handFrames, int handFrameCount)
    {
        List<int> featureIDList = new List<int>();

        for (int i = 0; i < handFrameCount; i++)
        {
            //gets Hand object from current frame
            Hand hand = handFrames[i];
            List<Finger> fingers = hand.Fingers;

            int featureID = 0;
            float yPinkyPrev = 0;
            float yPrev = 0;

            foreach (Finger finger in fingers)
            {

                //Bone bone = finger.Bone.BoneType(TYPE_METACARPAL);

                float yCurr = 0;// (finger.Bone(TYPE_METACARPAL).NextJoint)[1];

                if (finger.Type == Finger.FingerType.TYPE_PINKY)
                {
                    yPrev = yPinkyPrev;
                    yPinkyPrev = yCurr;
                }
                /*if (finger.Type == Finger.FingerType.TYPE_RING) 
                if (finger.Type == Finger.FingerType.TYPE_MIDDLE) 
                if (finger.Type == Finger.FingerType.TYPE_INDEX) 
                if (finger.Type == Finger.FingerType.TYPE_THUMB)*/
                if (yPrev == 0) break;
            }
             
        }
        return featureIDList;
    }

    List<int> checkDynamicFeatureID(List<Hand> rightHandFrames, List<Hand> leftHandFrames, int handFrameCount)
    {
        List<int> featureIDList = new List<int>();

        bool leftHand = false;
        if (leftHandFrames.Count == rightHandFrames.Count) leftHand = true;



        featureIDList.AddRange(checkPalmDynamic(rightHandFrames, handFrameCount));
        if (leftHand) featureIDList.AddRange(checkPalmDynamic(rightHandFrames, handFrameCount));

        //checkBoneDynamic();


        return featureIDList;
    }


    // Update is called once per frame
    void Update()
    {

        //this value will be true when recording button has been pressed (turns false when pressed again)\
        //will be implemented later
        recording = true;
        if (!recording) return;

        //this value will come from the selected sign
        //will be implemented later
        isDynamic = true;

        //gets frame from leap controller
        Frame frame = controller.Frame();

        //gets list of hands in frame
        List<Hand> hands = frame.Hands;

        //if no hands, nothing to track
        if (hands.Count == 0) return;

        //static signs will be immediately categorized by frame
        //if dynamic will find static features for first frame
        if (recording && (!isDynamic || handFrameCount == 0)) featureIDs = checkStaticFeatureID(hands);

        //dynamic signs will read in all frames until hand is paused
        else if (recording && isDynamic)
        {
            //fills array as list of hands for every frame
            List<int> staticFeatureIDs;

            //puts right hand at 0 index and left hand at 1 index
            hands = twoHandsStatic(hands);

            //fills hand with frames
            rightHandFrames.Add(hands[0]);
            if (hands.Count == 2) leftHandFrames.Add(hands[1]);

            //if there are enough frames and the hand is paused then find dynamic features
            if (handFrameCount >= 200 && checkPaused(rightHandFrames, leftHandFrames)) 
            {
                featureIDs = checkDynamicFeatureID(rightHandFrames, leftHandFrames, handFrameCount);
            }
        }
        
        //increments handFrameCount
        handFrameCount += 1;

        //printFeatureID(featureIDs);
    }
}
