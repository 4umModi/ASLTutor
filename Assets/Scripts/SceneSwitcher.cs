using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void SignMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void TutorScene()
    {
        SceneManager.LoadScene(2);
    }

    public void InstructionScene()
    {
        SceneManager.LoadScene(3);
    }

    public void next()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void previous()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
