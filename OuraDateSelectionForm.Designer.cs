namespace ExcelOuraVSTOAddIn
{
    partial class OuraDateSelectionForm
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
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.errorProviderApp = new System.Windows.Forms.ErrorProvider(this.components);
            this.FieldName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Section = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkIncludeHeaders = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtmEndDate = new System.Windows.Forms.DateTimePicker();
            this.dtmStartDate = new System.Windows.Forms.DateTimePicker();
            this.btnOk = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblInformationType = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderApp)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.OuraDarkSolid;
            this.pictureBox1.Location = new System.Drawing.Point(18, 14);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(54, 52);
            this.pictureBox1.TabIndex = 38;
            this.pictureBox1.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(122, 35);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(248, 13);
            this.label8.TabIndex = 35;
            this.label8.Text = "Select the date range you desire for this information";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(33, 137);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 33;
            this.label6.Text = "WARNING:";
            // 
            // errorProviderApp
            // 
            this.errorProviderApp.ContainerControl = this;
            // 
            // FieldName
            // 
            this.FieldName.Text = "Field Name";
            // 
            // Section
            // 
            this.Section.Text = "Section Area";
            // 
            // chkIncludeHeaders
            // 
            this.chkIncludeHeaders.AutoSize = true;
            this.chkIncludeHeaders.Location = new System.Drawing.Point(185, 61);
            this.chkIncludeHeaders.Name = "chkIncludeHeaders";
            this.chkIncludeHeaders.Size = new System.Drawing.Size(132, 17);
            this.chkIncludeHeaders.TabIndex = 25;
            this.chkIncludeHeaders.Text = "Display Excel Headers";
            this.chkIncludeHeaders.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(105, 137);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(311, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "If you press OK, this will replace various columns and rows within";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(355, 179);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 32;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(125, 108);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "End Date";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(125, 83);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Start Date";
            // 
            // dtmEndDate
            // 
            this.dtmEndDate.Location = new System.Drawing.Point(185, 108);
            this.dtmEndDate.Margin = new System.Windows.Forms.Padding(2);
            this.dtmEndDate.Name = "dtmEndDate";
            this.dtmEndDate.Size = new System.Drawing.Size(189, 20);
            this.dtmEndDate.TabIndex = 24;
            // 
            // dtmStartDate
            // 
            this.dtmStartDate.Location = new System.Drawing.Point(185, 83);
            this.dtmStartDate.Margin = new System.Windows.Forms.Padding(2);
            this.dtmStartDate.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dtmStartDate.Name = "dtmStartDate";
            this.dtmStartDate.Size = new System.Drawing.Size(189, 20);
            this.dtmStartDate.TabIndex = 22;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(275, 179);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 31;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(105, 152);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(230, 13);
            this.label5.TabIndex = 39;
            this.label5.Text = "your spreadsheet starting with the selected cell.";
            // 
            // lblInformationType
            // 
            this.lblInformationType.AutoSize = true;
            this.lblInformationType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInformationType.Location = new System.Drawing.Point(113, 14);
            this.lblInformationType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInformationType.Name = "lblInformationType";
            this.lblInformationType.Size = new System.Drawing.Size(176, 17);
            this.lblInformationType.TabIndex = 40;
            this.lblInformationType.Text = "<< Information Type >>";
            // 
            // OuraDateSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 205);
            this.Controls.Add(this.lblInformationType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkIncludeHeaders);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtmEndDate);
            this.Controls.Add(this.dtmStartDate);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "OuraDateSelectionForm";
            this.Text = "Oura Data Selection Range Form";
            this.Load += new System.EventHandler(this.OuraDateSelectionForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderApp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ErrorProvider errorProviderApp;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkIncludeHeaders;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtmEndDate;
        private System.Windows.Forms.DateTimePicker dtmStartDate;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ColumnHeader FieldName;
        private System.Windows.Forms.ColumnHeader Section;
        private System.Windows.Forms.Label lblInformationType;
    }
}