using System;
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
    /// Options Dialog for the chess engines loaded
    /// </summary>
    public partial class EngineOptionsDialog : Form
    {
        /// <summary>
        /// Create the options dialog
        /// </summary>
        public EngineOptionsDialog(bool reduceStrengthChecked)
        {
            InitializeComponent();
            checkBoxReduceEngineStrength.Checked = reduceStrengthChecked;
        }

        /// <summary>
        /// Returns option for strength reduction
        /// </summary>
        public bool ReduceEngineStrength
        {
            get
            {
                return checkBoxReduceEngineStrength.Checked;
            }
        }

        /// <summary>
        /// Button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
