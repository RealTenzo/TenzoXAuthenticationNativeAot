using System.Drawing;
using TXAAuth;

namespace TXC_Authentication_NativeAot
{
    public partial class Form1 : Form
    {
        private static readonly TXA Auth = new TXA(
            name: "test",
            secret: "TXA-ZKOM4LP9DA",
            version: "1.0"
        );

        private bool isRegisterMode;

        public Form1()
        {
            InitializeComponent();
            Shown += Form1_Shown;
            SwitchMode(false);
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            Shown -= Form1_Shown;
            statusLabel.Text = "Initializing...";
            ToggleInputs(false);

            try
            {
                await Auth.InitAsync();
                statusLabel.Text = Auth.IsInitialized ? "Ready" : Auth.Response;
            }
            catch
            {
                statusLabel.Text = "Initialization failed.";
            }
            finally
            {
                ToggleInputs(Auth.IsInitialized);
                usernameTextBox.Focus();
            }
        }

        private void LoginModeButton_Click(object sender, EventArgs e)
        {
            SwitchMode(false);
        }

        private void RegisterModeButton_Click(object sender, EventArgs e)
        {
            SwitchMode(true);
        }

        private async void PrimaryActionButton_Click(object sender, EventArgs e)
        {
            ToggleInputs(false);
            statusLabel.Text = isRegisterMode ? "Registering..." : "Logging in...";

            try
            {
                if (isRegisterMode)
                {
                    if (string.IsNullOrWhiteSpace(licenseTextBox.Text))
                    {
                        statusLabel.Text = "Enter a license key.";
                        return;
                    }

                    var result = await Auth.Register(
                        usernameTextBox.Text.Trim(),
                        passwordTextBox.Text,
                        licenseTextBox.Text.Trim()
                    );

                    statusLabel.Text = result.Message;
                }
                else
                {
                    var result = await Auth.Login(
                        usernameTextBox.Text.Trim(),
                        passwordTextBox.Text
                    );

                    statusLabel.Text = result.Message;
                }
            }
            finally
            {
                ToggleInputs(Auth.IsInitialized);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            usernameTextBox.Clear();
            passwordTextBox.Clear();
            licenseTextBox.Clear();
            statusLabel.Text = "Fields cleared.";
            usernameTextBox.Focus();
        }

        private void SwitchMode(bool registerMode)
        {
            isRegisterMode = registerMode;

            licensePanel.Visible = registerMode;
            formTitleLabel.Text = registerMode ? "Create account" : "Welcome back";
            formSubtitleLabel.Text = registerMode
                ? "Use a valid license to register a new account."
                : "Sign in with your username and password.";
            primaryActionButton.Text = registerMode ? "Register" : "Login";
            secondaryHintLabel.Text = registerMode
                ? "Already have an account? Switch back to login."
                : "Need an account? Switch to register.";

            loginModeButton.BackColor = registerMode ? Color.FromArgb(28, 32, 40) : Color.FromArgb(52, 107, 255);
            loginModeButton.ForeColor = registerMode ? Color.FromArgb(166, 174, 186) : Color.White;

            registerModeButton.BackColor = registerMode ? Color.FromArgb(52, 107, 255) : Color.FromArgb(28, 32, 40);
            registerModeButton.ForeColor = registerMode ? Color.White : Color.FromArgb(166, 174, 186);

            statusLabel.Text = registerMode
                ? "Register mode selected."
                : "Login mode selected.";
        }

        private void ToggleInputs(bool enabled)
        {
            loginModeButton.Enabled = enabled;
            registerModeButton.Enabled = enabled;
            usernameTextBox.Enabled = enabled;
            passwordTextBox.Enabled = enabled;
            licenseTextBox.Enabled = enabled;
            primaryActionButton.Enabled = enabled;
            clearButton.Enabled = enabled;
        }
    }
}
