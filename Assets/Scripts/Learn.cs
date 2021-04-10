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
    //variable of other script, current sign
    CurrentSign currentSign;

    //dynamic number (to get value)
    public Text dynNumber;

    public Button nextFrameButton;
    public Button prevFrameButton;
    public Button normal;
    public Canvas imageView;
    public UnityEngine.UI.Image frontBig;
    public UnityEngine.UI.Image backBig;
    public UnityEngine.UI.Image side1Big;
    public UnityEngine.UI.Image side2Big;
    public Text frontTextBig;
    public Text side1TextBig;
    public Text side2TextBig;
    public Text backTextBig;

    // Start is called before the first frame update
    void Start()
    {
        currentSign = GameObject.FindGameObjectWithTag("CurrentSign").GetComponent<CurrentSign>();
        dynNumber = GameObject.FindGameObjectWithTag("dyntxt").GetComponent<Text>();
        nextFrameButton = GameObject.FindGameObjectWithTag("nextframe").GetComponent<Button>();
        prevFrameButton = GameObject.FindGameObjectWithTag("prevframe").GetComponent<Button>();
        imageView = GameObject.FindGameObjectWithTag("preview").GetComponent<Canvas>();

        frontBig = GameObject.FindGameObjectWithTag("Front").GetComponent<UnityEngine.UI.Image>();
        side1Big = GameObject.FindGameObjectWithTag("Side1").GetComponent<UnityEngine.UI.Image>();
        side2Big = GameObject.FindGameObjectWithTag("Side2").GetComponent<UnityEngine.UI.Image>();
        backBig = GameObject.FindGameObjectWithTag("Back").GetComponent<UnityEngine.UI.Image>();

        frontTextBig = GameObject.FindGameObjectWithTag("FronttxtBig").GetComponent<Text>();
        side1TextBig = GameObject.FindGameObjectWithTag("SidetxtBig").GetComponent<Text>();
        side2TextBig = GameObject.FindGameObjectWithTag("Side2txtBig").GetComponent<Text>();
        backTextBig = GameObject.FindGameObjectWithTag("BacktxtBig").GetComponent<Text>();

        normal = GameObject.FindGameObjectWithTag("normal").GetComponent<Button>();

        frontBig.enabled = false;
        side1Big.enabled = false;
        side2Big.enabled = false;
        backBig.enabled = false;

        frontTextBig.enabled = false;
        side1TextBig.enabled = false;
        side2TextBig.enabled = false;
        backTextBig.enabled = false;

        normal.interactable = false;

        dynNumber.text = "0";
        if (currentSign.getIsDynamic()) 
        { 
            dynNumber.text = "1"; 
        }
        else
        {
            nextFrameButton.interactable = false;
            prevFrameButton.interactable = false;
        }
        createUI(currentSign.getName(), int.Parse(dynNumber.text));
    }

    public void frontView(UnityEngine.UI.Image frontIMG)
    {
        imageView.enabled = false;
        frontBig.enabled = true;
        frontTextBig.enabled = true;
        frontBig.sprite = frontIMG.sprite;
        normal.interactable = true;
    }

    public void backView(UnityEngine.UI.Image backIMG)
    {
        imageView.enabled = false;
        backBig.enabled = true;
        backTextBig.enabled = true;
        backBig.sprite = backIMG.sprite;
        normal.interactable = true;
    }

    public void side1View(UnityEngine.UI.Image side1IMG)
    {
        imageView.enabled = false;
        side1Big.enabled = true;
        side1TextBig.enabled = true;
        side1Big.sprite = side1IMG.sprite;
        normal.interactable = true;
    }

    public void side2View(UnityEngine.UI.Image side2IMG)
    {
        imageView.enabled = false;
        side2Big.enabled = true;
        side2TextBig.enabled = true;
        side2Big.sprite = side2IMG.sprite;
        normal.interactable = true;
    }

    public void normalView()
    {
        Debug.Log("HERE!");
        imageView.enabled = true;
        frontBig.enabled = false;
        side1Big.enabled = false;
        side2Big.enabled = false;
        backBig.enabled = false;

        frontTextBig.enabled = false;
        side1TextBig.enabled = false;
        side2TextBig.enabled = false;
        backTextBig.enabled = false;
        normal.interactable = false;
    }

    // Update is called once per frame
    void createUI(string name, int dynamic)
    {
        //if not dynamic sign, there is only one image

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
