
namespace ExcelOuraVSTOAddIn
{
    partial class ChangeLabelForm
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
            this.lblFieldName = new System.Windows.Forms.Label();
            this.txtFieldName = new System.Windows.Forms.TextBox();
            this.txtCustomLabel = new System.Windows.Forms.TextBox();
            this.lblCustomLabel = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSection = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFieldName
            // 
            this.lblFieldName.AutoSize = true;
            this.lblFieldName.Location = new System.Drawing.Point(20, 94);
            this.lblFieldName.Name = "lblFieldName";
            this.lblFieldName.Size = new System.Drawing.Size(60, 13);
            this.lblFieldName.TabIndex = 1;
            this.lblFieldName.Text = "Field Name";
            // 
            // txtFieldName
            // 
            this.txtFieldName.Location = new System.Drawing.Point(96, 91);
            this.txtFieldName.Name = "txtFieldName";
            this.txtFieldName.ReadOnly = true;
            this.txtFieldName.Size = new System.Drawing.Size(220, 20);
            this.txtFieldName.TabIndex = 2;
            this.txtFieldName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtFieldName_MouseClick);
            // 
            // txtCustomLabel
            // 
            this.txtCustomLabel.Location = new System.Drawing.Point(96, 143);
            this.txtCustomLabel.MaxLength = 60;
            this.txtCustomLabel.Name = "txtCustomLabel";
            this.txtCustomLabel.Size = new System.Drawing.Size(220, 20);
            this.txtCustomLabel.TabIndex = 4;
            // 
            // lblCustomLabel
            // 
            this.lblCustomLabel.AutoSize = true;
            this.lblCustomLabel.Location = new System.Drawing.Point(20, 146);
            this.lblCustomLabel.Name = "lblCustomLabel";
            this.lblCustomLabel.Size = new System.Drawing.Size(73, 13);
            this.lblCustomLabel.TabIndex = 3;
            this.lblCustomLabel.Text = "Custom Name";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(246, 184);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(166, 184);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(93, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(229, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Select the name you prefer for the field";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(93, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(222, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "(Click in Field Name to copy to Custom Name)";
            // 
            // txtSection
            // 
            this.txtSection.Location = new System.Drawing.Point(96, 117);
            this.txtSection.Name = "txtSection";
            this.txtSection.ReadOnly = true;
            this.txtSection.Size = new System.Drawing.Size(220, 20);
            this.txtSection.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Section";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.OuraDarkSolid;
            this.pictureBox1.Location = new System.Drawing.Point(23, 26);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(57, 49);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // ChangeLabelForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(333, 219);
            this.ControlBox = false;
            this.Controls.Add(this.txtSection);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtCustomLabel);
            this.Controls.Add(this.lblCustomLabel);
            this.Controls.Add(this.txtFieldName);
            this.Controls.Add(this.lblFieldName);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ChangeLabelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change Label";
            this.Load += new System.EventHandler(this.ChangeLabelForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblFieldName;
        private System.Windows.Forms.TextBox txtFieldName;
        private System.Windows.Forms.TextBox txtCustomLabel;
        private System.Windows.Forms.Label lblCustomLabel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSection;
        private System.Windows.Forms.Label label3;
    }
}