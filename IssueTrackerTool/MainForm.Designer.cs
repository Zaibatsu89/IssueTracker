namespace IssueTrackerTool
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.StatusLabel = new System.Windows.Forms.Label();
            this.StartTimerButton = new System.Windows.Forms.Button();
            this.StopTimerButton = new System.Windows.Forms.Button();
            this.Timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(13, 13);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(114, 13);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Status = special action";
            // 
            // StartTimerButton
            // 
            this.StartTimerButton.Location = new System.Drawing.Point(13, 30);
            this.StartTimerButton.Name = "StartTimerButton";
            this.StartTimerButton.Size = new System.Drawing.Size(100, 100);
            this.StartTimerButton.TabIndex = 1;
            this.StartTimerButton.Text = "Start timer (ETC)";
            this.StartTimerButton.UseVisualStyleBackColor = true;
            // 
            // StopTimerButton
            // 
            this.StopTimerButton.Location = new System.Drawing.Point(120, 30);
            this.StopTimerButton.Name = "StopTimerButton";
            this.StopTimerButton.Size = new System.Drawing.Size(100, 100);
            this.StopTimerButton.TabIndex = 2;
            this.StopTimerButton.Text = "Stop timer (ETC)";
            this.StopTimerButton.UseVisualStyleBackColor = true;
            // 
            // Timer
            // 
            this.Timer.Interval = 60000;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.StopTimerButton);
            this.Controls.Add(this.StartTimerButton);
            this.Controls.Add(this.StatusLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Issue Tracker Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Button StartTimerButton;
        private System.Windows.Forms.Button StopTimerButton;
        private System.Windows.Forms.Timer Timer;
    }
}

