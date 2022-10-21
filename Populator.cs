using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using System.Globalization;

public class Populator : MonoBehaviour
{
    public GameObject rowTem, filler;
    private GameObject adminCon;
    private DatabaseReference db;
    private IEnumerable<DataSnapshot> dscollection;
    private User theUser;
    private GameObject loadscreen, confirmReset, confirmResetCaps;
    private int currentM, currentY;
    public bool isNorm = false;
    private Calendar cal;
    private CalendarWeekRule cwr;
    private DayOfWeek dow;
    private int currentweek;

    private Text qdone, cdone, qlcom, clcom;
    // Start is called before the first frame update
    void Start()
    {
        qdone = GameObject.Find("complete").transform.GetChild(0).GetComponent<Text>();
        cdone = GameObject.Find("opscat").transform.GetChild(0).GetComponent<Text>();
        qlcom = GameObject.Find("timing").transform.GetChild(0).GetComponent<Text>();
        clcom = GameObject.Find("capslastcompletedheader").transform.GetChild(0).GetComponent<Text>();
        initCalendar();
        adminCon = GameObject.Find("adminContent");
        loadscreen = GameObject.Find("Loading");
        theUser = GameObject.Find("User").GetComponent<User>();
        if (theUser.testmode)
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("test");
        }
        else
        {
            db = FirebaseDatabase.DefaultInstance.GetReference("users");
        }
       
        pullData();
        currentM = DateTime.Now.Month;
        string sqn_t = theUser.sqn;
        if (theUser.sqn == "PC2 (USAF)" || theUser.sqn == "PC2")
        {
            sqn_t = "425 FS";
            if(theUser.sqn == "PC2 (USAF)")
            {
                qdone.text = "EL Done?";
                qlcom.text = "EL Last Completed";
            }

        }

        if(theUser.sqn == "150")
        {
            qdone.text = "OL Done?";
            cdone.text = "BF A Done?";
            qlcom.text = "OL Last Completed";
            clcom.text = "BF A Last Completed";
        }

        if(theUser.qual == "F15")
        {
            qdone.text = "OL Done?";
            qlcom.text = "OL Last Completed";
        }

        if (!isNorm)
        {
            confirmReset = GameObject.Find("confirmReset");
            confirmReset.SetActive(false);
            confirmResetCaps = GameObject.Find("confirmResetCaps");
            confirmResetCaps.SetActive(false);

            GameObject.Find("Title").GetComponent<Text>().text = sqn_t + " ADMIN";
        }
        else
        {
            GameObject.Find("Title").GetComponent<Text>().text = sqn_t + " Dashboard";
        }


        currentweek = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
        //currentY = DateTime.Now.Year;
        //Instantiate(rowTem, adminCon.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if(DateTime.Now.Month != currentM)
        {
            currentM = DateTime.Now.Month;
            indieMonthlyCheck();
        }

