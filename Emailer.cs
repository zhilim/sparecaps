using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Emailer : MonoBehaviour
{

    private GameObject plswait, success, warning;

    // Start is called before the first frame update
    void Start()
    {
        plswait = GameObject.Find("plswait");
        success = GameObject.Find("success");
        warning = GameObject.Find("warning");
        plswait.SetActive(false);
        success.SetActive(false);
        warning.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void sendMail()
    {
        plswait.SetActive(true);
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress(GameObject.Find("password").GetComponent<InputField>().text);
        mail.To.Add("zhilerlim@gmail.com");
        mail.Subject = "Request by: " + GameObject.Find("username").GetComponent<InputField>().text + " " + GameObject.Find("password").GetComponent<InputField>().text;
        mail.Body = GameObject.Find("callsign").GetComponent<InputField>().text;

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential("zhilerlim@gmail.com", "xwftwhvffexgvroy") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };

        smtpServer.Send(mail);
        Debug.Log("success");
        plswait.SetActive(false);
        success.SetActive(true);
        Invoke("back", 3.0f);
    }

    public void back()
    {
        SceneManager.LoadScene("Login");
    }
}
