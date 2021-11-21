
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    partial class OuraConfigurationForm
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
            this.btnOk = new System.Windows.Forms.Button();
            this.dtmStartDate = new System.Windows.Forms.DateTimePicker();
            this.dtmEndDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pImage = new System.Windows.Forms.PictureBox();
            this.chkIncludeHeaders = new System.Windows.Forms.CheckBox();
            this.Section = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FieldName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label5 = new System.Windows.Forms.Label();
            this.chkAll = new System.Windows.Forms.CheckBox();
            this.errorProviderApp = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnReinitialize = new System.Windows.Forms.Button();
            this.lvListView = new ExcelOuraVSTOAddIn.ListViewWithReordering();
            this.clmFieldName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmOuraSection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmCustomLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderApp)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(398, 442);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // dtmStartDate
            // 
            this.dtmStartDate.Location = new System.Drawing.Point(173, 81);
            this.dtmStartDate.Margin = new System.Windows.Forms.Padding(2);
            this.dtmStartDate.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dtmStartDate.Name = "dtmStartDate";
            this.dtmStartDate.Size = new System.Drawing.Size(189, 20);
            this.dtmStartDate.TabIndex = 1;
            // 
            // dtmEndDate
            // 
            this.dtmEndDate.Location = new System.Drawing.Point(173, 106);
            this.dtmEndDate.Margin = new System.Windows.Forms.Padding(2);
            this.dtmEndDate.Name = "dtmEndDate";
            this.dtmEndDate.Size = new System.Drawing.Size(189, 20);
            this.dtmEndDate.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(113, 81);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Date";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(113, 106);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "End Date";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(478, 442);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(93, 135);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(395, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "If you press OK, this will replace various columns and rows within your spreadshe" +
    "et";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(111, 22);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(273, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Select the information to insert into your Excel worksheet";
            // 
            // pImage
            // 
            this.pImage.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.OuraIcon;
            this.pImage.Location = new System.Drawing.Point(24, 22);
            this.pImage.Name = "pImage";
            this.pImage.Size = new System.Drawing.Size(56, 69);
            this.pImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pImage.TabIndex = 8;
            this.pImage.TabStop = false;
            // 
            // chkIncludeHeaders
            // 
            this.chkIncludeHeaders.AutoSize = true;
            this.chkIncludeHeaders.Location = new System.Drawing.Point(383, 81);
            this.chkIncludeHeaders.Name = "chkIncludeHeaders";
            this.chkIncludeHeaders.Size = new System.Drawing.Size(103, 17);
            this.chkIncludeHeaders.TabIndex = 4;
            this.chkIncludeHeaders.Text = "Display Headers";
            this.chkIncludeHeaders.UseVisualStyleBackColor = true;
            // 
            // Section
            // 
            this.Section.Text = "Section Area";
            // 
            // FieldName
            // 
            this.FieldName.Text = "Field Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(93, 148);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(146, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "starting with the selected cell.";
            // 
            // chkAll
            // 
            this.chkAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAll.AutoSize = true;
            this.chkAll.Location = new System.Drawing.Point(18, 444);
            this.chkAll.Name = "chkAll";
            this.chkAll.Size = new System.Drawing.Size(101, 17);
            this.chkAll.TabIndex = 6;
            this.chkAll.Text = "Check All Fields";
            this.chkAll.UseVisualStyleBackColor = true;
            this.chkAll.CheckedChanged += new System.EventHandler(this.chkAll_CheckedChanged);
            // 
            // errorProviderApp
            // 
            this.errorProviderApp.ContainerControl = this;
            // 
            // btnReinitialize
            // 
            this.btnReinitialize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReinitialize.Location = new System.Drawing.Point(125, 442);
            this.btnReinitialize.Name = "btnReinitialize";
            this.btnReinitialize.Size = new System.Drawing.Size(129, 23);
            this.btnReinitialize.TabIndex = 7;
            this.btnReinitialize.Text = "Reset Fields to Default";
            this.btnReinitialize.UseVisualStyleBackColor = true;
            this.btnReinitialize.Click += new System.EventHandler(this.btnReinitialize_Click);
            // 
            // lvListView
            // 
            this.lvListView.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.lvListView.AllowDrop = true;
            this.lvListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvListView.CheckBoxes = true;
            this.lvListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmFieldName,
            this.clmOuraSection,
            this.clmCustomLabel});
            this.lvListView.FullRowSelect = true;
            this.lvListView.GridLines = true;
            this.lvListView.HideSelection = false;
            this.lvListView.Location = new System.Drawing.Point(18, 169);
            this.lvListView.Name = "lvListView";
            this.lvListView.ShowGroups = false;
            this.lvListView.Size = new System.Drawing.Size(521, 267);
            this.lvListView.TabIndex = 5;
            this.lvListView.UseCompatibleStateImageBehavior = false;
            this.lvListView.View = System.Windows.Forms.View.Details;
            this.lvListView.DoubleClick += new System.EventHandler(this.lvListView_DoubleClick);
            // 
            // clmFieldName
            // 
            this.clmFieldName.Text = "Field Name";
            this.clmFieldName.Width = 200;
            // 
            // clmOuraSection
            // 
            this.clmOuraSection.Text = "Oura Section";
            this.clmOuraSection.Width = 100;
            // 
            // clmCustomLabel
            // 
            this.clmCustomLabel.Text = "Custom Label";
            this.clmCustomLabel.Width = 200;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(21, 135);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "WARNING:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(111, 35);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 13);
            this.label7.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(122, 39);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(290, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Change the order of one or more fields using drag and drop. ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(122, 54);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(346, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Double click a field to change the label displayed in the header in Excel.";
            // 
            // OuraConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(564, 473);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnReinitialize);
            this.Controls.Add(this.chkAll);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.chkIncludeHeaders);
            this.Controls.Add(this.pImage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtmEndDate);
            this.Controls.Add(this.dtmStartDate);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lvListView);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(580, 458);
            this.Name = "OuraConfigurationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Oura Configuration";
            this.Load += new System.EventHandler(this.OuraConfigurationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderApp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ListViewWithReordering lvListView;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.DateTimePicker dtmStartDate;
        private System.Windows.Forms.DateTimePicker dtmEndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pImage;
        private System.Windows.Forms.CheckBox chkIncludeHeaders;
        private System.Windows.Forms.ColumnHeader Section;
        private System.Windows.Forms.ColumnHeader FieldName;
        private ColumnHeader clmFieldName;
        private ColumnHeader clmOuraSection;
        public ColumnHeader clmCustomLabel;
        private Label label5;
        private CheckBox chkAll;
        private ErrorProvider errorProviderApp;
        private Button btnReinitialize;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
    }
}