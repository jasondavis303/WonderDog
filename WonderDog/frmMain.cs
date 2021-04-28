using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace WonderDog
{
    public partial class frmMain : Form
    {
        private const string MAGIC_STRING = "Krypto the Wonder Dog";

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Text += $" v{SelfUpdatingApp.Installer.GetInstalledVersion(Program.APP_ID)}";
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
            using var f = new frmConfirmPassword(tbPassword.Text);
            if (f.ShowDialog() != DialogResult.OK)
                return;

            UseWaitCursor = true;
            tlpMain.Enabled = false;

            string tmpFile = tbFilename.Text + ".tmp";

            try
            {
                await new Krypto().EncryptFileAsync(tbFilename.Text, tmpFile, tbPassword.Text, MAGIC_STRING);
                File.Move(tmpFile, tbFilename.Text, true);
                MessageBox.Show("File Encrypted", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions);
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex });
            }
            finally
            {
                if (File.Exists(tmpFile))
                    try { File.Delete(tmpFile); }
                    catch { }
            }

            UseWaitCursor = false;
            tlpMain.Enabled = true;
        }

        private async void btnDecrypt_Click(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            tlpMain.Enabled = false;

            string tmpFile = tbFilename.Text + ".tmp";

            try
            {
                await new Krypto().DecryptFileAsync(tbFilename.Text, tmpFile, tbPassword.Text, MAGIC_STRING);
                File.Move(tmpFile, tbFilename.Text, true);
                MessageBox.Show("File Decrypted", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions);
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex });
            }
            finally
            {
                if (File.Exists(tmpFile))
                    try { File.Delete(tmpFile); }
                    catch { }
            }

            UseWaitCursor = false;
            tlpMain.Enabled = true;
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
            foreach (var ex in exes)
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
