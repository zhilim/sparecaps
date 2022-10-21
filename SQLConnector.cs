using System.Collections;
using System.Collections.Generic;
using System;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Unity.Editor;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;

using UnityEngine;

public class SQLConnector : MonoBehaviour
{
    private DatabaseReference db;
    private GameObject user;
    private string username;
    private Animator viperanim;
    private Text opscat, cs, caps_t, ol_t, rstq_t, capsexpire, quizexpire;
    private GameObject admin, plswait, cdbut, timer, viper, manreset, leaderboard, confirmReset, capstimer, capsbutton, capsmanreset, plscaps;
    private GameObject table, confirmRCaps, uncaged, nccbutton, androidproblem;
    private Button cd;
    private DateTime dt;
    public string[] months;
    private int monthIndex;
    private int currentMonth;
    private int currentWeek;
    private Calendar cal;
    private CalendarWeekRule cwr;
    private DayOfWeek dow;
    private float timerCounter = 0f;
    private float capsTimerCounter = 0f;
    private bool isGuest = false;
    private bool quizcompletedahead = false;
    private bool quizcompleted = false;

    // Start is called before the first frame update
    void Start()
    {
        caps_t = GameObject.Find("caps").transform.GetChild(0).GetComponent<Text>();
        ol_t = GameObject.Find("quiz").transform.GetChild(0).GetComponent<Text>();
        rstq_t = GameObject.Find("manreset").transform.GetChild(0).GetComponent<Text>();

        confirmRCaps = GameObject.Find("confirmResetCaps");
        confirmRCaps.SetActive(false);
        capsbutton = GameObject.Find("capsbutton");
        capstimer = GameObject.Find("capstimer");
        capstimer.SetActive(false);
        capsexpire = GameObject.Find("capsbesttext").GetComponent<Text>();
        quizexpire = GameObject.Find("quizbesttext").GetComponent<Text>();
        capsmanreset = GameObject.Find("capsmanreset");
        capsmanreset.SetActive(false);
        plscaps = GameObject.Find("plscaps");

        uncaged = GameObject.Find("uncaged");
        uncaged.SetActive(false);

        nccbutton = GameObject.Find("nccbutton");

        initCalendar();
        table = GameObject.Find("table1");
        //TouchScreenKeyboard.hideInput = true;
        plswait = GameObject.Find("plswait");
        //plswait.SetActive(false);
        manreset = GameObject.Find("manreset");
        leaderboard = GameObject.Find("leaderboards");
        confirmReset = GameObject.Find("confirmReset");
        confirmReset.SetActive(false);
        manreset.SetActive(false);
        //leaderboard.SetActive(false);
        //dateCompleted = GameObject.Find("dateCompleted").GetComponent<Text>();
        //bestTime = GameObject.Find("bestTime").GetComponent<Text>();
        timer = GameObject.Find("timer");
        timer.SetActive(false);
        user = GameObject.Find("User");
        username = user.GetComponent<User>().username;
        if(username != "guest")
        {
            if (user.GetComponent<User>().testmode)
            {
                db = FirebaseDatabase.DefaultInstance.GetReference("test");
            }
            else
            {
                db = FirebaseDatabase.DefaultInstance.GetReference("users");
            }

        }
        else
        {
            isGuest = true;
        }

        androidproblem = GameObject.Find("androidproblem");
        if (user.GetComponent<User>().iphonebuild)
        {
            androidproblem.SetActive(false);
        }

        viperanim = GameObject.Find("viper").GetComponent<Animator>();
        viper = GameObject.Find("viper");

        viper.SetActive(false);
        opscat = GameObject.Find("opscat").GetComponent<Text>();

        cd = GameObject.Find("cd").GetComponent<Button>();

        cdbut = GameObject.Find("cd");

        cs = GameObject.Find("cs").GetComponent<Text>();
        admin = GameObject.Find("admin");
        admin.SetActive(false);

        if (!isGuest)
        {
            checkAdminRights();
            getCallSign();
        }



        dt = DateTime.Now;
        monthIndex = dt.Month - 1;
        //GameObject.Find("month").GetComponent<Text>().text = months[monthIndex];
        

        if (!isGuest)
        {
            InvokeRepeating("updateDateTime", 0f, 1f);
            checkQuizStatus();
            currentMonth = DateTime.Now.Month;
            currentWeek = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
            Debug.Log("current week is: " + currentWeek.ToString());
        }

        if (isGuest)
        {
            capstimer.SetActive(false);
            capsbutton.SetActive(true);
            cdbut.SetActive(true);
            cs.text = "Stranger";
            opscat.text = "So you wanna be a Viper Driver?";
            timer.SetActive(false);
            capsmanreset.SetActive(false);
            manreset.SetActive(false);
            leaderboard.SetActive(false);
            admin.SetActive(false);
            table.SetActive(false);
            plswait.SetActive(false);
            viper.SetActive(true);
        }



    }