        if(cal.GetWeekOfYear(DateTime.Now, cwr, dow) != currentweek)
        {
            currentweek = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
            indieMonthlyCheck();
        }
    }

    private void initCalendar()
    {
        CultureInfo myCI = new CultureInfo("en-US");
        cal = myCI.Calendar;
        cwr = myCI.DateTimeFormat.CalendarWeekRule;
        dow = myCI.DateTimeFormat.FirstDayOfWeek;
    }

    private void pullData()
    {
        
        db.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("check internet connection");
                return;

            }else if (task.IsCompleted)
            {
                Debug.Log("task complete");
                DataSnapshot userds = task.Result;
                Debug.Log("childs in snapshot" + userds.ChildrenCount.ToString());
                dscollection = userds.Children;
                UnityMainThread.wkr.AddJob(() =>
                {
                    populateTable();
                    addFillers();
                    checkForMonthlyReset();
                });

            }
        });
    }

    private void indieMonthlyCheck()
    {
        db.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("check internet connection");
                return;

            }
            else if (task.IsCompleted)
            {
                Debug.Log("task complete");
                DataSnapshot userds = task.Result;
                Debug.Log("childs in snapshot" + userds.ChildrenCount.ToString());
                dscollection = userds.Children;
                UnityMainThread.wkr.AddJob(() =>
                {
                    checkForMonthlyReset();
                });

            }
        });
    }

    private void addFillers()
    {
        for(int x = 0; x < 15; x++)
        {
            Instantiate(filler, adminCon.transform);
        }
    }


    private void populateTable()
    {
        if(theUser.sqn == "architect")
        {
            foreach(DataSnapshot ds in dscollection)
            {
                instantiateRow(ds);
            }

        }
        else
        {
            foreach (DataSnapshot ds in dscollection)
            {
                if ((string)ds.Child("sqn").Value == theUser.sqn)
                {
                    instantiateRow(ds);
                }
                else
                {
                    if(theUser.sqn == "PC2 (USAF)" && (string)ds.Child("sqn").Value == "PC2")
                    {
                        instantiateRow(ds);
                    }

                    if (theUser.sqn == "PC2" && (string)ds.Child("sqn").Value == "PC2 (USAF)")
                    {
                        instantiateRow(ds);
                    }
                }

            }
        }

        //tablePopulated = true;
        loadscreen.SetActive(false);
    }

    private void instantiateRow(DataSnapshot ds)
    {
        GameObject row = Instantiate(rowTem, adminCon.transform);
        row.name = ds.Key;
        row.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = ds.Key;
        if (ds.Child("capsProgress").Value != null)
        {
            row.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = ds.Child("capsProgress").Value.ToString();
        }
        else
        {
            row.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "-";
        }

        row.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = (ds.Child("quizProgress").Value.ToString()) + "%";

        if (theUser.sqn == "145")
        {
            if(ds.Child("nccCompleted").Value != null)
            {
                if ((bool)ds.Child("nccCompleted").Value)
                {
                    row.transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    row.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                row.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
            }

        }
        else
        {
            row.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
        }

        if ((bool)ds.Child("quizCompleted").Value)
        {
            row.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "Yes";
            row.transform.GetChild(2).GetComponent<Image>().color = new Color32(123, 219, 121, 255);

        }
        else
        {
            row.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "No";
            row.transform.GetChild(2).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
        }

        if (ds.Child("capsCompleted").Value != null)
        {
            if ((bool)ds.Child("capsCompleted").Value)
            {
                row.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = "Yes";
                row.transform.GetChild(5).GetComponent<Image>().color = new Color32(123, 219, 121, 255);
            }
            else
            {
                row.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = "No";
                row.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
            }

        }
        else
        {
            row.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = "No";
            row.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
        }

        if (ds.Child("dateLastCompleted").Value != null)
        {
            /*//FIXERRRR
            string istr = (string)ds.Child("dateLastCompleted").Value;
            DateTime odt = DateTime.Parse("01/04/21");


            try
            {
                odt = DateTime.ParseExact(istr, "dd/MM/yy", CultureInfo.InvariantCulture);
            }
            catch
            {
                odt = DateTime.Parse("01/04/21");
            }

            if(odt.Month == DateTime.Now.Month)
            {
                db.Child(ds.Key).Child("quizCompleted").SetValueAsync(true);
                db.Child(ds.Key).Child("quizProgress").SetValueAsync((double)100);
            }*/
            row.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = (string)ds.Child("dateLastCompleted").Value;
        }
        else
        {
            row.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "NA";
        }

        if (ds.Child("capsLastCompleted").Value != null)
        {
            /*//FIXERRR
            string istr = (string)ds.Child("capsLastCompleted").Value;
            DateTime odt = DateTime.Parse("01/04/21");

            try
            {
                odt = DateTime.ParseExact(istr, "dd/MM/yy", CultureInfo.InvariantCulture);
            }
            catch
            {
                odt = DateTime.Parse("01/04/21");
            }

            int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
            int coy = cal.GetWeekOfYear(odt, cwr, dow);

            if (coy == woy)
            {
                db.Child(ds.Key).Child("capsCompleted").SetValueAsync(true);
                db.Child(ds.Key).Child("capsProgress").SetValueAsync((double)100);
            }*/

            row.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = (string)ds.Child("capsLastCompleted").Value;
        }
        else
        {
            row.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = "NA";
        }

        db.Child(ds.Key).ValueChanged += updateScores;
    }

    private void checkForMonthlyReset()
    {
        int m = DateTime.Now.Month;
        int y = DateTime.Now.Year;
        foreach(DataSnapshot ds in dscollection)
        {
            if (y == int.Parse(ds.Child("latestDeletedYear").Value.ToString()))
            {
                //fixer!
                //db.Child(ds.Key).Child("latestDeletedMonth").SetValueAsync(6);


                if (m > int.Parse(ds.Child("latestDeletedMonth").Value.ToString()))
                {
                    db.Child(ds.Key).Child("quizCompleted").SetValueAsync(false);
                    db.Child(ds.Key).Child("quizProgress").SetValueAsync((double)0);
                    db.Child(ds.Key).Child("latestDeletedYear").SetValueAsync(y);
                    db.Child(ds.Key).Child("latestDeletedMonth").SetValueAsync(m);
                    db.Child(ds.Key).Child("nccCompleted").SetValueAsync(false);

                }

            }
            else if(y > int.Parse(ds.Child("latestDeletedYear").Value.ToString()))
            {
                db.Child(ds.Key).Child("quizCompleted").SetValueAsync(false);
                db.Child(ds.Key).Child("quizProgress").SetValueAsync((double)0);
                db.Child(ds.Key).Child("latestDeletedYear").SetValueAsync(y);
                db.Child(ds.Key).Child("latestDeletedMonth").SetValueAsync(m);
                db.Child(ds.Key).Child("nccCompleted").SetValueAsync(false);

            }

            if(ds.Child("capsDeletedWeek").Value == null)
            {
                Debug.Log("new user, resetting caps.");
                int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
                db.Child(ds.Key).Child("capsCompleted").SetValueAsync(false);
                db.Child(ds.Key).Child("capsProgress").SetValueAsync((double)0);
                db.Child(ds.Key).Child("capsDeletedWeek").SetValueAsync(woy);
                db.Child(ds.Key).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
            }
            else
            {
                //fixer!
                //db.Child(ds.Key).Child("capsDeletedWeek").SetValueAsync(24);
                if (y == int.Parse(ds.Child("capsDeletedYear").Value.ToString()))
                {
                    int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
                    if (woy > int.Parse(ds.Child("capsDeletedWeek").Value.ToString()))
                    {

                        db.Child(ds.Key).Child("capsCompleted").SetValueAsync(false);
                        db.Child(ds.Key).Child("capsProgress").SetValueAsync((double)0);
                        db.Child(ds.Key).Child("capsDeletedWeek").SetValueAsync(woy);
                        db.Child(ds.Key).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);

                        if(ds.Child("sqn").Value.ToString() == "150")
                        {
                            if(ds.Child("opscat").Value.ToString() == "Student")
                            {
                                db.Child(ds.Key).Child("quizCompleted").SetValueAsync(false);
                                db.Child(ds.Key).Child("quizProgress").SetValueAsync((double)0);
                                db.Child(ds.Key).Child("latestDeletedYear").SetValueAsync(y);
                                db.Child(ds.Key).Child("latestDeletedMonth").SetValueAsync(m);
                            }
                        }
                    }
                }else if(y > int.Parse(ds.Child("capsDeletedYear").Value.ToString()))
                {
                    int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
                    db.Child(ds.Key).Child("capsCompleted").SetValueAsync(false);
                    db.Child(ds.Key).Child("capsProgress").SetValueAsync((double)0);
                    db.Child(ds.Key).Child("capsDeletedWeek").SetValueAsync(woy);
                    db.Child(ds.Key).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
                }
            }
        }

    }

    public void removeHandlers()
    {
        foreach (DataSnapshot ds in dscollection)
        {
            Debug.Log("removing Handler for " + ds.Key);
            db.Child(ds.Key).ValueChanged -= updateScores;
        }
    }



    void updateScores(object sender, ValueChangedEventArgs e)
    {
        if(e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError);
            return;
        }
        else
        {
            DataSnapshot ds = e.Snapshot;
            GameObject r = GameObject.Find((string)ds.Key);
            r.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = (ds.Child("quizProgress").Value.ToString()) + "%";
            float prog = float.Parse(ds.Child("quizProgress").Value.ToString());
            float perc = prog/100f;
            r.transform.GetChild(3).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(perc, 1f, 1f);

            if(ds.Child("capsProgress").Value != null)
            {
                r.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = (ds.Child("capsProgress").Value.ToString()) + "%";
                float cprog = float.Parse(ds.Child("capsProgress").Value.ToString());
                float cperc = cprog / 100f;
                r.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(cperc, 1f, 1f);
            }
            else
            {
                float cprog = 0f;
                float cperc = cprog / 100f;
                r.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(cperc, 1f, 1f);
            }


            if ((bool)ds.Child("quizCompleted").Value)
            {
                r.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "Yes";
                r.transform.GetChild(2).GetComponent<Image>().color = new Color32(123, 219, 121, 255);
                r.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = (string)ds.Child("dateLastCompleted").Value;
            }
            else
            {
                r.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "No";
                r.transform.GetChild(2).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
            }

            if(ds.Child("capsCompleted").Value != null)
            {
                if ((bool)ds.Child("capsCompleted").Value)
                {
                    r.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = "Yes";
                    r.transform.GetChild(5).GetComponent<Image>().color = new Color32(123, 219, 121, 255);
                    r.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = (string)ds.Child("capsLastCompleted").Value;
                }
                else
                {
                    r.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = "No";
                    r.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
                }
            }
            else
            {
                r.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = "No";
                r.transform.GetChild(5).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
            }


        }
    }

    public void AddPilot()
    {
        //removeHandlers();
        SceneManager.LoadScene("AddPilot");
    }

    public void backToMain()
    {
        SceneManager.LoadScene("Landing");
    }

    public void resetAll()
    {
        confirmReset.SetActive(true);
    }

    public void resetAllCaps()
    {
        confirmResetCaps.SetActive(true);
    }

    public void dontresetcaps()
    {
        confirmResetCaps.SetActive(false);
    }

    public void nahdontreset()
    {
        confirmReset.SetActive(false);
    }

    public void resetForReal()
    {
        int m = DateTime.Now.Month;
        int y = DateTime.Now.Year;
        foreach (DataSnapshot ds in dscollection)
        {
            if(theUser.sqn == "architect")
            {
                db.Child(ds.Key).Child("quizCompleted").SetValueAsync(false);
                db.Child(ds.Key).Child("quizProgress").SetValueAsync((double)0);

            }
            else
            {
                if(theUser.sqn == (string)ds.Child("sqn").Value)
                {
                    db.Child(ds.Key).Child("quizCompleted").SetValueAsync(false);
                    db.Child(ds.Key).Child("quizProgress").SetValueAsync((double)0);

                }
            }



        }
        confirmReset.SetActive(false);
    }


    public void resetCapsForReal()
    {
        int woy = cal.GetWeekOfYear(DateTime.Now, cwr, dow);
        foreach (DataSnapshot ds in dscollection)
        {

            if(ds.Child("capsCompleted").Value != null)
            {
                if (theUser.sqn == "architect")
                {
                    db.Child(ds.Key).Child("capsCompleted").SetValueAsync(false);
                    db.Child(ds.Key).Child("capsProgress").SetValueAsync((double)0);
                    db.Child(ds.Key).Child("capsDeletedWeek").SetValueAsync(woy);
                    db.Child(ds.Key).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
                }
                else
                {
                    if (theUser.sqn == (string)ds.Child("sqn").Value)
                    {
                        db.Child(ds.Key).Child("capsCompleted").SetValueAsync(false);
                        db.Child(ds.Key).Child("capsProgress").SetValueAsync((double)0);
                        db.Child(ds.Key).Child("capsDeletedWeek").SetValueAsync(woy);
                        db.Child(ds.Key).Child("capsDeletedYear").SetValueAsync(DateTime.Now.Year);
                    }
                }


            }

        }
        confirmResetCaps.SetActive(false);

    }




}
