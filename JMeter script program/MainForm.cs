using JMeter_script_program.UIClasses;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            // Настройка формы
            Text = "JMX script";
            Size = new System.Drawing.Size(500, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Поле для отображения пути к файлу
            txtFilePath = new TextBox { Left = 20, Top = 20, Width = 340, PlaceholderText = "Enter path to JMX file" };
            Controls.Add(txtFilePath);

            // Кнопка выбора файла
            btnSelectFile = new Button { Text = "Choose file", Left = 375, Top = 18, Width = 100 };
            btnSelectFile.Click += BtnSelectFile_Click;
            Controls.Add(btnSelectFile);

            // Поле для ввода текста (например, "google.com")
            txtUserInput = new TextBox { Left = 20, Top = 60, Width = 340, PlaceholderText = "Enter base URL (e.g., google.com)" };
            Controls.Add(txtUserInput);

            // CheckBox для удаления введённого слова
            checkBoxRemoveUserInput = new CheckBox
            {
                Left = 20,
                Top = 90,
                Width = 340,
                Text = "Remove base URL from test name"
            };
            Controls.Add(checkBoxRemoveUserInput);

            // Кнопка обработки файла
            btnProcessFile = new Button { Text = "Start script", Left = 370, Top = 220, Width = 100, Enabled = false };
            btnProcessFile.Click += async (sender, e) => await ProcessFileAsync();
            Controls.Add(btnProcessFile);

            // Кнопка восстановления ссылки
            btnRestoreURL = new Button { Text = "Restore base URL", Left = 250, Top = 220, Width = 100, Enabled = false };
            btnRestoreURL.Click += async (sender, e) => await RestoreFileAsync();
            Controls.Add(btnRestoreURL);

            // Полоса прогресса
            progressBar = new ProgressBar { Left = 20, Top = 160, Width = 450, Height = 20, Minimum = 0, Maximum = 100 };
            Controls.Add(progressBar);

            // Текстовое отображение прогресса
            lblProgress = new Label { Left = 20, Top = 190, Width = 450, Text = "Progress: 0%" };
            Controls.Add(lblProgress);

            // Диалог выбора файла
            openFileDialog = new OpenFileDialog
            {
                Filter = "JMX files (*.jmx)|*.jmx|All files (*.*)|*.*",
                Title = "Choose JMX file"
            };
            List<Button> necessaryButtons = new List<Button> { btnSelectFile, btnProcessFile, btnRestoreURL };
            List<TextBox> necessaryFields = new List<TextBox> { txtFilePath, txtUserInput };
            uiController = new UIController(necessaryButtons, necessaryFields, progressBar, lblProgress);
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
                btnProcessFile.Enabled = true;
                btnRestoreURL.Enabled = true;
            }
        }

        private async Task ProcessFileAsync()
        {
            if (!uiController.isNecessaryFieldsPopulated())
            {
                return;
            }

            string userInput = txtUserInput.Text.Trim();
            bool removeUserInput = checkBoxRemoveUserInput.Checked;

            uiController.showStartOfWorkUI();

            try
            {
                string[] lines = await File.ReadAllLinesAsync(txtFilePath.Text);
                int totalLines = lines.Length;
                int processedLines = 0;

                string pattern = @"(<HTTPSamplerProxy.*?>.*?</HTTPSamplerProxy>)";
                string fileContent = string.Join("\n", lines);

                fileContent = await Task.Run(() =>
                {
                    return Regex.Replace(fileContent, pattern, match =>
                    {
                        string block = match.Value;

                        string methodPattern = @"<stringProp name=""HTTPSampler.method"">(.*?)</stringProp>";
                        Match methodMatch = getMethodName(block);

                        if (methodMatch.Success)
                        {
                            string method = methodMatch.Groups[1].Value;

                            // Шаблон для testname
                            string testnamePattern = @"testname=""([^""]+)""";
                            block = Regex.Replace(block, testnamePattern, match =>
                            {
                                string testName = match.Groups[1].Value;

                                // Убираем пользовательский ввод, если чекбокс включён
                                if (removeUserInput && !string.IsNullOrEmpty(userInput))
                                {
                                    testName = testName.Replace(userInput, "").Trim();
                                }

                                return $"testname=\"[{method}] {testName}\"";
                            }, RegexOptions.IgnoreCase);
                        }

                        showPercentageOfWork(ref processedLines, totalLines);

                        return block;
                    }, RegexOptions.Singleline);
                });

                await File.WriteAllTextAsync(txtFilePath.Text, fileContent);
                MessageBox.Show("Script successfully done!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private async Task RestoreFileAsync()
        {
            if (!uiController.isNecessaryFieldsPopulated())
            {
                return;
            }

            string userUrl = txtUserInput.Text.Trim();

            uiController.showStartOfWorkUI();

            try
            {
                string[] lines = await File.ReadAllLinesAsync(txtFilePath.Text);
                int totalLines = lines.Length;
                int processedLines = 0;

                string pattern = @"testname=""\[(GET|POST|HEAD|PUT|OPTIONS|TRACE|DELETE|PATCH|PROPFIND|PROPPATCH|MKCOL|COPY|MOVE|LOCK|UNLOCK|REPORT|MKCALENDAR|SEARCH)\] (.*?)""";
                string fileContent = string.Join("\n", lines);

                fileContent = await Task.Run(() =>
                {
                    return Regex.Replace(fileContent, pattern, match =>
                    {
                        string method = match.Groups[1].Value;
                        string endpoint = match.Groups[2].Value;
                        string newTestName = $"testname=\"{userUrl}{endpoint}\"";

                        showPercentageOfWork(ref processedLines, totalLines);

                        return newTestName;
                    }, RegexOptions.IgnoreCase);
                });

                await File.WriteAllTextAsync(txtFilePath.Text, fileContent);
                MessageBox.Show("Base URL successfully restored!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // Find HTTP type of request (GET, POST and etc.)
        private Match getMethodName(string block)
        {
            string methodPattern = @"<stringProp name=""HTTPSampler.method"">(.*?)</stringProp>";
            return Regex.Match(block, methodPattern);
        }

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
    }
}
