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
    /// Simple dialog for selecting promotion of a piece.  It is dead simple
    /// right now, with a simple radio list to choose from.
    /// </summary>
    public partial class PromotionDialog : Form
    {
        /// <summary>
        /// Initialize a new PromotionDialog object - creates and sets up dialog
        /// controls, mostly generated code from the wizard.
        /// </summary>
        public PromotionDialog()
        {
            job = PieceClass.Queen; // Default to Queen
            InitializeComponent();
        }

        /// <summary>
        /// Ok button click handler
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            // Get the checked button in the group box (only 1)
            RadioButton checkedButton = RadioButtonHelper.GetCheckedRadio(groupBoxPromotion);

            if (checkedButton == radioButtonQueen)
            {
                job = PieceClass.Queen;
            }
            else if (checkedButton == radioButtonRook)
            {
                job = PieceClass.Rook;
            }
            else if (checkedButton == radioButtonBishop)
            {
                job = PieceClass.Bishop;
            }
            else if (checkedButton == radioButtonKnight)
            {
                job = PieceClass.Knight;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            // Close the form, selection has been made
            Close();
        }

        /// <summary>
        /// PieceClass selected when the dialog closed
        /// </summary>
        public PieceClass PromotionJob { get { return job; } }

        private PieceClass job; // private copy of the selection
    }
}
