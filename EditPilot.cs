using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;

public class EditPilot : MonoBehaviour
{
    private string username;
    private Toggle isAdmin;
    private InputField us, cs, pw, pwc;
    private Dropdown opscat, qual, sqn;
    private DatabaseReference db, dbg;
    private GameObject plswait, success, warning, confirm, confirmReset, sqnObj, confirmResetCaps;
    private bool pwempty, pwcempty, csempty;
    private Button change, delete;
    private User theUser;

    private GameObject[] gos;

    private Calendar cal;
    private CalendarWeekRule cwr;
    private DayOfWeek dow;

    private bool initialload = true;

    // Start is called before the first frame update
    void Start()
    {
        initCalendar();
        confirmResetCaps = GameObject.Find("confirmResetCaps");
        confirmResetCaps.SetActive(false);
        TouchScreenKeyboard.hideInput = true;
        sqnObj = GameObject.Find("squadron");
        theUser = GameObject.Find("User").GetComponent<User>();
        change = GameObject.Find("Submit").GetComponent<Button>();
        change.interactable = false;
        delete = GameObject.Find("Delete").GetComponent<Button>();
        pwempty = false; pwcempty = false; csempty = false;

        if (theUser.testmode)
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("test");
        }
        else
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("users");
        }

        dbg = FirebaseDatabase.DefaultInstance.GetReference("global");
        username = GameObject.Find("User").GetComponent<User>().userbeingeditted;
        //username = "Jack";
        isAdmin = GameObject.Find("issitadmin").GetComponent<Toggle>();
        us = GameObject.Find("username").GetComponent<InputField>();
        cs = GameObject.Find("callsign").GetComponent<InputField>();
        pw = GameObject.Find("password").GetComponent<InputField>();
        pwc = GameObject.Find("passwordconfirm").GetComponent<InputField>();
        opscat = GameObject.Find("opscat").GetComponent<Dropdown>();
        qual = GameObject.Find("qualification").GetComponent<Dropdown>();
        sqn = GameObject.Find("squadron").GetComponent<Dropdown>();
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
        us.interactable = false;
        plswait = GameObject.Find("plswait");
        success = GameObject.Find("success");
        success.SetActive(false);

        warning = GameObject.Find("warning");
        warning.SetActive(false);
        confirm = GameObject.Find("confirmDelete");
        confirm.SetActive(false);
        confirmReset = GameObject.Find("confirmReset");
        confirmReset.SetActive(false);
        updateSqnOptions();
        //updateTypeOpt();
        //UpdateCatOptions();

        //fillBlanks();
    }

    // Update is called once per frame
    void Update()
    {
        
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
            }
            else if (task.IsCompleted)
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
            if (theUser.sqn == "150")
            {
                qual.options.Add(new Dropdown.OptionData() { text = "M346" });
                qual.value = 1;
                qual.value = 0;
            }

            if (theUser.sqn == "140")
            {
                qual.options.Add(new Dropdown.OptionData() { text = "F16" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 D+" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 (Dual Qual)" });
                qual.options.Add(new Dropdown.OptionData() { text = "F15" });
                qual.value = 1;
                qual.value = 0;
            }

            if (theUser.sqn != "150" && theUser.sqn != "140")
            {
                qual.options.Add(new Dropdown.OptionData() { text = "F16" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 D+" });
                qual.options.Add(new Dropdown.OptionData() { text = "F16 (Dual Qual)" });
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

                    if (initialload)
                    {
                        fillBlanks();
                    }
                    else
                    {
                        opscat.value = 1;
                        opscat.value = 0;
                    }

                });

            }
        });
    }

    public void UpdateCatOptions()
    {
        bool fti = false;
        if (theUser.sqn == "150")
        {
            fti = true;
        }

        if (theUser.sqn == "architect")
        {
            if (sqn.options[sqn.value].text == "150")
            {
                fti = true;
            }
        }

        updateCatopt(fti);


    }

    private void initCalendar()
    {
        CultureInfo myCI = new CultureInfo("en-US");
        cal = myCI.Calendar;
        cwr = myCI.DateTimeFormat.CalendarWeekRule;
        dow = myCI.DateTimeFormat.FirstDayOfWeek;
    }

    private void fillBlanks()
    {
        initialload = false;
        db.Child(username).GetValueAsync().ContinueWith(task =>
        {


            if (task.IsFaulted)
            {
                Debug.LogError("db problem.");
            }else if (task.IsCompleted)
            {
                DataSnapshot ds = task.Result;
                if ((bool)ds.Child("isAdmin").Value)
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        isAdmin.isOn = true;
                    });

                }
                else
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        isAdmin.isOn = false;
                    });

                }
                UnityMainThread.wkr.AddJob(() =>
                {
                    us.text = ds.Key;
                    cs.text = (string)ds.Child("callsign").Value;
                    pw.text = (string)ds.Child("password").Value;
                    pwc.text = (string)ds.Child("password").Value;

                    string qq = (string)ds.Child("qualification").Value;
                    string oo = (string)ds.Child("opscat").Value;
                    string ss = (string)ds.Child("sqn").Value;

                    sqn.value = sqn.options.FindIndex((j) =>
                    {
                        return j.text.Equals(ss);
                    });

                    if(qq == "F16 C/D")
                    {
                        qq = "F16";
                        ds.Reference.Child("qualification").SetValueAsync("F16");
                    }
                    else
                    {
                        if(qq == "Dual Qual")
                        {
                            qq = "F16 (Dual Qual)";
                            ds.Reference.Child("qualification").SetValueAsync("F16 (Dual Qual)");
                        }
                        qual.value = qual.options.FindIndex((i) =>
                        {
                            return i.text.Equals(qq);
                        });
                    }

                   
                    opscat.value = opscat.options.FindIndex((k) =>
                    {
                        //Debug.Log(oo);
                        return k.text.Equals(oo);
                    });
                    plswait.SetActive(false);
                    //opscat.value = 1;


                });


            }
        });
    }

    public void goDashboard()
    {
        SceneManager.LoadScene("Dashboard");
    }

    public void checkPwEmpty(string s)
    {
        if(s != null && s != "")
        {
            pwempty = false;
        }
        else
        {
            pwempty = true;
        }
    }

    public void checkPwcEmpty(string s)
    {
        if (s != null && s != "")
        {
            pwcempty = false;
        }
        else
        {
            pwcempty = true;
        }
    }

    public void checkCsEmpty(string s)
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

    public void checkToChangeButton()
    {
        if(!pwempty && !pwcempty && ! csempty)
        {
            change.interactable = true;
        }
        else
        {
            change.interactable = false;
        }
    }

    public void makeChanges()
    {

        string u = us.text;
        string c = cs.text;
        string p = pw.text;
        string pc = pwc.text;
        string oc = opscat.options[opscat.value].text;
        string ql = qual.options[qual.value].text;
        string sq = theUser.sqn;
        if (theUser.sqn == "architect")
        {
            sq = sqn.options[sqn.value].text;
        }

        if(p == pc)
        {
            warning.SetActive(false);
            GameObject.Find("back").GetComponent<Button>().interactable = false;
            change.interactable = false;
            plswait.SetActive(true);
            db.Child(u).Child("callsign").SetValueAsync(c);
            db.Child(u).Child("qualification").SetValueAsync(ql);
            db.Child(u).Child("sqn").SetValueAsync(sq);
            db.Child(u).Child("password").SetValueAsync(p);


            if (isAdmin.isOn)
            {
                db.Child(u).Child("isAdmin").SetValueAsync(true);
            }
            else
            {
                db.Child(u).Child("isAdmin").SetValueAsync(false);
            }

            db.Child(u).Child("opscat").SetValueAsync(oc).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("db error");
                }else if (task.IsCompleted)
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        success.SetActive(true);
                        Invoke("goDashboard", 1.3f);
                    });
                }
            });
        }
        else
        {
            Debug.Log("password desync");
            warning.SetActive(true);
        }
    }

    public void deleteUser()
    {
        confirm.SetActive(true);
    }

    public void nahdontdelete()
    {
        confirm.SetActive(false);
    }

    public void deleteforreal()
    {
        plswait.SetActive(true);
        db.Child(username).RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("db error");
            }else if (task.IsCompleted)
            {
                UnityMainThread.wkr.AddJob(() =>
                {
                    SceneManager.LoadScene("Dashboard");
                });
            }
        });
    }

    public void resetQuizforThis()
    {
        confirmReset.SetActive(true);
    }

    public void resetCapsforthis()
    {
        confirmResetCaps.SetActive(true);
    }

    public void donresetcaps()
    {
        confirmResetCaps.SetActive(false);
    }

    public void nahdontreset()
    {
        confirmReset.SetActive(false);
    }

    public void resetForReal()
    {
        plswait.SetActive(true);
        db.Child(username).Child("quizCompleted").SetValueAsync(false);
        db.Child(username).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
        db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);
        db.Child(username).Child("quizProgress").SetValueAsync((double)0).ContinueWith(task=>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("db error");

            }else if (task.IsCompleted)
            {
                UnityMainThread.wkr.AddJob(() =>
                {
                    SceneManager.LoadScene("Dashboard");
                });
            }
        });
    }

    public void resetCapsFurreal()
    {
        plswait.SetActive(true);
        db.Child(username).Child("capsCompleted").SetValueAsync(false);
        db.Child(username).Child("capsDeletedWeek").SetValueAsync(cal.GetWeekOfYear(DateTime.Now, cwr, dow));
        db.Child(username).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
        db.Child(username).Child("capsProgress").SetValueAsync((double)0).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("db error");

            }
            else if (task.IsCompleted)
            {
                UnityMainThread.wkr.AddJob(() =>
                {
                    SceneManager.LoadScene("Dashboard");
                });
            }
        });
    }
}