    // Update is called once per frame
    void Update()
    {
        updateTimer();

        if (capstimer.activeSelf)
        {
            updateCapsTimer();
        }



    }

    public void hasAndroidProblem(bool yes)
    {
        if (yes)
        {
            user.GetComponent<User>().hasAndroidProblem = true;
        }
        else
        {
            user.GetComponent<User>().hasAndroidProblem = false;
        }

    }

    private void initCalendar()
    {
        CultureInfo myCI = new CultureInfo("en-US");
        cal = myCI.Calendar;
        cwr = myCI.DateTimeFormat.CalendarWeekRule;
        dow = myCI.DateTimeFormat.FirstDayOfWeek;
    }

    private void updateDateTime()
    {
        table.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = DateTime.Now.ToString();
    }

    private void checkQuizStatus()
    {
        db.Child(username).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("db error");

            }else if (task.IsCompleted)
            {
                Debug.Log("task complete");
                UnityMainThread.wkr.AddJob(() =>
                {
                    plswait.SetActive(false);
                    viper.SetActive(true);
                    
                });

                DataSnapshot ds = task.Result;
                if(ds.Child("bestTime").Value != null && (string)ds.Child("bestTime").Value != "")
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        var dateString = (string)ds.Child("bestTime").Value;
                        TimeSpan elapsed = TimeSpan.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

                        table.transform.GetChild(8).GetChild(0).GetComponent<Text>().text = "Best: " + elapsed.Minutes.ToString() + "m " + elapsed.Seconds.ToString() + "s";

                        //bestTime.text = "Best: " + elapsed.Minutes.ToString() + "m " + elapsed.Seconds.ToString() + "s";
                        var lastdstring = (string)ds.Child("dateLastCompleted").Value;
                        //DateTime lastcdt = DateTime.Parse(lastdstring, System.Globalization.CultureInfo.InvariantCulture);
                        table.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = lastdstring;
                        //dateCompleted.text = "Last Completed: " + lastdstring;
                    });
                }

                if(ds.Child("bestTimeCaps").Value != null && (string)ds.Child("bestTimeCaps").Value != "")
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        var dateString = (string)ds.Child("bestTimeCaps").Value;
                        TimeSpan elapsed = TimeSpan.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

                        table.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = "Best: " + elapsed.Minutes.ToString() + "m " + elapsed.Seconds.ToString() + "s";

                        //bestTime.text = "Best: " + elapsed.Minutes.ToString() + "m " + elapsed.Seconds.ToString() + "s";
                        var lastdstring = (string)ds.Child("capsLastCompleted").Value;
                        //DateTime lastcdt = DateTime.Parse(lastdstring, System.Globalization.CultureInfo.InvariantCulture);
                        table.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = lastdstring;
                        //dateCompleted.text = "Last Completed: " + lastdstring;
                    });
                }
                UnityMainThread.wkr.AddJob(() => { capsexpire.text = "EXPIRED"; });


                string oc = (string)ds.Child("opscat").Value;
                string ql = (string)ds.Child("qualification").Value;
                string sq = (string)ds.Child("sqn").Value;
                UnityMainThread.wkr.AddJob(() =>
                {
                    opscat.text = "Ops Cat: " + oc + ",  Qual: " + ql + ",  Sqn: " + sq;
                    user.GetComponent<User>().opscat = oc;
                    user.GetComponent<User>().qual = ql;
                    user.GetComponent<User>().sqn = sq;

                    if(sq != "145")
                    {
                        nccbutton.SetActive(false);
                    }


                    if (sq == "150")
                    {
                        cdbut.transform.GetChild(0).GetComponent<Text>().text = "BF B / OL";
                        capsbutton.transform.GetChild(0).GetComponent<Text>().text = "BF A";

                        caps_t.text = "BF A";
                        ol_t.text = "B / OL";
                        rstq_t.text = "RESET OL";
                    }

                    if(ql == "F15")
                    {
                        viper.GetComponent<Image>().enabled = false;
                        cdbut.transform.GetChild(0).GetComponent<Text>().text = "OL";
                        ol_t.text = "OL";
                        rstq_t.text = "RESET OL";
                    }
                    else
                    {
                        viper.transform.Find("f15dog").gameObject.SetActive(false);
                    }

                    if (sq == "PC2 (USAF)")
                    {
                        cdbut.transform.GetChild(0).GetComponent<Text>().text = "EL";
                        ol_t.text = "EL";
                        rstq_t.text = "RESET EL";
                    }

                });



                if (DateTime.Now.Year == int.Parse(ds.Child("latestDeletedYear").Value.ToString()))
                {
                    if (DateTime.Now.Month > int.Parse(ds.Child("latestDeletedMonth").Value.ToString()))
                    {
                        Debug.Log("New month, resetting quiz");
                        db.Child(username).Child("quizCompleted").SetValueAsync(false);
                        db.Child(username).Child("quizProgress").SetValueAsync((double)0);
                        db.Child(username).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
                        db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);
                        if (sq == "145")
                        {
                            db.Child(username).Child("nccCompleted").SetValueAsync(false);
                            UnityMainThread.wkr.AddJob(() =>
                            {
                                uncaged.SetActive(false);
                            });

                        }

                        //PlayerPrefs.SetInt(username + "quizCompleted", 0);
                        //GameObject.Find("notif").GetComponent<Text>().text = "you have yet to complete the monthly quiz for";
                        UnityMainThread.wkr.AddJob(() =>
                        {
                            cdbut.SetActive(true);
                            table.transform.GetChild(4).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                            table.transform.GetChild(7).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                            table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Not done";
                            timer.SetActive(false);
                            manreset.SetActive(false);
                            GameObject.Find("pls").GetComponent<Text>().text = "";
                            //leaderboard.SetActive(false);
                        });


                    }
                    else
                    {
                        if ((bool)ds.Child("quizCompleted").Value)
                        {
                            Debug.Log("User has completed quiz for this month");
                            quizcompleted = true;
                            UnityMainThread.wkr.AddJob(() =>
                            {
                                if(ds.Child("quizcompletedahead").Value != null)
                                {
                                    if(DateTime.Now.Month < int.Parse(ds.Child("latestDeletedMonth").Value.ToString()))
                                    {
                                        if ((bool)ds.Child("quizcompletedahead").Value)
                                        {
                                            quizcompletedahead = true;
                                        }
                                    }
                                    else
                                    {
                                        db.Child(username).Child("quizcompletedahead").SetValueAsync(false);
                                        quizcompletedahead = false;
                                    }

                                }

                               

                                //GameObject.Find("notif").GetComponent<Text>().text = "you have completed the monthly quiz for";
                                cdbut.SetActive(false);
                                table.transform.GetChild(4).GetComponent<Image>().color = new Color32(123, 180, 144, 210);
                                table.transform.GetChild(7).GetComponent<Image>().color = new Color32(123, 180, 144, 210);
                                table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "done";
                                manreset.SetActive(true);
                                GameObject.Find("pls").GetComponent<Text>().text = "Quiz reopens in";
                                timer.SetActive(true);
                                //leaderboard.SetActive(true);


                            });

                        }

                        if (ds.Child("nccCompleted").Value != null)
                        {
                            if ((bool)ds.Child("nccCompleted").Value)
                            {
                                UnityMainThread.wkr.AddJob(() =>
                                {
                                    uncaged.SetActive(true);

                                });

                            }

                        }
                        /*
                        else
                        {
                            if (PlayerPrefs.GetInt(username + "quizCompleted", 0) == 1)
                            {
                                UnityMainThread.wkr.AddJob(() =>
                                {
                                    Debug.Log("user completed quiz while offline... defaulting to prefs");
                                    cdbut.SetActive(false);
                                    table.transform.GetChild(4).GetComponent<Image>().color = new Color32(123, 180, 144, 210);
                                    table.transform.GetChild(7).GetComponent<Image>().color = new Color32(123, 180, 144, 210);
                                    table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "done";
                                    manreset.SetActive(true);
                                    GameObject.Find("pls").GetComponent<Text>().text = "Quiz reopens in";
                                    timer.SetActive(true);

                                    var lastdstring = PlayerPrefs.GetString(username + "dateLastCompleted");

                                    table.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = lastdstring;

                                });
                            }
                        }*/

                    }
                }else if(DateTime.Now.Year > int.Parse(ds.Child("latestDeletedYear").Value.ToString()))
                {
                    Debug.Log("new year, resetting quiz");
                    db.Child(username).Child("quizCompleted").SetValueAsync(false);
                    db.Child(username).Child("quizProgress").SetValueAsync((double)0);
                    db.Child(username).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
                    db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);
                    if (sq == "145")
                    {
                        db.Child(username).Child("nccCompleted").SetValueAsync(false);
                        UnityMainThread.wkr.AddJob(() =>
                        {
                            uncaged.SetActive(false);
                        });

                    }
                    //GameObject.Find("notif").GetComponent<Text>().text = "you have yet to complete the monthly quiz for";
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        cdbut.SetActive(true);
                        table.transform.GetChild(4).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                        table.transform.GetChild(7).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                        table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Not done";
                        timer.SetActive(false);
                        manreset.SetActive(false);
                        GameObject.Find("pls").GetComponent<Text>().text = "";
                        //leaderboard.SetActive(false);
                    });
                   
                }

                Debug.Log("good day sir we here");

                if(ds.Child("capsDeletedWeek").Value != null)
                {
                    Debug.Log("caps value check passed");
                    if(DateTime.Now.Year == int.Parse(ds.Child("capsDeletedYear").Value.ToString()))
                    {
                        Debug.Log("user is yearly up to date");
                        int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
                        if (woy > int.Parse(ds.Child("capsDeletedWeek").Value.ToString()))
                        {
                            Debug.Log("new week, resetting");
                            db.Child(username).Child("capsCompleted").SetValueAsync(false);
                            db.Child(username).Child("capsProgress").SetValueAsync((double)0);
                            db.Child(username).Child("capsDeletedWeek").SetValueAsync(woy);
                            db.Child(username).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);




                            UnityMainThread.wkr.AddJob(() =>
                            {
                                capsbutton.SetActive(true);
                                capsmanreset.SetActive(false);
                                capstimer.SetActive(false);
                                plscaps.GetComponent<Text>().text = "";
                                table.transform.GetChild(3).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                                table.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                                table.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "Not done";
                                capsexpire.text = "EXPIRED";
                                Debug.Log("user is a: " + user.GetComponent<User>().opscat);
                                if (user.GetComponent<User>().sqn == "150")
                                {
                                    if (user.GetComponent<User>().opscat == "Student")
                                    {
                                        db.Child(username).Child("quizCompleted").SetValueAsync(false);
                                        db.Child(username).Child("quizProgress").SetValueAsync((double)0);
                                        db.Child(username).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
                                        db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);

                                        UnityMainThread.wkr.AddJob(() =>
                                        {
                                            cdbut.SetActive(true);
                                            table.transform.GetChild(4).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                                            table.transform.GetChild(7).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                                            table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Not done";
                                            timer.SetActive(false);
                                            manreset.SetActive(false);
                                            GameObject.Find("pls").GetComponent<Text>().text = "";
                                            //leaderboard.SetActive(false);
                                        });
                                    }
                                }
                            });


                        }
                        else
                        {
                            Debug.Log("User is up to date");
                            if ((bool)ds.Child("capsCompleted").Value)
                            {
                                Debug.Log("user completed dog");
                                UnityMainThread.wkr.AddJob(() =>
                                {
                                    capsbutton.SetActive(false);
                                    table.transform.GetChild(3).GetComponent<Image>().color = new Color32(123, 180, 144, 210);
                                    table.transform.GetChild(5).GetComponent<Image>().color = new Color32(123, 180, 144, 210);
                                    table.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "done";
                                    capsmanreset.SetActive(true);
                                    leaderboard.SetActive(true);
                                    capstimer.SetActive(true);
                                    plscaps.GetComponent<Text>().text = "Caps reopens in";
                                });
                            }
                        }

                    }else if (DateTime.Now.Year > int.Parse(ds.Child("capsDeletedYear").Value.ToString()))
                    {
                        int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
                        Debug.Log("new year, resetting");
                        db.Child(username).Child("capsCompleted").SetValueAsync(false);
                        db.Child(username).Child("capsProgress").SetValueAsync((double)0);
                        db.Child(username).Child("capsDeletedWeek").SetValueAsync((double)1);
                        db.Child(username).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
                        UnityMainThread.wkr.AddJob(() =>
                        {
                            capsbutton.SetActive(true);
                            capsmanreset.SetActive(false);
                            capstimer.SetActive(false);
                            plscaps.GetComponent<Text>().text = "";
                            table.transform.GetChild(3).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                            table.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                            table.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "Not done";
                            capsexpire.text = "EXPIRED";
                        });
                    }
                }
                else
                {
                    Debug.Log("new user, resetting caps.");
                    int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
                    db.Child(username).Child("capsCompleted").SetValueAsync(false);
                    db.Child(username).Child("capsProgress").SetValueAsync((double)0);
                    db.Child(username).Child("capsDeletedWeek").SetValueAsync(woy);
                    db.Child(username).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);

                    UnityMainThread.wkr.AddJob(() =>
                    {
                        capsbutton.SetActive(true);
                        capsmanreset.SetActive(false);
                        capstimer.SetActive(false);
                        plscaps.GetComponent<Text>().text = "";
                        table.transform.GetChild(3).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                        table.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                        table.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "Not done";
                        capsexpire.text = "EXPIRED";
                    });
                }

            }
        });
    }

    private void updateCapsTimer()
    {
        capsTimerCounter += Time.deltaTime;
        int d = 6 - (int)DateTime.Now.DayOfWeek;
        int h = 23 - DateTime.Now.Hour;
        int m = 59 - DateTime.Now.Minute;
        int s = 59 - DateTime.Now.Second;
        if(capsTimerCounter > 1f)
        {
            capsTimerCounter = 0f;
            capstimer.GetComponent<Text>().text = d.ToString() + "d " + h.ToString() + "h " + m.ToString() + "m " + s.ToString() + "s";
            capsexpire.text = "Valid For: " + (d).ToString() + "d " + h.ToString() + "h " + m.ToString() + "m";
        }

        if(cal.GetWeekOfYear(DateTime.Now, cwr, dow) != currentWeek)
        {
            db.Child(username).Child("capsCompleted").SetValueAsync(false);
            db.Child(username).Child("capsProgress").SetValueAsync((double)0);
            db.Child(username).Child("capsDeletedWeek").SetValueAsync(cal.GetWeekOfYear(DateTime.Now, cwr, dow));
            db.Child(username).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
            UnityMainThread.wkr.AddJob(() =>
            {
                capsbutton.SetActive(true);
                capsmanreset.SetActive(false);
                capstimer.SetActive(false);
                plscaps.GetComponent<Text>().text = "";
                table.transform.GetChild(3).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                table.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                table.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = "Not done";
                capsexpire.text = "EXPIRED";

                if (user.GetComponent<User>().sqn == "150")
                {
                    if (user.GetComponent<User>().opscat == "Student")
                    {
                        db.Child(username).Child("quizCompleted").SetValueAsync(false);
                        db.Child(username).Child("quizProgress").SetValueAsync((double)0);
                        db.Child(username).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
                        db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);

                        UnityMainThread.wkr.AddJob(() =>
                        {
                            cdbut.SetActive(true);
                            table.transform.GetChild(4).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                            table.transform.GetChild(7).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                            table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Not done";
                            timer.SetActive(false);
                            manreset.SetActive(false);
                            GameObject.Find("pls").GetComponent<Text>().text = "";
                            quizexpire.text = "EXPIRED";
                            //leaderboard.SetActive(false);
                        });
                    }
                }
            });



        }
    }

    

    private void updateTimer()
    {
        timerCounter += Time.deltaTime;
        int y = DateTime.Now.Year;
        int m = DateTime.Now.Month;
        int m1 = DateTime.Now.AddDays(1).Month;
        if (quizcompletedahead)
        {
            m = m1;
        }
        var resetDT = new DateTime(y, m, DateTime.DaysInMonth(y, m), 23, 59, 59);

        var diff = resetDT - DateTime.Now.AddDays(1);
        var altdiff = resetDT - DateTime.Now;


        if (timerCounter > 1f)
        {
            timerCounter = 0f;
            if (quizcompleted)
            {
                quizexpire.text = "Valid For: " + (altdiff.Days).ToString() + "d " + altdiff.Hours.ToString() + "h " + altdiff.Minutes.ToString() + "m";
            }
            else
            {
                quizexpire.text = "EXPIRED";
            }

            if (timer.activeSelf)
            {
                timer.GetComponent<Text>().text = diff.Days.ToString() + "d " + diff.Hours.ToString() + "h " + diff.Minutes.ToString() + "m " + diff.Seconds.ToString() + "s";
            }

            //Debug.Log(resetDT - DateTime.Now);
        }

        if (!quizcompletedahead)
        {
            if (m != currentMonth)
            {
                db.Child(username).Child("quizCompleted").SetValueAsync(false);
                db.Child(username).Child("quizProgress").SetValueAsync((double)0);
                db.Child(username).Child("latestDeletedMonth").SetValueAsync(DateTime.Now.Month);
                db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.Year);

                if (user.GetComponent<User>().sqn == "145")
                {
                    db.Child(username).Child("nccCompleted").SetValueAsync(false);
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        uncaged.SetActive(false);
                    });

                }
                //GameObject.Find("notif").GetComponent<Text>().text = "you have yet to complete the monthly quiz for";
                //GameObject.Find("month").GetComponent<Text>().text = months[m-1];
                //GameObject.Find("pls").GetComponent<Text>().text = "choose quiz type.";
                cdbut.SetActive(true);

                table.transform.GetChild(4).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                table.transform.GetChild(7).GetComponent<Image>().color = new Color32(255, 125, 112, 210);
                table.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "Not done";
                timer.SetActive(false);
                manreset.SetActive(false);
                quizexpire.text = "EXPIRED";
                //leaderboard.SetActive(false);
            }

            if (m1 != currentMonth)
            {
                Debug.Log("reached here");
                db.Child(username).Child("latestDeletedMonth").SetValueAsync(m1);
                db.Child(username).Child("latestDeletedYear").SetValueAsync(DateTime.Now.AddDays(1).Year);
                db.Child(username).Child("quizcompletedahead").SetValueAsync(false);
                cdbut.SetActive(true);
                timer.SetActive(false);
                manreset.SetActive(false);
                GameObject.Find("pls").GetComponent<Text>().text = "";


            }
        }




    }

    private void getCallSign()
    {
        db.Child(username).Child("callsign").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("db down");
            }else if (task.IsCompleted)
            {
                //plswait.SetActive(false);
                DataSnapshot ds = task.Result;
                if(ds.Value != null && (string)ds.Value != "")
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        cs.text = (string)ds.Value;
                    });
                }
                else
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        cs.text = "FNG 69";
                    });
                }
            }
        });
    }



    private void checkAdminRights()
    {
        db.Child(username).Child("isAdmin").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("db down");

            }else if (task.IsCompleted)
            {
                DataSnapshot ds = task.Result;
                if ((bool)ds.Value)
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        admin.SetActive(true);
                    });
                   
                }
                else
                {
                    UnityMainThread.wkr.AddJob(() =>
                    {
                        admin.SetActive(false);
                    });
                }
            }
        });
    }


    public void loadForCD()
    {
        user.GetComponent<User>().isDualQual = false;
        viperanim.SetTrigger("flyout");
        GameObject.Find("hello").SetActive(false);
        GameObject.Find("cs").SetActive(false);
        //GameObject.Find("notif").SetActive(false);
        //GameObject.Find("month").SetActive(false);
        table.SetActive(false);
        cdbut.SetActive(false);
        if (!user.GetComponent<User>().iphonebuild)
        {
            GameObject.Find("androidproblem").SetActive(false);
        }
        GameObject.Find("pls").SetActive(false);
        GameObject.Find("opscat").SetActive(false);
        //GameObject.Find("bestTime").SetActive(false);
        //GameObject.Find("dateCompleted").SetActive(false);
        admin.SetActive(false);
        leaderboard.SetActive(false);
        capsbutton.SetActive(false);
        plscaps.SetActive(false);
        nccbutton.SetActive(false);

        Invoke("loadQuiz", 1f);

    }

    public void loadForDQ()
    {
        user.GetComponent<User>().isDualQual = true;
        viperanim.SetTrigger("flyout");
        GameObject.Find("hello").SetActive(false);
        GameObject.Find("cs").SetActive(false);
        //GameObject.Find("notif").SetActive(false);
        //GameObject.Find("month").SetActive(false);
        table.SetActive(false);
        cdbut.SetActive(false);
        if (!user.GetComponent<User>().iphonebuild)
        {
            GameObject.Find("androidproblem").SetActive(false);
        }
        GameObject.Find("pls").SetActive(false);
        GameObject.Find("opscat").SetActive(false);
        //GameObject.Find("bestTime").SetActive(false);
        //GameObject.Find("dateCompleted").SetActive(false);
        admin.SetActive(false);
        leaderboard.SetActive(false);
        capsbutton.SetActive(false);
        plscaps.SetActive(false);

        nccbutton.SetActive(false);
        Invoke("loadQuiz", 1f);

    }

    public void loadCapsActions()
    {
        viperanim.SetTrigger("flyout");
        GameObject.Find("hello").SetActive(false);
        GameObject.Find("cs").SetActive(false);
        //GameObject.Find("notif").SetActive(false);
        //GameObject.Find("month").SetActive(false);
        table.SetActive(false);
        cdbut.SetActive(false);
        if (!user.GetComponent<User>().iphonebuild)
        {
            GameObject.Find("androidproblem").SetActive(false);
        }
        GameObject.Find("pls").SetActive(false);
        GameObject.Find("opscat").SetActive(false);
        //GameObject.Find("bestTime").SetActive(false);
        //GameObject.Find("dateCompleted").SetActive(false);
        admin.SetActive(false);
        leaderboard.SetActive(false);
        capsbutton.SetActive(false);
        plscaps.SetActive(false);
        nccbutton.SetActive(false);
        Invoke("loadCaps", 1f);
    }

    private void loadCaps()
    {
        if(user.GetComponent<User>().sqn == "150")
        {
            SceneManager.LoadScene("Caps150");
        }
        else
        {
            if(user.GetComponent<User>().qual == "F15")
            {
                SceneManager.LoadScene("CapsF15");
            }
            else
            {
                if(user.GetComponent<User>().sqn == "PC2 (USAF)")
                {
                    SceneManager.LoadScene("CapsUSAF");
                }
                else
                {
                    SceneManager.LoadScene("Caps");
                }

            }

        }

    }

    private void loadQuiz()
    {
        if(user.GetComponent<User>().sqn == "150")
        {
            SceneManager.LoadScene("OpsLim150");
        }
        else
        {
            if(user.GetComponent<User>().qual == "F15")
            {
                SceneManager.LoadScene("OLF15");
            }
            else
            {
                if(user.GetComponent<User>().qual == "F16" || user.GetComponent<User>().qual == "F16 C/D")
                {
                    if(user.GetComponent<User>().sqn == "PC2 (USAF)")
                    {
                        SceneManager.LoadScene("OLF16USAF");
                    }
                    else
                    {
                        SceneManager.LoadScene("quizCD");
                    }

                }
                else
                {
                    SceneManager.LoadScene("MainQuiz");
                }

            }

        }

    }

    private void loadAd()
    {
        SceneManager.LoadScene("Dashboard");
    }
    public void loadAdmin()
    {
        viperanim.SetTrigger("flyout");
        GameObject.Find("hello").SetActive(false);
        GameObject.Find("cs").SetActive(false);
        //GameObject.Find("notif").SetActive(false);
        //GameObject.Find("month").SetActive(false);
        table.SetActive(false);
        cdbut.SetActive(false);
        if (!user.GetComponent<User>().iphonebuild)
        {
            GameObject.Find("androidproblem").SetActive(false);
        }

        GameObject.Find("pls").SetActive(false);
        GameObject.Find("opscat").SetActive(false);
        //GameObject.Find("bestTime").SetActive(false);
        //GameObject.Find("dateCompleted").SetActive(false);
        admin.SetActive(false);
        leaderboard.SetActive(false);
        capsbutton.SetActive(false);
        plscaps.SetActive(false);
        nccbutton.SetActive(false);
        Invoke("loadAd", 1f);
    }

    public void loadDash()
    {
        viperanim.SetTrigger("flyout");
        GameObject.Find("hello").SetActive(false);
        GameObject.Find("cs").SetActive(false);
        //GameObject.Find("notif").SetActive(false);
        //GameObject.Find("month").SetActive(false);
        table.SetActive(false);
        cdbut.SetActive(false);
        if (!user.GetComponent<User>().iphonebuild)
        {
            GameObject.Find("androidproblem").SetActive(false);
        }
        GameObject.Find("pls").SetActive(false);
        GameObject.Find("opscat").SetActive(false);
        //GameObject.Find("bestTime").SetActive(false);
        //GameObject.Find("dateCompleted").SetActive(false);
        admin.SetActive(false);
        leaderboard.SetActive(false);
        capsbutton.SetActive(false);
        plscaps.SetActive(false);
        nccbutton.SetActive(false);
        Invoke("LoadD", 1f);
    }

    public void loadncc()
    {
        viperanim.SetTrigger("flyout");
        GameObject.Find("hello").SetActive(false);
        GameObject.Find("cs").SetActive(false);
        //GameObject.Find("notif").SetActive(false);
        //GameObject.Find("month").SetActive(false);
        table.SetActive(false);
        cdbut.SetActive(false);
        if (!user.GetComponent<User>().iphonebuild)
        {
            GameObject.Find("androidproblem").SetActive(false);
        }
        GameObject.Find("pls").SetActive(false);
        GameObject.Find("opscat").SetActive(false);
        //GameObject.Find("bestTime").SetActive(false);
        //GameObject.Find("dateCompleted").SetActive(false);
        admin.SetActive(false);
        leaderboard.SetActive(false);
        capsbutton.SetActive(false);
        plscaps.SetActive(false);
        nccbutton.SetActive(false);
        Invoke("LoadNCC", 1f);
    }

    private void LoadD()
    {
        SceneManager.LoadScene("NormDash");
    }

    private void LoadNCC()
    {
        SceneManager.LoadSceneAsync("NCC");
    }

    public void resetOwnQuiz()
    {
        confirmReset.SetActive(true);

    }

    public void resetOwnCaps()
    {
        confirmRCaps.SetActive(true);
    }

    public void nahdontrcaps()
    {
        confirmRCaps.SetActive(false);
    }

    public void nahdontreset()
    {
        confirmReset.SetActive(false);
    }

    public void resetforreal()
    {
        plswait.SetActive(true);
        db.Child(username).Child("quizCompleted").SetValueAsync(false);
        db.Child(username).Child("quizProgress").SetValueAsync((double)0);
        UnityMainThread.wkr.AddJob(() =>
        {
            SceneManager.LoadScene("Landing");
        });
    }

    public void resetcapsFurreal()
    {
        plswait.SetActive(true);
        db.Child(username).Child("capsCompleted").SetValueAsync(false);
        db.Child(username).Child("capsProgress").SetValueAsync((double)0);
        db.Child(username).Child("capsDeletedWeek").SetValueAsync(cal.GetWeekOfYear(DateTime.Now, cwr, dow));
        db.Child(username).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("db error");
            }
            else if (task.IsCompleted)
            {
                UnityMainThread.wkr.AddJob(() =>
                {
                    SceneManager.LoadScene("Landing");
                });

            }
        });
    }

    public void logout()
    {
        user.GetComponent<User>().username = null;
        PlayerPrefs.SetString("prevusername", null);
        PlayerPrefs.SetString("prevpassword", null);
        SceneManager.LoadScene("Login");
    }



}
