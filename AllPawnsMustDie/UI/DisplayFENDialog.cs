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
    /// Dialog to show the current FEN.  Better than a messagebox, since the
    /// textbox can be selected and copied
    /// </summary>
    public partial class DisplayFENDialog : Form
    {
        /// <summary>
        /// Initialiaze the dialog
        /// </summary>
        /// <param name="fen">FEN string to display</param>
        public DisplayFENDialog(string fen)
        {
            displayFen = fen;
            InitializeComponent();

            textBoxDisplayFEN.Text = displayFen;
        }

        // Copy of the string to display
        private string displayFen;
    }
}
