using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class Recorder : MonoBehaviour
{ 

    //for taking screenshots
    private static Recorder instance;
    Camera cam;

    //Bone Types
    int TYPE_METACARPAL = 0;
    int TYPE_PROXIMAL = 1;
    int TYPE_INTERMEDIATE = 2;
    int TYPE_DISTAL = 3;

    //For camera control when taking images
    public Camera mainCamera;
    public Camera frontCamera;
    public Camera backCamera;
    public Camera side1Camera;
    public Camera side2Camera;
    public Canvas mainCanvas;
    public Canvas imageCanvas;
    public Button nextFrame;
    public Text dynamicText;
    public int dynamicImageCounter = 0;
    public UnityEngine.UI.Image signbox;
    public UnityEngine.UI.Image signbox2;
    public Button done;
    public Text userText;
    public Canvas imagePreview;
    public GameObject frontIMG;
    public Text signname;
    public Text dynamicValue;

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
    public static bool isDynamic = false;

    public Toggle dynamicToggle;
    public Toggle imagesToggle;

    //variable for number of hands
    int numHands;

    //variables for countdown timer
    GameObject countDown;
    public int countdownTime;
    bool disableCountdown = true;

    //object of other script
    InputField iField;

    //THRESHOLD VALUES

    double bentAngle = 0.8;
    double noticableXDisFinger = 0.5;
    double noticableYDisFinger = 0.5;
    double avgXPalmDis = 0.8;
    double avgYPalmDis = 0.8;
    double angleThreshold = 0.1;

    Button recButton;
    Button addButton;

    //to get postions of hand for generating a recording of the sign
    public static List<float> leftAnimationCoords = new List<float>();
    public static List<float> rightAnimationCoords = new List<float>();

    //Coroutine for Countdown timer
    //reference: https://www.youtube.com/watch?v=ulxXGht5D2U
    IEnumerator CountdownToSign()
    {
        //to prevent errors turn buttons off that start coroutines
        recButton.interactable = false;
        addButton.interactable = false;

        //runs start
        Start();

        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        //sets time to 4
        countdownTime = 4;

        //counts down from 3 to 0
        while (countdownTime > 0)
        {
            if (countdownTime == 4)
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

        countDown.GetComponent<Text>().text = "Finished recording!\n If you would like to add images for learning check add images before pressing Add Sign";
        yield return new WaitForSeconds(2f);

        //toggles off text
        countDown.gameObject.SetActive(false);
        addButton.interactable = true;
        recButton.interactable = true;
    }

    IEnumerator finishedImagesFeedback()
    {
        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        countDown.GetComponent<Text>().text = "Added Images!";
        yield return new WaitForSeconds(2f);

        //toggles off text
        countDown.gameObject.SetActive(false);
        addButton.interactable = true;
        recButton.interactable = true;
    }

    //feedback to user when sign has been added
    IEnumerator finishedAddingSignFeedback(string name)
    {
        addButton.interactable = false;
        recButton.interactable = false;
        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        countDown.GetComponent<Text>().text = "Added the sign " + name;
        yield return new WaitForSeconds(4f);

        //toggles off text
        countDown.gameObject.SetActive(false);
        addButton.interactable = true;
        recButton.interactable = true;
    }

    //feedback to user when sign has been added
    IEnumerator userPrompt(string s)
    {
        addButton.interactable = false;
        recButton.interactable = false;
        //toggles on countdown text
        countDown.gameObject.SetActive(true);

        //changes size to 300
        countDown.GetComponent<Text>().fontSize = 45;

        countDown.GetComponent<Text>().text = s;
        yield return new WaitForSeconds(2f);

        signbox.enabled = true;

        int counter = 3;

        while (counter > 0)
        {

           countDown.GetComponent<Text>().fontSize = 300;
           countDown.GetComponent<Text>().text = counter.ToString();

            yield return new WaitForSeconds(1f);

            counter = counter - 1;
        }

        //tells user to sign
        countDown.GetComponent<Text>().text = "Sign!";


        //toggles off text
        countDown.gameObject.SetActive(false);
        addButton.interactable = true;
        recButton.interactable = true;
        signbox.enabled = false;
        postRender();
    }

    IEnumerator userPromptDynamic(string s)
    {

        done.interactable = false;
        nextFrame.interactable = false;
        //toggles on countdown text
        userText.enabled = true;

        //changes size to 300
        userText.GetComponent<Text>().fontSize = 45;

        userText.GetComponent<Text>().text = s;
        yield return new WaitForSeconds(2f);

        signbox2.enabled = true;

        int counter = 3;

        while (counter > 0)
        {

            userText.GetComponent<Text>().fontSize = 300;
            userText.GetComponent<Text>().text = counter.ToString();

            yield return new WaitForSeconds(1f);

            counter = counter - 1;
        }

        //tells user to sign
        userText.GetComponent<Text>().text = "Sign!";


        //toggles off text
        userText.enabled = false;
        done.interactable = true;
        nextFrame.interactable = true;
        signbox2.enabled = false;
        imagePreview.enabled = false;
        postRender();
    }


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        signbox = GameObject.FindGameObjectWithTag("signbox").GetComponent<UnityEngine.UI.Image>();
        signbox.enabled = false;

        imagePreview = GameObject.FindGameObjectWithTag("preview").GetComponent<Canvas>();
        imagePreview.enabled = false;

        signbox2 = GameObject.FindGameObjectWithTag("signbox2").GetComponent<UnityEngine.UI.Image>();
        signbox2.enabled = false;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        frontCamera = GameObject.FindGameObjectWithTag("Front").GetComponent<Camera>();
        backCamera = GameObject.FindGameObjectWithTag("Back").GetComponent<Camera>();
        side1Camera = GameObject.FindGameObjectWithTag("Side1").GetComponent<Camera>();
        side2Camera = GameObject.FindGameObjectWithTag("Side2").GetComponent<Camera>();

        mainCamera.enabled = true;
        frontCamera.enabled = false;
        backCamera.enabled = false;
        side1Camera.enabled = false;
        side2Camera.enabled = false;

        mainCanvas = GameObject.FindGameObjectWithTag("mainCanvas").GetComponent<Canvas>(); 
        imageCanvas = GameObject.FindGameObjectWithTag("imageCanvas").GetComponent<Canvas>();
        nextFrame = GameObject.FindGameObjectWithTag("nextframe").GetComponent<Button>();
        done = GameObject.FindGameObjectWithTag("done").GetComponent<Button>();
        dynamicText = GameObject.FindGameObjectWithTag("dyntxt").GetComponent<Text>();
        userText = GameObject.FindGameObjectWithTag("prompt").GetComponent<Text>();

        mainCanvas.enabled = true;
        imageCanvas.enabled = false;
        nextFrame.interactable = false;
        done.interactable = true;
        dynamicText.enabled = false;
        userText.enabled = false;

        mainCanvas.worldCamera = mainCamera;

        //finds countdown game object
        if (disableCountdown) 
        {
            countDown = GameObject.Find("CountdownText");
            recButton = GameObject.Find("StartRecButton").GetComponent<Button>();
            addButton = GameObject.Find("AddSign").GetComponent<Button>();
            addButton.interactable = false;
        }

        signname = GameObject.FindGameObjectWithTag("nameholder").GetComponent<Text>();
        dynamicValue = GameObject.FindGameObjectWithTag("dynholder").GetComponent<Text>();


        //sets count down time to 4
        countdownTime = 4;

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
        staticFeatureIDs = new List<int>();
        dynamicFeatureIDs = new List<int>();
        rightHandFrames = new List<Hand>();
        leftHandFrames = new List<Hand>();
        StartCoroutine(CountdownToSign());
    }

    //prints out featureID to unity console
    public string getFeatureID(List<int> featureIDs)
    {
        string featureIDString = string.Join(" ", featureIDs);
        return featureIDString;
    }

    public string getJoints(List<float> jointList)
    {
        string jointString = string.Join(", ", jointList);
        return jointString;
    }

    public string getHeader(List<string> jointList)
    {
        string jointString = string.Join(", ", jointList);
        return jointString;
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

        //Vector for index/middle finger
        Vector index = new Vector();
        Vector middle = new Vector();

        foreach (Finger finger in fingers)
        {

            //assigns feature ID number based on finger
            if (finger.Type == Finger.FingerType.TYPE_PINKY) featureID = 3;
            if (finger.Type == Finger.FingerType.TYPE_RING) featureID = 7;
            if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
            {
                Bone bone = finger.Bone((Bone.BoneType)(TYPE_DISTAL));
                middle = bone.NextJoint;
                featureID = 11;
            }
            if (finger.Type == Finger.FingerType.TYPE_INDEX)
            {
                Bone bone = finger.Bone((Bone.BoneType)(TYPE_DISTAL));
                index = bone.NextJoint;
                featureID = 15;
            }
            if (finger.Type == Finger.FingerType.TYPE_THUMB) featureID = 19;

            //adds feature ID numbers for corresponding fingers based upon if they are bent or extended
            if (finger.IsExtended) featureIDList.Add(featureID + leftHandID);
            if (isBent(finger)) featureIDList.Add(featureID + bentFeatureFactor + leftHandID);

        }

        //finds angle between middle and index finger
        //if greater than .1, then fingers are apart
        if (middle.AngleTo(index) > angleThreshold) featureIDList.Add(25 + leftHandID);

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
        Vector palmDirection = hand.Direction;

        //find Y value of palm normal vector
        float palmNormalY = palmNormal[1];

        //finds y value of palm direction(checks if fingers face up or down)
        float palmDirectionZ = palmDirection[2];

        //if palm normal Y value is less than 0, the palm is facing the leap, add to featureID list
        if (palmNormalY < 0) featureIDList.Add(1 + leftHandFactor(hand));
        if (palmDirectionZ < -0.7) featureIDList.Add(23 + leftHandFactor(hand));

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

    public void askUserForImages(bool isDynamic)
    {

        StartCoroutine(userPrompt("Please record the sign again in \nthe black box to take an image of it"));

    }

    public void askUserForImagesDynamic()
    {
        StartCoroutine(userPromptDynamic("Please record the sign again in \nthe black box to take an image of it"));
        imagePreview.enabled = false;
    }

    //deletes last create image
    public void redoImage()
    {
        string name = signname.text;
        string path = Application.dataPath + "/Resources/DataImages/" + name + "/" + name;
        if (dynamicValue.text.Equals("0") && File.Exists(path + "_front.png"))
        {
            File.Delete(path + "_front.png");
            File.Delete(path + "_back.png");
            File.Delete(path + "_side1.png");
            File.Delete(path + "_side2.png");
        }
        else
        {
            File.Delete(path + "_front-" + dynamicValue.text + ".png");
            File.Delete(path + "_back-" + dynamicValue.text + ".png");
            File.Delete(path + "_side1-" + dynamicValue.text + ".png");
            File.Delete(path + "_side2-" + dynamicValue.text + ".png");
        }

        askUserForImagesDynamic();
        nextFrame.enabled = false;

    }

    public void postRender()
    {
        mainCanvas.enabled = false;
        mainCamera.enabled = false;

        imageCanvas.enabled = true;
        backCamera.enabled = true;

        string name = signname.text;

        string path = Application.dataPath + "/Resources/DataImages/" + name +"/";

        int width = Screen.width / 2;
        int height = Screen.height / 2;
        int startX = Screen.width / 2;
        int startY = 0;

        string frontPath = path + name + "_front.png";
        string backPath = path + name + "_back.png";
        string side1Path = path + name + "_side1.png";
        string side2Path = path + name + "_side2.png";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        for (int i = 0; i < 4; i++)
        {
            if (i == 0) cam = frontCamera;
            if (i == 1) cam = backCamera;
            if (i == 2) cam = side1Camera;
            if (i == 3) cam = side2Camera;

            cam.enabled = true;
            imageCanvas.worldCamera = cam;
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            cam.targetTexture = renderTexture;
            cam.Render();
            cam.targetTexture = null;

            RenderTexture.active = renderTexture;
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

            Rect pixelRect = new Rect(startX, startY, width, height);
            screenshot.ReadPixels(pixelRect, 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;

            byte[] bytes = screenshot.EncodeToPNG();
            Destroy(screenshot);

            if (!isDynamic)
            {
                if (i == 0) System.IO.File.WriteAllBytes(path + name + "_front.png", bytes);
                if (i == 1) System.IO.File.WriteAllBytes(path + name + "_back.png", bytes);
                if (i == 2) System.IO.File.WriteAllBytes(path + name + "_side1.png", bytes);
                if (i == 3) System.IO.File.WriteAllBytes(path + name + "_side2.png", bytes);
            }

            else
            {
                string file = "";
                if (i == 0) file = path + name + "_front-";
                if (i == 1) file = path + name + "_back-";
                if (i == 2) file = path + name + "_side1-";
                if (i == 3) file = path + name + "_side2-";

                int dynamicNum = 1;

                for (int j = 0; j<7; j++)
                {
                    if (System.IO.File.Exists(file + dynamicNum.ToString() + ".png")) dynamicNum = dynamicNum + 1;
                    else break;
                    dynamicValue.text = dynamicNum.ToString();
                }

                System.IO.File.WriteAllBytes(file + dynamicNum.ToString() + ".png", bytes);

                if (i == 0) frontPath = file + dynamicNum.ToString() + ".png";
                if (i == 1) backPath = file + dynamicNum.ToString() + ".png";
                if (i == 2) side1Path = file + dynamicNum.ToString() + ".png";
                if (i == 3) side2Path = file + dynamicNum.ToString() + ".png";
            }
        }

        backCamera.enabled = false;
        frontCamera.enabled = false;
        side1Camera.enabled = false;
        side2Camera.enabled = false;

        mainCamera.enabled = true;
        imageCanvas.worldCamera = mainCamera;
        signbox2.enabled = false;
        if (isDynamic)
        {
            nextFrame.interactable = true;
            dynamicText.enabled = true;
        }

        //Allows user to view images just taken, if user needs to redo it, then clicks redo button

        imagePreview.enabled = true;

        UnityEngine.UI.Image frontIMG = GameObject.FindGameObjectWithTag("frontprev").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image backIMG = GameObject.FindGameObjectWithTag("backprev").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image side1IMG = GameObject.FindGameObjectWithTag("side1prev").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image side2IMG = GameObject.FindGameObjectWithTag("side2prev").GetComponent<UnityEngine.UI.Image>();

        byte[] frontBytes = System.IO.File.ReadAllBytes(frontPath);
        byte[] backBytes = System.IO.File.ReadAllBytes(backPath);
        byte[] side1Bytes = System.IO.File.ReadAllBytes(side1Path);
        byte[] side2Bytes = System.IO.File.ReadAllBytes(side2Path);

        Texture2D frontTex = new Texture2D(2, 2);
        Texture2D backTex = new Texture2D(2, 2);
        Texture2D side1Tex = new Texture2D(2, 2);
        Texture2D side2Tex = new Texture2D(2, 2);

        frontTex.LoadImage(frontBytes);
        backTex.LoadImage(backBytes);
        side1Tex.LoadImage(side1Bytes);
        side2Tex.LoadImage(side2Bytes);

        Sprite frontSprite = Sprite.Create(frontTex, new Rect(0.0f, 0.0f, frontTex.width, frontTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        Sprite backSprite = Sprite.Create(backTex, new Rect(0.0f, 0.0f, backTex.width, backTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        Sprite side1Sprite = Sprite.Create(side1Tex, new Rect(0.0f, 0.0f, side1Tex.width, side1Tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        Sprite side2Sprite = Sprite.Create(side2Tex, new Rect(0.0f, 0.0f, side2Tex.width, side2Tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        frontIMG.sprite = frontSprite;
        backIMG.sprite = backSprite;
        side2IMG.sprite = side1Sprite;
        side1IMG.sprite = side2Sprite;

    }

    //adds sign information to text file
    public void addSign()
    {
        //if no static or dynamic features return
        if (staticFeatureIDs.Count == 0 && dynamicFeatureIDs.Count == 0) return;

        //find value of dynamic toggle
        Toggle d = dynamicToggle.GetComponent<Toggle>();
        isDynamic = d.isOn;

        if (isDynamic) dynamicValue.text = "1";
        else dynamicValue.text = "0";


        //get the name of the string
        string name;
        iField = GameObject.Find("SignName").GetComponent<InputField>();
        name = iField.text;
        signname.text = name;

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

        //find value of dynamic toggle
        Toggle images = imagesToggle.GetComponent<Toggle>();
        bool addImages = images.isOn;

        Debug.Log(addImages);

        if (addImages) askUserForImages(isDynamic);
        //inform user that the sign has been added
        else StartCoroutine((finishedAddingSignFeedback(name)));
    }

    public void doneImages()
    {
        StartCoroutine(finishedImagesFeedback());
    }

    //For trying to animate the hand
    /*
    //writes to CSV file
    public void writeToCSV()
    {
        
        List<string>header = new List<string>() {"Palm_PositionX", "Palm_PositionY", "Palm_PositionZ", "Palm_RotationX", "Palm_RotationY", "Palm_RotationZ", "Palm_RotationW",
            "Wrist_PositionX", "Wrist_PositionY", "Wrist_PositionZ", "Forearm_RotationX", "Forearm_RotationY", "Forearm_RotationZ", "Forearm_RotationW",
            "IndexProx_PostionX", "IndexProx_PostionY", "IndexProx_PostionZ", "IndexProx_RotationX", "IndexProx_RotationY", "IndexProx_RotationZ", "IndexProx_RotationW",
            "IndexInter_PostionX", "IndexInter_PostionY", "IndexInter_PostionZ", "IndexInter_RotationX", "IndexInter_RotationY", "IndexInter_RotationZ", "IndexInter_RotationW",
            "IndexDistal_PostionX", "IndexDistal_PostionY", "IndexDistal_PostionZ", "IndexDistal_RotationX", "IndexDistal_RotationY", "IndexDistal_RotationZ", "IndexDistal_RotationW",
            "MiddleProx_PostionX", "MiddleProx_PostionY", "MiddleProx_PostionZ", "MiddleProx_RotationX", "MiddleProx_RotationY", "MiddleProx_RotationZ", "MiddleProx_RotationW",
            "MiddleInter_PostionX", "MiddleInter_PostionY", "MiddleInter_PostionZ", "MiddleInter_RotationX", "MiddleInter_RotationY", "MiddleInter_RotationZ", "MiddleInter_RotationW",
            "MiddleDistal_PostionX", "MiddleDistal_PostionY", "MiddleDistal_PostionZ", "MiddleDistal_RotationX", "MiddleDistal_RotationY", "MiddleDistal_RotationZ", "MiddleDistal_RotationW",
            "PinkyProx_PostionX", "PinkyProx_PostionY", "PinkyProx_PostionZ", "PinkyProx_RotationX", "PinkyProx_RotationY", "PinkyProx_RotationZ", "PinkyProx_RotationW",
            "PinkyInter_PostionX", "PinkyInter_PostionY", "PinkyInter_PostionZ", "PinkyInter_RotationX", "PinkyInter_RotationY", "PinkyInter_RotationZ", "PinkyInter_RotationW",
            "PinkyDistal_PostionX", "PinkyDistal_PostionY", "PinkyDistal_PostionZ", "PinkyDistal_RotationX", "PinkyDistal_RotationY", "PinkyDistal_RotationZ", "PinkyDistal_RotationW",
            "RingProx_PostionX", "RingProx_PostionY", "RingProx_PostionZ", "RingProx_RotationX", "RingProx_RotationY", "RingProx_RotationZ", "RingProx_RotationW",
            "RingInter_PostionX", "RingInter_PostionY", "RingInter_PostionZ", "RingInter_RotationX", "RingInter_RotationY", "RingInter_RotationZ", "RingInter_RotationW",
            "RingDistal_PostionX", "RingDistal_PostionY", "RingDistal_PostionZ", "RingDistal_RotationX", "RingDistal_RotationY", "RingDistal_RotationZ", "RingDistal_RotationW",
            "ThumbProx_PostionX", "ThumbProx_PostionY", "ThumbProx_PostionZ", "ThumbProx_RotationX", "ThumbProx_RotationY", "ThumbProx_RotationZ", "ThumbProx_RotationW",
            "ThumbInter_PostionX", "ThumbInter_PostionY", "ThumbInter_PostionZ", "ThumbInter_RotationX", "ThumbInter_RotationY", "ThumbInter_RotationZ", "ThumbInter_RotationW",
            "ThumbDistal_PostionX", "ThumbDistal_PostionY", "ThumbDistal_PostionZ", "ThumbDistal_RotationX", "ThumbDistal_RotationY", "ThumbDistal_RotationZ", "ThumbDistal_RotationW",
            "middletoPalmDistanceX", "middletoPalmDistanceY", "middletoPalmDistanceZ"};
        
        string name;
        iField = GameObject.Find("SignName").GetComponent<InputField>();
        name = iField.text;
        string strFilePath = "";

        if (rightAnimationCoords.Count > 0)
        {
            strFilePath = "Assets/Scripts/" + name + "_right.csv";
            File.WriteAllText(strFilePath, getHeader(header) + Environment.NewLine);
            File.AppendAllText(strFilePath, (getJoints(rightAnimationCoords) + Environment.NewLine));
        }

        if (leftAnimationCoords.Count > 0)
        {
            strFilePath = "Assets/Scripts/" + name + "_left.csv";
            File.WriteAllText(strFilePath, getHeader(header) + Environment.NewLine);
            File.AppendAllText(strFilePath, (getJoints(leftAnimationCoords) + Environment.NewLine));
        }

    }




    /*
    //method to get rotation in quaternion
    private Quaternion calculateRotation(LeapTransform trs)
    {
        Vector3 up = trs.yBasis.ToVector3();
        Vector3 forward = trs.zBasis.ToVector3();
        return Quaternion.LookRotation(forward, up);
    }

    public void getFrameValues(Hand hand)
    {
        List<float> handFrame = new List<float>();
        List<float> index = new List<float>();
        List<float> middle = new List<float>();
        List<float> pinky = new List<float>();
        List<float> ring = new List<float>();
        List<float> thumb = new List<float>();

        Arm arm = hand.Arm;
        Vector palmPos = hand.PalmPosition;
        Vector wristPos = arm.WristPosition;
        LeapTransform palmRotation = hand.Basis;
        Quaternion palmRot = calculateRotation(palmRotation);
        Quaternion forearmRot = hand.Arm.Rotation.ToQuaternion();
        Vector middleDistal = new Vector();

        List<Finger> fingers = hand.Fingers;

        foreach (Finger finger in fingers)
        {

            Bone distalBone = finger.Bone((Bone.BoneType)(TYPE_DISTAL));
            Bone interBone = finger.Bone((Bone.BoneType)(TYPE_INTERMEDIATE));
            Bone proxBone = finger.Bone((Bone.BoneType)(TYPE_PROXIMAL));

            Vector distal = distalBone.NextJoint;
            Vector inter = interBone.NextJoint;
            Vector prox = proxBone.NextJoint;

            Quaternion distalRot = distalBone.Rotation.ToQuaternion();
            Quaternion interRot = interBone.Rotation.ToQuaternion();
            Quaternion proxRot = proxBone.Rotation.ToQuaternion();

            if (finger.Type == Finger.FingerType.TYPE_INDEX)
            {
                index.Add(prox[0], prox[1], prox[2]);
                index.Add(inter[0], inter[1], inter[2]);
                index.Add(distal[0], distal[1], distal[2]);
                index.Add(proxRot[0], proxRot[1], proxRot[2], proxRot[3]);
                index.Add(interRot[0], interRot[1], interRot[2], interRot[3]);
                index.Add(distalRot[0], distalRot[1], distalRot[2], distalRot[3]);

            }

            if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
            {
                middleDistal = distal;
                middle.Add(prox[0], prox[1], prox[2]);
                middle.Add(inter[0], inter[1], inter[2]);
                middle.Add(distal[0], distal[1], distal[2]);
                middle.Add(proxRot[0], proxRot[1], proxRot[2], proxRot[3]);
                middle.Add(interRot[0], interRot[1], interRot[2], interRot[3]);
                middle.Add(distalRot[0], distalRot[1], distalRot[2], distalRot[3]);
            }

            if (finger.Type == Finger.FingerType.TYPE_PINKY)
            {
                pinky.Add(prox[0], prox[1], prox[2]);
                pinky.Add(inter[0], inter[1], inter[2]);
                pinky.Add(distal[0], distal[1], distal[2]);
                pinky.Add(proxRot[0], proxRot[1], proxRot[2], proxRot[3]);
                pinky.Add(interRot[0], interRot[1], interRot[2], interRot[3]);
                pinky.Add(distalRot[0], distalRot[1], distalRot[2], distalRot[3]); 
            }

            if (finger.Type == Finger.FingerType.TYPE_RING)
            {
                ring.Add(prox[0], prox[1], prox[2]);
                ring.Add(inter[0], inter[1], inter[2]);
                ring.Add(distal[0], distal[1], distal[2]);
                ring.Add(proxRot[0], proxRot[1], proxRot[2], proxRot[3]);
                ring.Add(interRot[0], interRot[1], interRot[2], interRot[3]);
                ring.Add(distalRot[0], distalRot[1], distalRot[2], distalRot[3]);
            }

            if (finger.Type == Finger.FingerType.TYPE_THUMB)
            {
                thumb.Add(prox[0], prox[1], prox[2]);
                thumb.Add(inter[0], inter[1], inter[2]);
                thumb.Add(distal[0], distal[1], distal[2]);
                thumb.Add(proxRot[0], proxRot[1], proxRot[2], proxRot[3]);
                thumb.Add(interRot[0], interRot[1], interRot[2], interRot[3]);
                thumb.Add(distalRot[0], distalRot[1], distalRot[2], distalRot[3]);
            }
        }

        Vector distance = palmPos - middleDistal;

        handFrame.Add(palmPos[0], palmPos[1], palmPos[2]);
        handFrame.Add(palmRot[0], palmRot[1], palmRot[2], palmRot[3]);
        handFrame.Add(wristPos[0], wristPos[1], wristPos[2]);
        handFrame.Add(forearmRot[0], forearmRot[1], forearmRot[2], forearmRot[3]);
        handFrame.AddRange(index);
        handFrame.AddRange(middle);
        handFrame.AddRange(pinky);
        handFrame.AddRange(ring);
        handFrame.AddRange(thumb);
        handFrame.Add(distance[0], distance[1], distance[2]);
        

        if (leftHandFactor(hand) == 1) leftAnimationCoords.AddRange(handFrame);
        else rightAnimationCoords.AddRange(handFrame);

        Debug.Log(getJoints(handFrame));
    }*/

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

            //fills hand with get
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

