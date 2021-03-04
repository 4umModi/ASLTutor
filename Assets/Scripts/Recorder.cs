﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.UI;
using System.IO;

public class Recorder : MonoBehaviour
{ 
    //Bone Types
    int TYPE_METACARPAL = 0;
    int TYPE_PROXIMAL = 1;
    int TYPE_INTERMEDIATE = 2;
    int TYPE_DISTAL = 3;

    //creates controller variable
    Controller controller;

    //creates list of feature ID variable
    public static List<int> staticFeatureIDs;
    public static List<int> dynamicFeatureIDs;

    //creates array of Hand list frames variable
    List<Hand> rightHandFrames;
    List<Hand> leftHandFrames;

    //creates counter variable
    int handFrameCount;

    //amount of frames skipped when filling hands list (for sake of storage)
    int framesSkip;

    //amount of frames checked in checkpause function
    int pauseAmount;

    //creates max count variable (max array size)
    int maxFrameCount;

    //creates boolean variable for when recording
    bool recording;

    //creates boolean variable for dynamic signs
    static bool isDynamic = false;

    public Toggle dynamicToggle;

    //variable for number of hands
    int numHands;

    //variables for countdown timer
    GameObject countDown;
    int countdownTime;
    bool disableCountdown = true;

    //object of other script
    InputField iField; 

    //THRESHOLD VALUES

    double bentAngle = 0.8;
    double noticableXDisFinger = 0.5;
    double noticableYDisFinger = 0.5;
    double avgXPalmDis = 0.8;
    double avgYPalmDis = 0.8;



    //Coroutine for Countdown timer
    //reference: https://www.youtube.com/watch?v=ulxXGht5D2U
    IEnumerator CountdownToSign()
    {
        //runs start
        Start();

        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        //sets time to 6
        countdownTime = 6;

        //counts down from 5 to 0
        while (countdownTime > 0)
        {
            if (countdownTime == 6)
            {
                countDown.GetComponent<Text>().text = "Please start with the beginning \n handshape after the countdown";
                yield return new WaitForSeconds(2f);
            }

            else
            {
                countDown.GetComponent<Text>().fontSize = 300;
                countDown.GetComponent<Text>().text = countdownTime.ToString();
            }

            yield return new WaitForSeconds(1f);

            countdownTime = countdownTime - 1;


        }

        //tells user to sign
        countDown.GetComponent<Text>().text = "Sign!";

        //waits another second
        yield return new WaitForSeconds(1f);

        //toggles off text
        countDown.gameObject.SetActive(false);

        //sets recording to true
        recording = true;

    }

    //feedback for finished recording to user
    IEnumerator finishedRecordingFeedback()
    {
        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        countDown.GetComponent<Text>().text = "Finished recording!";
        yield return new WaitForSeconds(5f);

        //toggles off text
        countDown.gameObject.SetActive(false);
    }

    //feedback to user when sign has been added
    IEnumerator finishedAddingSignFeedback(string name)
    {
        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        countDown.GetComponent<Text>().text = "Added the sign " + name;
        yield return new WaitForSeconds(5f);

        //toggles off text
        countDown.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

        //finds countdown game object
        if (disableCountdown) countDown = GameObject.Find("CountdownText");

        //sets count down time to 6
        countdownTime = 6;

        //creates controller for leap
        controller = new Controller();

        //creates new list for feature IDs
        staticFeatureIDs = new List<int>();
        dynamicFeatureIDs = new List<int>();

        //sets count to 0 initially
        handFrameCount = 0;

        //sets max frame count to 1000
        maxFrameCount = 2000;

        //creates new array for hand lists of size maxFrameCount
        rightHandFrames = new List<Hand>();
        leftHandFrames = new List<Hand>();

        //sets number of hands to 0 at start
        numHands = 0;

        //sets recording to false at start
        recording = false;

        //sets isDynamic to false at start
        isDynamic = false;

        //because the unity takes in so many frames, we skip a few for storage reasons
        framesSkip = 5;

        //amount of frames we check to consecutively be paused
        pauseAmount = 15;

    }

    //attached to start rec button
    public void startRecording()
    {
        endRecording();
        StartCoroutine(CountdownToSign());
    }

    //attached to end rec button
    public void endRecording()
    {
        recording = false;
        Start();
    }


    //prints out featureID to unity console
    public string getFeatureID(List<int> featureIDs)
    {
        string featureIDString = string.Join(" ", featureIDs);
        return featureIDString;
    }

    //Method to check for left hand.
    //if true returns factor of 1 to add to featureID list b/c all left hand features are even 
    int leftHandFactor(Hand hand)
    {
        int leftHandID = 1;
        if (hand.IsRight || numHands == 1) leftHandID = 0;
        return leftHandID;
    }


