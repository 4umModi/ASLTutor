using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script will give feedback based upon the featureIDs expected and the ones present in the users recorded signing
public class Feedback : MonoBehaviour
{
    //called countdown because it uses the coundown text to display
    public GameObject countDown;
    
    //Buttons for button handling
    Button recButton;
    Button feedbackButton;

    //list of all feedback
    public static List<string> allFeedback;
    public static int count;

    //If there is no feedback, tell the user directly on the screen
    IEnumerator userFeedback(string feedback)
    {
  
        recButton.interactable = false;
        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        //informs the user they have done the sign correctly
        countDown.GetComponent<Text>().text = feedback;

        //wait 5 seconds
        yield return new WaitForSeconds(4f);

        //toggles off text
        countDown.gameObject.SetActive(false);
        recButton.interactable = true;
        feedbackButton.interactable = true;
    }

    public void Start()
    {
        allFeedback = new List<string>();
        count = 0;
        recButton = GameObject.Find("StartRecButton").GetComponent<Button>();
        feedbackButton = GameObject.Find("Feedback").GetComponent<Button>();
    }

    //prints out featureID to unity console
    public void printFeatureID(List<int> featureIDs)
    {
        string featureIDString = string.Join(",", featureIDs);
        //Debug.Log(featureIDString);
    }

    //gets the feature IDs and other information from classifier script
    public void getFeatureIDs(List<int> staticFeatureIDs, List<int> dynamicFeatureIDs, string name, bool isDynamic, List<int> expectedFeatureIDs)
    {
        List<int> featureIDs = staticFeatureIDs; 
        List<int> missingFeatureIDs = new List<int>();
        List<int> extraFeatureIDs = new List<int>();

        featureIDs.AddRange(dynamicFeatureIDs);
        /*Debug.Log("In Feedback:");
        printFeatureID(featureIDs);
        Debug.Log("Expected");
        printFeatureID(expectedFeatureIDs);*/

        missingFeatureIDs = findMissing(featureIDs, expectedFeatureIDs);
        extraFeatureIDs = findExtra(featureIDs, expectedFeatureIDs);

        /*printFeatureID(missingFeatureIDs);
        printFeatureID(extraFeatureIDs);*/

        giveFeedback(missingFeatureIDs, extraFeatureIDs, isDynamic, expectedFeatureIDs.Contains(0));
    }

