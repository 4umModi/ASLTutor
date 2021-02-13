using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class Classifier : MonoBehaviour
{
    //leap finger types
    int TYPE_THUMB = 0;
    int TYPE_INDEX = 1;
    int TYPE_MIDDLE = 2;
    int TYPE_RING = 3;
    int TYPE_PINKY = 4;

    //creates controller variable
    Controller controller;

    //creates list of feature ID variable
    List<int> featureIDs;

    //creates array of Hand list frames variable
    List<Hand>[] handFrames;

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
        handFrames = new List<Hand>[maxFrameCount];

        recording = false;

        isDynamic = false;
    
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
    bool checkPaused(List<Hand>[] handFrames)
    {

        //code to check if hand is paused
        //check if there was movement in the last 50 frames 

        //check if hand is paused
        return false;
    }

    bool isBent(Finger finger)
    {
        //find intermediate bone direction
        //find procimal bone direction
        //find angle between two directions
        //if above a threshold of .8 radians, then feature is present
        return false;
    }

    List<int> checkFingersStatic(Hand hand)
    {
        List<int> featureIDList = new List<int>();

        //gets the value if lefthand (makes featureID even)
        int leftHandFactor = leftHandFactor(hand);

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
            if (finger.IsExtended) featureIDList.Add(featureID + leftHandFactor(hand));
            if (isBent(finger)) featureIDList.Add(featureID + bentFeatureFactor + leftHandFactor(hand));

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

    List<int> checkDynamicFeatureID(List<Hand>[] handFrames, int handFrameCount)
    {
        List<int> featureIDList = new List<int>();
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
        bool isDynamic = false;

        //gets frame from leap controller
        Frame frame = controller.Frame();

        //gets list of hands in frame
        List<Hand> hands = frame.Hands;

        //if no hands, nothing to track
        if (hands.Count == 0) return;

        //static signs will be immediately categorized by frame
        if (recording && (!isDynamic))
        {
            featureIDs = checkStaticFeatureID(hands);
        }

        //dynamic signs will read in all frames until hand is paused
        else if (recording && isDynamic)
        {
            //fills array as list of hands for every frame
            handFrames[handFrameCount] = hands;
            handFrameCount++;
            if (handFrameCount == maxFrameCount) handFrameCount = 0;
            
            if (handFrameCount >= 200 && checkPaused(handFrames)) 
            {
                featureIDs = checkDynamicFeatureID(handFrames, handFrameCount);
            }
        }
    }
}
