using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IssueTrackerTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L62
            SetupWindow();
            // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L68
            StopTimerButton.Click += StopTimerButton_Click;
            // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L72
            StartTimerButton.Click += StartTimerButton_Click;
            // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.timer?view=netframework-4.7.2#examples
            Timer.Tick += Timer_Tick;
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L81
        private void SetupWindow()
        {
            Size = new Size(1000, 1000);
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.timer?view=netframework-4.7.2#examples
        private void Timer_Tick(object sender, EventArgs e)
        {
            Timer.Enabled = false;
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L136
        private void StartTimerButton_Click(object sender, EventArgs e)
        {
            Timer.Enabled = true;
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L523
        private void StopTimerButton_Click(object sender, EventArgs e)
        {
            Timer.Enabled = false;
        }
    }
}
