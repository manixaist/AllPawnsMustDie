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
    /// Wraps reused helper code to get a radio selection.  Shared with the simple
    /// dialogs.
    /// </summary>
    class RadioButtonHelper
    {
        /// <summary>
        /// Helper to find the checked radio button
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static RadioButton GetCheckedRadio(Control container)
        {
            foreach (var control in container.Controls)
            {
                RadioButton radio = control as RadioButton;
                if ((radio != null) && (radio.Checked == true))
                {
                    return radio;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Simple dialog to allow the user to select Black or White as the starting color
    /// </summary>
    public partial class ColorSelectDialog : Form
    {
        /// <summary>
        /// Initialize the dialog controls
        /// </summary>
        public ColorSelectDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ok button handler
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            // Get the checked button in the group box (only 1)
            RadioButton checkedButton = RadioButtonHelper.GetCheckedRadio(groupBoxSelectColor);

            if (checkedButton == radioButtonWhite)
            {
                playerColor = PieceColor.White;
            }
            else if (checkedButton == radioButtonBlack)
            {
                playerColor = PieceColor.Black;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            // Close the form, selection has been made
            Close();
        }

        /// <summary>
        /// Returns the selected player color
        /// </summary>
        public PieceColor PlayerColor { get { return playerColor; } }

        private PieceColor playerColor;
    }
}
