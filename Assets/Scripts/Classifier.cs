using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class Classifier : MonoBehaviour
{
    //creates controller variable
    Controller controller;

    List<int> featureIDs;
    List<Hand>[] handFrames;
    int handFrameCount;

    // Start is called before the first frame update
    void Start()
    {
        //creates controller for leap
        controller = new Controller();
        featureIDs = new List<int>();
        handFrames = new List<Hand>[300];
        handFrameCount = 0;
    }

    bool checkPaused(List<Hand>[] handFrames, bool isDynamic)
    {

        //check if hand is paused
        return false;
    }

    List<int> checkFingers(List<Hand>[] handFrames, int handFrameCount)
    {
        List<int> featureIDList = new List<int>();
        return featureIDList;
    }

    List<int> checkPalmFacingLeap(List<Hand>[] handFrames, int handFrameCount)
    {
        List<int> featureIDList = new List<int>();
        return featureIDList;
    }

    //Method returning featureIDs for two hand feature
    //the feature exists if for all frames there are two hands
    List<int> twoHands(List<Hand>[] handFrames, int handFrameCount) 
    {
        List<int> featureIDList = new List<int>();
        bool twoHands = true;

        //checks two hands for frames 0 to handFrameCount
        for (int i = 0; i < handFrameCount; i++)
        {
            //if a frame is not filled yet, breaks from loop
            if (handFrames[i].Count == 0) break;

            //if there aren't two hands, breaks and sets twoHands to false
            if (handFrames[i].Count < 2)
            {
                twoHands = false;
                break;
            }

        }

        for (int i = 300; i >= handFrameCount; i--)
        {
            //if a frame is not filled yet, breaks from loop
            if (handFrames[i].Count == 0) break;

            //if there aren't two hands, breaks and sets twoHands to false
            if (handFrames[i].Count < 2)
            {
                twoHands = false;
                break;
            }
        }

        if (twoHands) featureIDList.Add(0);
        return featureIDList;
    }




    List<int> checkFeatureID(List<Hand>[] handFrames, int handFrameCount, bool isDynamic)
    {
        List<int> featureIDList = new List<int>();

        featureIDList.AddRange(twoHands(handFrames, handFrameCount));
        featureIDList.AddRange(checkPalmFacingLeap(handFrames, handFrameCount));
        featureIDList.AddRange(checkFingers(handFrames, handFrameCount));

        //if (isDynamic)
            //add dynamic feature methods


        return featureIDList;
        
    }



    // Update is called once per frame
    void Update()
    {
        Frame frame = controller.Frame();
        List<Hand> hands = frame.Hands;

        //if no hands, nothing to track
        if (hands.Count == 0) return;

        //fills array as list of hands for every frame
        handFrames[handFrameCount] = hands;
        handFrameCount++;
        if (handFrameCount == 300) handFrameCount = 0;


        //this value will come from the selected sign
        bool isDynamic = false;

        if (handFrameCount >= 50)
        {
            if (!(checkPaused(handFrames, isDynamic)))
            {

                featureIDs = checkFeatureID(handFrames, handFrameCount, isDynamic);

            }
        }

    }
}
