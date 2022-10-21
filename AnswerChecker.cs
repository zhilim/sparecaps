using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;

public class AnswerChecker : MonoBehaviour
{
    public string[] answers;
    public InputField next;
    private InputField ownself;
    public bool isFinal = false;
    private float autoScrollRate = 1200f;
    private bool scrollActivate = false;
    private float scrollTargetY = 0f;
    private Vector3 velo = Vector3.zero;
    public bool isAnswered = false;
    public bool isRemarksTrigger = false;
    private bool switchable = true;
    private bool scrollable = true;
    private float scrolllevel = 0.8f;
    private float scrollthres = 2f;


    //123,219,121
    //219,127,123
    // Start is called before the first frame update
    void Start()
    {
        if(Application.platform == RuntimePlatform.Android)
        {
            scrollable = true;
            switchable = true;
            scrolllevel = 0.74f;
            scrollthres = 1.7f;
        }

        ownself = GetComponent<InputField>();

    }

    // Update is called once per frame
    void Update()
    {
        if (scrollable)
        {
            scroll();
        }

    }

    public void checkAnswers(string ass)
    {
        string ans = Regex.Replace(ass, @"\s", "");

        if(ans == "worklifebalance69")
        {
            GameObject.Find("quizMaster").GetComponent<qm1>().autocomplete();
            return;
        }

        foreach (string s in answers)
        {
            string norms = Regex.Replace(s, @"\s", "");
            bool samesame = string.Equals(norms, ans, StringComparison.OrdinalIgnoreCase);
            if (samesame)
            {
                isAnswered = true;
                GameObject.Find("quizMaster").GetComponent<qm1>().incrementScore();
                Debug.Log("User has answered: " + GameObject.Find("quizMaster").GetComponent<qm1>().getScore().ToString());
                GetComponent<Image>().color = new Color32(123, 219, 121, 255);
                //GetComponent<InputField>().touchScreenKeyboard.text = "";
                ownself.interactable = false;
                //GetComponent<InputField>().text = s;


                if (isRemarksTrigger)
                {
                    GameObject.Find("quizMaster").GetComponent<qm1>().checkForRemarksRelease();
                }

                if (!isFinal)
                {

                    if (!scrollable)
                    {
                        TouchScreenKeyboard k = TouchScreenKeyboard.Open("");
                        k.text = "";
                        TouchScreenKeyboard.hideInput = true;

                    }
                    else
                    {
                        autoScroll();
                    }
                    if (switchable)
                    {
                        TouchScreenKeyboard k = TouchScreenKeyboard.Open("");
                        k.text = "";
                        Invoke("switchnextfield", 0.1f);

                        //next.GetComponent<InputField>().touchScreenKeyboard.text = "";
                        //TouchScreenKeyboard.hideInput = true;
                    }
                   
                  
                }
            }

        }
    }

    private void switchnextfield()
    {
        next.GetComponent<InputField>().Select();
        next.GetComponent<InputField>().text = "";
    }



    private void autoScroll()
    {
        Vector3 np = next.GetComponent<RectTransform>().position;
       //Debug.Log("next form viewport posit: " + np.ToString());

        float nextpos = np.y;
        Debug.Log(next.GetComponent<RectTransform>().position.y);
        Debug.Log("screen height: " + Screen.height.ToString());
        //RectTransform p = GameObject.Find("QuizContent").GetComponent<RectTransform>();
        //Debug.Log(topblankheight-nextpos);
        if(nextpos < Screen.height/scrollthres)
        {
           
            //Debug.Log("present focus point: " + p.position.y.ToString());
            //float newy = p.position.y + 200f;
            //Debug.Log("new focus point: " + newy.ToString());
            //Vector3 newpos = new Vector3(p.position.x, newy, p.position.z);
            //p.position = newpos;
            //scrollTargetY = newpos.y;
            //qm.currentYFocusPos = newpos.y;
            scrollActivate = true;

        }
    }

    private void scroll()
    {
        if (scrollActivate)
        {
            RectTransform p = GameObject.Find("QuizContent").GetComponent<RectTransform>();
            Vector3 nps = next.GetComponent<RectTransform>().position;
            float nextpos = nps.y;
            if (nextpos < Screen.height*scrolllevel)
            {
                Vector3 np = new Vector3(p.position.x, p.position.y + Time.deltaTime * autoScrollRate, p.position.z);
                p.position = np;
            }
            else
            {
                scrollActivate = false;
            }

            
        }
       

    }
}
