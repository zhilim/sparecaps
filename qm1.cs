using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;

public class qm1 : MonoBehaviour
{
    private GameObject gj;
    private int totalNumOfBlanks;
    private int totalAnswered = 0;
    public InputField firstBlank;
    private float canvasHeight;
    public float currentYFocusPos = 0f;
    private RectTransform pj;
    private float pjTargetLoc = 0f;
    private bool flying = false;
    public float jetspeed;
    private Text percentage;
    private DatabaseReference db;
    private string username;
    private GameObject plswait;
    private DateTime startTime;
    private bool virgin = true;
    private TimeSpan bestTime;
    public bool isCaps = false;
    public bool isF16quiz = true;
    public bool isNCC = false;
    private string bestTimeRef = "bestTime";
    private string completedRef, datelastcomRef, percRef;
    private GameObject rm1, rm2, rm3, rm4, rm5, rm6, rm7, rm8;
    public GameObject[] rm1blanks, rm2blanks, rm3blanks, rm4blanks, rm5blanks, rm6blanks, rm7blanks, rm8blanks, gos;


    // Start is called before the first frame update
    void Start()
    {
        //TouchScreenKeyboard.hideInput = true;
        //Debug.Log(DateTime.Now.ToString("dd/MM/yy"));
        if (GameObject.Find("User").GetComponent<User>().testmode)
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("test");
        }
        else
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("users");
        }

        gj = GameObject.Find("goodjob");
        gj.SetActive(false);
        canvasHeight = GameObject.Find("Panel").GetComponent<RectTransform>().sizeDelta.y;
        Debug.Log("canvas height: " + canvasHeight.ToString());
        gos = GameObject.FindGameObjectsWithTag("field");
        checkForAndroidProblem();
        //PlayerPrefs.DeleteAll();
        firstBlank.GetComponent<InputField>().Select();
        //TouchScreenKeyboard.hideInput = true;
        plswait = GameObject.Find("plswait").gameObject;
        plswait.SetActive(false);
        pj = GameObject.Find("progressjet").GetComponent<RectTransform>();

        Debug.Log("there are " + gos.Length.ToString() + " fields!");
        totalNumOfBlanks = gos.Length;
        percentage = GameObject.Find("percentage").GetComponent<Text>();
        username = GameObject.Find("User").GetComponent<User>().username;
        //username = "Zhi Ler";

        if (!isCaps)
        {
            if (isF16quiz)
            {
                rm1 = GameObject.Find("Remarks1"); rm2 = GameObject.Find("Remarks2");
                rm3 = GameObject.Find("Remarks3"); rm4 = GameObject.Find("Remarks4");
                rm5 = GameObject.Find("Remarks5"); rm6 = GameObject.Find("Remarks6");
                rm7 = GameObject.Find("Remarks7"); rm8 = GameObject.Find("Remarks8");

                rm1.SetActive(false); rm2.SetActive(false); rm3.SetActive(false); rm4.SetActive(false);
                rm5.SetActive(false); rm6.SetActive(false); rm7.SetActive(false); rm8.SetActive(false);
            }
           
            datelastcomRef = "dateLastCompleted";
            completedRef = "quizCompleted";
            percRef = "quizProgress";
        }

        if (isCaps)
        {
            bestTimeRef = "bestTimeCaps";
            datelastcomRef = "capsLastCompleted";
            completedRef = "capsCompleted";
            percRef = "capsProgress";

            if (isNCC)
            {
                completedRef = "nccCompleted";
            }
        }


        startTime = DateTime.Now;

        if(username != "guest")
        {
            db.Child(username).Child(bestTimeRef).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("db error");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot ds = task.Result;
                    if (ds.Value != null)
                    {
                        virgin = false;
                        var dateString = (string)ds.Value;
                        bestTime = TimeSpan.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
                    }

                }
            });
        }

        //username = "Jack";
        //db.Child(username).Child("quizProgress").SetValueAsync((double)0);
        //db.Child(username).Child("quizCompleted").SetValueAsync(false);
    }

    // Update is called once per frame
    void Update()
    {
        flyprogressjet();
    }

    public void autocomplete()
    {
        foreach(GameObject go in gos)
        {
            go.GetComponent<InputField>().text = go.GetComponent<AnswerChecker>().answers[0];
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

    public void checkForRemarksRelease()
    {
        if (!rm1.activeSelf)
        {
            int p = 0;
            foreach(GameObject b in rm1blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if( p == 0)
            {
                rm1.SetActive(true);
            }
        }

        if (!rm2.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm2blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm2.SetActive(true);
            }
        }

        if (!rm3.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm3blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm3.SetActive(true);
            }
        }

        if (!rm4.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm4blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm4.SetActive(true);
            }
        }

        if (!rm5.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm5blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm5.SetActive(true);
            }
        }

        if (!rm6.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm6blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm6.SetActive(true);
            }
        }

        if (!rm7.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm7blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm7.SetActive(true);
            }
        }

        if (!rm8.activeSelf)
        {
            int p = 0;
            foreach (GameObject b in rm8blanks)
            {
                if (!b.GetComponent<AnswerChecker>().isAnswered)
                {
                    p += 1;
                }
            }

            if (p == 0)
            {
                rm8.SetActive(true);
            }
        }
    }

    public void incrementScore()
    {
        if(true)
        {
            totalAnswered += 1;
           
            updateProgressJet();

            if(totalAnswered >= totalNumOfBlanks)
            {
                TimeSpan elapsed = DateTime.Now.Subtract(startTime);
                if(username != "guest")
                {
                    string dtval = DateTime.Now.ToString("dd/MM/yy");
                    if (!isCaps)
                    {

                        db.Child(username).GetValueAsync().ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                Debug.LogError("db error");

                            }
                            else if (task.IsCompleted)
                            {
                                DataSnapshot ds = task.Result;
                                if(DateTime.Now.Month < int.Parse(ds.Child("latestDeletedMonth").Value.ToString()))
                                {
                                    db.Child(username).Child("quizcompletedahead").SetValueAsync(true);
                                    UnityMainThread.wkr.AddJob(() =>
                                    {
                                        dtval = DateTime.Now.AddDays(1).ToString("dd/MM/yy");
                                        db.Child(username).Child(datelastcomRef).SetValueAsync(dtval);
                                    });

                                }
                                else
                                {
                                    UnityMainThread.wkr.AddJob(() =>
                                    {
                                        dtval = DateTime.Now.ToString("dd/MM/yy");
                                        db.Child(username).Child(datelastcomRef).SetValueAsync(dtval);
                                    });
                                }

                            }
                        });

                    }
                    else
                    {
                        if (!isNCC)
                        {
                            db.Child(username).Child(datelastcomRef).SetValueAsync(dtval);
                        }

                    }

                    //PlayerPrefs.SetString(username + datelastcomRef, dtval);
                    //PlayerPrefs.SetInt(username + completedRef, 1);
                    if (!virgin)
                    {
                        if (elapsed < bestTime)
                        {
                            if (!isNCC)
                            {
                                db.Child(username).Child(bestTimeRef).SetValueAsync(elapsed.ToString());
                            }

                        }
                    }
                    else
                    {
                        if (!isNCC)
                        {
                            db.Child(username).Child(bestTimeRef).SetValueAsync(elapsed.ToString());
                        }

                    }
                }



                Debug.Log("Time elapsed: " + elapsed.TotalSeconds.ToString() + " seconds.");
                plswait.SetActive(true);
                jetspeed = 1200f;
                pjTargetLoc = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta.x + pj.sizeDelta.x;

                if(username != "guest")
                {
                    if (!isNCC)
                    {
                        db.Child(username).Child(percRef).SetValueAsync((double)100);
                    }

                    db.Child(username).Child(completedRef).SetValueAsync(true).ContinueWith(task => {
                        if (task.IsFaulted)
                        {
                            Debug.Log("check internet connection");

                        }
                        else if (task.IsCompleted)
                        {
                            UnityMainThread.wkr.AddJob(() =>
                            {
                                SceneManager.LoadScene("Landing");
                            });

                        }

                    });
                }else if(username == "guest")
                {

                    gj.SetActive(true);
                    Invoke("goBack", 3.0f);
                    //invoke go back
                }


            }


        }
       
    }

    private void updateProgressJet()
    {
        float fractionDone = (float)totalAnswered / (float)totalNumOfBlanks;

        float progressX = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta.x - pj.sizeDelta.x;

        float pjX = fractionDone * progressX + pj.sizeDelta.x / 2;
      
        pjTargetLoc = pjX;
        flying = true;
        double perc = (double)Mathf.Round(fractionDone * 100);
        percentage.text = perc.ToString() + "%";

        if(username != "guest")
        {
            if (!isNCC)
            {
                db.Child(username).Child(percRef).SetValueAsync(perc);
            }

        }




    }

    private void flyprogressjet()
    {
        if (flying)
        {
            
            RectTransform progj = GameObject.Find("progressjet").GetComponent<RectTransform>();
            if(progj.position.x < pjTargetLoc)
            {
                Vector3 npjp = new Vector3(progj.position.x + Time.deltaTime * jetspeed, progj.position.y, progj.position.z);
                progj.position = npjp;
            }
            else
            {
                flying = false;
            }

        }
    }

    public int getScore()
    {
        return totalAnswered;
    }

    public float getCanvasHeight()
    {
        return canvasHeight;
    }

    public void goBack()
    {
        SceneManager.LoadScene("Landing");
    }

}
