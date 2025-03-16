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
        public List<TextBox> necessaryTextBoxes;
        public ProgressBar progressBar;
        public Label lblProgress;

        public UIController(List<Button> necessaryButtons, List<TextBox> necessaryTextBoxes, ProgressBar progressBar, Label lblProgress)
        {
            this.necessaryButtons = necessaryButtons;
            this.necessaryTextBoxes = necessaryTextBoxes;
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

        public bool isNecessaryFieldsPopulated()
        {
            if (necessaryTextBoxes.Any(textBox => string.IsNullOrEmpty(textBox.Text))) {
                MessageBox.Show("Please populate all necessary fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
