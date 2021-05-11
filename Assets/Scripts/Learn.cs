//Learn.cs
//Written by Forum Modi for CSC498 with Dr.Salgian at The College of New Jersey 5/2020
//


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

    //all buttons and previews for images
    public Button nextFrameButton;
    public Button prevFrameButton;
    public Canvas normal;
    public Canvas imageView;

    //images and text for the bigger view
    public UnityEngine.UI.Image frontBig;
    public UnityEngine.UI.Image backBig;
    public UnityEngine.UI.Image side1Big;
    public UnityEngine.UI.Image side2Big;
    public Text frontTextBig;
    public Text side1TextBig;
    public Text side2TextBig;
    public Text backTextBig;
    public Text description;

    // Start is called before the first frame update
    void Start()
    {
        //finds necessary game objects
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

        normal = GameObject.FindGameObjectWithTag("normal").GetComponent<Canvas>();

        //finds description object
        description = GameObject.FindGameObjectWithTag("desc").GetComponent<Text>();
        //
        description.text = getDescription(currentSign.getName());

        //disables big view of each image of the sign (user can click on image to make it bigger)
        frontBig.enabled = false;
        side1Big.enabled = false;
        side2Big.enabled = false;
        backBig.enabled = false;

        frontTextBig.enabled = false;
        side1TextBig.enabled = false;
        side2TextBig.enabled = false;
        backTextBig.enabled = false;

        normal.enabled = false;

        //uses current sign script to get dynamic value and sign name
        //if dynamic, then users can look at several frames of images
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

    //changes to big version of front view
    public void frontView(UnityEngine.UI.Image frontIMG)
    {
        imageView.enabled = false;
        frontBig.enabled = true;
        frontTextBig.enabled = true;
        frontBig.sprite = frontIMG.sprite;
        normal.enabled = true;
    }


    //changes to big version of back view
    public void backView(UnityEngine.UI.Image backIMG)
    {
        imageView.enabled = false;
        backBig.enabled = true;
        backTextBig.enabled = true;
        backBig.sprite = backIMG.sprite;
        normal.enabled = true;
    }

    //changes to big version of left side view
    public void side1View(UnityEngine.UI.Image side1IMG)
    {
        imageView.enabled = false;
        side1Big.enabled = true;
        side1TextBig.enabled = true;
        side1Big.sprite = side1IMG.sprite;
        normal.enabled = true;
    }

    //changes to big version of right side view
    public void side2View(UnityEngine.UI.Image side2IMG)
    {
        imageView.enabled = false;
        side2Big.enabled = true;
        side2TextBig.enabled = true;
        side2Big.sprite = side2IMG.sprite;
        normal.enabled = true;
    }

    //changes to normal view
    public void normalView()
    {
        imageView.enabled = true;
        frontBig.enabled = false;
        side1Big.enabled = false;
        side2Big.enabled = false;
        backBig.enabled = false;

        frontTextBig.enabled = false;
        side1TextBig.enabled = false;
        side2TextBig.enabled = false;
        backTextBig.enabled = false;
        normal.enabled = false;
    }

    //Creates the UI
    //adds images to scene
    void createUI(string name, int dynamic)
    {
        //if not dynamic sign, there is only one image

        //path of all images
        string path = "Assets\\Resources\\DataImages\\" + name + "\\" + name;
        string frontPath = path + "_front.png";
        string backPath = path + "_back.png";
        string side1Path = path + "_side1.png";
        string side2Path = path + "_side2.png";

        //path if dynamic
        if (dynamic > 0)
        {
            frontPath = path + "_front-" + dynamic.ToString() + ".png";
            backPath = path + "_back-" + dynamic.ToString() + ".png";
            side1Path = path + "_side1-" + dynamic.ToString() + ".png";
            side2Path = path + "_side2-" + dynamic.ToString() + ".png";
        }

        //if file doesnt script return
        if (!File.Exists(frontPath)) return;

        //finds UI image
        UnityEngine.UI.Image frontIMG = GameObject.FindGameObjectWithTag("frontprev").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image backIMG = GameObject.FindGameObjectWithTag("backprev").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image side1IMG = GameObject.FindGameObjectWithTag("side1prev").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image side2IMG = GameObject.FindGameObjectWithTag("side2prev").GetComponent<UnityEngine.UI.Image>();

        //reads bytes of images in from path
        byte[] frontBytes = System.IO.File.ReadAllBytes(frontPath);
        byte[] backBytes = System.IO.File.ReadAllBytes(backPath);
        byte[] side1Bytes = System.IO.File.ReadAllBytes(side1Path);
        byte[] side2Bytes = System.IO.File.ReadAllBytes(side2Path);

        //creates a texture for each image
        Texture2D frontTex = new Texture2D(2, 2);
        Texture2D backTex = new Texture2D(2, 2);
        Texture2D side1Tex = new Texture2D(2, 2);
        Texture2D side2Tex = new Texture2D(2, 2);

        //loads image as texture
        frontTex.LoadImage(frontBytes);
        backTex.LoadImage(backBytes);
        side1Tex.LoadImage(side1Bytes);
        side2Tex.LoadImage(side2Bytes);

        //creates a sprite for each image
        Sprite frontSprite = Sprite.Create(frontTex, new Rect(0.0f, 0.0f, frontTex.width, frontTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        Sprite backSprite = Sprite.Create(backTex, new Rect(0.0f, 0.0f, backTex.width, backTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        Sprite side1Sprite = Sprite.Create(side1Tex, new Rect(0.0f, 0.0f, side1Tex.width, side1Tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        Sprite side2Sprite = Sprite.Create(side2Tex, new Rect(0.0f, 0.0f, side2Tex.width, side2Tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        //sets the image sprite to the UI image's sprite
        frontIMG.sprite = frontSprite;
        backIMG.sprite = backSprite;
        side2IMG.sprite = side1Sprite;
        side1IMG.sprite = side2Sprite;
    }

    //increments to next frame
    public void nextFrame()
    {
        int dyn = int.Parse(dynNumber.text);

        string name = currentSign.getName();
        string frontPath = "Assets\\Resources\\DataImages\\" + name + "\\" + name + "_front-" + (dyn+1).ToString() + ".png";
        Debug.Log(frontPath);
        if (!File.Exists(frontPath)) return;

        dyn = dyn + 1;
        dynNumber.text = dyn.ToString();
        createUI(name, dyn);
    }

    //decrements to previous frame
    public void prevFrame()
    {
        int dyn = int.Parse(dynNumber.text);

        string name = currentSign.getName();
        string frontPath = "Assets\\Resources\\DataImages\\" + name + "\\" + name + "_front-" + (dyn-1).ToString() + ".png";
        Debug.Log(frontPath);

        if (!File.Exists(frontPath)) return;

        dyn = dyn - 1;
        dynNumber.text = dyn.ToString();
        createUI(name, dyn);
    }

    //gets the sign description and puts in on UI
    string getDescription(string name)
    {
        List<string> array = new List<string>();
        StreamReader file = new StreamReader("Assets\\Resources\\sign_desc.txt");

        while (!file.EndOfStream) array.Add(file.ReadLine());

        file.Close();

        string des = "";
        for (int i = 0; i < array.Count; i++)
        {
            int firstSpaceIndex = array[i].IndexOf(" ");
            string signName = array[i].Substring(0, firstSpaceIndex);
            if (signName == name) {des = array[i].Substring(firstSpaceIndex); break; }

        }
        return "Description:\n" + des.Trim();

    }
}
