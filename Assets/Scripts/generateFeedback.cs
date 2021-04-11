﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.IO;

public class generateFeedback : MonoBehaviour
{
    public static List<string> feedbackList;
    public GameObject buttonTemplate;
    public int feedbackNum;

    //creates a serializable sign struct
    [Serializable]
    public struct Feedback
    {
        public string line;
    }

    //array of sign struct
    [SerializeField] Feedback[] allFeedback;

    public void Start()
    {
        Scene m_Scene;
        string sceneName;
        m_Scene = SceneManager.GetActiveScene();
        sceneName = m_Scene.name;
        if (sceneName.Equals("Feedback"))
        {
            allFeedback = new Feedback[1000];
            int num = allFeedback.Length;

            buttonTemplate = transform.GetChild(0).gameObject;
            GameObject sign;

            for (int i = 0; i < num; i++)
            {
                if (i >= feedbackList.Count) break;
                int j = i;
                sign = Instantiate(buttonTemplate, transform);
                allFeedback[i].line = feedbackList[i];
                sign.transform.GetChild(0).GetComponent<Text>().text = allFeedback[i].line;
            }

            Destroy(buttonTemplate);
        }
    }

    public void showFeedback(List<string> feedback)
    {
        feedbackList = feedback;
        //loads feedback screen
        SceneManager.LoadScene(5);
    }


}