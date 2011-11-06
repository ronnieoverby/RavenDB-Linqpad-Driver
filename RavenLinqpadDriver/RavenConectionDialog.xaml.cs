using System;
using System.Windows;

namespace RavenLinqpadDriver
{
    /// <remarks>
    /// A view model is not needed for this simple window.
    /// </remarks>
    public partial class RavenConectionDialog : Window
    {
        private RavenConnectionInfo _conn;

        public RavenConectionDialog(RavenConnectionInfo conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");

            InitializeComponent();

            _conn = conn;

            txtName.Text = _conn.Name;
            txtUrl.Text = _conn.Url;
            txtDatabase.Text = _conn.DefaultDatabase;
            txtUsername.Text = _conn.Username;
            txtPassword.Password = _conn.Password;
            if (_conn.ResourceManagerId.HasValue)
                txtResourceManagerId.Text = _conn.ResourceManagerId.Value.ToString();

        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            _conn.Name = txtName.Text;
            _conn.Url = txtUrl.Text;
            _conn.DefaultDatabase = txtDatabase.Text;            
            if (!string.IsNullOrEmpty(txtResourceManagerId.Text))
                _conn.ResourceManagerId = new Guid(txtResourceManagerId.Text);
            _conn.Username = txtUsername.Text;
            _conn.Password = txtPassword.Password;          

            DialogResult = true;
        }

        private bool ValidateInput()
        {
            // trim all text (except password)
            txtName.Text = txtName.Text.Trim();
            txtDatabase.Text = txtDatabase.Text.Trim();
            txtUrl.Text = txtUrl.Text.Trim();
            txtUsername.Text = txtUsername.Text.Trim();
            txtResourceManagerId.Text = txtResourceManagerId.Text.Trim();

            // validate name
            if (string.IsNullOrEmpty(txtName.Name))
            {
                MessageBox.Show("Name is required.");
                return false;
            }


            // validate url
            if (string.IsNullOrEmpty(txtUrl.Text))
            {
                MessageBox.Show("URL is required.");
                return false;
            }
            
            // validate resource manager input
            try
            {
                var id = txtResourceManagerId.Text;
                if (!string.IsNullOrEmpty(id)) new Guid(id);
            }
            catch (FormatException)
            {
                MessageBox.Show("Resource Manager Id must be a GUID.");
                return false;
            }

            // validate credentials
            if ((!string.IsNullOrEmpty(txtUsername.Text) && string.IsNullOrEmpty(txtPassword.Password)) ||
                (string.IsNullOrEmpty(txtUsername.Text) && !string.IsNullOrEmpty(txtPassword.Password)))
            {
                MessageBox.Show("When supplying credentials, you must specify both a username and password.");
                return false;
            }

            return true;
        }
    }
}
