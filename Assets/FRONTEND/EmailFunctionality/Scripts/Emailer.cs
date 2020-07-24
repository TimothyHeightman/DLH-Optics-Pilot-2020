//  Emailer.cs
//  http://www.mrventures.net/all-tutorials/sending-emails
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Emailer : MonoBehaviour
{
    //[SerializeField] TMPro.TMP_Text toAddressText;
    //[SerializeField] TMPro.TMP_Text ccAddressText;




    [SerializeField] Text toText;
    [SerializeField] Text ccText;

    [SerializeField] UnityEngine.UI.Button btnSubmit;
    [SerializeField] bool sendDirect;

    const string kSenderEmailAddress = "dlhoptics@gmail.com";
    const string kSenderPassword = "BigBrainTime9000";
    const string file = "Assets/FRONTEND/DetectorScreen/img_reduced.txt";


    [SerializeField] string subject = "DLH Optics Lab Data";
    [SerializeField] string message = "This is a test message";
    

    // Method 2: Server request
    const string url = "https://coderboy6000.000webhostapp.com/emailer.php";


    void Start()
    {
        UnityEngine.Assertions.Assert.IsNotNull(toText);
        UnityEngine.Assertions.Assert.IsNotNull(ccText);
        UnityEngine.Assertions.Assert.IsNotNull(btnSubmit);
        btnSubmit.onClick.AddListener(delegate {
            if (sendDirect)
            {
                //string _toAddress = toAddressText.text.Replace(" ", string.Empty);
                string _toAddress = toText.text + "@ic.ac.uk";
                string _ccAddress = ccText.text + "@ic.ac.uk";


                MailAddress toAddress = new MailAddress(_toAddress);
                MailAddress ccAddress = new MailAddress(_ccAddress);


                SendAnEmail(toAddress, ccAddress, subject, message);
            }
            else
            {
                //SendServerRequestForEmail(toAddressText.text, ccAddressText.text, subject, message);
            }
        });
    }


    public static bool IsValidEmail(string Email)
    {
        if (Email != null && Email != "")
            return Regex.IsMatch(Email, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        else
            return false;
    }

    // Method 1: Direct message
    private static void SendAnEmail(MailAddress toAddress, MailAddress ccAddress, string subject, string message)
    {
        // Create mail
        MailAddress fromAddress = new MailAddress(kSenderEmailAddress);
        MailMessage mail = new MailMessage(fromAddress, toAddress);
        mail.CC.Add(ccAddress);
        mail.Subject = subject;
        mail.Body = message;

        // Create  the file attachment for this email message.
        Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);

        // Add time stamp information for the file.
        ContentDisposition disposition = data.ContentDisposition;
        disposition.CreationDate = System.IO.File.GetCreationTime(file);
        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
        disposition.ReadDate = System.IO.File.GetLastAccessTime(file);

        // Add the file attachment to this email message.
        mail.Attachments.Add(data);


        // Setup server 
        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential(
            kSenderEmailAddress, kSenderPassword) as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                Debug.Log("Email success!");
                return true;
            };

        // Send mail to server, print results
        try
        {
            smtpServer.Send(mail);
        }
        catch (System.Exception e)
        {
            Debug.Log("Email error: " + e.Message);
        }
        finally
        {
            Debug.Log("FINISHED");
        }
    }

    // Method 2: Server request
    private void SendServerRequestForEmail(string toAddress, string ccAddress, string subject, string message)
    {
        StartCoroutine(SendMailRequestToServer(toAddress, ccAddress, subject, message));
    }

    // Method 2: Server request
    static IEnumerator SendMailRequestToServer(string toAddress, string ccAddress, string subject, string message)
    {
        // Setup form responses
        WWWForm form = new WWWForm();
        form.AddField("name", "It's me!");
        form.AddField("fromEmail", kSenderEmailAddress);
        form.AddField("toEmail", toAddress);
        form.AddField("message", message);

        // Submit form to our server, then wait
        WWW www = new WWW(url, form);
        Debug.Log("Email sent!");

        yield return www;

        // Print results
        if (www.error == null)
        {
            Debug.Log("WWW Success!: " + www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }
}