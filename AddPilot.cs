using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AddPilot : MonoBehaviour
{
    private InputField username, callsign, password, passwordconfirm;
    private Dropdown opscat, qual, sqn;
    private Button submit;
    private bool userempty, csempty, pwempty, pwconfirmempty;
    private DatabaseReference db, dbg;
    private User theUser;
    private GameObject plswait, success, back, warning, sqnObj;
    private Toggle isAdmin;
    private bool commited = false;

    private GameObject[] gos;

    // Start is called before the first frame update
    void Start()
    {
        //TouchScreenKeyboard.hideInput = true;
        theUser = GameObject.Find("User").GetComponent<User>();
        userempty = true; csempty = true; pwempty = true; pwconfirmempty = true;
        submit = GameObject.Find("Submit").GetComponent<Button>();
        submit.interactable = false;
        username = GameObject.Find("username").GetComponent<InputField>();
        callsign = GameObject.Find("callsign").GetComponent<InputField>();
        password = GameObject.Find("password").GetComponent<InputField>();
        passwordconfirm = GameObject.Find("passwordconfirm").GetComponent<InputField>();
        opscat = GameObject.Find("opscat").GetComponent<Dropdown>();
        qual = GameObject.Find("qualification").GetComponent<Dropdown>();
        sqn = GameObject.Find("squadron").GetComponent<Dropdown>();
        sqnObj = GameObject.Find("squadron");

        gos = GameObject.FindGameObjectsWithTag("field");
        checkForAndroidProblem();
        if (theUser.sqn == "architect")
        {
            sqnObj.SetActive(true);
        }
        else
        {
            sqnObj.SetActive(false);
        }
        if (theUser.testmode)
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("test");
        }
        else
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("users");
        }

        dbg = FirebaseDatabase.DefaultInstance.GetReference("global");
        plswait = GameObject.Find("plswait");
        //plswait.SetActive(false);
        success = GameObject.Find("success");
        success.SetActive(false);
        back = GameObject.Find("back");
        isAdmin = GameObject.Find("issitadmin").GetComponent<Toggle>();
        warning = GameObject.Find("warning");
        warning.SetActive(false);

        //sqn.options.Add(new Dropdown.OptionData() { text = "69!" });

        updateSqnOptions();
        //updateTypeOpt();
        //UpdateCatOptions();
        plswait.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (commited)
        {
            commitSuccess();
        }
    }

    private void checkForAndroidProblem()
    {
        if (GameObject.Find("User").GetComponent<User>().hasAndroidProblem)
        {
            foreach (GameObject go in gos)
            {
                go.GetComponent<InputField>().shouldHideMobileInput = false;
            }
        }

    }

    private void updateSqnOptions()
    {

        dbg.Child("squadrons").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("oops");
            }else if (task.IsCompleted)
            {
                DataSnapshot ds = task.Result;

                UnityMainThread.wkr.AddJob(() =>
                {
                    sqn.options.Clear();
                    foreach (DataSnapshot sds in ds.Children)
                    {
                        sqn.options.Add(new Dropdown.OptionData() { text = sds.Value.ToString() });
                       
                    }
                    sqn.value = 1;
                    sqn.value = 0;

                    updateTypeOpt();

                });


            }
        });
    }

    private void updateTypeOpt()
    {

        if (true)
        {
            dbg.Child("type").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("oops");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot ds = task.Result;


                    UnityMainThread.wkr.AddJob(() =>
                    {
                        qual.options.Clear();
                        foreach (DataSnapshot tds in ds.Children)
                        {
                           qual.options.Add(new Dropdown.OptionData() { text = tds.Value.ToString() });
                          
                        }
                        qual.value = 1;
                        qual.value = 0;
                        UpdateCatOptions();
                    });
                }
            });
        }
        else
        {
            qual.options.Clear();
            if(theUser.sqn == "150")
            {
                qual.options.Add(new Dropdown.OptionData() { text = "M346" });

                qual.value = 1;
                qual.value = 0;
            }

            if(theUser.sqn == "140")
            {
                qual.options.Add(new Dropdown.OptionData() { text = "F16" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 D+" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 (Dual Qual)" });
                qual.options.Add(new Dropdown.OptionData() { text = "F15" });
                qual.value = 1;
                qual.value = 0;
            }

            if(theUser.sqn != "150" && theUser.sqn != "140")
            {
                qual.options.Add(new Dropdown.OptionData() { text = "F16" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 D+" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 (Dual Qual)" });
                qual.value = 1;
                qual.value = 0;
            }
            UpdateCatOptions();
        }

    }

    private void updateCatopt(bool fti)
    {

        string path = "viper";
        if (fti)
        {
            path = "150";
        }

        dbg.Child("qualifications").Child(path).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("oops");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot ds = task.Result;

                UnityMainThread.wkr.AddJob(() =>
                {
                    opscat.options.Clear();
                    foreach (DataSnapshot qds in ds.Children)
                    {
                       
                       opscat.options.Add(new Dropdown.OptionData() { text = qds.Value.ToString() });

                    }

                    opscat.value = 1;
                    opscat.value = 0;

                });
               
            }
        });
    }

    public void UpdateCatOptions()
    {
        bool fti = false;
        if(theUser.sqn == "150")
        {
            fti = true;
        }

        if(theUser.sqn == "architect")
        {
            if(sqn.options[sqn.value].text == "150")
            {
                fti = true;
            }
        }

        updateCatopt(fti);


    }

    public void checkuserempty(string s)
    {
        if(s != null && s != "")
        {
            userempty = false;
        }
        else
        {
            userempty = true;
        }
    }

    public void checkcsempty(string s)
    {
        if (s != null && s != "")
        {
            csempty = false;
        }
        else
        {
            csempty = true;
        }
    }

    public void checkpwempty(string s)
    {
        if (s != null && s != "")
        {
            pwempty = false;
        }
        else
        {
            pwempty = true;
        }
    }

    public void checkpwcempty(string s)
    {
        if (s != null && s != "")
        {
            pwconfirmempty = false;
        }
        else
        {
            pwconfirmempty = true;
        }
    }

    public void checktoSubmitButton()
    {
        if(!csempty && !pwempty && !userempty && !pwconfirmempty)
        {
            submit.interactable = true;
        }
        else
        {
            submit.interactable = false;
        }
    }

    public void submitInfo()
    {
        string u = username.text;
        string c = callsign.text;
        string p = password.text;
        string pc = passwordconfirm.text;
        string oc = opscat.options[opscat.value].text;
        string quali = qual.options[qual.value].text;
        string sq = theUser.sqn;
        if (theUser.sqn == "architect")
        {
            sq = sqn.options[sqn.value].text;
        }



        //Debug.Log(u + c + p + pc + oc);

        if(p == pc)
        {
            warning.SetActive(false);
            back.GetComponent<Button>().interactable = false;
            submit.gameObject.SetActive(false);
            plswait.SetActive(true);
            db.Child(u).Child("callsign").SetValueAsync(c);

            db.Child(u).Child("password").SetValueAsync(p);
            db.Child(u).Child("qualification").SetValueAsync(quali);
            db.Child(u).Child("opscat").SetValueAsync(oc);
            db.Child(u).Child("sqn").SetValueAsync(sq);
            db.Child(u).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
            db.Child(u).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);
            db.Child(u).Child("loggedIn").SetValueAsync(false);
            //db.Child(u).Child("bestTime").SetValueAsync("");
            //db.Child(u).Child("dateLastCompleted").SetValueAsync("");
            db.Child(u).Child("quizCompleted").SetValueAsync(false);
            if (isAdmin.isOn)
            {
                db.Child(u).Child("isAdmin").SetValueAsync(true);
            }
            else
            {
                db.Child(u).Child("isAdmin").SetValueAsync(false);
            }
            db.Child(u).Child("quizProgress").SetValueAsync((double)0).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("db prob");
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("commit complete.");
                    commited = true;
                    //load the admin dashboard
                }

            });
        }
        else
        {
            Debug.Log("pw desync");
            warning.SetActive(true);
        }


    }

    public void backToAdmin()
    {
        SceneManager.LoadScene("Dashboard");
    }

    private void commitSuccess()
    {
        success.SetActive(true);
        Invoke("backToAdmin", 1.3f);
    }

}
