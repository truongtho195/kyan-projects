namespace CPC.POSCustomInstaller
{
    partial class frmPrerequisites
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkRestore = new System.Windows.Forms.CheckBox();
            this.chkDataBase = new System.Windows.Forms.CheckBox();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkRestore);
            this.groupBox1.Controls.Add(this.chkDataBase);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 8.75F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(10, 11);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(404, 77);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Install Prerequisites Components:";
            // 
            // chkRestore
            // 
            this.chkRestore.AutoSize = true;
            this.chkRestore.Font = new System.Drawing.Font("Arial", 8.75F);
            this.chkRestore.Location = new System.Drawing.Point(5, 44);
            this.chkRestore.Name = "chkRestore";
            this.chkRestore.Size = new System.Drawing.Size(290, 19);
            this.chkRestore.TabIndex = 0;
            this.chkRestore.Text = "Create (if not exists) and restore POS  database.";
            this.chkRestore.UseVisualStyleBackColor = true;
            // 
            // chkDataBase
            // 
            this.chkDataBase.AutoSize = true;
            this.chkDataBase.Checked = true;
            this.chkDataBase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDataBase.Font = new System.Drawing.Font("Arial", 8.75F);
            this.chkDataBase.Location = new System.Drawing.Point(5, 20);
            this.chkDataBase.Name = "chkDataBase";
            this.chkDataBase.Size = new System.Drawing.Size(278, 19);
            this.chkDataBase.TabIndex = 0;
            this.chkDataBase.Text = "Config the account connect to PostgreSQL 9.x.";
            this.chkDataBase.UseVisualStyleBackColor = true;
            // 
            // btnInstall
            // 
            this.btnInstall.Font = new System.Drawing.Font("Arial", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnInstall.Location = new System.Drawing.Point(277, 117);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(66, 22);
            this.btnInstall.TabIndex = 2;
            this.btnInstall.Text = "&Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 8.75F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(348, 117);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 22);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.LightGray;
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Blue;
            this.label8.Location = new System.Drawing.Point(0, 103);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(426, 47);
            this.label8.TabIndex = 8;
            // 
            // frmPrerequisites
            // 
            this.AcceptButton = this.btnInstall;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(426, 150);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label8);
            this.Font = new System.Drawing.Font("Arial", 8.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmPrerequisites";
            this.Text = "Install Prerequisites Components ...";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkDataBase;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkRestore;
        private System.Windows.Forms.Label label8;

    }

}