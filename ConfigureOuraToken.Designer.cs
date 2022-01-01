namespace ExcelOuraVSTOAddIn
{
    partial class ConfigureOuraToken
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.txtGender = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOuraAPIKey = new System.Windows.Forms.TextBox();
            this.lblOuraAPIKeyLabel = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblFieldName = new System.Windows.Forms.Label();
            this.btnTestAPIKey = new System.Windows.Forms.Button();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnReload = new System.Windows.Forms.Button();
            this.lblDetails1 = new System.Windows.Forms.Label();
            this.lblLink = new System.Windows.Forms.LinkLabel();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(89, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(345, 21);
            this.label1.TabIndex = 13;
            this.label1.Text = "Configuration for the Oura Excel Add-In";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(387, 287);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(307, 287);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txtGender
            // 
            this.txtGender.Location = new System.Drawing.Point(108, 201);
            this.txtGender.Name = "txtGender";
            this.txtGender.ReadOnly = true;
            this.txtGender.Size = new System.Drawing.Size(254, 20);
            this.txtGender.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 204);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Gender";
            // 
            // txtOuraAPIKey
            // 
            this.txtOuraAPIKey.Location = new System.Drawing.Point(108, 123);
            this.txtOuraAPIKey.MaxLength = 60;
            this.txtOuraAPIKey.Name = "txtOuraAPIKey";
            this.txtOuraAPIKey.Size = new System.Drawing.Size(254, 20);
            this.txtOuraAPIKey.TabIndex = 0;
            // 
            // lblOuraAPIKeyLabel
            // 
            this.lblOuraAPIKeyLabel.AutoSize = true;
            this.lblOuraAPIKeyLabel.Location = new System.Drawing.Point(32, 126);
            this.lblOuraAPIKeyLabel.Name = "lblOuraAPIKeyLabel";
            this.lblOuraAPIKeyLabel.Size = new System.Drawing.Size(64, 13);
            this.lblOuraAPIKeyLabel.TabIndex = 17;
            this.lblOuraAPIKeyLabel.Text = "Oura Token";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(108, 149);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.Size = new System.Drawing.Size(254, 20);
            this.txtEmail.TabIndex = 6;
            // 
            // lblFieldName
            // 
            this.lblFieldName.AutoSize = true;
            this.lblFieldName.Location = new System.Drawing.Point(32, 152);
            this.lblFieldName.Name = "lblFieldName";
            this.lblFieldName.Size = new System.Drawing.Size(32, 13);
            this.lblFieldName.TabIndex = 15;
            this.lblFieldName.Text = "Email";
            // 
            // btnTestAPIKey
            // 
            this.btnTestAPIKey.Location = new System.Drawing.Point(375, 120);
            this.btnTestAPIKey.Name = "btnTestAPIKey";
            this.btnTestAPIKey.Size = new System.Drawing.Size(87, 23);
            this.btnTestAPIKey.TabIndex = 1;
            this.btnTestAPIKey.Text = "Test Token";
            this.btnTestAPIKey.UseVisualStyleBackColor = true;
            this.btnTestAPIKey.Click += new System.EventHandler(this.btnTestAPIKey_Click);
            // 
            // txtAge
            // 
            this.txtAge.Location = new System.Drawing.Point(108, 175);
            this.txtAge.Name = "txtAge";
            this.txtAge.ReadOnly = true;
            this.txtAge.Size = new System.Drawing.Size(254, 20);
            this.txtAge.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 178);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Age";
            // 
            // txtWeight
            // 
            this.txtWeight.Location = new System.Drawing.Point(108, 227);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.ReadOnly = true;
            this.txtWeight.Size = new System.Drawing.Size(254, 20);
            this.txtWeight.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 230);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Weight";
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(375, 149);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(87, 23);
            this.btnReload.TabIndex = 2;
            this.btnReload.Text = "Reload Token";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // lblDetails1
            // 
            this.lblDetails1.Location = new System.Drawing.Point(89, 42);
            this.lblDetails1.Name = "lblDetails1";
            this.lblDetails1.Size = new System.Drawing.Size(356, 52);
            this.lblDetails1.TabIndex = 27;
            this.lblDetails1.Text = "To use this add-in, you need to expose a Personal Access Token from the Oura Clou" +
    "d Dashboard. Go to the below link and generate one, then copy it into the \"Oura " +
    "Token\" field.";
            // 
            // lblLink
            // 
            this.lblLink.AutoSize = true;
            this.lblLink.Location = new System.Drawing.Point(105, 92);
            this.lblLink.Name = "lblLink";
            this.lblLink.Size = new System.Drawing.Size(250, 13);
            this.lblLink.TabIndex = 5;
            this.lblLink.TabStop = true;
            this.lblLink.Text = "https://cloud.ouraring.com/personal-access-tokens";
            this.lblLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblLink_LinkClicked);
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(108, 253);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.ReadOnly = true;
            this.txtHeight.Size = new System.Drawing.Size(254, 20);
            this.txtHeight.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(32, 256);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 31;
            this.label7.Text = "Height";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.Settings;
            this.pictureBox1.Location = new System.Drawing.Point(18, 20);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(46, 59);
            this.pictureBox1.TabIndex = 33;
            this.pictureBox1.TabStop = false;
            // 
            // ConfigureOuraToken
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(474, 322);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblLink);
            this.Controls.Add(this.lblDetails1);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.txtAge);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtWeight);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnTestAPIKey);
            this.Controls.Add(this.txtGender);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtOuraAPIKey);
            this.Controls.Add(this.lblOuraAPIKeyLabel);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblFieldName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureOuraToken";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Oura Token";
            this.Load += new System.EventHandler(this.ConfigureOuraAPIKey_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox txtGender;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOuraAPIKey;
        private System.Windows.Forms.Label lblOuraAPIKeyLabel;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblFieldName;
        private System.Windows.Forms.Button btnTestAPIKey;
        private System.Windows.Forms.TextBox txtAge;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtWeight;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Label lblDetails1;
        private System.Windows.Forms.LinkLabel lblLink;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}