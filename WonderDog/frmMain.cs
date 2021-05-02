using Krypto.WonderDog;
using Krypto.WonderDog.Symmetric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            pbProgress.Value = 0;
            pbProgress.Visible = true;

            string tmpFile = tbFilename.Text + ".tmp";

            try
            {
                var mres = new ManualResetEventSlim();
                var key = new Key(tbPassword.Text, new byte[8]);
                var alg = SymmetricFactory.CreateAES();
                IProgress<KryptoProgress> prog = new Progress<KryptoProgress>(p =>
                {
                    pbProgress.Value = (int)Math.Floor(p.Percent * 100);
                    if (p.Done)
                        mres.Set();
                });
                await alg.EncryptFileAsync(key, tbFilename.Text, tmpFile, prog);                
                File.Move(tmpFile, tbFilename.Text, true);
                await Task.Run(() => mres.Wait());
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

            pbProgress.Visible = false;
            UseWaitCursor = false;
            tlpMain.Enabled = true;
        }

        private async void btnDecrypt_Click(object sender, EventArgs e)
        {
            using var f = new frmConfirmPassword(tbPassword.Text);
            if (f.ShowDialog() != DialogResult.OK)
                return;

            UseWaitCursor = true;
            tlpMain.Enabled = false;
            pbProgress.Value = 0;
            pbProgress.Visible = true;

            string tmpFile = tbFilename.Text + ".tmp";

            try
            {
                var mres = new ManualResetEventSlim();
                var key = new Key(tbPassword.Text, new byte[8]);
                var alg = SymmetricFactory.CreateAES();
                IProgress<KryptoProgress> prog = new Progress<KryptoProgress>(p =>
                {
                    pbProgress.Value = (int)Math.Floor(p.Percent * 100);
                    if (p.Done)
                        mres.Set();
                });
                await alg.DecryptFileAsync(key, tbFilename.Text, tmpFile, prog);
                File.Move(tmpFile, tbFilename.Text, true);
                await Task.Run(() => mres.Wait());
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

            pbProgress.Visible = false;
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
