using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    public string username;
    public string userbeingeditted;
    public bool isDualQual = false;
    public string opscat = "ocu";
    public string qual = "F16 C/D";
    public string sqn = "140";

    public bool hasAndroidProblem = false;
    public bool iphonebuild = false;
    public bool testmode = false;

    private int sccounter = 0;
    // Start is called before the first frame update
    void Awake()
    {
        //TouchScreenKeyboard.hideInput = true;
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    

 
}
