using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Unity.Editor;
using UnityEngine.Events;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{


    private bool userfieldempty = true;
    private bool passwordfieldempty = true;
    public GameObject loader, start;
    private DatabaseReference db;

    private InputField user, pw;
    private GameObject userEntity;


    private string prevusername, prevpassword;

    private float timeoutcounter = 0f;
    private float timeout = 10f;
    private bool timingout = false;
   

    // Start is called before the first frame update
    void Start()
    {

        Application.targetFrameRate = 60;
        //TouchScreenKeyboard.hideInput = true;
        user = GameObject.Find("Username").GetComponent<InputField>();
        pw = GameObject.Find("Password").GetComponent<InputField>();
        db = FirebaseDatabase.DefaultInstance.GetReference("users");
        userEntity = GameObject.Find("User");
        if (userEntity.GetComponent<User>().testmode)
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("test");
        }


        if (PlayerPrefs.HasKey("prevusername"))
        {
            prevusername = PlayerPrefs.GetString("prevusername");
            prevpassword = PlayerPrefs.GetString("prevpassword");

            user.text = prevusername;
            pw.text = prevpassword;
            userfieldempty = false;
            passwordfieldempty = false;
        }

        //Debug.Log(DateTime.Now.AddDays(-1));

    }

 

    // Update is called once per frame
    void Update()
    {
        if(!userfieldempty && !passwordfieldempty)
        {
            start.transform.GetChild(1).gameObject.SetActive(false);
            start.GetComponent<Button>().interactable = true;
        }
        else
        {
            start.transform.GetChild(1).gameObject.SetActive(true);
            start.GetComponent<Button>().interactable = false;
        }

        timeoutcount();

    }

    private void timeoutcount()
    {
        if (timingout)
        {
            if(timeoutcounter < timeout)
            {
                timeoutcounter += Time.deltaTime;
            }
            else 
            {
                timeoutcounter = 0f;
                timingout = false;
                start.SetActive(true);
                loader.SetActive(false);
                start.transform.GetChild(2).gameObject.SetActive(true);
                start.transform.GetChild(2).GetComponent<Text>().text = "Feels like a weak internet connection...";

            }
        }
    }

    public void guestEntry()
    {
        loader.SetActive(false);
        start.SetActive(false);

        userEntity.GetComponent<User>().username = "guest";

        GameObject.Find("Title").SetActive(false);

        GameObject.Find("Username").SetActive(false);
        GameObject.Find("Password").SetActive(false);
        GameObject.Find("guest").SetActive(false);
        GameObject.Find("request").SetActive(false);
        Invoke("goToQuiz", 0f);
    }

    public void checkCredentials()
    {
        start.SetActive(false);
        loader.SetActive(true);
        timingout = true;

        db.Child(user.text).Child("password").GetValueAsync().ContinueWith(task =>
        {
            timingout = false;
            timeoutcounter = 0f;
            if (task.IsFaulted)
            {
                Debug.Log("we encountered a db problem.");
               
            }else if (task.IsCompleted)
            {
                DataSnapshot ds = task.Result;
                if(ds.Value == null)
                {
                    Debug.Log("User does not exist.");
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        start.SetActive(true);
                        start.transform.GetChild(2).gameObject.SetActive(true);
                        start.transform.GetChild(2).GetComponent<Text>().text = "Username does not exist! Check with your shags pls.";
                        loader.SetActive(false);
                    });


                }
                else
                {
                    if((string)ds.Value == pw.text)
                    {
                        Debug.Log("Login successful.");

                        UnityMainThread.wkr.AddJob(() =>
                        {
                            PlayerPrefs.SetString("prevusername", user.text);
                            PlayerPrefs.SetString("prevpassword", pw.text);
                            userEntity.GetComponent<User>().username = user.text;

                            GameObject.Find("Title").SetActive(false);
                            loader.SetActive(false);
                            start.SetActive(false);
                            GameObject.Find("Username").SetActive(false);
                            GameObject.Find("Password").SetActive(false);
                            GameObject.Find("guest").SetActive(false);
                            GameObject.Find("request").SetActive(false);
                            Invoke("goToQuiz", 0f);
                        });

                    }
                    else
                    {
                        Debug.Log("password wrong");
                        UnityMainThread.wkr.AddJob(() =>
                        {
                            start.SetActive(true);
                            start.transform.GetChild(2).gameObject.SetActive(true);
                            start.transform.GetChild(2).GetComponent<Text>().text = "Password is wrong, please try again or check with your shags";
                            loader.SetActive(false);
                        });

                    }
                }

            }
        });
    }

    private void goToQuiz()
    {
        SceneManager.LoadScene("Landing");
    }

    private void reActivateFields()
    {
        start.SetActive(true);
        loader.SetActive(false);
    }

    public void checkUserFieldForNull(string s)
    {
        if (s == null || s == "")
        {
            userfieldempty = true;
        }
        else
        {
            userfieldempty = false;
        }
    }

    public void checkPwFieldForNull(string s)
    {
        if(s == null || s == "")
        {
            passwordfieldempty = true;
        }
        else
        {
            passwordfieldempty = false;
        }
    }

    public void goRequest()
    {
        //userEntity.GetComponent<User>().username = "guest";

        GameObject.Find("Title").SetActive(false);
        loader.SetActive(false);
        start.SetActive(false);
        GameObject.Find("Username").SetActive(false);
        GameObject.Find("Password").SetActive(false);
        GameObject.Find("guest").SetActive(false);
        GameObject.Find("request").SetActive(false);
        Invoke("gotorequest", 0f);
    }

    private void gotorequest()
    {
        SceneManager.LoadScene("Request");
    }
}
