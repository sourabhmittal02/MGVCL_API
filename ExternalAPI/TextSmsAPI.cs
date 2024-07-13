using CallCenterCoreAPI.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
namespace CallCenterCoreAPI.ExternalAPI.TextSmsAPI
{
    public class TextSmsAPI
    {
        
        ModelSmsAPI modelsmsClone = null;
        public async Task<string> RegisterComplaintSMS(ModelSmsAPI modelsms)
        {
            Stream dataStream;
            modelsmsClone = modelsms;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //forcing .Net framework to use TLSv1.2

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequestDLT");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;

            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";

            request.Method = "POST";

            //System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();
            String U_Convertedmessage = "";

            foreach (char c in modelsms.Smstext)
            {
                int j = (int)c;
                String sss = "&#" + j + ";";
                U_Convertedmessage = U_Convertedmessage + sss;
            }
            String encryptedPassword = encryptedPasswod(modelsmsClone.Password);
            String NewsecureKey = hashGenerator(modelsmsClone.Username.Trim(), modelsmsClone.SenderId.Trim(), U_Convertedmessage.Trim(), modelsmsClone.SecureKey.Trim());


            String smsservicetype = "unicodemsg"; // for unicode msg
            String query = "username=" + HttpUtility.UrlEncode(modelsmsClone.Username.Trim()) +
            "&password=" + HttpUtility.UrlEncode(encryptedPassword) +
            "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +
            "&content=" + HttpUtility.UrlEncode(U_Convertedmessage.Trim()) +
            "&bulkmobno=" + HttpUtility.UrlEncode(modelsms.To) +
            "&senderid=" + HttpUtility.UrlEncode(modelsmsClone.SenderId.Trim()) +
            "&key=" + HttpUtility.UrlEncode(NewsecureKey.Trim()) +
            "&templateid=" + HttpUtility.UrlEncode(modelsms.Smstemplete.Trim());


            byte[] byteArray = Encoding.ASCII.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            String Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            String responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            

            //log.Information("IN RegisterComplaintSMS");
            //modelsmsClone = modelsms;
            //var client = new RestClient(modelsmsClone.SmsApiURL);
            //var restRequest = new RestRequest();
            //restRequest.Method = Method.POST;
            //restRequest.AddHeader("Accept", "application/json");
            //restRequest.RequestFormat = DataFormat.Json;
            //restRequest.AddJsonBody(new
            //{
            //    appid = modelsmsClone.Appid,
            //    userId = modelsmsClone.UserId,
            //    pass = modelsmsClone.Pass,
            //    contenttype = modelsmsClone.Contenttype,
            //    from = modelsmsClone.From,
            //    alert = modelsmsClone.Alert,
            //    selfid = modelsmsClone.Selfid,
            //    to = modelsmsClone.To,
            //    text = modelsmsClone.Smstext,

            //});
            
            ////log.Information("SmsApiURL" + modelsmsClone.SmsApiURL);
            ////log.Information("smstext" + modelsmsClone.Smstext);

            //var response = await client.ExecuteAsync(restRequest);

            ////log.Information(response.Content);
            ////log.Information(response.ResponseStatus.ToString());

            ////response.Content
            //if (response.StatusCode ==  System.Net.HttpStatusCode.OK)
            //{
                
            //    return response.Content;
            //}
            //else
                return responseFromServer;
        }

        public async Task<string> RegisterComplaintSMSEng(ModelSmsAPI modelsms)
        {
            modelsmsClone = modelsms;
            Stream dataStream;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //forcing .Net framework to use TLSv1.2
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(modelsms.SmsApiURL);
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";
            //System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();
            String encryptedPassword = encryptedPasswod(modelsmsClone.Password);
            String NewsecureKey = hashGenerator(modelsmsClone.Username.Trim(), modelsmsClone.SenderId.Trim(), modelsms.Smstext, modelsmsClone.SecureKey.Trim());
            String smsservicetype = "singlemsg"; //For single message.
            String query = "username=" + HttpUtility.UrlEncode(modelsmsClone.Username.Trim()) +
            "&password=" + HttpUtility.UrlEncode(encryptedPassword) +
            "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +
            "&content=" + HttpUtility.UrlEncode(modelsms.Smstext.Trim()) +
            "&mobileno=" + HttpUtility.UrlEncode(modelsms.To) +
            "&senderid=" + HttpUtility.UrlEncode(modelsmsClone.SenderId.Trim()) +
            "&key=" + HttpUtility.UrlEncode(NewsecureKey.Trim()) +
            "&templateid=" + HttpUtility.UrlEncode(modelsms.Smstemplete.Trim());
            byte[] byteArray = Encoding.ASCII.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            String Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            String responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();


            return responseFromServer;
        }
        public static String encryptedPasswod(String password)
        {
            byte[] encPwd = Encoding.UTF8.GetBytes(password);
            //static byte[] pwd = new byte[encPwd.Length];
            HashAlgorithm sha1 = HashAlgorithm.Create("SHA1");
            byte[] pp = sha1.ComputeHash(encPwd);
            // static string result =
            System.Text.Encoding.UTF8.GetString(pp);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in pp)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// Method to Generate hash code
        /// </summary>
        /// <param name= "secure_key">your last generated Secure_key </param>
        public static String hashGenerator(String Username, String sender_id,
        String message, String secure_key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Username).Append(sender_id).Append(message).Append(secure_key);
            byte[] genkey = Encoding.UTF8.GetBytes(sb.ToString());
            //static byte[] pwd = new byte[encPwd.Length];
            HashAlgorithm sha1 = HashAlgorithm.Create("SHA512");
            byte[] sec_key = sha1.ComputeHash(genkey);
            StringBuilder sb1 = new StringBuilder();
            for (int i = 0; i < sec_key.Length; i++)
            {
                sb1.Append(sec_key[i].ToString("x2"));
            }
            return sb1.ToString();
        }

    }
}