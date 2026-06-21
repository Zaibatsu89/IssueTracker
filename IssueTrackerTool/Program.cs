using System;
using System.Windows.Forms;

namespace IssueTrackerTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 3 && args[0] == "--test-generate")
            {
                string jiraIssue = args[1];
                string outputPath = args[2];
                try
                {
                    Console.WriteLine($"Starting headless generation for {jiraIssue} to {outputPath}...");
                    DocumentGenerator.Generate(jiraIssue, outputPath);
                    Console.WriteLine("Headless generation SUCCEEDED!");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Headless generation FAILED: {ex.Message}");
                    Environment.Exit(1);
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}