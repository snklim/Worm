namespace Worm
{
    partial class Form1
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
            this.m_GameField = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_GameField)).BeginInit();
            this.SuspendLayout();
            // 
            // m_GameField
            // 
            this.m_GameField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_GameField.Location = new System.Drawing.Point(0, 0);
            this.m_GameField.Name = "m_GameField";
            this.m_GameField.Size = new System.Drawing.Size(671, 416);
            this.m_GameField.TabIndex = 0;
            this.m_GameField.TabStop = false;
            this.m_GameField.Paint += new System.Windows.Forms.PaintEventHandler(this.m_GameField_Paint);
            this.m_GameField.Resize += new System.EventHandler(this.m_GameField_Resize);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 416);
            this.Controls.Add(this.m_GameField);
            this.Name = "Form1";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.m_GameField)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox m_GameField;
    }
}

