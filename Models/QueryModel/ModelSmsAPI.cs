

using CallCenterCoreAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace CallCenterCoreAPI.Models
{
    public class ModelSmsAPI 
    {
        private string _smsApiURL = AppSettingsHelper.Setting(Key: "SMSAPI:smsApiURL");
        private string _username = AppSettingsHelper.Setting(Key: "SMSAPI:username");
        private string _password = AppSettingsHelper.Setting(Key: "SMSAPI:password");
        private string _senderid = AppSettingsHelper.Setting(Key: "SMSAPI:senderid");
        private string _secureKey = AppSettingsHelper.Setting(Key: "SMSAPI:secureKey");

        private string _to;
        private string _smstext;
        private string _smstemplete;
        public string SmsApiURL { get { return _smsApiURL; } }

        public string Username {get {return _username; }}
        public string Password { get { return _password; } }
        public string SenderId { get { return _senderid; } }

        public string SecureKey { get { return _secureKey; } }
        public string To { get { return _to;  }  set { _to = value;   }  }
        public string Smstext { get { return _smstext; } set { _smstext = value; } }
        public string Smstemplete { get { return _smstemplete; } set { _smstemplete = value; } }

    }
}