    //Method to check if hand is moving or paused
    //if hand is paused for 50 consecutive frames, then end the recording
    bool checkPaused(List<Hand> rightHandFrames, List<Hand> leftHandFrames, int handFrameCount)
    {

        //creates featureID list
        List<int> featureIDList = new List<int>();

        //checks if there is a lefthand, if there is finds featureID for left hand
        bool leftHand = false;
        if (leftHandFrames.Count == rightHandFrames.Count) leftHand = true;

        //creates array of Hand list frames variable
        List<Hand> pausedRightHandFrames = new List<Hand>();
        List<Hand> pausedLeftHandFrames = new List<Hand>();


        for (int i = (handFrameCount - pauseAmount); i < handFrameCount; i++)
        {
            pausedRightHandFrames.Add(rightHandFrames[i]);
            if (leftHand) pausedLeftHandFrames.Add(leftHandFrames[i]);
        }

        featureIDList = checkDynamicFeatureID(pausedRightHandFrames, pausedLeftHandFrames, pauseAmount);

        if (featureIDList.Count == 0) return true;

        return false;
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
        if (angle >= bentAngle) return true;

        //if feature is not present return false
        return false;
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

    //Method for getting feature IDs for Dynamic Bone Features
    //returns list of feature IDs
    List<int> checkBoneDynamic(List<Hand> handFrames, int handFrameCount)
    {
        //creates featureID list
        List<int> featureIDList = new List<int>();

        //gets lefthandID
        int leftHandID = leftHandFactor(handFrames[0]);

        //variables for previous balues
        float yPinkyPrev = 0;
        float yRingPrev = 0;
        float yMiddlePrev = 0;
        float yIndexPrev = 0;
        float xIndexPrev = 0;
        float yThumbPrev = 0;
        float yPrev = 0;
        float xPrev = 0;

        //variables for displacement
        float yPinkyDis = 0;
        float yRingDis = 0;
        float yMiddleDis = 0;
        float yIndexDis = 0;
        float xIndexDis = 0;
        float yThumbDis = 0;

        //loops through all hand frames
        for (int i = 0; i < handFrameCount; i++)
        {
            //gets Hand object from current frame
            Hand hand = handFrames[i];

            //gets list of fingers from hand 
            List<Finger> fingers = hand.Fingers;

            //loops through each finger
            foreach (Finger finger in fingers)
            {
                //gets the distal bone of finger
                Bone bone = finger.Bone((Bone.BoneType)(TYPE_DISTAL));

                //gets current y value of bone
                float yCurr = bone.NextJoint[1];

                //each if statements checks finger type, and then 
                //finds displacement between the last two frames
                //and adds to respective fingers displacement variable

                if (finger.Type == Finger.FingerType.TYPE_PINKY)
                {
                    yPrev = yPinkyPrev;
                    yPinkyPrev = yCurr;
                    if (yPrev == 0) break;
                    yPinkyDis += Math.Abs(yPrev - yCurr);
                }

                if (finger.Type == Finger.FingerType.TYPE_RING)
                {
                    yPrev = yRingPrev;
                    yRingPrev = yCurr;
                    if (yPrev == 0) break;
                    yRingDis += Math.Abs(yPrev - yCurr);
                }

                if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
                {
                    yPrev = yMiddlePrev;
                    yMiddlePrev = yCurr;
                    if (yPrev == 0) break;
                    yMiddleDis += Math.Abs(yPrev - yCurr);
                }

                if (finger.Type == Finger.FingerType.TYPE_INDEX)
                {
                    float xCurr = bone.NextJoint[0];
                    xPrev = xIndexPrev;
                    xIndexPrev = xCurr;

                    yPrev = yIndexPrev;
                    yIndexPrev = yCurr;
                    if (yPrev == 0) break;

                    xIndexDis += Math.Abs(xPrev - xCurr);
                    yIndexDis += Math.Abs(yPrev - yCurr);
                }
                if (finger.Type == Finger.FingerType.TYPE_THUMB)
                {
                    yPrev = yThumbPrev;
                    yThumbPrev = yCurr;
                    if (yPrev == 0) break;
                    yThumbDis += Math.Abs(yPrev - yCurr);
                }
            }
        }

        //If the average displacement is above a threshold, add the feature ID of the finger's y or x displacement
        //the left hand factor just checks if it is a left hand and adds 1 if that is the case
        if (yPinkyDis / handFrameCount >= noticableYDisFinger) featureIDList.Add(35 + leftHandID);
        if (yRingDis / handFrameCount >= noticableYDisFinger) featureIDList.Add(37 + leftHandID);
        if (yMiddleDis / handFrameCount >= noticableYDisFinger) featureIDList.Add(39 + leftHandID);
        if (yIndexDis / handFrameCount >= noticableYDisFinger) featureIDList.Add(41 + leftHandID);
        if (yThumbDis / handFrameCount >= noticableYDisFinger) featureIDList.Add(43 + leftHandID);
        if (xIndexDis / handFrameCount >= noticableYDisFinger) featureIDList.Add(45 + leftHandID);

        //returns featureID list
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

    //Method that returns feature IDs related to dynamic palm features
    List<int> checkPalmDynamic(List<Hand> handFrames, int handFrameCount)
    {
        //creates featureIDlist
        List<int> featureIDList = new List<int>();

        int leftHandID = leftHandFactor(handFrames[0]);

        //initial x and z displacement values
        float xDisplacement = 0;
        float zDisplacement = 0;

        //intial palm position values
        float xPrev = (handFrames[0].PalmPosition)[0];
        float zPrev = (handFrames[0].PalmPosition)[2];

        //intializes x and y mean values
        float xMean = 0;
        float zMean = 0;

        for (int i = 1; i < handFrameCount; i++)
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

        //testing and debugging statements
        //Debug.Log("X: "+ xMean);
        //Debug.Log("Z: " + zMean);

        //if the features are present than there was more than .8 avg movement in the frames
        //if they are present, add them to the feature list and return it
        if (xMean > avgXPalmDis) featureIDList.Add(31 + leftHandID);
        if (zMean > avgYPalmDis) featureIDList.Add(33 + leftHandID);

        return featureIDList;

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
        if (handNum == 2)
        {
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

    //Checks for all dynamic feature IDs
    //Returns list of feature IDs that classify the sign 
    List<int> checkDynamicFeatureID(List<Hand> rightHandFrames, List<Hand> leftHandFrames, int handFrameCount)
    {
        //creates feature ID list
        List<int> featureIDList = new List<int>();

        //checks if there is a lefthand, if there is finds featureID for left hand
        bool leftHand = false;
        if (leftHandFrames.Count == rightHandFrames.Count) leftHand = true;

        //calls method to get Dynamic Palm Feature IDs and adds to featureIDlist
        featureIDList.AddRange(checkPalmDynamic(rightHandFrames, handFrameCount));
        if (leftHand) featureIDList.AddRange(checkPalmDynamic(leftHandFrames, handFrameCount));

        //calls method to get Dynamic finger/bone feature IDs and adds to featureID list
        featureIDList.AddRange(checkBoneDynamic(rightHandFrames, handFrameCount));
        if (leftHand) featureIDList.AddRange(checkBoneDynamic(leftHandFrames, handFrameCount));

        //returns featureIDlist
        return featureIDList;
    }

    //adds sign information to text file
    public void addSign()
    {
        //if no static or dynamic features return
        if (staticFeatureIDs.Count == 0 && dynamicFeatureIDs.Count == 0) return;

        //find value of dynamic toggle
        Toggle d = dynamicToggle.GetComponent<Toggle>();
        isDynamic = d.isOn;

        //get the name of the string
        string name;
        iField = GameObject.Find("SignName").GetComponent<InputField>();
        name = iField.text;

        //if there is no string name, return
        if (name.Length == 0) return;

        //get featureList
        List<int> featureList = new List<int>();

        //add static features
        featureList.AddRange(staticFeatureIDs);

        //add dynamic features
        if (isDynamic) featureList.AddRange(dynamicFeatureIDs);
        string features = getFeatureID(featureList);

        //create line for text file
        string line;
        if (isDynamic) line = name + " " + "1" + " " + features;
        else line = name + " " + "0" + " " + features;

        //if line is full of only dynamic feauters and spaces, return
        if (line.Length < 4) return;

        //write to file
        string filePath = "Assets/Scripts/signs.txt";
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(line);

        //close file
        writer.Close();

        Debug.Log(line);
        //inform user that the sign has been added
        StartCoroutine((finishedAddingSignFeedback(name)));

    }



// Update is called once per frame
    public void Update()
    {
        //toggles off countdown timer game object initially (there is an error doing this in start)
        if (disableCountdown)
        {
            disableCountdown = false;
            countDown.gameObject.SetActive(false);

        }

        //this value will be true when recording button has been pressed (turns false when finished)
        if (!recording) return;

        //gets frame from leap controller
        Frame frame = controller.Frame();

        //gets list of hands in frame
        List<Hand> hands = frame.Hands;

        Toggle d = dynamicToggle.GetComponent<Toggle>();
        isDynamic = d.isOn;

        //gets number of hands
        numHands = hands.Count;

        //if no hands, nothing to track
        if (numHands == 0) return;

        //static signs will be immediately categorized by frame
        //if dynamic will find static features for first frame
        if (recording && (handFrameCount % 800 == 0) && (!isDynamic || handFrameCount == 0))
        {
            //gets static features
            staticFeatureIDs = checkStaticFeatureID(hands);

            if (!isDynamic)
            {
                StartCoroutine(finishedRecordingFeedback());
                recording = false;
            }
        }

        //dynamic signs will read in all frames until hand is paused
        else if (recording && isDynamic)
        {
            
            //puts right hand at 0 index and left hand at 1 index
            hands = twoHandsStatic(hands);

            //fills hand with frames
            if (handFrameCount % framesSkip == 0)
            {
                rightHandFrames.Add(hands[0]);
                if (hands.Count == 2) leftHandFrames.Add(hands[1]);
            }

            //if there are enough frames and the hand is paused then find dynamic features
            if (handFrameCount / framesSkip >= 50 && checkPaused(rightHandFrames, leftHandFrames, handFrameCount / framesSkip))
            {
                //gets dynamic features
                dynamicFeatureIDs = checkDynamicFeatureID(rightHandFrames, leftHandFrames, handFrameCount / framesSkip);

                StartCoroutine(finishedRecordingFeedback());
                recording = false;
            }

        }
        handFrameCount += 1;
    }
}

