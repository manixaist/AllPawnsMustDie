using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllPawnsMustDie
{
    /// <summary>
    /// Simple about dialog
    /// </summary>
    public partial class AboutDialog : Form
    {
        /// <summary>
        /// About dialog form
        /// </summary>
        public AboutDialog()
        {
            InitializeComponent();

            labelBuildVersion.Text = String.Format("{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        /// <summary>
        /// Launches the browser link using the shell, so it picks the user's browser, etc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process browserProcess = new Process();
            browserProcess.StartInfo.UseShellExecute = true;
            browserProcess.StartInfo.FileName = linkLabelGitHub.Text;
            browserProcess.Start();
        }
    }
}
