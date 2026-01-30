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
            this.ActieLabel = new System.Windows.Forms.Label();
            this.ActieInvoer = new System.Windows.Forms.TextBox();
            this.ActieKnop = new System.Windows.Forms.Button();
            this.TimerLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(12, 9);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(114, 13);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Status = special action";
            // 
            // StartTimerButton
            // 
            this.StartTimerButton.Location = new System.Drawing.Point(12, 70);
            this.StartTimerButton.Name = "StartTimerButton";
            this.StartTimerButton.Size = new System.Drawing.Size(100, 70);
            this.StartTimerButton.TabIndex = 1;
            this.StartTimerButton.Text = "Start timer (ETC)";
            this.StartTimerButton.UseVisualStyleBackColor = true;
            // 
            // StopTimerButton
            // 
            this.StopTimerButton.Location = new System.Drawing.Point(118, 70);
            this.StopTimerButton.Name = "StopTimerButton";
            this.StopTimerButton.Size = new System.Drawing.Size(100, 70);
            this.StopTimerButton.TabIndex = 2;
            this.StopTimerButton.Text = "Stop timer (ETC)";
            this.StopTimerButton.UseVisualStyleBackColor = true;
            // 
            // Timer
            // 
            this.Timer.Interval = 60000;
            // 
            // ActieLabel
            // 
            this.ActieLabel.AutoSize = true;
            this.ActieLabel.Location = new System.Drawing.Point(12, 143);
            this.ActieLabel.Name = "ActieLabel";
            this.ActieLabel.Size = new System.Drawing.Size(90, 13);
            this.ActieLabel.TabIndex = 3;
            this.ActieLabel.Text = "Intake verwerken";
            // 
            // ActieInvoer
            // 
            this.ActieInvoer.Location = new System.Drawing.Point(12, 159);
            this.ActieInvoer.Multiline = true;
            this.ActieInvoer.Name = "ActieInvoer";
            this.ActieInvoer.Size = new System.Drawing.Size(204, 70);
            this.ActieInvoer.TabIndex = 4;
            // 
            // ActieKnop
            // 
            this.ActieKnop.Location = new System.Drawing.Point(12, 235);
            this.ActieKnop.Name = "ActieKnop";
            this.ActieKnop.Size = new System.Drawing.Size(204, 23);
            this.ActieKnop.TabIndex = 5;
            this.ActieKnop.Text = "OK";
            this.ActieKnop.UseVisualStyleBackColor = true;
            // 
            // TimerLabel
            // 
            this.TimerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TimerLabel.Location = new System.Drawing.Point(12, 32);
            this.TimerLabel.Name = "TimerLabel";
            this.TimerLabel.Size = new System.Drawing.Size(204, 35);
            this.TimerLabel.TabIndex = 6;
            this.TimerLabel.Text = "00:00";
            this.TimerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TimerLabel);
            this.Controls.Add(this.ActieKnop);
            this.Controls.Add(this.ActieInvoer);
            this.Controls.Add(this.ActieLabel);
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
        private System.Windows.Forms.Label ActieLabel;
        private System.Windows.Forms.TextBox ActieInvoer;
        private System.Windows.Forms.Button ActieKnop;
        private System.Windows.Forms.Label TimerLabel;
    }
}

