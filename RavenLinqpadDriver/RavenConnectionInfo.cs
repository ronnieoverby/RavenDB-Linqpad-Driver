using System;
using System.Net;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using Raven.Client.Document;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace RavenLinqpadDriver
{
    public class RavenConnectionInfo : ViewModelBase
    {
        public const string RavenConnectionInfoKey = "RavenConnectionInfo";

        [JsonIgnore]
        public IConnectionInfo CxInfo { get; set; }

        [JsonIgnore]
        public RelayCommand SaveCommand { get; set; }

        public const string UrlPropertyName = "Url";
        private string _url = "http://localhost:8080";
        [Required]
        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                if (_url == value)
                {
                    return;
                }

                _url = value;

                RaisePropertyChanged(UrlPropertyName);
            }
        }

        public const string DefaultDatabasePropertyName = "DefaultDatabase";
        private string _defaultDatabase = null;
        public string DefaultDatabase
        {
            get
            {
                return _defaultDatabase;
            }

            set
            {
                if (_defaultDatabase == value)
                {
                    return;
                }
                _defaultDatabase = value;

                RaisePropertyChanged(DefaultDatabasePropertyName);
            }
        }

        public const string ResourceManagerIdPropertyName = "ResourceManagerId";
        private Guid? _resourceManagerId = null;
        public Guid? ResourceManagerId
        {
            get
            {
                return _resourceManagerId;
            }

            set
            {
                if (_resourceManagerId == value)
                {
                    return;
                }

                _resourceManagerId = value;

                RaisePropertyChanged(ResourceManagerIdPropertyName);
            }
        }

        public const string UsernamePropertyName = "Username";
        private string _username = null;
        public string Username
        {
            get
            {
                return _username;
            }

            set
            {
                if (_username == value)
                {
                    return;
                }

                _username = value;

                RaisePropertyChanged(UsernamePropertyName);
            }
        }

        public const string PasswordPropertyName = "Password";
        private string _password = null;
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                if (_password == value)
                    return;

                _password = value;

                RaisePropertyChanged(PasswordPropertyName);
            }
        }

        public RavenConnectionInfo()
        {
            SaveCommand = new RelayCommand(Save, CanSave);
        }

        public void Save()
        {
            string pw = Password;
            if (!Password.IsNullOrWhitespace())
                Password = CxInfo.Encrypt(Password);

            var json = JsonConvert.SerializeObject(this);
            CxInfo.DriverData.SetElementValue(RavenConnectionInfoKey, json);

            Password = pw;
        }

        public bool CanSave()
        {
            return !Url.IsNullOrWhitespace();
        }

        public static RavenConnectionInfo Load(IConnectionInfo cxInfo)
        {
            XElement xe = cxInfo.DriverData.Element(RavenConnectionInfoKey);
            if (xe != null)
            {
                var json = xe.Value;
                var rvnConn = JsonConvert.DeserializeObject<RavenConnectionInfo>(json);
                rvnConn.CxInfo = cxInfo;

                if (!rvnConn.Password.IsNullOrWhitespace())
                    rvnConn.Password = cxInfo.Decrypt(rvnConn.Password);

                return rvnConn;
            }

            return null; 
        }

        public DocumentStore CreateDocStore()
        {
            try
            {
                var docStore = new DocumentStore
                {
                    Url = Url
                };

                if (!DefaultDatabase.IsNullOrWhitespace())
                    docStore.DefaultDatabase = DefaultDatabase.Trim();

                if (ResourceManagerId.HasValue)
                    docStore.ResourceManagerId = ResourceManagerId.Value;

                if (!Username.IsNullOrWhitespace())
                    docStore.Credentials = new NetworkCredential(Username, Password);

                return docStore;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create DocumentStore", ex);
            }
        }

    }
}
