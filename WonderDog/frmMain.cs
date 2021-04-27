using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace WonderDog
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void tbFilename_TextChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void btnFilename_Click(object sender, EventArgs e)
        {
            if (ofdFilename.ShowDialog() != DialogResult.OK)
                return;
            tbFilename.Text = ofdFilename.FileName;
            EnableButtons();
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private async void btnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                await Krypto.EncryptFileAsync(tbFilename.Text, tbPassword.Text);
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions);
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex });
            }
        }

        private async void btnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                await Krypto.DecryptFileAsync(tbFilename.Text, tbPassword.Text);
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions);
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex });
            }
        }

        private void EnableButtons()
        {
            bool e = ShouldEnableButtons();
            btnEncrypt.Enabled = e;
            btnDecrypt.Enabled = e;
        }

        private bool ShouldEnableButtons()
        {
            if (string.IsNullOrWhiteSpace(tbFilename.Text))
                return false;

            if (!File.Exists(tbFilename.Text))
                return false;

            if (string.IsNullOrWhiteSpace(tbPassword.Text))
                return false;

            return true;
        }

        private static void ShowErrors(IEnumerable<Exception> exes)
        {
            foreach(var ex in exes)
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
