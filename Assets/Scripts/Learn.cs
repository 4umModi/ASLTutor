using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class Learn : MonoBehaviour
{
    CurrentSign currentSign;
    public Text dynNumber;

    // Start is called before the first frame update
    void Start()
    {
        currentSign = GameObject.FindGameObjectWithTag("CurrentSign").GetComponent<CurrentSign>();
        dynNumber = GameObject.FindGameObjectWithTag("dyntxt").GetComponent<Text>();

        dynNumber.text = "0";
        if (currentSign.getIsDynamic()) dynNumber.text = "1";
        createUI(currentSign.getName(), int.Parse(dynNumber.text));
    }

    // Update is called once per frame
    void createUI(string name, int dynamic)
    {
        string path = Application.dataPath + "/Resources/DataImages/" + name + "/" + name;
        string frontPath = path + "_front.png";
        string backPath = path + "_back.png";
        string side1Path = path + "_side1.png";
        string side2Path = path + "_side2.png";

        if (dynamic > 0)
        {
            frontPath = path + "_front-" + dynamic.ToString() + ".png";
            backPath = path + "_back-" + dynamic.ToString() + ".png";
            side1Path = path + "_side1-" + dynamic.ToString() + ".png";
            side2Path = path + "_side2-" + dynamic.ToString() + ".png";
        }

        if (!File.Exists(frontPath)) return;

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

    public void nextFrame()
    {
        int dyn = int.Parse(dynNumber.text);

        string name = currentSign.getName();
        string frontPath = Application.dataPath + "/Resources/DataImages/" + name + "/" + name + "_front-" + (dyn+1).ToString() + ".png";
        Debug.Log(frontPath);
        if (!File.Exists(frontPath)) return;

        dyn = dyn + 1;
        dynNumber.text = dyn.ToString();
        createUI(name, dyn);
    }

    public void prevFrame()
    {
        int dyn = int.Parse(dynNumber.text);

        string name = currentSign.getName();
        string frontPath = Application.dataPath + "/Resources/DataImages/" + name + "/" + name + "_front-" + (dyn-1).ToString() + ".png";
        Debug.Log(frontPath);

        if (!File.Exists(frontPath)) return;

        dyn = dyn - 1;
        dynNumber.text = dyn.ToString();
        createUI(name, dyn);
    }
}
