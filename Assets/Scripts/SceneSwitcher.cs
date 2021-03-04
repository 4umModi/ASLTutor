﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//script for changing scenes
public class SceneSwitcher : MonoBehaviour
{

    //changes scene to main menu
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    //changes scene to sign menu
    public void SignMenu()
    {
        SceneManager.LoadScene(1);
    }

    //changes scene to tutor scnee
    public void TutorScene()
    {
        SceneManager.LoadScene(2);
    }

    //changes scene to intruction scene
    public void InstructionScene()
    {
        SceneManager.LoadScene(3);
    }

    //changes scene to record scene (for adding signs)
    public void RecordScene()
    {
        SceneManager.LoadScene(4);
    }

    //next scene
    public void next()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //previous scene
    public void previous()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