    public void giveFeedback(List<int> missing, List<int> extra, bool isDynamic, bool twoHands)
    {
        //if there are no missing or extra features then the user has correctly done the sing
        if (missing.Count == 0 && extra.Count == 0)
        {
            //gives on screen feedback of doing sign correctly
            StartCoroutine(userFeedback("You have done the sign correctly!"));
            feedbackButton.interactable = false;
            return;
        }

        //strings for appending feedback
        string endOfFeedback = "";
        string precedingFeedback = "";

        //if there are two hands, feedback must specify the right hand
        if (twoHands) precedingFeedback = " Right ";

        //if the sign is dynamic all static features are only for the first frame
        if (isDynamic) endOfFeedback = " when you begin the sign";

        //creates an array of feedback for missing features
        string[] feedback = new string[50];

        //creates an array of feedback for extra features
        string[] feedbackExtra = new string[50];

        //Static features
        feedback[0] = ("Both hands need to be over the sensor");
        feedback[1] = (precedingFeedback + "Palm should be facing the sensor" + endOfFeedback);
        feedback[2] = ("Left Palm should be facing the sensor" + endOfFeedback);
        feedback[3] = (precedingFeedback + "Hand should Extend Pinky" + endOfFeedback);
        feedback[4] = ("Left Hand should Extend Pinky" + endOfFeedback);
        feedback[5] = (precedingFeedback + "Hand should Bend Pinky" + endOfFeedback);
        feedback[6] = ("Left Hand should Bend Pinky" + endOfFeedback);
        feedback[7] = (precedingFeedback + "Hand should Extend Ring Finger" + endOfFeedback);
        feedback[8] = ("Left Hand should Extend Ring Finger" + endOfFeedback);
        feedback[9] = (precedingFeedback + "Hand should Bend Ring Finger" + endOfFeedback);
        feedback[10] = ("Left Hand should Bend Ring Finger" + endOfFeedback);
        feedback[11] = (precedingFeedback + "Hand should Extend Middle Finger" + endOfFeedback);
        feedback[12] = ("Left Hand should Extend Middle Finger" + endOfFeedback);
        feedback[13] = (precedingFeedback + "Hand should Bend Middle Finger" + endOfFeedback);
        feedback[14] = ("Left Hand should Bend Middle Finger" + endOfFeedback);
        feedback[15] = (precedingFeedback + "Hand should Extend Index Finger" + endOfFeedback);
        feedback[16] = ("Left Hand should Extend Index Finger" + endOfFeedback);
        feedback[17] = (precedingFeedback + "Hand should Bend Index Finger" + endOfFeedback);
        feedback[18] = ("Left Hand should Bend Index Finger" + endOfFeedback);
        feedback[19] = (precedingFeedback + "Hand should Extend Thumb" + endOfFeedback);
        feedback[20] = ("Left Hand should Extend Thumb" + endOfFeedback);
        feedback[21] = (precedingFeedback + "Hand should Bend Thumb" + endOfFeedback);
        feedback[22] = ("Left Hand should Bend Thumb" + endOfFeedback);
        feedback[23] = (precedingFeedback + " Hand should have fingers facing up" + endOfFeedback);
        feedback[24] = ("Left Hand should have fingers facing up" + endOfFeedback);
        feedback[25] = (precedingFeedback + "Index and Middle Fingers should not be touching" + endOfFeedback);
        feedback[26] = ("Left Index and Middle Fingers should not be touching" + endOfFeedback);

        //dynamic features
        feedback[31] = ("Are you moving your" + precedingFeedback + "palm correctly (left/right direction)?");
        feedback[32] = ("Are you moving your left palm correctly (left/right direction)?");
        feedback[33] = ("Are you moving your" + precedingFeedback + "palm correctly (towards/away from sensor direction)?");
        feedback[34] = ("Are you moving your left palm correctly (towards/away from sensor direction)?");
        feedback[35] = ("Are you moving your" + precedingFeedback + "pinky correctly (up/down direction)?");
        feedback[36] = ("Are you moving your left pinky correctly (up/down direction)?");
        feedback[37] = ("Are you moving your" + precedingFeedback + "ring finger correctly (up/down direction)?");
        feedback[38] = ("Are you moving your left ring finger correctly (up/down direction)?");
        feedback[39] = ("Are you moving your" + precedingFeedback + "middle finger correctly (up/down direction)?");
        feedback[40] = ("Are you moving your left middle finger correctly (up/down direction)?");
        feedback[41] = ("Are you moving your" + precedingFeedback + "index finger correctly (up/down direction)?");
        feedback[42] = ("Are you moving your left index finger correctly (up/down direction)?");
        feedback[43] = ("Are you moving your" + precedingFeedback + "thumb correctly (up/down direction)?");
        feedback[44] = ("Are you moving your left thumb correctly (up/down direction)?");
        feedback[45] = ("Are you moving your" + precedingFeedback + "index finger correctly (left/right direction)?");
        feedback[46] = ("Are you moving your left index finger correctly (right/left direction)?");

        //feedback for static features if they are presenet but shouldn't be
        feedbackExtra[0] = ("Only one hand should be over the sensor");
        feedbackExtra[1] = (precedingFeedback + "Palm should be facing away from the sensor" + endOfFeedback);
        feedbackExtra[2] = ("Left Palm should be facing away from the sensor" + endOfFeedback);
        feedbackExtra[3] = (precedingFeedback + "Hand should NOT Extend Pinky" + endOfFeedback);
        feedbackExtra[4] = ("Left Hand should NOT Extend Pinky" + endOfFeedback);
        feedbackExtra[5] = (precedingFeedback + "Hand should NOT Bend Pinky" + endOfFeedback);
        feedbackExtra[6] = ("Left Hand should NOT Bend Pinky" + endOfFeedback);
        feedbackExtra[7] = (precedingFeedback + "Hand should NOT Extend Ring Finger" + endOfFeedback);
        feedbackExtra[8] = ("Left Hand should NOT Extend Ring Finger" + endOfFeedback);
        feedbackExtra[9] = (precedingFeedback + "Hand should NOT Bend Ring Finger" + endOfFeedback);
        feedbackExtra[10] = ("Left Hand should NOT Bend Ring Finger" + endOfFeedback);
        feedbackExtra[11] = (precedingFeedback + "Hand should NOT Extend Middle Finger" + endOfFeedback);
        feedbackExtra[12] = ("Left Hand should NOT Extend Middle Finger" + endOfFeedback);
        feedbackExtra[13] = (precedingFeedback + "Hand should NOT Bend Middle Finger" + endOfFeedback);
        feedbackExtra[14] = ("Left Hand should NOT Bend Middle Finger" + endOfFeedback);
        feedbackExtra[15] = (precedingFeedback + "Hand should NOT Extend Index Finger" + endOfFeedback);
        feedbackExtra[16] = ("Left Hand should NOT Extend Index Finger" + endOfFeedback);
        feedbackExtra[17] = (precedingFeedback + "Hand should NOT Bend Index Finger" + endOfFeedback);
        feedbackExtra[18] = ("Left Hand should NOT Bend Index Finger" + endOfFeedback);
        feedbackExtra[19] = (precedingFeedback + "Hand should NOT Extend Thumb" + endOfFeedback);
        feedbackExtra[20] = ("Left Hand should NOT Extend Thumb" + endOfFeedback);
        feedbackExtra[21] = (precedingFeedback + "Hand should NOT Bend Index Thumb" + endOfFeedback);
        feedbackExtra[22] = ("Left Hand should NOT Bend Index Thumb" + endOfFeedback);
        feedbackExtra[23] = (precedingFeedback + " Hand should NOT have fingers facing up" + endOfFeedback);
        feedbackExtra[24] = ("Left Hand should NOT have fingers facing up" + endOfFeedback);
        feedbackExtra[25] = (precedingFeedback + " Index and Middle Fingers should be touching" + endOfFeedback);
        feedbackExtra[26] = ("Left Index and Middle Fingers should be touching" + endOfFeedback);

        if (extra.Contains(0) || missing.Contains(0))
        {
            string line = feedbackExtra[0];
            if (missing.Contains(0)) line = feedback[0];
            StartCoroutine(userFeedback(line));
            return;
        }

        //get the feedback for missing features
        if (missing.Count > 0)
        {
            for (int i = 0; i < missing.Count; i++)
            {
                allFeedback.Add(feedback[(missing[i])].Trim());
                count = count + 1;
            }
        }

        bool offFeedbackButton = false;

        //gets the feedback for extra features present
        if (extra.Count > 0)
        {
            for (int i = 0; i < extra.Count; i++)
            {
                if (i < 30)
                {
                    if (extra[i] == 0) { allFeedback.Add(feedbackExtra[(extra[i])].Trim()); offFeedbackButton = true; }
                }
                else allFeedback.Add(feedback[(extra[i])].Trim());
                count = count + 1;
            }
        }

        if (allFeedback.Count == 0) {StartCoroutine(userFeedback("You have done the sign correctly!")); feedbackButton.interactable = false; return; }
        StartCoroutine(userFeedback("You have done the Sign incorrectly. \n Click Get Feedback or Learn to improve!"));
        feedbackButton.interactable = offFeedbackButton;
    }

