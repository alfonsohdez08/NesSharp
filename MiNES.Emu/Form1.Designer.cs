namespace MiNES.Emu
{
    partial class Form1
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
            this.GameScreen = new System.Windows.Forms.PictureBox();
            this.ManageEmulation = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.GameScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // GameScreen
            // 
            this.GameScreen.Location = new System.Drawing.Point(12, 12);
            this.GameScreen.Name = "GameScreen";
            this.GameScreen.Size = new System.Drawing.Size(551, 606);
            this.GameScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.GameScreen.TabIndex = 0;
            this.GameScreen.TabStop = false;
            // 
            // ManageEmulation
            // 
            this.ManageEmulation.Location = new System.Drawing.Point(616, 12);
            this.ManageEmulation.Name = "ManageEmulation";
            this.ManageEmulation.Size = new System.Drawing.Size(143, 27);
            this.ManageEmulation.TabIndex = 2;
            this.ManageEmulation.Text = "Stop Emulation";
            this.ManageEmulation.UseVisualStyleBackColor = true;
            this.ManageEmulation.Click += new System.EventHandler(this.button1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(765, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 27);
            this.button1.TabIndex = 3;
            this.button1.Text = "Nametable Debugger";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(616, 45);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(143, 24);
            this.button2.TabIndex = 4;
            this.button2.Text = "Background Tiles";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(765, 46);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(145, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Palettes";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 648);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ManageEmulation);
            this.Controls.Add(this.GameScreen);
            this.Name = "Form1";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.GameScreen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox GameScreen;
        private System.Windows.Forms.Button ManageEmulation;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

