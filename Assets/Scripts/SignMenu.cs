using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;
//using System.Windows.Forms;

//This script reads in the sign.txt file to get the signs in the system
public class SignMenu : MonoBehaviour
{
    //List of each line in the sign.txt file
    List<string> stringSignArray = new List<string>();

    //variable for number of signs
    public int signNum;

    //creates an array of buttons
    Button[] signButtons;

    //creates a serializable sign struct
    [Serializable]
    public struct Sign
    {
        public string name;
        public string isDynamic;
        public string signFeatureID;
    }

    //array of sign struct
    [SerializeField] Sign[] allSigns;

    // Start is called before the first frame update
    void Start()
    {
        //calls method to read Textfile
        stringSignArray = readTextFile("Assets\\Scripts\\signs.txt");

        //gets length of array
        signNum = stringSignArray.Count;

        //creates game object of sign and buttonTemplate
        GameObject buttonTemplate = transform.GetChild(0).gameObject;
        GameObject sign;

        //gets length of all signs (could not set to signNum, got an error each time. Manually set to 1000 in Unity Inspector)
        int num = allSigns.Length;

        for (int i = 0; i < num; i++)
        {
            //breaks whenever reaches signNum
            if (i >= signNum) break;
            int j = i;

            //instantiates button object based off template
            sign = Instantiate(buttonTemplate, transform);

            //sets name, isDynamic, and featureId in struct
            allSigns[j].name = getName(stringSignArray[j]);
            allSigns[j].isDynamic = getDynamic(stringSignArray[j]);
            allSigns[j].signFeatureID = getFeatures(stringSignArray[j]);

            //sets components of button equal to values in array
            sign.transform.GetChild(0).GetComponent<Text>().text = allSigns[j].name;
            sign.transform.GetChild (1).GetComponent<Text>().text = allSigns[j].isDynamic;
            sign.transform.GetChild (2).GetComponent<Text>().text = allSigns[j].signFeatureID;

            //Adds a Listener to button, with the sign information
            sign.GetComponent<Button>().onClick.AddListener(() => sendToClassify(allSigns[j]));
        }

        Destroy(buttonTemplate);

    }

    //gets name based on index of first space
    string getName(string signString)
    {
        int firstSpaceIndex = signString.IndexOf(" ");
        string signName = signString.Substring(0, firstSpaceIndex);
        return signName;
    }

    //gets dynamic value based on index of first space
    string getDynamic(string signString)
    {
        int firstSpaceIndex = signString.IndexOf(" ");
        char signIsDynamic = signString[firstSpaceIndex+1];
        string signDynamic = signIsDynamic.ToString();
        return signDynamic;
    }

    //gets features based on index of first space
    string getFeatures(string signString)
    { 
        int firstSpaceIndex = signString.IndexOf(" ");
        string signFeatures = signString.Substring(firstSpaceIndex + 3);
        return signFeatures;
    }


    //method that sends sign to be classified
    void sendToClassify(Sign s) 
    {
        //loads new scene
        SceneManager.LoadScene(2);
        
        //object of other script
        CurrentSign signInfo;

        //allows us to call other script, that will hold sign information
        signInfo = GameObject.FindGameObjectWithTag("CurrentSign").GetComponent<CurrentSign>();
        //calls currentSignInfo
        signInfo.currentSignInfo(s.name, s.isDynamic, s.signFeatureID);
    }

    //adds each line of the textfile list
    List<string> readTextFile(string filePath)
    {
        List<string> signArray = new List<string>();
        StreamReader signsFile = new StreamReader(filePath);

        while (!signsFile.EndOfStream) signArray.Add(signsFile.ReadLine());

        signsFile.Close();
        return signArray;
    }

}