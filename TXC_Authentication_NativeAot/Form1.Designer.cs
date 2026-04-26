namespace TXC_Authentication_NativeAot
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            heroPanel = new Panel();
            heroCaptionLabel = new Label();
            heroBodyLabel = new Label();
            heroTitleLabel = new Label();
            authCardPanel = new Panel();
            statusLabel = new Label();
            secondaryHintLabel = new Label();
            clearButton = new Button();
            primaryActionButton = new Button();
            licensePanel = new Panel();
            licenseTextBox = new TextBox();
            licenseLabel = new Label();
            passwordTextBox = new TextBox();
            passwordLabel = new Label();
            usernameTextBox = new TextBox();
            usernameLabel = new Label();
            formSubtitleLabel = new Label();
            formTitleLabel = new Label();
            modePanel = new Panel();
            registerModeButton = new Button();
            loginModeButton = new Button();
            heroPanel.SuspendLayout();
            authCardPanel.SuspendLayout();
            licensePanel.SuspendLayout();
            modePanel.SuspendLayout();
            SuspendLayout();
            // 
            // heroPanel
            // 
            heroPanel.BackColor = Color.FromArgb(20, 24, 31);
            heroPanel.Controls.Add(heroCaptionLabel);
            heroPanel.Controls.Add(heroBodyLabel);
            heroPanel.Controls.Add(heroTitleLabel);
            heroPanel.Dock = DockStyle.Left;
            heroPanel.Location = new Point(0, 0);
            heroPanel.Name = "heroPanel";
            heroPanel.Padding = new Padding(36);
            heroPanel.Size = new Size(320, 620);
            heroPanel.TabIndex = 0;
            // 
            // heroCaptionLabel
            // 
            heroCaptionLabel.AutoSize = true;
            heroCaptionLabel.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            heroCaptionLabel.ForeColor = Color.FromArgb(95, 144, 255);
            heroCaptionLabel.Location = new Point(40, 45);
            heroCaptionLabel.Name = "heroCaptionLabel";
            heroCaptionLabel.Size = new Size(104, 17);
            heroCaptionLabel.TabIndex = 2;
            heroCaptionLabel.Text = "TXC AUTH SDK";
            // 
            // heroBodyLabel
            // 
            heroBodyLabel.ForeColor = Color.FromArgb(154, 163, 177);
            heroBodyLabel.Location = new Point(40, 176);
            heroBodyLabel.Name = "heroBodyLabel";
            heroBodyLabel.Size = new Size(224, 118);
            heroBodyLabel.TabIndex = 1;
            heroBodyLabel.Text = "A simple desktop authentication example for Native AOT projects.\r\n\r\nUse this as a clean starting point for login and register flows.";
            // 
            // heroTitleLabel
            // 
            heroTitleLabel.Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold, GraphicsUnit.Point);
            heroTitleLabel.ForeColor = Color.White;
            heroTitleLabel.Location = new Point(40, 82);
            heroTitleLabel.Name = "heroTitleLabel";
            heroTitleLabel.Size = new Size(227, 78);
            heroTitleLabel.TabIndex = 0;
            heroTitleLabel.Text = "Simple auth screen";
            // 
            // authCardPanel
            // 
            authCardPanel.BackColor = Color.FromArgb(17, 19, 26);
            authCardPanel.Controls.Add(statusLabel);
            authCardPanel.Controls.Add(secondaryHintLabel);
            authCardPanel.Controls.Add(clearButton);
            authCardPanel.Controls.Add(primaryActionButton);
            authCardPanel.Controls.Add(licensePanel);
            authCardPanel.Controls.Add(passwordTextBox);
            authCardPanel.Controls.Add(passwordLabel);
            authCardPanel.Controls.Add(usernameTextBox);
            authCardPanel.Controls.Add(usernameLabel);
            authCardPanel.Controls.Add(formSubtitleLabel);
            authCardPanel.Controls.Add(formTitleLabel);
            authCardPanel.Controls.Add(modePanel);
            authCardPanel.Location = new Point(370, 58);
            authCardPanel.Name = "authCardPanel";
            authCardPanel.Size = new Size(420, 504);
            authCardPanel.TabIndex = 1;
            // 
            // statusLabel
            // 
            statusLabel.BackColor = Color.FromArgb(22, 26, 35);
            statusLabel.ForeColor = Color.FromArgb(139, 201, 144);
            statusLabel.Location = new Point(36, 447);
            statusLabel.Name = "statusLabel";
            statusLabel.Padding = new Padding(12, 10, 12, 10);
            statusLabel.Size = new Size(348, 36);
            statusLabel.TabIndex = 11;
            statusLabel.Text = "Ready";
            // 
            // secondaryHintLabel
            // 
            secondaryHintLabel.ForeColor = Color.FromArgb(124, 132, 145);
            secondaryHintLabel.Location = new Point(40, 400);
            secondaryHintLabel.Name = "secondaryHintLabel";
            secondaryHintLabel.Size = new Size(344, 22);
            secondaryHintLabel.TabIndex = 10;
            secondaryHintLabel.Text = "Need an account? Switch to register.";
            secondaryHintLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // clearButton
            // 
            clearButton.BackColor = Color.FromArgb(28, 32, 40);
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.FlatStyle = FlatStyle.Flat;
            clearButton.ForeColor = Color.FromArgb(215, 220, 228);
            clearButton.Location = new Point(40, 342);
            clearButton.Name = "clearButton";
            clearButton.Size = new Size(105, 42);
            clearButton.TabIndex = 9;
            clearButton.Text = "Clear";
            clearButton.UseVisualStyleBackColor = false;
            clearButton.Click += ClearButton_Click;
            // 
            // primaryActionButton
            // 
            primaryActionButton.BackColor = Color.FromArgb(52, 107, 255);
            primaryActionButton.FlatAppearance.BorderSize = 0;
            primaryActionButton.FlatStyle = FlatStyle.Flat;
            primaryActionButton.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            primaryActionButton.ForeColor = Color.White;
            primaryActionButton.Location = new Point(157, 342);
            primaryActionButton.Name = "primaryActionButton";
            primaryActionButton.Size = new Size(227, 42);
            primaryActionButton.TabIndex = 8;
            primaryActionButton.Text = "Login";
            primaryActionButton.UseVisualStyleBackColor = false;
            primaryActionButton.Click += PrimaryActionButton_Click;
            // 
            // licensePanel
            // 
            licensePanel.Controls.Add(licenseTextBox);
            licensePanel.Controls.Add(licenseLabel);
            licensePanel.Location = new Point(40, 260);
            licensePanel.Name = "licensePanel";
            licensePanel.Size = new Size(344, 66);
            licensePanel.TabIndex = 7;
            // 
            // licenseTextBox
            // 
            licenseTextBox.BackColor = Color.FromArgb(22, 26, 35);
            licenseTextBox.BorderStyle = BorderStyle.FixedSingle;
            licenseTextBox.ForeColor = Color.White;
            licenseTextBox.Location = new Point(0, 31);
            licenseTextBox.Name = "licenseTextBox";
            licenseTextBox.PlaceholderText = "XXXX-XXXX-XXXX";
            licenseTextBox.Size = new Size(344, 23);
            licenseTextBox.TabIndex = 1;
            // 
            // licenseLabel
            // 
            licenseLabel.AutoSize = true;
            licenseLabel.ForeColor = Color.FromArgb(191, 198, 209);
            licenseLabel.Location = new Point(0, 9);
            licenseLabel.Name = "licenseLabel";
            licenseLabel.Size = new Size(46, 15);
            licenseLabel.TabIndex = 0;
            licenseLabel.Text = "License";
            // 
            // passwordTextBox
            // 
            passwordTextBox.BackColor = Color.FromArgb(22, 26, 35);
            passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
            passwordTextBox.ForeColor = Color.White;
            passwordTextBox.Location = new Point(40, 230);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.PlaceholderText = "Enter password";
            passwordTextBox.Size = new Size(344, 23);
            passwordTextBox.TabIndex = 6;
            passwordTextBox.UseSystemPasswordChar = true;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.ForeColor = Color.FromArgb(191, 198, 209);
            passwordLabel.Location = new Point(40, 208);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(57, 15);
            passwordLabel.TabIndex = 5;
            passwordLabel.Text = "Password";
            // 
            // usernameTextBox
            // 
            usernameTextBox.BackColor = Color.FromArgb(22, 26, 35);
            usernameTextBox.BorderStyle = BorderStyle.FixedSingle;
            usernameTextBox.ForeColor = Color.White;
            usernameTextBox.Location = new Point(40, 163);
            usernameTextBox.Name = "usernameTextBox";
            usernameTextBox.PlaceholderText = "Enter username";
            usernameTextBox.Size = new Size(344, 23);
            usernameTextBox.TabIndex = 4;
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.ForeColor = Color.FromArgb(191, 198, 209);
            usernameLabel.Location = new Point(40, 141);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(60, 15);
            usernameLabel.TabIndex = 3;
            usernameLabel.Text = "Username";
            // 
            // formSubtitleLabel
            // 
            formSubtitleLabel.ForeColor = Color.FromArgb(130, 138, 150);
            formSubtitleLabel.Location = new Point(40, 90);
            formSubtitleLabel.Name = "formSubtitleLabel";
            formSubtitleLabel.Size = new Size(302, 32);
            formSubtitleLabel.TabIndex = 2;
            formSubtitleLabel.Text = "Sign in with your username and password.";
            // 
            // formTitleLabel
            // 
            formTitleLabel.AutoSize = true;
            formTitleLabel.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point);
            formTitleLabel.ForeColor = Color.White;
            formTitleLabel.Location = new Point(40, 53);
            formTitleLabel.Name = "formTitleLabel";
            formTitleLabel.Size = new Size(151, 32);
            formTitleLabel.TabIndex = 1;
            formTitleLabel.Text = "Welcome back";
            // 
            // modePanel
            // 
            modePanel.BackColor = Color.FromArgb(11, 14, 20);
            modePanel.Controls.Add(registerModeButton);
            modePanel.Controls.Add(loginModeButton);
            modePanel.Location = new Point(40, 22);
            modePanel.Name = "modePanel";
            modePanel.Padding = new Padding(4);
            modePanel.Size = new Size(202, 30);
            modePanel.TabIndex = 0;
            // 
            // registerModeButton
            // 
            registerModeButton.BackColor = Color.FromArgb(28, 32, 40);
            registerModeButton.FlatAppearance.BorderSize = 0;
            registerModeButton.FlatStyle = FlatStyle.Flat;
            registerModeButton.ForeColor = Color.FromArgb(166, 174, 186);
            registerModeButton.Location = new Point(101, 4);
            registerModeButton.Name = "registerModeButton";
            registerModeButton.Size = new Size(97, 22);
            registerModeButton.TabIndex = 1;
            registerModeButton.Text = "Register";
            registerModeButton.UseVisualStyleBackColor = false;
            registerModeButton.Click += RegisterModeButton_Click;
            // 
            // loginModeButton
            // 
            loginModeButton.BackColor = Color.FromArgb(52, 107, 255);
            loginModeButton.FlatAppearance.BorderSize = 0;
            loginModeButton.FlatStyle = FlatStyle.Flat;
            loginModeButton.ForeColor = Color.White;
            loginModeButton.Location = new Point(4, 4);
            loginModeButton.Name = "loginModeButton";
            loginModeButton.Size = new Size(94, 22);
            loginModeButton.TabIndex = 0;
            loginModeButton.Text = "Login";
            loginModeButton.UseVisualStyleBackColor = false;
            loginModeButton.Click += LoginModeButton_Click;
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 12, 18);
            ClientSize = new Size(840, 620);
            Controls.Add(authCardPanel);
            Controls.Add(heroPanel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TXC Authentication Native AOT";
            heroPanel.ResumeLayout(false);
            heroPanel.PerformLayout();
            authCardPanel.ResumeLayout(false);
            authCardPanel.PerformLayout();
            licensePanel.ResumeLayout(false);
            licensePanel.PerformLayout();
            modePanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel heroPanel;
        private Label heroCaptionLabel;
        private Label heroBodyLabel;
        private Label heroTitleLabel;
        private Panel authCardPanel;
        private Label statusLabel;
        private Label secondaryHintLabel;
        private Button clearButton;
        private Button primaryActionButton;
        private Panel licensePanel;
        private TextBox licenseTextBox;
        private Label licenseLabel;
        private TextBox passwordTextBox;
        private Label passwordLabel;
        private TextBox usernameTextBox;
        private Label usernameLabel;
        private Label formSubtitleLabel;
        private Label formTitleLabel;
        private Panel modePanel;
        private Button registerModeButton;
        private Button loginModeButton;
    }
}
