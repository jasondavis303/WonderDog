using Krypto.WonderDog;
using Krypto.WonderDog.Symmetric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WonderDog
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Text += $" v{Application.ProductVersion}";
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
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

        private async void btnEncrypt_Click(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            tlpMain.Enabled = false;
            pbProgress.Value = 0;
            
            string tmpFile = tbFilename.Text + ".tmp";

            try
            {
                using var mre = new ManualResetEvent(false);
                var key = new Key(tbPassword.Text);
                var alg = SymmetricFactory.CreateAES();
                IProgress<KryptoProgress> prog = new Progress<KryptoProgress>(p =>
                {
                    pbProgress.Value = (int)Math.Floor(p.Percent * 100);
                    if (p.Done)
                        mre.Set();
                });
                await alg.EncryptFileAsync(key, tbFilename.Text, tmpFile, prog);                
                File.Move(tmpFile, tbFilename.Text, true);
                await mre.WaitOneAsync();
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
            pbProgress.Value = 0;
            
            string tmpFile = tbFilename.Text + ".tmp";

            try
            {
                using var mre = new ManualResetEvent(false);
                var key = new Key(tbPassword.Text);
                var alg = SymmetricFactory.CreateAES();
                IProgress<KryptoProgress> prog = new Progress<KryptoProgress>(p =>
                {
                    pbProgress.Value = (int)Math.Floor(p.Percent * 100);
                    if (p.Done)
                        mre.Set();
                });
                await alg.DecryptFileAsync(key, tbFilename.Text, tmpFile, prog);
                File.Move(tmpFile, tbFilename.Text, true);
                await mre.WaitOneAsync();
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

            if (tbPassword.Text.Length < 8)
                return false;

            if (tbPassword.Text != tbConfirm.Text)
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
