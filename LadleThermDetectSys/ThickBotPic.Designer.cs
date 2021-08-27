namespace LadleThermDetectSys
{
    partial class ThickBotPic
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
            this.picBxThickBotPic = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBxThickBotPic)).BeginInit();
            this.SuspendLayout();
            // 
            // picBxThickBotPic
            // 
            this.picBxThickBotPic.Location = new System.Drawing.Point(0, 0);
            this.picBxThickBotPic.Name = "picBxThickBotPic";
            this.picBxThickBotPic.Size = new System.Drawing.Size(750, 1000);
            this.picBxThickBotPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBxThickBotPic.TabIndex = 0;
            this.picBxThickBotPic.TabStop = false;
            this.picBxThickBotPic.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.picBxThickBotPic_MouseWheel);
            // 
            // ThickBotPic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 1001);
            this.Controls.Add(this.picBxThickBotPic);
            this.Name = "ThickBotPic";
            this.Text = "ThickBotPic";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThickBotPic_FormClosing);
            this.Load += new System.EventHandler(this.ThickBotPic_Load);
            this.SizeChanged += new System.EventHandler(this.ThickBotPic_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.picBxThickBotPic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picBxThickBotPic;
    }
}