using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMeter_script_program.UIClasses
{
    internal class UIController
    {
        public List<Button> necessaryButtons;
        public TextBox filePathTextBox;
        public TextBox userInputTextBox;
        public ProgressBar progressBar;
        public Label lblProgress;

        public UIController(List<Button> necessaryButtons, TextBox filePathTextBox, TextBox userInputTextBox, ProgressBar progressBar, Label lblProgress)
        {
            this.necessaryButtons = necessaryButtons;
            this.filePathTextBox = filePathTextBox;
            this.userInputTextBox = userInputTextBox;
            this.progressBar = progressBar;
            this.lblProgress = lblProgress;
        }

        public void showStartOfWorkUI()
        {
            necessaryButtons.ForEach(button => {
                button.Enabled = false;
            });
            progressBar.Value = 0;
            lblProgress.Text = "Progress: 0%";
        }

        public void showEndOfWorkUI()
        {
            necessaryButtons.ForEach(button => {
                button.Enabled = true;
            });
            progressBar.Value = 100;
            lblProgress.Text = "Progress: 100%";
        }

        public bool isFilePathTextBoxPopulated()
        {
            if (string.IsNullOrEmpty(filePathTextBox.Text)) {
                MessageBox.Show("Please populate path to JMX file", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else return true;
        }

        public bool isInputPathTextBoxPopulated()
        {
            if (string.IsNullOrEmpty(userInputTextBox.Text))
            {
                MessageBox.Show("Please populate base URL field", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else return true;
        }
    }
}
