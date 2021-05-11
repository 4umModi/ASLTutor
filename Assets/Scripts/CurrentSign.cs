//CurrentSign.cs
//Written by Forum Modi for CSC498 with Dr.Salgian at The College of New Jersey 5/2020
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;


//This script will hold the value of the current sign, between the SignMenu Scene and Tutoring Scene
public class CurrentSign : MonoBehaviour
{ 
    //creates a public sign structure
    public struct Sign
    {
        public string name;
        public bool isDynamic;
        public List<int> featureIDs;
    }

    Sign currentSign = new Sign();

    //this makes it so that in s new scene, the object that holds this script will not be reloaded or destroyed. The same copy of the script will be used regardless of scene changes
    void Awake()
    {
      DontDestroyOnLoad(this.gameObject);
    }

    //this update function destroys the current sign object if it is at the home screen (to prevent errors with 2 sign values held)
    void Update()
    {
        Scene m_Scene;
        string sceneName;
        m_Scene = SceneManager.GetActiveScene();
        sceneName = m_Scene.name;
        if (sceneName.Equals("Main Menu"))
        {
            Destroy(this.gameObject);
            return;
        }
    }

    //values passed in from Sign Menu Selectiong
    public void currentSignInfo(string signName, string dynamic, string features)
    {
        //sets name string to sign's name
        currentSign.name = signName;

        //finds boolean value of dynamic string
        if (dynamic.Equals("1 ") || dynamic.Equals("1") || dynamic.Equals(" 1 ") || dynamic.Equals(" 1")) currentSign.isDynamic = true;
        else currentSign.isDynamic = false;

        //creates new feature ID list
        currentSign.featureIDs = new List<int>();

        //loops through featureID string, parses through spaces to fill a List of ints
        while (true)
        {
            int firstSpaceIndex = features.IndexOf(" ");
            string f;
            bool needBreak = false;
            if (firstSpaceIndex > 0)
            {
                f = features.Substring(0, firstSpaceIndex);
            }
            else
            {
                f = features.Substring(0);
                needBreak = true;
            }

            int feature = Int32.Parse(f);
            (currentSign.featureIDs).Add(feature);
            if (needBreak) break;
            features = features.Substring(firstSpaceIndex+1);
        }

        //write to text files for scripts that can not access script
        //quick fix for dependency issues
        string filePath = "Assets/Resources/currentsign.txt";
        StreamWriter writer = new StreamWriter(filePath, false);
        string dynamicString = "0";
        if (currentSign.isDynamic) dynamicString = "1";
        string line = signName + " " + dynamicString;
        writer.WriteLine(line);
        writer.Close();

    }

    //these methods are get methods to be used by the Classifier script
    
    //get name of current sign
    public string getName()
    {
        return currentSign.name;
    }

    //get isdynamic value of sign
    public bool getIsDynamic()
    {
        return currentSign.isDynamic;
    }

    //gets feature IDs of sign
    public List<int> getFeatures()
    {
        return currentSign.featureIDs;
    }

}
