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
    /// New game dialog class.  This is the Form UI
    /// </summary>
    public partial class NewGameDialog : Form
    {
        /// <summary>
        /// Holds info needed for a new game
        /// </summary>
        public struct NewGameInfo
        {
            /// <summary>
            /// Create a record of new game settings
            /// </summary>
            /// <param name="color">Player color</param>
            /// <param name="engineThinkTimeInMs">Ms to allow the engine to come
            /// up with a best move</param>
            public NewGameInfo(PieceColor color, int engineThinkTimeInMs)
            {
                playerColor = color;
                thinkTime = engineThinkTimeInMs;
                fen = null;
            }

            /// <summary>Think time in ms for the engine</summary>
            public int ThinkTime { get { return thinkTime; } }

            /// <summary>Player color (e.g. white or black)</summary>
            public PieceColor PlayerColor { get { return playerColor; } }

            /// <summary>FEN positional info if set, or null for normal play</summary>
            public string FEN { get { return fen; } set { fen = value; } }

            private string fen;
            private int thinkTime;
            private PieceColor playerColor;
        }

        /// <summary>
        /// Type of new game
        /// </summary>
        public enum NewGameType
        {
            /// <summary>Normal starting board position</summary>
            Normal,
            /// <summary>Positional start based on FEN</summary>
            PositionalFEN,
            /// <summary>Engine self play</summary>
            SelfPlay
        };
        
        /// <summary>
        /// Initialize the form controls, mostly to all wizard code
        /// </summary>
        public NewGameDialog(NewGameType newGameType)
        {
            dialogType = newGameType;
            InitializeComponent();

            SuspendLayout();
            if (dialogType == NewGameType.Normal)
            {
                RemoveControlAndShrinkForm(labelFEN);
                RemoveControlAndShrinkForm(textBoxFEN);
            }
            else if (dialogType == NewGameType.SelfPlay)
            {
                RemoveControlAndShrinkForm(labelFEN);
                RemoveControlAndShrinkForm(textBoxFEN);
                // Changing the visibility here causes layout problems, even when
                // suspending the control layout as well, so just disable it
                // the choice is meaningless for self play and it ignored by
                // calling code
                groupBoxPlayerColor.Enabled = false;
            }
            ResumeLayout();
        }

        /// <summary>
        /// OK button handler
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            // Capture the elements needed for the game
            // 1] Player Color
            PieceColor playerColor = PieceColor.White;
            RadioButton playerButton = RadioButtonHelper.GetCheckedRadio(groupBoxPlayerColor);
            // There are only 2 options, so change if it's black
            if (playerButton == radioButtonBlack)
            {
                playerColor = PieceColor.Black;
            }

            // 2] Think time for chess engine
            int engineThinkTime = (int)numericUpDownThinkTime.Value;

            newGameInfo = new NewGameInfo(playerColor, engineThinkTime);

            // 3] Optional FEN
            if (dialogType == NewGameType.PositionalFEN)
            {
                newGameInfo.FEN = textBoxFEN.Text;
            }
            Close();
        }

        /// <summary>
        /// Small helper to hide a control and shrink the client height by that amount
        /// </summary>
        /// <param name="control">Control to hide</param>
        private void RemoveControlAndShrinkForm(Control control)
        {
            control.Visible = false;
            Height -= control.Height;

            // Shift the OK/Cancel button up the same amount
            buttonOk.Location = new Point(buttonOk.Location.X, buttonOk.Location.Y - control.Height);
            buttonCancel.Location = new Point(buttonCancel.Location.X, buttonCancel.Location.Y - control.Height);
        }

        /// <summary>
        /// Returns the results of the dialog selections
        /// </summary>
        public NewGameInfo Info { get { return newGameInfo; } }

        /// <summary>Stored new game state</summary>
        private NewGameInfo newGameInfo;

        /// <summary>Defines available controls</summary>
        private NewGameType dialogType;
    }

    /// <summary>
    /// Wraps reused helper code to get a radio selection.  Shared with the simple
    /// dialogs.
    /// </summary>
    class RadioButtonHelper
    {
        /// <summary>
        /// Helper to find the checked radio button
        /// </summary>
        /// <param name="container">Control that holds the radio buttons, 
        /// i.e. a groupbox</param>
        /// <returns>The RadioButton control that is selected.</returns>
        public static RadioButton GetCheckedRadio(Control container)
        {
            // Loop through each control in the container
            foreach (var control in container.Controls)
            {
                // If the control is a RadioButton (it might not be) and if that
                // RadioButton is currently checked, then we're done looking
                RadioButton radio = control as RadioButton;
                if ((radio != null) && (radio.Checked == true))
                {
                    return radio;
                }
            }
            // In this case we're also done looking, but found no checked 
            // RadioButtons in the container control
            return null;
        }
    }
}
