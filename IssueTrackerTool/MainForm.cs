using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace IssueTrackerTool
{
    public partial class MainForm : Form
    {
        #region Fields and Constants

        private const int STANDAARD_TIMER_SECONDEN = 60;
        private const string AUDIT_LOG_BESTANDSNAAM = "WitTronics_AuditLog.txt";

        // referentie: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays#single-dimensional-arrays
        private readonly string[] acties = new string[]
        {
            "Intake verwerken",
            "Oplossing implementeren",
            "Evalueer deliverables"
        };

        private int actieIndex = 0;
        private int resterendeSeconden = STANDAARD_TIMER_SECONDEN;
        // referentie: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/collections#indexable-collections
        private readonly List<AuditInvoer> auditTraject = new List<AuditInvoer>();
        private readonly object timerSlot = new object();

        #endregion

        #region Constructor and Initialization

        public MainForm()
        {
            InitializeComponent();
            InitialiseerAuditLog();
        }

        private void InitialiseerAuditLog()
        {
            try
            {
                string logPath = KrijgAuditLogPad();
                if (!File.Exists(logPath))
                {
                    File.WriteAllText(logPath, $"=== WitTronics Issue Tracker - Sessie Gestart: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==={Environment.NewLine}");
                }
                else
                {
                    File.AppendAllText(logPath, $"{Environment.NewLine}=== Nieuwe Sessie: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==={Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                ToonFout($"Kon audit log niet initialiseren: {ex.Message}");
            }
        }

        #endregion

        #region Timer Management
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
        private void StartTimerKnop_Click(object sender, EventArgs e)
        {
            lock (timerSlot)
            {
                Timer.Start();
                TimerLabel.ForeColor = Color.DarkGreen;
                LogActie("Timer gestart");
            }
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
        private void StopTimerKnop_Click(object sender, EventArgs e)
        {
            lock (timerSlot)
            {
                Timer.Stop();
                TimerLabel.ForeColor = Color.Black;
                LogActie("Timer gestopt");
            }
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
        private void ResetTimerKnop_Click(object sender, EventArgs e)
        {
            lock (timerSlot)
            {
                ResetTimer();
                LogActie("Timer gereset");
            }
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.timer?view=netframework-4.7.2#examples
        private void Timer_Tick(object sender, EventArgs e)
        {
            lock (timerSlot)
            {
                if (resterendeSeconden > 0)
                {
                    resterendeSeconden--;
                    UpdateTimerControl();

                    // Visuele feedback bij kritische drempel
                    if (resterendeSeconden <= 10)
                    {
                        TimerLabel.ForeColor = Color.Red;
                        if (resterendeSeconden == 10)
                        {
                            SystemSounds.Asterisk.Play();
                        }
                    }
                    else if (resterendeSeconden <= 30)
                    {
                        TimerLabel.ForeColor = Color.Orange;
                    }
                }
                else
                {
                    TimerLabel.ForeColor = Color.DarkRed;
                    SystemSounds.Exclamation.Play();
                    LogActie("Timer afgelopen - deadline bereikt");
                }
            }
        }

        private void ResetTimer()
        {
            resterendeSeconden = STANDAARD_TIMER_SECONDEN;
            UpdateTimerControl();
            TimerLabel.ForeColor = Color.DarkGreen;
        }

        private void UpdateTimerControl()
        {
            int minuten = resterendeSeconden / 60;
            int seconden = resterendeSeconden % 60;
            TimerLabel.Text = $"{minuten:D2}:{seconden:D2}";
        }

        #endregion

        #region Actie Management
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
        private void ActieKnop_Click(object sender, EventArgs e)
        {
            VerwerkActie();
        }

        private void ActieInvoer_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter toestaan om actie te verwerken (met Ctrl+Enter voor nieuwe regel)
            if (e.KeyCode == Keys.Enter && !e.Control)
            {
                e.SuppressKeyPress = true;
                VerwerkActie();
            }
        }

        private void VerwerkActie()
        {
            string actieInvoer = ActieInvoer.Text.Trim();

            // Valideer invoer
            if (string.IsNullOrWhiteSpace(actieInvoer))
            {
                ToonWaarschuwing("Voer details in voordat je de actie voltooit.");
                ActieInvoer.Focus();
                return;
            }

            // Maak audit invoer
            AuditInvoer auditInvoer = new AuditInvoer
            {
                Tijdstip = DateTime.Now,
                ActieNummer = actieIndex + 1,
                ActieNaam = acties[actieIndex],
                ActieDetails = actieInvoer,
                BestedeTijd = STANDAARD_TIMER_SECONDEN - resterendeSeconden
            };

            auditTraject.Add(auditInvoer);

            // Bewaar invoer
            BewaarAuditInvoer(auditInvoer);

            // Controleer of traject voltooid is
            if (actieIndex < (acties.Length - 1))
            {
                // Ga naar volgende actie
                actieIndex++;
                ResetTimer();
                Timer.Start();

                ToonInfo($"Actie {actieIndex} voltooid! Doorgaan naar actie {actieIndex + 1}.");
            }
            else
            {
                // Traject voltooid
                Timer.Stop();

                string samenvatting = string.Empty;
                if (auditTraject.Count == 0)
                    samenvatting = "Geen acties geregistreerd.";

                int totaleTijd = auditTraject.Sum(invoer =>
                {
                    return invoer.BestedeTijd;
                });

                samenvatting = $"Totale tijd: {(totaleTijd / 60)}m {(totaleTijd % 60)}s{Environment.NewLine}" +
                   $"Acties voltooid: {auditTraject.Count}";

                DialogResult resultaat = MessageBox.Show(
                    $"🏆 MISSIE VOLTOOID! 🏆{Environment.NewLine}{Environment.NewLine}" +
                    $"Alle traject-acties zijn afgerond.{Environment.NewLine}{Environment.NewLine}" +
                    samenvatting + Environment.NewLine + Environment.NewLine +
                    "Wil je de volledige audit log bekijken?",
                    "3-0 Overwinning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                LogActie("Traject voltooid - alle acties afgerond");

                if (resultaat == DialogResult.Yes)
                {
                    OpenAuditLog();
                }

                // Reset voor volgend traject
                actieIndex = 0;
                auditTraject.Clear();
                ResetTimer();
            }

            ActieLabel.Text = $"Stap {(actieIndex + 1)}: {acties[actieIndex]}";
            VoortgangLabel.Text = $"Voortgang: {actieIndex}/{acties.Length}";
            ActieInvoer.Clear();
        }

        #endregion

        #region Audit Traject Management

        private void BewaarAuditInvoer(AuditInvoer invoer)
        {
            try
            {
                string logPad = KrijgAuditLogPad();
                string logRegel = $"[{invoer.Tijdstip:yyyy-MM-dd HH:mm:ss}] " +
                   $"Stap {invoer.ActieNummer} - {invoer.ActieNaam}: " +
                   $"{invoer.ActieDetails} " +
                   $"(Tijd: {invoer.BestedeTijd}s)";

                File.AppendAllText(logPad, logRegel + Environment.NewLine);
            }
            catch (IOException ex)
            {
                ToonFout($"Kon audit invoer niet opslaan: {ex.Message}");
            }
        }

        private void LogActie(string actie)
        {
            try
            {
                string logPad = KrijgAuditLogPad();
                string logRegel = $"[{DateTime.Now:HH:mm:ss}] {actie}";
                File.AppendAllText(logPad, logRegel + Environment.NewLine);
            }
            catch
            {
                // Stil falen voor niet-kritieke logging
            }
        }

        private string KrijgAuditLogPad()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AUDIT_LOG_BESTANDSNAAM);
        }
        // referentie: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.click?view=netframework-4.7.2#examples
        private void AuditLogKnop_Click(object sender, EventArgs e)
        {
            OpenAuditLog();
        }

        private void OpenAuditLog()
        {
            try
            {
                string logPad = KrijgAuditLogPad();

                if (!File.Exists(logPad))
                {
                    ToonWaarschuwing("Geen audit log gevonden. Voltooi eerst een traject-actie.");
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = logPad,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                ToonFout($"Kon audit log niet openen: {ex.Message}");
            }
        }

        #endregion

        #region UI Helper Methods

        private void ToonInfo(string bericht)
        {
            MessageBox.Show(bericht, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToonWaarschuwing(string bericht)
        {
            MessageBox.Show(bericht, "Waarschuwing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ToonFout(string bericht)
        {
            MessageBox.Show(bericht, "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region Cleanup

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Opruimen en eindlogging
            if (auditTraject.Count != 0)
            {
                DialogResult resultaat = MessageBox.Show(
                    "Er zijn nog niet-voltooide traject-acties. Weet je zeker dat je wilt afsluiten?",
                    "Bevestig afsluiten",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultaat == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            LogActie("Applicatie afgesloten");
        }

        #endregion

        #region Nested Classes

        private class AuditInvoer
        {
            public DateTime Tijdstip { get; set; }
            public int ActieNummer { get; set; }
            public string ActieNaam { get; set; }
            public string ActieDetails { get; set; }
            public int BestedeTijd { get; set; } // in seconden
        }

        #endregion
    }
}
