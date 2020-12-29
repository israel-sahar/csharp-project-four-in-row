using Client.CFGameServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public ClientCallback Callback { set; get; }
        public FourRowServiceClient Client { set; get; }

        public LoginWindow()
        {
            InitializeComponent();
            Callback = new ClientCallback();
            Client = new FourRowServiceClient(new InstanceContext(Callback));
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ConnectServer(false);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            ConnectServer(true);
        }

        private void ConnectServer(bool register)
        {
            if (AnyFieldEmpty())
            {
                MessageBox.Show("Fill username and password please.");
                return;
            }
            string username = tbUsername.Text.Trim();
            string hashedPassword = HashValue(tbPassword.Password.Trim());

            try
            {
                Client.Connect(username, hashedPassword, register);

                //go to main menu
                MainWindow mainWindow = new MainWindow(username, hashedPassword, Client, Callback);
                this.Close();
                mainWindow.Show();

            }
            catch (FaultException<UserExistsFault> msg)
            {
                MessageBox.Show(msg.Detail.Message);
            }
            catch (FaultException<UserNotExistsFault> msg)
            {
                MessageBox.Show(msg.Detail.Message);
            }
            catch (FaultException<WrongPasswordFault> msg)
            {
                MessageBox.Show(msg.Detail.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool AnyFieldEmpty()
        {
            return String.IsNullOrEmpty(tbUsername.Text.Trim()) || String.IsNullOrEmpty(tbPassword.Password);
        }

        private string HashValue(string password)
        {
            using (SHA256 hashObject = SHA256.Create())
            {
                byte[] hashBytes = hashObject.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
