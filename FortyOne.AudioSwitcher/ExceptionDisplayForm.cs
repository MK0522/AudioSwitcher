using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using FortyOne.AudioSwitcher.Helpers;
using FortyOne.AudioSwitcher.Properties;

namespace FortyOne.AudioSwitcher
{
    public sealed partial class ExceptionDisplayForm : Form
    {
        private readonly Exception exception;

        public ExceptionDisplayForm()
        {
            InitializeComponent();
        }

        public ExceptionDisplayForm(string title, Exception ex)
        {
            InitializeComponent();

            exception = ex;
            Text = title;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                var user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            try
            {
                var p = new Ping();
                p.Send(new Uri(Resources.WebServiceURL).Host);

                //using (Cursor = Cursors.WaitCursor)
                using (var client = ConnectionHelper.GetAudioSwitcherProxy())
                {
                    if (client == null)
                        return;

                    var asm = Assembly.GetExecutingAssembly();
                    var attribs = (asm.GetCustomAttributes(typeof (GuidAttribute), true));
                    var guid = (attribs[0] as GuidAttribute).Value;

                    var body = "";
                    body += "Audio Switcher" + Environment.NewLine;
                    body += "Version: " + Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine;
                    body += "Operating System: " + Environment.OSVersion + (IntPtr.Size == 8 ? " 64-bit" : " 32-bit") +
                            Environment.NewLine;
                    body += "Administrator Privileges: " + IsUserAdministrator() + Environment.NewLine;

                    var x = client.SendBugReport(guid, txtErrorDetails.Text, body, exception.ToString());

                    if (x != "")
                    {
                        var bugConfStr = string.Format(
                            "Bug Report Received. Thank you!\r\nBug ID: {0}\r\nBug Url: {1}{0}", x,
                            "https://github.com/xenolightning/AudioSwitcher_v1/issues/");
                        MessageBox.Show(this, bugConfStr);
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(this, "Error Sending bug report. Please try again later");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error Sending bug report. Please try again later");
            }
        }

        private void ExceptionDisplayForm_Load(object sender, EventArgs e)
        {
            if (exception != null)
                txtError.Text = exception.ToString();
        }
    }
}