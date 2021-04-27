using System;
using System.Windows.Forms;

namespace WonderDog
{
    public partial class frmConfirmPassword : Form
    {
        private string _password;

        public frmConfirmPassword(string password)
        {
            InitializeComponent();
            _password = password;
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            btnEncrypt.Enabled = tbPassword.Text == _password;
        }
    }
}
