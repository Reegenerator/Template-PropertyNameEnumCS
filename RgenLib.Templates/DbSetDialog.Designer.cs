namespace RgenLib.Templates {
    partial class DbSetDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.classesList = new System.Windows.Forms.CheckedListBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // classesList
            // 
            this.classesList.FormattingEnabled = true;
            this.classesList.Location = new System.Drawing.Point(12, 12);
            this.classesList.Name = "classesList";
            this.classesList.Size = new System.Drawing.Size(390, 259);
            this.classesList.TabIndex = 0;
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(287, 290);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(114, 24);
            this.generateButton.TabIndex = 1;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            // 
            // DbSetDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 322);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.classesList);
            this.Name = "DbSetDialog";
            this.Text = "DbSetDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox classesList;
        private System.Windows.Forms.Button generateButton;
    }
}