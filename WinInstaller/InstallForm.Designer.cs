
namespace WinInstaller
{
    partial class InstallForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallForm));
            this.vscodeCheckbox = new System.Windows.Forms.CheckBox();
            this.visualStudioCheckbox = new System.Windows.Forms.CheckBox();
            this.linkWheelCheckbox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.installButton = new System.Windows.Forms.Button();
            this.uninstallButton = new System.Windows.Forms.Button();
            this.globalConfigCheckbox = new System.Windows.Forms.CheckBox();
            this.englishToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // vscodeCheckbox
            // 
            this.vscodeCheckbox.AutoSize = true;
            this.vscodeCheckbox.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.vscodeCheckbox.Location = new System.Drawing.Point(59, 100);
            this.vscodeCheckbox.Name = "vscodeCheckbox";
            this.vscodeCheckbox.Size = new System.Drawing.Size(124, 19);
            this.vscodeCheckbox.TabIndex = 0;
            this.vscodeCheckbox.Text = "VS Code Extension";
            this.englishToolTip.SetToolTip(this.vscodeCheckbox, "A VSCode extension to generate links and register repos for LinkWheel");
            this.vscodeCheckbox.UseVisualStyleBackColor = true;
            // 
            // visualStudioCheckbox
            // 
            this.visualStudioCheckbox.AutoSize = true;
            this.visualStudioCheckbox.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.visualStudioCheckbox.Location = new System.Drawing.Point(59, 122);
            this.visualStudioCheckbox.Margin = new System.Windows.Forms.Padding(50, 3, 50, 3);
            this.visualStudioCheckbox.Name = "visualStudioCheckbox";
            this.visualStudioCheckbox.Size = new System.Drawing.Size(148, 19);
            this.visualStudioCheckbox.TabIndex = 1;
            this.visualStudioCheckbox.Text = "Visual Studio Extension";
            this.englishToolTip.SetToolTip(this.visualStudioCheckbox, "A Visual Studio extension to generate links and register repos for LinkWheel");
            this.visualStudioCheckbox.UseVisualStyleBackColor = true;
            // 
            // linkWheelCheckbox
            // 
            this.linkWheelCheckbox.AutoSize = true;
            this.linkWheelCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.linkWheelCheckbox.Checked = true;
            this.linkWheelCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.linkWheelCheckbox.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.linkWheelCheckbox.Location = new System.Drawing.Point(59, 80);
            this.linkWheelCheckbox.Name = "linkWheelCheckbox";
            this.linkWheelCheckbox.Size = new System.Drawing.Size(81, 19);
            this.linkWheelCheckbox.TabIndex = 2;
            this.linkWheelCheckbox.Text = "LinkWheel";
            this.englishToolTip.SetToolTip(this.linkWheelCheckbox, "The required executables on the system path for LinkWheel to work");
            this.linkWheelCheckbox.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label2.Location = new System.Drawing.Point(40, 29);
            this.label2.Margin = new System.Windows.Forms.Padding(50, 0, 50, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(181, 28);
            this.label2.TabIndex = 4;
            this.label2.Text = "Select Components";
            // 
            // installButton
            // 
            this.installButton.BackColor = System.Drawing.Color.Cyan;
            this.installButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.installButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.installButton.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.installButton.ForeColor = System.Drawing.SystemColors.InfoText;
            this.installButton.Location = new System.Drawing.Point(40, 218);
            this.installButton.Margin = new System.Windows.Forms.Padding(10, 10, 10, 5);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(181, 37);
            this.installButton.TabIndex = 5;
            this.installButton.Text = "Install";
            this.installButton.UseVisualStyleBackColor = false;
            this.installButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // uninstallButton
            // 
            this.uninstallButton.BackColor = System.Drawing.Color.DarkGray;
            this.uninstallButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.uninstallButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.uninstallButton.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.uninstallButton.ForeColor = System.Drawing.SystemColors.InfoText;
            this.uninstallButton.Location = new System.Drawing.Point(40, 265);
            this.uninstallButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 10);
            this.uninstallButton.Name = "uninstallButton";
            this.uninstallButton.Size = new System.Drawing.Size(181, 37);
            this.uninstallButton.TabIndex = 6;
            this.uninstallButton.Text = "Uninstall";
            this.uninstallButton.UseVisualStyleBackColor = false;
            this.uninstallButton.Click += new System.EventHandler(this.UninstallButton_Click);
            // 
            // globalConfigCheckbox
            // 
            this.globalConfigCheckbox.AutoSize = true;
            this.globalConfigCheckbox.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.globalConfigCheckbox.Location = new System.Drawing.Point(59, 144);
            this.globalConfigCheckbox.Margin = new System.Windows.Forms.Padding(50, 3, 50, 3);
            this.globalConfigCheckbox.Name = "globalConfigCheckbox";
            this.globalConfigCheckbox.Size = new System.Drawing.Size(140, 19);
            this.globalConfigCheckbox.TabIndex = 7;
            this.globalConfigCheckbox.Text = "Default Global Config";
            this.englishToolTip.SetToolTip(this.globalConfigCheckbox, resources.GetString("globalConfigCheckbox.ToolTip"));
            this.globalConfigCheckbox.UseVisualStyleBackColor = true;
            // 
            // statusLabel
            // 
            this.statusLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.statusLabel.Location = new System.Drawing.Point(12, 166);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(242, 42);
            this.statusLabel.TabIndex = 8;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InstallForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.ClientSize = new System.Drawing.Size(266, 317);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.globalConfigCheckbox);
            this.Controls.Add(this.uninstallButton);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.linkWheelCheckbox);
            this.Controls.Add(this.visualStudioCheckbox);
            this.Controls.Add(this.vscodeCheckbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "InstallForm";
            this.Text = "LinkWheel Installer";
            this.Load += new System.EventHandler(this.InstallForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox vscodeCheckbox;
        private System.Windows.Forms.CheckBox visualStudioCheckbox;
        private System.Windows.Forms.CheckBox linkWheelCheckbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button installButton;
        private System.Windows.Forms.Button uninstallButton;
        private System.Windows.Forms.CheckBox globalConfigCheckbox;
        private System.Windows.Forms.ToolTip englishToolTip;
        private System.Windows.Forms.Label statusLabel;
    }
}

