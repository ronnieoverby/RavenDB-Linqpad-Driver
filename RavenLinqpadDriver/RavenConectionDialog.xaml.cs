using System;
using System.Windows;

namespace RavenLinqpadDriver
{
    public partial class RavenConectionDialog : Window
    {
        private RavenConnectionDialogViewModel _connInfo;

        public RavenConectionDialog(RavenConnectionDialogViewModel conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");

            InitializeComponent();

            DataContext = _connInfo = conn;
            txtPassword.Password = conn.Password;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _connInfo.Password = txtPassword.Password;
        }
    }
}
