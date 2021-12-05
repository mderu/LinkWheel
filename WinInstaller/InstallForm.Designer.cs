
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallForm));
            this.vscodeCheckbox = new System.Windows.Forms.CheckBox();
            this.visualStudioCheckbox = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.installButton = new System.Windows.Forms.Button();
            this.uninstallButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // vscodeCheckbox
            // 
            this.vscodeCheckbox.AutoSize = true;
            this.vscodeCheckbox.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.vscodeCheckbox.Location = new System.Drawing.Point(59, 99);
            this.vscodeCheckbox.Name = "vscodeCheckbox";
            this.vscodeCheckbox.Size = new System.Drawing.Size(124, 19);
            this.vscodeCheckbox.TabIndex = 0;
            this.vscodeCheckbox.Text = "VS Code Extension";
            this.vscodeCheckbox.UseVisualStyleBackColor = true;
            // 
            // visualStudioCheckbox
            // 
            this.visualStudioCheckbox.AutoSize = true;
            this.visualStudioCheckbox.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.visualStudioCheckbox.Location = new System.Drawing.Point(59, 121);
            this.visualStudioCheckbox.Margin = new System.Windows.Forms.Padding(50, 3, 50, 3);
            this.visualStudioCheckbox.Name = "visualStudioCheckbox";
            this.visualStudioCheckbox.Size = new System.Drawing.Size(148, 19);
            this.visualStudioCheckbox.TabIndex = 1;
            this.visualStudioCheckbox.Text = "Visual Studio Extension";
            this.visualStudioCheckbox.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.BackColor = System.Drawing.Color.Transparent;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Enabled = false;
            this.checkBox3.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.checkBox3.Location = new System.Drawing.Point(59, 79);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(15, 14);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label1.Location = new System.Drawing.Point(74, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "LinkWheel";
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
            this.installButton.Location = new System.Drawing.Point(40, 165);
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
            this.uninstallButton.Location = new System.Drawing.Point(40, 212);
            this.uninstallButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 10);
            this.uninstallButton.Name = "uninstallButton";
            this.uninstallButton.Size = new System.Drawing.Size(181, 37);
            this.uninstallButton.TabIndex = 6;
            this.uninstallButton.Text = "Uninstall";
            this.uninstallButton.UseVisualStyleBackColor = false;
            this.uninstallButton.Click += new System.EventHandler(this.UninstallButton_Click);
            // 
            // InstallForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.ClientSize = new System.Drawing.Size(266, 268);
            this.Controls.Add(this.uninstallButton);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox3);
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
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button installButton;
        private System.Windows.Forms.Button uninstallButton;
    }
}