    public void getFeedback()
    {
        //object of another script
        generateFeedback sendFeedback;

        //allows us to call other script, that will hold sign information
        sendFeedback = GameObject.FindGameObjectWithTag("TabB").GetComponent<generateFeedback>();

        sendFeedback.showFeedback(allFeedback);
    }

    //compares array to find features that are expected but not present
    //returns list of features that are missing
    List<int> findMissing(List<int> featureIDs, List<int> expectedFeatureIDs)
    {
        List<int> missingFeatureIDs = new List<int>(); 
        for (int i = 0; i < expectedFeatureIDs.Count; i++)
        {
            bool contain = featureIDs.Contains(expectedFeatureIDs[i]);
            if (!contain) missingFeatureIDs.Add(expectedFeatureIDs[i]);
        }
        return missingFeatureIDs;
    }

    //compares array to find features that are present but not expected
    //returns list of features that are extra
    List<int> findExtra(List<int> featureIDs, List<int> expectedFeatureIDs)
    {
        List<int> extraFeatureIDs = new List<int>();
        for (int i = 0; i < expectedFeatureIDs.Count; i++)
        {

            int index = featureIDs.IndexOf(expectedFeatureIDs[i]);
            if (index != -1)
            {
                featureIDs[index] = -1;
            }
        }

        for (int i = 0; i < featureIDs.Count; i++)
        {
            if (featureIDs[i] != -1) extraFeatureIDs.Add(featureIDs[i]);
        }

        return extraFeatureIDs;
    }
}
