using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using UnityEngine.UI;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class GoEdit : MonoBehaviour
{
    private GameObject userObject;
    private string thisusername;
    private Populator pop;
    // Start is called before the first frame update
    void Start()
    {
        userObject = GameObject.Find("User");
        thisusername = transform.Find("namecell").GetChild(0).GetComponent<Text>().text;
        pop = GameObject.Find("Databaser").GetComponent<Populator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void editThisUser()
    {
        //pop.removeHandlers();
        userObject.GetComponent<User>().userbeingeditted = thisusername;
        SceneManager.LoadScene("EditPilot");
    }
}
