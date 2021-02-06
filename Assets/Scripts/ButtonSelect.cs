using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class ButtonSelect : MonoBehaviour
{
    //creates controller variable
    Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        //creates controller for leap
        controller = new Controller();
    }

    bool Fist(Hand hand)
    {
        float grabAngle = hand.GrabAngle; //0 radian for open hand, pi radian when tight fist
        float grabStrength = hand.GrabStrength; //0 for open hand, 1 for grabbing hang pose
        bool fist = false;

        //list of fingers
        List<Finger> fingers = hand.Fingers;

        //Fist Gesture detection
        if (grabStrength > 0.9 && grabAngle > 3)
        {
            fist = true;
            foreach (Finger finger in fingers)
            {
                if (finger.IsExtended == true) //checks if all fingers are not extended
                {
                    fist = false;
                    break;
                }
            }
        }

        return fist;

    }

    bool CheckHands(Frame frame)
    {
        //gets list of hands
        List<Hand> hands = frame.Hands;
        //gets number of hands
        int num_hands = hands.Count;

        //creates bools for left/right hand existance
        bool leftHandExist = false;
        bool rightHandExist = false;

        //if no hands over leap, return 
        if (num_hands < 1) return false;

        //creates new hand variables
        Hand rightHand = new Hand();
        Hand leftHand = new Hand();

        //if there is only one hand, set corresponding hand to true and set hand object equal to corresponding hand
        if (num_hands == 1)
        {

            if (hands[0].IsRight)
            {
                rightHand = hands[0];
                rightHandExist = true;
            }
            else
            {
                leftHand = hands[0];
                leftHandExist = true;
            }
        }

        //if there are two hands, set both hands to true and set hand objects equal to corresponding hands
        if (num_hands == 2)
        {

            leftHandExist = true;
            rightHandExist = true;

            if (hands[0].IsRight)
            {
                rightHand = hands[0];
                leftHand = hands[1];
            }
            else
            {
                rightHand = hands[1];
                leftHand = hands[0];
            }
        }

        //if only one hand calls Fist to check if hand is in fist shape
        if (leftHandExist && !rightHandExist) return Fist(leftHand);
        if (rightHandExist && !leftHandExist) return Fist(rightHand);

        //if both hands are in view, then check if either is in a fist shape
        if (leftHandExist && rightHandExist)
        {
            bool leftFist = Fist(leftHand);
            bool rightFist = Fist(rightHand);

            if (leftFist == false) return rightFist;
            if (rightFist == false) return leftFist;
            if (rightFist == leftFist) return rightFist;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        //gets frame from controller
        Frame frame = controller.Frame();
        bool selection = CheckHands(frame);
        if (selection) Debug.Log("User Select"); 
    }
}
