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
            this.StartTimerKnop = new System.Windows.Forms.Button();
            this.StopTimerKnop = new System.Windows.Forms.Button();
            this.Timer = new System.Windows.Forms.Timer(this.components);
            this.ActieLabel = new System.Windows.Forms.Label();
            this.ActieInvoer = new System.Windows.Forms.TextBox();
            this.ActieKnop = new System.Windows.Forms.Button();
            this.TimerLabel = new System.Windows.Forms.Label();
            this.VoortgangLabel = new System.Windows.Forms.Label();
            this.ResetTimerKnop = new System.Windows.Forms.Button();
            this.AuditLogKnop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLabel.Location = new System.Drawing.Point(12, 9);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(122, 15);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Status: special action";
            // 
            // StartTimerKnop
            // 
            this.StartTimerKnop.BackColor = System.Drawing.Color.PaleGreen;
            this.StartTimerKnop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartTimerKnop.Location = new System.Drawing.Point(12, 85);
            this.StartTimerKnop.Name = "StartTimerKnop";
            this.StartTimerKnop.Size = new System.Drawing.Size(95, 45);
            this.StartTimerKnop.TabIndex = 2;
            this.StartTimerKnop.Text = "▶ START";
            this.StartTimerKnop.UseVisualStyleBackColor = false;
            this.StartTimerKnop.Click += new System.EventHandler(this.StartTimerKnop_Click);
            // 
            // StopTimerKnop
            // 
            this.StopTimerKnop.BackColor = System.Drawing.Color.LightCoral;
            this.StopTimerKnop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopTimerKnop.Location = new System.Drawing.Point(113, 85);
            this.StopTimerKnop.Name = "StopTimerKnop";
            this.StopTimerKnop.Size = new System.Drawing.Size(95, 45);
            this.StopTimerKnop.TabIndex = 3;
            this.StopTimerKnop.Text = "⏸ STOP";
            this.StopTimerKnop.UseVisualStyleBackColor = false;
            this.StopTimerKnop.Click += new System.EventHandler(this.StopTimerKnop_Click);
            // 
            // Timer
            // 
            this.Timer.Interval = 1000;
            this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // ActieLabel
            // 
            this.ActieLabel.AutoSize = true;
            this.ActieLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ActieLabel.Location = new System.Drawing.Point(12, 158);
            this.ActieLabel.Name = "ActieLabel";
            this.ActieLabel.Size = new System.Drawing.Size(162, 19);
            this.ActieLabel.TabIndex = 6;
            this.ActieLabel.Text = "Stap 1: Intake verwerken";
            // 
            // ActieInvoer
            // 
            this.ActieInvoer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ActieInvoer.Location = new System.Drawing.Point(12, 180);
            this.ActieInvoer.Multiline = true;
            this.ActieInvoer.Name = "ActieInvoer";
            this.ActieInvoer.Size = new System.Drawing.Size(297, 80);
            this.ActieInvoer.TabIndex = 7;
            this.ActieInvoer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ActieInvoer_KeyDown);
            // 
            // ActieKnop
            // 
            this.ActieKnop.BackColor = System.Drawing.Color.LightGreen;
            this.ActieKnop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ActieKnop.Location = new System.Drawing.Point(12, 266);
            this.ActieKnop.Name = "ActieKnop";
            this.ActieKnop.Size = new System.Drawing.Size(297, 35);
            this.ActieKnop.TabIndex = 8;
            this.ActieKnop.Text = "✓ VOLTOOI STAP";
            this.ActieKnop.UseVisualStyleBackColor = false;
            this.ActieKnop.Click += new System.EventHandler(this.ActieKnop_Click);
            // 
            // TimerLabel
            // 
            this.TimerLabel.Font = new System.Drawing.Font("Consolas", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TimerLabel.ForeColor = System.Drawing.Color.DarkGreen;
            this.TimerLabel.Location = new System.Drawing.Point(12, 32);
            this.TimerLabel.Name = "TimerLabel";
            this.TimerLabel.Size = new System.Drawing.Size(300, 45);
            this.TimerLabel.TabIndex = 1;
            this.TimerLabel.Text = "01:00";
            this.TimerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // VoortgangLabel
            // 
            this.VoortgangLabel.AutoSize = true;
            this.VoortgangLabel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VoortgangLabel.ForeColor = System.Drawing.Color.Gray;
            this.VoortgangLabel.Location = new System.Drawing.Point(12, 138);
            this.VoortgangLabel.Name = "VoortgangLabel";
            this.VoortgangLabel.Size = new System.Drawing.Size(84, 13);
            this.VoortgangLabel.TabIndex = 5;
            this.VoortgangLabel.Text = "Voortgang: 0/3";
            // 
            // ResetTimerKnop
            // 
            this.ResetTimerKnop.BackColor = System.Drawing.Color.LightBlue;
            this.ResetTimerKnop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResetTimerKnop.Location = new System.Drawing.Point(214, 85);
            this.ResetTimerKnop.Name = "ResetTimerKnop";
            this.ResetTimerKnop.Size = new System.Drawing.Size(95, 45);
            this.ResetTimerKnop.TabIndex = 4;
            this.ResetTimerKnop.Text = "↻ RESET";
            this.ResetTimerKnop.UseVisualStyleBackColor = false;
            this.ResetTimerKnop.Click += new System.EventHandler(this.ResetTimerKnop_Click);
            // 
            // AuditLogKnop
            // 
            this.AuditLogKnop.BackColor = System.Drawing.Color.WhiteSmoke;
            this.AuditLogKnop.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AuditLogKnop.Location = new System.Drawing.Point(12, 307);
            this.AuditLogKnop.Name = "AuditLogKnop";
            this.AuditLogKnop.Size = new System.Drawing.Size(297, 30);
            this.AuditLogKnop.TabIndex = 9;
            this.AuditLogKnop.Text = "📋 Bekijk Audit Log";
            this.AuditLogKnop.UseVisualStyleBackColor = false;
            this.AuditLogKnop.Click += new System.EventHandler(this.AuditLogKnop_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 350);
            this.Controls.Add(this.AuditLogKnop);
            this.Controls.Add(this.ResetTimerKnop);
            this.Controls.Add(this.VoortgangLabel);
            this.Controls.Add(this.TimerLabel);
            this.Controls.Add(this.ActieKnop);
            this.Controls.Add(this.ActieInvoer);
            this.Controls.Add(this.ActieLabel);
            this.Controls.Add(this.StopTimerKnop);
            this.Controls.Add(this.StartTimerKnop);
            this.Controls.Add(this.StatusLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Issue Tracker Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Button StartTimerKnop;
        private System.Windows.Forms.Button StopTimerKnop;
        private System.Windows.Forms.Timer Timer;
        private System.Windows.Forms.Label ActieLabel;
        private System.Windows.Forms.TextBox ActieInvoer;
        private System.Windows.Forms.Button ActieKnop;
        private System.Windows.Forms.Label TimerLabel;
        private System.Windows.Forms.Label VoortgangLabel;
        private System.Windows.Forms.Button ResetTimerKnop;
        private System.Windows.Forms.Button AuditLogKnop;
    }
}

