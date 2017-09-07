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
    public partial class FenInputDialog : Form
    {
        /// <summary>
        /// Simple dialog to get a single string.  This will be used as the FEN
        /// string to set a new position with the chess engine.
        /// </summary>
        public FenInputDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cancel the dialog.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Set the result and close
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Confirm the dialog.  Reads the input string and saves it for later
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            fen = textBoxFenInput.Text;
            Close();
        }

        private string fen;

        /// <summary>
        /// Property for the returned FEN string
        /// </summary>
        public string FEN
        {
            get { return fen; }
        }
    }
}
