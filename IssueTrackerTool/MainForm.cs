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
        // referentie: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays#single-dimensional-arrays
        private readonly string[] acties = new string[]
        {
            "Intake verwerken",
            "Oplossing implementeren",
            "Evalueer of de deliverables correct en volledig zijn afgerond"
        };

        private uint actieIndex = 0;
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L46
        private string actieInvoer = string.Empty;

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
            // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
            ActieKnop.Click += ActieKnop_Click;
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L81
        private void SetupWindow()
        {
            Size = new Size(1000, 1000);
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
        private void ActieKnop_Click(object sender, EventArgs e)
        {
            actieInvoer = ActieInvoer.Text; // TODO: oude actieInvoer verdwijnt
            if (actieIndex < (acties.Length - 1))
                ActieLabel.Text = acties[++actieIndex];
            ActieInvoer.Text = string.Empty;
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.timer?view=netframework-4.7.2#examples
        private void Timer_Tick(object sender, EventArgs e)
        {
            TimerLabel.Text = "00:00"; // TODO: tekst op basis van 1 minuut
            Timer.Enabled = false;
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L136
        private void StartTimerButton_Click(object sender, EventArgs e)
        {
            TimerLabel.Text = "00:01"; // TODO: tekst op basis van 1 minuut
            Timer.Enabled = true;
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L523
        private void StopTimerButton_Click(object sender, EventArgs e)
        {
            TimerLabel.Text = "00:01"; // TODO: tekst op basis van 1 minuut
            Timer.Enabled = false;
        }
    }
}
