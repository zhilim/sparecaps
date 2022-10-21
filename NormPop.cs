using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NormPop : MonoBehaviour
{
    public GameObject normTem, filler;
    private User theUser;
    private GameObject adminCon;
    private DatabaseReference db;
    private IEnumerable<DataSnapshot> dscollection;

    private GameObject loadscreen;
    // Start is called before the first frame update
    void Start()
    {
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

        GameObject.Find("Title").GetComponent<Text>().text = theUser.sqn + " ADMIN";
        pullData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void pullData()
    {

        db.OrderByChild("opscat").GetValueAsync().ContinueWith(task =>
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
                    populateTable();
                    addFillers();
                });

            }
        });
    }

    private void addFillers()
    {
        for (int x = 0; x < 15; x++)
        {
            Instantiate(filler, adminCon.transform);
        }
    }

    private void populateTable()
    {
        if(theUser.sqn == "architect")
        {
            foreach (DataSnapshot ds in dscollection)
            {
                //Debug.Log("doing for 1 child");
                GameObject row = Instantiate(normTem, adminCon.transform);
                row.name = ds.Key;
                row.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = ds.Key;
                row.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = (string)ds.Child("callsign").Value;
                row.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = (ds.Child("quizProgress").Value.ToString()) + "%";

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
                if (ds.Child("dateLastCompleted").Value != null)
                {
                    row.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = (string)ds.Child("dateLastCompleted").Value;
                    var dateString = (string)ds.Child("bestTime").Value;
                    DateTime elapsed = DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
                    row.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = elapsed.Minute.ToString() + "m " + elapsed.Second.ToString() + "s";
                }
                else
                {
                    row.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "NA";
                }

                row.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = (string)ds.Child("opscat").Value;

                db.Child(ds.Key).ValueChanged += updateScores;



            }
        }
        else
        {
            foreach (DataSnapshot ds in dscollection)
            {
                if((string)ds.Child("sqn").Value == theUser.sqn)
                {
                    //Debug.Log("doing for 1 child");
                    GameObject row = Instantiate(normTem, adminCon.transform);
                    row.name = ds.Key;
                    row.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = ds.Key;
                    row.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = (string)ds.Child("callsign").Value;
                    row.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = (ds.Child("quizProgress").Value.ToString()) + "%";

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
                    if (ds.Child("dateLastCompleted").Value != null)
                    {
                        row.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = (string)ds.Child("dateLastCompleted").Value;
                        var dateString = (string)ds.Child("bestTime").Value;
                        DateTime elapsed = DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
                        row.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = elapsed.Minute.ToString() + "m " + elapsed.Second.ToString() + "s";
                    }
                    else
                    {
                        row.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "NA";
                    }

                    row.transform.GetChild(5).GetChild(0).GetComponent<Text>().text = (string)ds.Child("opscat").Value;

                    db.Child(ds.Key).ValueChanged += updateScores;
                }
            }
        }

        //tablePopulated = true;
        loadscreen.SetActive(false);
    }

    void updateScores(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
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
            float perc = prog / 100f;
            r.transform.GetChild(3).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(perc, 1f, 1f);
            if ((bool)ds.Child("quizCompleted").Value)
            {
                r.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "Yes";
                r.transform.GetChild(2).GetComponent<Image>().color = new Color32(123, 219, 121, 255);
                r.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = (string)ds.Child("dateLastCompleted").Value;
                var dateString = (string)ds.Child("bestTime").Value;
                TimeSpan elapsed = TimeSpan.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
                r.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = elapsed.Minutes.ToString() + "m " + elapsed.Seconds.ToString() + "s";
            }
            else
            {
                r.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "No";
                r.transform.GetChild(2).GetComponent<Image>().color = new Color32(255, 134, 137, 255);
            }
        }
    }

    public void backToMain()
    {
        SceneManager.LoadScene("Landing");
    }

}
