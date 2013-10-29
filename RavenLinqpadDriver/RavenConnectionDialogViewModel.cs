using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using Microsoft.Win32;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace RavenLinqpadDriver
{
    public class RavenConnectionDialogViewModel : ViewModelBase
    {
        public const string RavenConnectionInfoKey = "RavenConnectionInfo";

        public const string NamePropertyName = "Name";

        public const string UrlPropertyName = "Url";

        public const string DefaultDatabasePropertyName = "DefaultDatabase";

        public const string ApiKeyPropertyName = "ApiKey";

        public const string ResourceManagerIdPropertyName = "ResourceManagerId";

        public const string UsernamePropertyName = "Username";

        public const string PasswordPropertyName = "Password";
        public const string NamespacesPropertyName = "Namespaces";
        private string _apiKey;
        private string _defaultDatabase;
        private string _name;
        private string _namespaces;
        private string _password;
        private Guid? _resourceManagerId;
        private string _url = "http://localhost:8080";
        private string _username;
        private string _selectedAssemblyPath;

        public RavenConnectionDialogViewModel()
        {
            AssemblyPaths = new BindingList<string>();
            SaveCommand = new RelayCommand(Save, CanSave);
            BrowseAssembliesCommand = new RelayCommand(BrowseAssemblies);
            RemoveAssemblyCommand = new RelayCommand(RemoveAssembly,CanRemoveAssembly);
        }

        private bool CanRemoveAssembly()
        {
            return !string.IsNullOrWhiteSpace(SelectedAssemblyPath);
        }

        [JsonIgnore]
        public IConnectionInfo CxInfo { get; set; }

        [JsonIgnore]
        public RelayCommand SaveCommand { get; set; }

        [Required]
        public string Name
        {
            get { return _name; }

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;

                OnPropertyChanged(NamePropertyName);
                SaveCommand.OnCanExecuteChanged();
            }
        }

        [Required]
        public string Url
        {
            get { return _url; }

            set
            {
                if (_url == value)
                {
                    return;
                }

                _url = value;

                OnPropertyChanged(UrlPropertyName);
                SaveCommand.OnCanExecuteChanged();
            }
        }

        public string DefaultDatabase
        {
            get { return _defaultDatabase; }

            set
            {
                if (_defaultDatabase == value)
                {
                    return;
                }
                _defaultDatabase = value;

                OnPropertyChanged(DefaultDatabasePropertyName);
            }
        }

        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey == value)
                {
                    return;
                }
                _apiKey = value;

                OnPropertyChanged(ApiKeyPropertyName);
            }
        }

        public Guid? ResourceManagerId
        {
            get { return _resourceManagerId; }

            set
            {
                if (_resourceManagerId == value)
                {
                    return;
                }

                _resourceManagerId = value;

                OnPropertyChanged(ResourceManagerIdPropertyName);
            }
        }

        public string Username
        {
            get { return _username; }

            set
            {
                if (_username == value)
                {
                    return;
                }

                _username = value;

                OnPropertyChanged(UsernamePropertyName);
            }
        }

        public string Password
        {
            get { return _password; }

            set
            {
                if (_password == value)
                    return;

                _password = value;

                OnPropertyChanged(PasswordPropertyName);
            }
        }

        public BindingList<string> AssemblyPaths { get; set; }

        public string Namespaces
        {
            get { return _namespaces; }

            set
            {
                if (_namespaces == value)
                {
                    return;
                }

                _namespaces = value;

                OnPropertyChanged(NamespacesPropertyName);
            }
        }

        [JsonIgnore]
        public RelayCommand BrowseAssembliesCommand { get; set; }

        [JsonIgnore]
        public RelayCommand RemoveAssemblyCommand { get; set; }

        public void Save()
        {
            ValidateAssemblies();

            var pw = Password;
            if (!string.IsNullOrWhiteSpace(Password))
                Password = CxInfo.Encrypt(Password);

            var json = JsonConvert.SerializeObject(this);
            CxInfo.DriverData.SetElementValue(RavenConnectionInfoKey, json);

            Password = pw;
        }

        public bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name)
                   && !string.IsNullOrWhiteSpace(Url);
        }

        public bool ValidateAssemblies()
        {
            foreach (var path in AssemblyPaths)
            {
                if (!File.Exists(path))
                    return false;

                try
                {
                    if (Assembly.LoadFile(path) == null)
                        throw new Exception();
                }
                catch (Exception ex)
                {
                    // mvvm.... screw it
                    MessageBox.Show(string.Format(
                        "Could not load assemly:{0}{1}{0}{0}Reason:{0}{2}",
                        Environment.NewLine,
                        path,
                        ex.Message));

                    return false;
                }
            }

            return true;
        }

        public IEnumerable<string> GetNamespaces()
        {
            return (Namespaces ?? "")
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());
        }

        public static RavenConnectionDialogViewModel Load(IConnectionInfo cxInfo)
        {
            var xe = cxInfo.DriverData.Element(RavenConnectionInfoKey);
            if (xe == null) return null;
            var json = xe.Value;
            var rvnConn = JsonConvert.DeserializeObject<RavenConnectionDialogViewModel>(json);
            rvnConn.CxInfo = cxInfo;

            if (!string.IsNullOrWhiteSpace(rvnConn.Password))
                rvnConn.Password = cxInfo.Decrypt(rvnConn.Password);

            return rvnConn;
        }

        public DocumentStore CreateDocStore()
        {
            try
            {
                var docStore = new DocumentStore
                {
                    Url = Url
                };

                if (!string.IsNullOrWhiteSpace(DefaultDatabase))
                    docStore.DefaultDatabase = DefaultDatabase.Trim();

                if (ResourceManagerId.HasValue)
                    docStore.ResourceManagerId = ResourceManagerId.Value;

                if (!string.IsNullOrWhiteSpace(Username))
                    docStore.Credentials = new NetworkCredential(Username, Password);

                if (!string.IsNullOrWhiteSpace(ApiKey))
                    docStore.ApiKey = ApiKey;

                return docStore;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create DocumentStore", ex);
            }
        }

        public void BrowseAssemblies()
        {
            var win = new OpenFileDialog
            {
                DefaultExt = ".dll",
                Multiselect = true,
            };

            if (win.ShowDialog() != true)
                return;

            var newPaths = win.FileNames.Except(AssemblyPaths, StringComparer.OrdinalIgnoreCase).ToArray();
            foreach (var fileName in newPaths)
                AssemblyPaths.Add(fileName);
        }

        public void RemoveAssembly()
        {
            AssemblyPaths.Remove(SelectedAssemblyPath);
        }

        public string SelectedAssemblyPath
        {
            get { return _selectedAssemblyPath; }
            set
            {
                if (value == _selectedAssemblyPath) return;
                _selectedAssemblyPath = value;
                OnPropertyChanged("SelectedAssemblyPath");
                RemoveAssemblyCommand.OnCanExecuteChanged();
            }
        }
    }
}