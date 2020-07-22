using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Mail;
using System;


public class EmailManager : MonoBehaviour
{
    [SerializeField] private string from = "pmassowalsh@gmail.com";
    [SerializeField] private string to = "monstadzn@gmail.com";
    [SerializeField] private string cc = "pm4217@ic.ac.uk";
    [SerializeField] private string subject = "My Subject";
    [SerializeField] private string body = "My Body \rn Full of non-escaped chars";

    //MailAddress adressFrom = new MailAddress(from);
    
    void Start()
    {
        //// checks if the strings entered are URL friendly
        //subject = MyEscapeURL(subject);
        //body = MyEscapeURL(body);

        // generate addresses
        MailAddress addressFrom = new MailAddress(from);
        MailAddress addressTo = new MailAddress(to);
        MailAddress addressCC = new MailAddress(cc);
    }


    public void SendEmail()
    {
        // construct message
        //MailMessage message = new MailMessage(addressFrom, addressTo);
        //message.Subject = subject;
        //message.Body = @body;

        //// add cc
        //message.CC.Add(addressCC);
        //SmtpClient client = new SmtpClient(server);
    }



    //public void SendEmail()
    //{       
    //    //Application.OpenURL("mailto: " + email + "?subject=" + subject + "&body=" + body);
    //}


    //private string MyEscapeURL(string URL)
    //{
    //    return UnityWebRequest.EscapeURL(URL).Replace("+", "%20");
    //}

}
