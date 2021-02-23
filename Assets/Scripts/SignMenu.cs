using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using System.IO;
//using System.Windows.Forms;

public class SignMenu : MonoBehaviour
{
    List<string> stringSignArray;
    int signNum;
    Button[] signButtons;

    // Start is called before the first frame update
    void Start()
    {
        stringSignArray = readTextFile("Assets\\Scripts\\signs.txt");
        signNum = stringSignArray.Count;
        makeGUI();

    }

    // Update is called once per frame
    void Update()
    {

    }
    void makeGUI()
    {
        /*int i = 0;
        //for (int i = 0; i < signNum; i++)
        //{
            string _tempString = stringSignArray[i];
            Debug.Log(_tempString);
            GameObject button = (GameObject)Instantiate(Resources.Load("SignButton"));
            button.name = stringSignArray[i];
            Button _tempButton = button.GetComponent<Button>();
            int j = i;
            _tempButton.onClick.AddListener(() => tokenize(stringSignArray[j]));
        //}*/
    }

    void tokenize(string sign)
    {
        Debug.Log(sign);
    }


    List<string> readTextFile(string filePath)
    {
        List<string> signArray = new List<string>();
        StreamReader signsFile = new StreamReader(filePath);

        while (!signsFile.EndOfStream) signArray.Add(signsFile.ReadLine());

        signsFile.Close();
        return signArray;
    }

}
