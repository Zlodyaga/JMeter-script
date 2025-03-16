using JMeter_script_program.UIClasses;
using System.Text.RegularExpressions;

namespace JMeter_script_program
{
        public partial class MainForm : Form
        {
        private CheckBox checkBoxRemoveUserInput;
        private Button btnRestoreURL;
        private Button btnSelectFile;
        private Button btnProcessFile;
        private TextBox txtFilePath;
        private TextBox txtUserInput;
        private ProgressBar progressBar;
        private Label lblProgress;
        private OpenFileDialog openFileDialog;
        private UIController uiController;

        public MainForm()
        {
            // Settings of form
            Text = "JMX script";
            Size = new System.Drawing.Size(500, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Input field for showing file path
            txtFilePath = new TextBox { Left = 20, Top = 20, Width = 340, PlaceholderText = "Enter path to JMX file" };
            Controls.Add(txtFilePath);

            // Button of choosing JMX file
            btnSelectFile = new Button { Text = "Choose file", Left = 375, Top = 18, Width = 100 };
            btnSelectFile.Click += BtnSelectFile_Click;
            Controls.Add(btnSelectFile);

            // Choosing file dialogue
            openFileDialog = new OpenFileDialog
            {
                Filter = "JMX files (*.jmx)|*.jmx|All files (*.*)|*.*",
                Title = "Choose JMX file"
            };

            // Input field for baseURL
            txtUserInput = new TextBox { Left = 20, Top = 60, Width = 340, PlaceholderText = "Enter base URL (e.g., google.com)" };
            Controls.Add(txtUserInput);

            // CheckBox for deleting baseURL
            checkBoxRemoveUserInput = new CheckBox
            {
                Left = 20,
                Top = 90,
                Width = 340,
                Text = "Remove base URL from test name"
            };
            Controls.Add(checkBoxRemoveUserInput);

            // Button of start script
            btnProcessFile = new Button { Text = "Start script", Left = 370, Top = 220, Width = 100, Enabled = true };
            btnProcessFile.Click += async (sender, e) => await ProcessFileAsync();
            Controls.Add(btnProcessFile);

            // Restore baseURL button
            btnRestoreURL = new Button { Text = "Restore base URL", Left = 250, Top = 220, Width = 100, Enabled = true };
            btnRestoreURL.Click += async (sender, e) => await RestoreFileAsync();
            Controls.Add(btnRestoreURL);

            // Progress bar of progress
            progressBar = new ProgressBar { Left = 20, Top = 160, Width = 450, Height = 20, Minimum = 0, Maximum = 100 };
            Controls.Add(progressBar);

            // Text view of progress
            lblProgress = new Label { Left = 20, Top = 190, Width = 450, Text = "Progress: 0%" };
            Controls.Add(lblProgress);

            List<Button> necessaryButtons = new List<Button> { btnSelectFile, btnProcessFile, btnRestoreURL };
            uiController = new UIController(necessaryButtons, txtFilePath, txtUserInput, progressBar, lblProgress);
        }

        /* 
         * Actions
         */

        // Choose file action
        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        // Start script action
        private async Task ProcessFileAsync()
        {
            if (!uiController.isFilePathTextBoxPopulated() || !uiController.isInputPathTextBoxPopulated())
            {
                return;
            }

            string userInput = txtUserInput.Text.Trim();
            bool removeUserInput = checkBoxRemoveUserInput.Checked;

            await ProcessFileWithRegexAsync(@"(<HTTPSamplerProxy.*?>.*?</HTTPSamplerProxy>)", match =>
            {
                string block = match.Value;
                Match methodMatch = getTypeRequestName(block);

                if (methodMatch.Success)
                {
                    string method = methodMatch.Groups[1].Value;
                    string testnamePattern = @"testname=""([^""]+)""";

                    block = Regex.Replace(block, testnamePattern, match =>
                    {
                        string testName = match.Groups[1].Value;

                        // Check that testName starts from userInput
                        if (!string.IsNullOrEmpty(userInput) && testName.StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
                        {
                            if (removeUserInput)
                            {
                                testName = testName.Substring(userInput.Length).Trim();
                            }
                            return $"testname=\"[{method}] {testName}\"";
                        }

                        return match.Value;
                    }, RegexOptions.IgnoreCase);
                }

                return block;
            }, "Script successfully done!");
        }

        // Restore baseURL action
        private async Task RestoreFileAsync()
        {
            if (!uiController.isFilePathTextBoxPopulated())
            {
                return;
            }
            string userUrl = txtUserInput.Text.Trim();

            await ProcessFileWithRegexAsync(@"testname=""\[(GET|POST|HEAD|PUT|OPTIONS|TRACE|DELETE|PATCH|PROPFIND|PROPPATCH|MKCOL|COPY|MOVE|LOCK|UNLOCK|REPORT|MKCALENDAR|SEARCH)\] (.*?)""",
            match =>
            {
                string method = match.Groups[1].Value;
                string endpoint = match.Groups[2].Value;
                if(string.IsNullOrEmpty(userUrl))
                    return $"testname=\"{endpoint}\"";
                else
                    return $"testname=\"{userUrl}{endpoint}\"";
            }, "Base URL successfully restored!");
        }

        /* 
         * Private methods for actions
         */

        // Find HTTP type of request (GET, POST and etc.)
        private Match getTypeRequestName(string block)
        {
            string methodPattern = @"<stringProp name=""HTTPSampler.method"">(.*?)</stringProp>";
            return Regex.Match(block, methodPattern);
        }

        // UI show progress
        private void showPercentageOfWork(ref int processedLines, int totalLines)
        {
            Interlocked.Increment(ref processedLines);
            int progress = (int)((double)processedLines / totalLines * 100);
            Invoke(new Action(() =>
            {
                progressBar.Value = progress;
                lblProgress.Text = $"Progress: {progress}%";
            }));
        }
        
        // Regex private function
        private async Task ProcessFileWithRegexAsync(string pattern, Func<Match, string> processMatch, string successMessage)
        {
            uiController.showStartOfWorkUI();

            try
            {
                string[] lines = await File.ReadAllLinesAsync(txtFilePath.Text);
                int totalLines = lines.Length;
                int processedLines = 0;

                string fileContent = string.Join("\n", lines);
                fileContent = await Task.Run(() =>
                {
                    return Regex.Replace(fileContent, pattern, match =>
                    {
                        string modifiedBlock = processMatch(match);
                        showPercentageOfWork(ref processedLines, totalLines);
                        return modifiedBlock;
                    }, RegexOptions.Singleline);
                });

                await File.WriteAllTextAsync(txtFilePath.Text, fileContent);
                MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                uiController.showEndOfWorkUI();
            }
        }
    }
}
