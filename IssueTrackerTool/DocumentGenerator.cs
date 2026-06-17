using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace IssueTrackerTool
{
    public static class DocumentGenerator
    {
        private static readonly string[] Emojis = { "⏱️", "🫱🏻‍🫲🏻", "🔬", "✏️", "⌨️", "🔎", "⚠️", "⚙️" };

        public class PhaseInfo
        {
            public int Number { get; set; }
            public string Name { get; set; }
            public string DrawioFilename { get; set; }
            public string Emoji { get; set; }
            public string SectionKeyword { get; set; }
        }

        public class PhaseSection
        {
            public string Heading { get; set; }
            public List<string> Actions { get; set; } = new List<string>();
        }

        public class CheckGroup
        {
            public string Heading { get; set; }
            public List<string> Checks { get; set; } = new List<string>();
            public string SubPhase { get; set; } = string.Empty;
        }

        private static readonly List<PhaseInfo> Phases = new List<PhaseInfo>
        {
            new PhaseInfo { Number = 1, Name = "Intake", DrawioFilename = "1 Issue Tracker intake.drawio", Emoji = "⏱️", SectionKeyword = "intake" },
            new PhaseInfo { Number = 2, Name = "Review aanpak", DrawioFilename = "2 Issue Tracker review aanpak.drawio", Emoji = "🫱🏻‍🫲🏻", SectionKeyword = "review aanpak" },
            new PhaseInfo { Number = 3, Name = "Analyse", DrawioFilename = "3 Issue Tracker analyse.drawio", Emoji = "🔬", SectionKeyword = "analyse" },
            new PhaseInfo { Number = 4, Name = "Review analyse", DrawioFilename = "4 Issue Tracker review analyse.drawio", Emoji = "🫱🏻‍🫲🏻", SectionKeyword = "review analyse" },
            new PhaseInfo { Number = 5, Name = "Ontwerp", DrawioFilename = "5 Issue Tracker ontwerp.drawio", Emoji = "✏️", SectionKeyword = "Ontwerp" },
            new PhaseInfo { Number = 6, Name = "Review ontwerp", DrawioFilename = "6 Issue Tracker review ontwerp.drawio", Emoji = "🫱🏻‍🫲🏻", SectionKeyword = "review ontwerp" },
            new PhaseInfo { Number = 7, Name = "Implementatie", DrawioFilename = "7 Issue Tracker implementatie.drawio", Emoji = "⌨️", SectionKeyword = "Implementatie" },
            new PhaseInfo { Number = 8, Name = "Test", DrawioFilename = "8 Issue Tracker test.drawio", Emoji = "🔎", SectionKeyword = "Test" },
            new PhaseInfo { Number = 9, Name = "Meldplicht", DrawioFilename = "9 Issje Tracker meldplicht.drawio", Emoji = "⚠️", SectionKeyword = "meldplicht" },
            new PhaseInfo { Number = 10, Name = "Review final", DrawioFilename = "10 Issue Tracker review final.drawio", Emoji = "🫱🏻‍🫲🏻", SectionKeyword = "Review Final" },
            new PhaseInfo { Number = 11, Name = "Special action", DrawioFilename = "11 Issue Tracker special action.drawio", Emoji = "⚙️", SectionKeyword = "special action" },
            new PhaseInfo { Number = 12, Name = "Review special action", DrawioFilename = "12 Issue Tracker review special action.drawio", Emoji = "🫱🏻‍🫲🏻", SectionKeyword = "review special action" }
        };

        public static void Generate(string jiraIssue, string outputPath)
        {
            string projectRoot = GetProjectRoot();
            string actielijstPath = Path.Combine(projectRoot, "Actielijst.docx");
            string checklijstPath = Path.Combine(projectRoot, "Checklijst.docx");

            if (!File.Exists(actielijstPath))
                throw new FileNotFoundException("Kan Actielijst.docx niet vinden in de hoofdmap.", actielijstPath);

            if (!File.Exists(checklijstPath))
                throw new FileNotFoundException("Kan Checklijst.docx niet vinden in de hoofdmap.", checklijstPath);

            // Step 1: Read source document paragraphs
            var actielijstParas = ReadParagraphs(actielijstPath);
            var checklijstParas = ReadParagraphs(checklijstPath);

            // Step 2: Parse structures
            var actieSections = ParseActielijst(actielijstParas);
            var checkGroups = ParseChecklijst(checklijstParas);

            // Step 3: Create Output Word Document
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Title
                Paragraph titleP = body.AppendChild(new Paragraph());
                ParagraphProperties titlePPr = titleP.AppendChild(new ParagraphProperties());
                titlePPr.AppendChild(new Justification { Val = JustificationValues.Center });
                titlePPr.AppendChild(new SpacingBetweenLines { After = "360" });

                Run titleR = titleP.AppendChild(new Run());
                RunProperties titleRPr = titleR.AppendChild(new RunProperties());
                titleRPr.AppendChild(new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" });
                titleRPr.AppendChild(new FontSize { Val = "44" }); // 22pt
                titleRPr.AppendChild(new Bold());
                titleRPr.AppendChild(new Color { Val = "1F4E78" });
                titleR.AppendChild(new Text($"WitTronics Process Rapport - Jira-issue: {jiraIssue}"));

                // Meta Info
                Paragraph metaP = body.AppendChild(new Paragraph());
                ParagraphProperties metaPPr = metaP.AppendChild(new ParagraphProperties());
                metaPPr.AppendChild(new Justification { Val = JustificationValues.Center });
                metaPPr.AppendChild(new SpacingBetweenLines { After = "480" });

                Run metaR = metaP.AppendChild(new Run());
                RunProperties metaRPr = metaR.AppendChild(new RunProperties());
                metaRPr.AppendChild(new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" });
                metaRPr.AppendChild(new FontSize { Val = "22" }); // 11pt
                metaRPr.AppendChild(new Italic());
                metaRPr.AppendChild(new Color { Val = "595959" });
                metaR.AppendChild(new Text($"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss} | Status: Actief"));

                // Write phases
                foreach (var phase in Phases)
                {
                    AddHeading1(body, $"Fase {phase.Number}: {phase.Name}", "1F4E78");

                    // A. Extract and write Draw.io texts and mappings
                    string drawioPath = Path.Combine(projectRoot, phase.DrawioFilename);
                    var drawioMappings = ParseDrawioActionMappings(drawioPath);

                    // Fallback: If no action mappings exist, print all flowchart elements in bulk at the top
                    if (drawioMappings.Count == 0)
                    {
                        var drawioTexts = ParseDrawioText(drawioPath);
                        if (drawioTexts.Count > 0)
                        {
                            AddHeading2(body, "Flowchart Elementen (Draw.io)", "7F7F7F");
                            foreach (var txt in drawioTexts)
                            {
                                AddParagraph(body, $"• {txt}", italic: true, colorHex: "595959", leftIndent: 360);
                            }
                        }
                    }

                    // B. Extract and write Actions and matching Checks
                    PhaseSection matchingSection = null;
                    foreach (var s in actieSections)
                    {
                        if (s.Heading.ToLowerInvariant().Contains(phase.SectionKeyword.ToLowerInvariant()))
                        {
                            matchingSection = s;
                            break;
                        }
                    }

                    // Re-use standard review template if specific review isn't found
                    if (matchingSection == null && phase.Emoji == "🫱🏻‍🫲🏻")
                    {
                        foreach (var s in actieSections)
                        {
                            if (s.Heading.ToLowerInvariant().Contains("review") && !s.Heading.ToLowerInvariant().Contains("final"))
                            {
                                matchingSection = s;
                                break;
                            }
                        }
                    }

                    if (matchingSection != null)
                    {
                                                bool isFallback = !matchingSection.Heading.Contains(phase.Number.ToString() + "0") && 
                                           !matchingSection.Heading.Contains(phase.Number.ToString());
                         
                        string headingToUse = matchingSection.Heading;
                        if (isFallback)
                        {
                            string oldPrefix = matchingSection.Heading.Contains("201") ? "20" : 
                                               matchingSection.Heading.Contains("401") ? "40" : "60";
                            string newPrefix = phase.Number.ToString() + "0";
                             
                            headingToUse = headingToUse.Replace(oldPrefix, newPrefix)
                                                      .Replace("review aanpak", phase.SectionKeyword.ToLowerInvariant())
                                                      .Replace("review analyse", phase.SectionKeyword.ToLowerInvariant())
                                                      .Replace("review ontwerp", phase.SectionKeyword.ToLowerInvariant());
                        }

                        AddHeading2(body, headingToUse, "2E75B6");

                        foreach (var action in matchingSection.Actions)
                        {
                            string actionToUse = action;
                            if (isFallback)
                            {
                                string oldPrefix = matchingSection.Heading.Contains("201") ? "20" : 
                                                   matchingSection.Heading.Contains("401") ? "40" : "60";
                                string newPrefix = phase.Number.ToString() + "0";
                                actionToUse = actionToUse.Replace(oldPrefix, newPrefix);
                            }

                            bool isSubHeader = false;
                            foreach (var em in Emojis)
                            {
                                if (actionToUse.StartsWith(em, StringComparison.Ordinal))
                                {
                                    isSubHeader = true;
                                    break;
                                }
                            }

                            if (isSubHeader)
                            {
                                AddParagraph(body, actionToUse, bold: true, colorHex: "1F497D", leftIndent: 180);

                                var checks = FindChecksForAction(actionToUse, checkGroups, phase.Emoji, phase.SectionKeyword);
                                if (checks.Count > 0)
                                {
                                    foreach (var check in checks)
                                    {
                                        AddBullet(body, check, indent: 540);
                                    }
                                }
                            }
                            else
                            {
                                AddParagraph(body, actionToUse, leftIndent: 360);

                                var checks = FindChecksForAction(actionToUse, checkGroups, phase.Emoji, phase.SectionKeyword);
                                if (checks.Count > 0)
                                {
                                    foreach (var check in checks)
                                    {
                                        AddBullet(body, check, indent: 720);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AddHeading2(body, "Acties & Checklijst", "2E75B6");
                        AddParagraph(body, "Geen specifieke acties of checklists gedefinieerd voor deze fase.", italic: true, colorHex: "7F7F7F", leftIndent: 360);
                    }

                    // Spacer between phases
                    body.AppendChild(new Paragraph(new Run(new Text(""))));
                }

                mainPart.Document.Save();
            }
        }

        private static string GetProjectRoot()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string current = baseDir;
            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "Actielijst.docx")))
                {
                    return current;
                }
                string parent = Path.GetDirectoryName(current);
                if (parent == current) break;
                current = parent;
            }
            return baseDir;
        }

        private static List<string> ReadParagraphs(string filePath)
        {
            var list = new List<string>();
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                var body = doc.MainDocumentPart.Document.Body;
                foreach (var p in body.Descendants<Paragraph>())
                {
                    var sb = new StringBuilder();
                    foreach (var text in p.Descendants<Text>())
                    {
                        sb.Append(text.Text);
                    }
                    string txt = sb.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(txt))
                    {
                        list.Add(txt);
                    }
                }
            }
            return list;
        }

        private static List<PhaseSection> ParseActielijst(List<string> paragraphs)
        {
            var sections = new List<PhaseSection>();
            PhaseSection currentSection = null;

            foreach (var p in paragraphs)
            {
                bool isHeading = false;
                foreach (var em in Emojis)
                {
                    if (p.StartsWith(em, StringComparison.Ordinal))
                    {
                        if (p.Contains("Status =") || p == "✏️ Ontwerp" || p == "⌨️ Implementatie" || p == "🔎 Test" || p == "🫱🏻‍🫲🏻 Review Final" || p == "⚠️ Meldplicht" || p == "⚙️ Special action")
                        {
                            isHeading = true;
                            break;
                        }
                    }
                }

                if (isHeading)
                {
                    currentSection = new PhaseSection { Heading = p };
                    sections.Add(currentSection);
                }
                else if (currentSection != null)
                {
                    currentSection.Actions.Add(p);
                }
            }
            return sections;
        }

        private static List<CheckGroup> ParseChecklijst(List<string> paragraphs)
        {
            var list = new List<CheckGroup>();
            CheckGroup currentGroup = null;
            string currentSubPhase = "review aanpak"; // Initial default for review check groups

            foreach (var p in paragraphs)
            {
                bool isActionRef = false;
                foreach (var em in Emojis)
                {
                    if (p.StartsWith(em, StringComparison.Ordinal))
                    {
                        isActionRef = true;
                        break;
                    }
                }

                if (isActionRef)
                {
                    // Dynamically track the review sub-phase based on action codes or action heading text!
                    if (p.Contains("40") || p.ToLowerInvariant().Contains("review analyse"))
                    {
                        currentSubPhase = "review analyse";
                    }
                    else if (p.Contains("60") || p.ToLowerInvariant().Contains("review ontwerp"))
                    {
                        currentSubPhase = "review ontwerp";
                    }
                    else if (p.Contains("100") || p.ToLowerInvariant().Contains("review final"))
                    {
                        currentSubPhase = "review final";
                    }
                    else if (p.Contains("20") || p.ToLowerInvariant().Contains("review aanpak"))
                    {
                        currentSubPhase = "review aanpak";
                    }

                    currentGroup = new CheckGroup
                    {
                        Heading = p,
                        SubPhase = p.StartsWith("🫱🏻‍🫲🏻", StringComparison.Ordinal) ? currentSubPhase : string.Empty
                    };
                    list.Add(currentGroup);
                }
                else if (currentGroup != null)
                {
                    currentGroup.Checks.Add(p);
                }
            }
            return list;
        }

        private static string Normalize(string text)
        {
            if (text == null) return string.Empty;

            // Remove leading emojis/symbols, then leading numbers/letters
            string result = Regex.Replace(text, @"^[^\w\s\d]+", "").Trim();
            result = Regex.Replace(result, @"^\s*\d+[\.\s\-]*", "");
            result = Regex.Replace(result, @"^\s*[a-z][\.\s\-]+", "");

            var sb = new StringBuilder();
            foreach (char c in result)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }
            return sb.ToString();
        }

        private static List<string> FindChecksForAction(string action, List<CheckGroup> checkGroups, string phaseEmoji, string sectionKeyword)
        {
            string normAction = Normalize(action);
            if (string.IsNullOrEmpty(normAction)) return new List<string>();

            foreach (var cg in checkGroups)
            {
                // 1. Must match the phase emoji prefix
                if (!cg.Heading.StartsWith(phaseEmoji, StringComparison.Ordinal)) continue;

                // 2. If it's a review phase, must match the specific review sub-phase keyword!
                if (phaseEmoji == "🫱🏻‍🫲🏻")
                {
                    if (!string.IsNullOrEmpty(cg.SubPhase) && 
                        !string.IsNullOrEmpty(sectionKeyword) && 
                        !cg.SubPhase.ToLowerInvariant().Contains(sectionKeyword.ToLowerInvariant()) &&
                        !sectionKeyword.ToLowerInvariant().Contains(cg.SubPhase.ToLowerInvariant()))
                    {
                        continue;
                    }
                }

                string normRef = Normalize(cg.Heading);
                if (normRef == normAction || (normAction.Length > 10 && (normRef.Contains(normAction) || normAction.Contains(normRef))))
                {
                    return cg.Checks;
                }
            }
            return new List<string>();
        }

        private static List<string> ParseDrawioText(string filePath)
        {
            var list = new List<string>();
            if (!File.Exists(filePath)) return list;

            try
            {
                var doc = XDocument.Load(filePath);
                foreach (var cell in doc.Descendants().Where(e => e.Name.LocalName == "mxCell"))
                {
                    string val = (string)cell.Attribute("value");
                    if (!string.IsNullOrEmpty(val))
                    {
                        string decoded = WebUtility.HtmlDecode(val);
                        string clean = Regex.Replace(decoded, "<[^<]+?>", " ");
                        clean = Regex.Replace(clean, @"\s+", " ").Trim();
                        if (clean.Length > 2 && !list.Contains(clean))
                        {
                            list.Add(clean);
                        }
                    }
                }
            }
            catch
            {
                // Ignore read failures for robust generation
            }
            return list;
        }

        private static Dictionary<string, List<string>> ParseDrawioActionMappings(string filePath)
        {
            var mappings = new Dictionary<string, List<string>>();
            if (!File.Exists(filePath)) return mappings;

            try
            {
                var doc = XDocument.Load(filePath);
                foreach (var obj in doc.Descendants().Where(e => e.Name.LocalName == "object"))
                {
                    string action = (string)obj.Attribute("Action");
                    string label = (string)obj.Attribute("label");

                    if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(label))
                    {
                        string decoded = WebUtility.HtmlDecode(label);
                        string clean = Regex.Replace(decoded, "<[^<]+?>", " ");
                        clean = Regex.Replace(clean, @"\s+", " ").Trim();

                        if (clean.Length > 0)
                        {
                            if (!mappings.ContainsKey(action))
                            {
                                mappings[action] = new List<string>();
                            }
                            if (!mappings[action].Contains(clean))
                            {
                                mappings[action].Add(clean);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore read errors
            }
            return mappings;
        }

        private static List<string> FindActionCodesForHeading(string heading, Dictionary<string, List<string>> drawioMappings)
        {
            var codes = new List<string>();
            if (string.IsNullOrEmpty(heading)) return codes;

            // 1. Prioritize explicit action code word match (e.g. 101, 102, 201, 207T, 312F)
            var match = Regex.Match(heading, @"\b([1-9]\d{2}[A-Z]?)\b");
            if (match.Success)
            {
                string code = match.Groups[1].Value;
                if (drawioMappings.ContainsKey(code))
                {
                    codes.Add(code);
                    return codes;
                }
            }

            string normHeading = Normalize(heading);

            foreach (var kvp in drawioMappings)
            {
                foreach (var label in kvp.Value)
                {
                    if (string.IsNullOrEmpty(label)) continue;

                    // If the label is a pure emoji/symbol (like ⏱️), do NOT match if the heading contains a numeric code that doesn't match this key
                    if (label.Length <= 3 && Regex.IsMatch(label, @"[^\x00-\x7F]"))
                    {
                        if (Regex.IsMatch(heading, @"\b([1-9]\d{2}[A-Z]?)\b"))
                        {
                            continue;
                        }
                    }

                    // 2. Raw exact or substring match (good for emojis like ⏱️)
                    if (heading.Contains(label) || label.Contains(heading))
                    {
                        if (!codes.Contains(kvp.Key))
                        {
                            codes.Add(kvp.Key);
                        }
                        continue;
                    }

                    // 3. Normalized match (ignores casing, whitespace, punctuation, accents)
                    string normLabel = Normalize(label);
                    if (!string.IsNullOrEmpty(normLabel))
                    {
                        if (normHeading.Contains(normLabel) || normLabel.Contains(normHeading))
                        {
                            if (!codes.Contains(kvp.Key))
                            {
                                codes.Add(kvp.Key);
                            }
                        }
                    }
                }
            }
            return codes;
        }

        private static void AddHeading1(Body body, string text, string colorHex = "1F4E78")
        {
            Paragraph p = body.AppendChild(new Paragraph());
            ParagraphProperties pPr = new ParagraphProperties(
                new SpacingBetweenLines { Before = "240", After = "120" },
                new OutlineLevel { Val = 0 }
            );
            p.AppendChild(pPr);

            Run r = p.AppendChild(new Run());
            RunProperties rPr = new RunProperties(
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" },
                new FontSize { Val = "32" },
                new Bold(),
                new Color { Val = colorHex }
            );
            r.AppendChild(rPr);
            r.AppendChild(new Text(text));
        }

        private static void AddHeading2(Body body, string text, string colorHex = "2E75B6")
        {
            Paragraph p = body.AppendChild(new Paragraph());
            ParagraphProperties pPr = new ParagraphProperties(
                new SpacingBetweenLines { Before = "180", After = "60" },
                new OutlineLevel { Val = 1 }
            );
            p.AppendChild(pPr);

            Run r = p.AppendChild(new Run());
            RunProperties rPr = new RunProperties(
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" },
                new FontSize { Val = "26" },
                new Bold(),
                new Color { Val = colorHex }
            );
            r.AppendChild(rPr);
            r.AppendChild(new Text(text));
        }

        private static void AddParagraph(Body body, string text, bool bold = false, bool italic = false, string colorHex = "000000", int leftIndent = 0)
        {
            Paragraph p = body.AppendChild(new Paragraph());
            ParagraphProperties pPr = new ParagraphProperties(
                new SpacingBetweenLines { After = "120" }
            );
            if (leftIndent > 0)
            {
                pPr.Append(new Indentation { Left = leftIndent.ToString() });
            }
            p.AppendChild(pPr);

            Run r = p.AppendChild(new Run());
            var rPr = new RunProperties(
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" },
                new FontSize { Val = "22" },
                new Color { Val = colorHex }
            );
            if (bold) rPr.Append(new Bold());
            if (italic) rPr.Append(new Italic());
            r.AppendChild(rPr);
            r.AppendChild(new Text(text));
        }

        private static void AddBullet(Body body, string text, int indent = 720)
        {
            Paragraph p = body.AppendChild(new Paragraph());
            ParagraphProperties pPr = new ParagraphProperties(
                new SpacingBetweenLines { After = "60" },
                new Indentation { Left = indent.ToString(), Hanging = "360" }
            );
            p.AppendChild(pPr);

            Run bulletRun = p.AppendChild(new Run());
            RunProperties bulletPr = new RunProperties(
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" },
                new FontSize { Val = "22" }
            );
            bulletRun.AppendChild(bulletPr);
            bulletRun.AppendChild(new Text("☐  "));

            Run textRun = p.AppendChild(new Run());
            RunProperties textPr = new RunProperties(
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" },
                new FontSize { Val = "22" }
            );
            textRun.AppendChild(textPr);
            textRun.AppendChild(new Text(text));
        }

        public static void PatchSourceDocuments()
        {
            string projectRoot = GetProjectRoot();
            string actielijstPath = Path.Combine(projectRoot, "Actielijst.docx");
            string checklijstPath = Path.Combine(projectRoot, "Checklijst.docx");

            // Copy pristine files from template files in the project root to ensure clean state
            string templateActielijst = Path.Combine(projectRoot, "Actielijst_Template.docx");
            string templateChecklijst = Path.Combine(projectRoot, "Checklijst_Template.docx");
            if (File.Exists(templateActielijst)) File.Copy(templateActielijst, actielijstPath, true);
            if (File.Exists(templateChecklijst)) File.Copy(templateChecklijst, checklijstPath, true);

            var phase2Mappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 A. Status = review aanpak", "🫱🏻‍🫲🏻 201. Status = review aanpak" },
                { "🫱🏻‍🫲🏻 B. Is de baas aanwezig?", "🫱🏻‍🫲🏻 202. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 C. Meteen meldplicht wanneer de baas aanwezig is", "🫱🏻‍🫲🏻 202F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 D. Na dat de baas weer aanwezig is", "🫱🏻‍🫲🏻 202T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 E. Bespreek oplossingsrichting, deliverables en ETC’s", "🫱🏻‍🫲🏻 204. Bespreek oplossingsrichting, deliverables en ETC’s" },
                { "🫱🏻‍🫲🏻 F. Goedkeuring?", "🫱🏻‍🫲🏻 206. Goedkeuring?" },
                { "🫱🏻‍🫲🏻 G. De baas bepaald volgende status", "🫱🏻‍🫲🏻 206F. De baas bepaald volgende status" },
                { "🫱🏻‍🫲🏻 H. Volg de analyse", "🫱🏻‍🫲🏻 206T. Volg de analyse" }
            };

            var phase3Mappings = new Dictionary<string, string>
            {
                { "🔬 A. Status = analyse", "🔬 301. Status = analyse" },
                { "A. Aanleiding of probleem (BUGS) helder?", "🔬 302. Start timer (ETC)" },
                { "B. Is het issue een Bug? (Bug(s) reproduceren!)", "🔬 303. Intake verwerken" },
                { "C. Mogelijke oplossingen bedenken (minimaal 2, bestaande templates gebruiken!)", "🔬 304. Oplossing analyseren" },
                { "D. Beste oplossing kiezen (KISS), onderbouwen waarom deze de voorkeur heeft", "🔬 305. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "E. Evaluatie eerste acties:", "🔬 306. Stop timer (ETC)" },
                { "a. Hoe lang denk je nog nodig te hebben?", "🔬 307. Bijzonderheden gezien?" },
                { "b. Past het binnen de 150%? Past het binnen de tijdtrigger? Zo ja, ga door", "🔬 307T. Volg de review analyse" },
                { "F. Evaluatie laatste acties:", "🔬 312. Haal ik het?" },
                { "a. Hoe heeft het zo lang kunnen duren?", "🔬 313. Hoe heeft het zo lang kunnen duren?" },
                { "b. Zijn er snellere methodieken mogelijk?", "🔬 314. Stop timer (4 min)" },
                { "c. Heb ik mijn antwoorden getest voordat ik JW spreek?", "🔬 315. Significant inzicht gekregen" },
                { "🔬 308. Na 2/3 ETC Na 100% ETC Na 150% ETC", "🔬 313. Hoe heeft het zo lang kunnen duren?" },
                { "🔬 309. Start timer (1 min)", "🔬 314. Stop timer (4 min)" },
                { "🔬 310. Hoe lang denk je nog nodig te hebben?", "🔬 315. Significant inzicht gekregen" }
            };

            var phase4Mappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 A. Status = review analyse", "🫱🏻‍🫲🏻 401. Status = review analyse" },
                { "🫱🏻‍🫲🏻 B. Is de baas aanwezig?", "🫱🏻‍🫲🏻 402. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 C. Meteen meldplicht wanneer de baas aanwezig is", "🫱🏻‍🫲🏻 402F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 D. Na dat de baas weer aanwezig is", "🫱🏻‍🫲🏻 402T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 E. Overleg resultaten voor goedkeuring vervolg", "🫱🏻‍🫲🏻 404. Overleg resultaten voor goedkeuring vervolg" },
                { "🫱🏻‍🫲🏻 F. Goedkeuring?", "🫱🏻‍🫲🏻 406. Goedkeuring?" },
                { "🫱🏻‍🫲🏻 G. De baas bepaald volgende status", "🫱🏻‍🫲🏻 406F. De baas bepaald volgende status en verifieert effort per resterende fase" },
                { "🫱🏻‍🫲🏻 H. Volg het ontwerp", "🫱🏻‍🫲🏻 406T. Volg het ontwerp" }
            };

            var phase5Mappings = new Dictionary<string, string>
            {
                { "✏️ Ontwerp", "✏️ 501. Status = ontwerp" },
                { "A. Concept gekozen, eventueel met pseudocode of direct implementatie", "✏️ 502. Start timer (ETC)" },
                { "B. Implementatie van het gekozen template (op de kop af?)", "✏️ 503. Analyse verwerken" },
                { "C. Confidence level vastgesteld", "✏️ 504. Oplossing ontwerpen" },
                { "D. Is de pseudocode juist?", "✏️ 505. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "E. Zit alles op de juiste plek? Zo niet, bespreken en terug!", "✏️ 506. Stop timer (ETC)" },
                { "F. Testplan voorgesteld", "✏️ 507. Bijzonderheden gezien?" },
                { "G. Evaluatie eerste acties:", "✏️ 508. Na 2/3 ETC Na 100% ETC Na 150% ETC" },
                { "a. Hoe lang denk je nog nodig te hebben?", "✏️ 509. Start timer (1 min)" },
                { "b. Past het binnen de 150%? Tijdtrigger gehaald? Zo ja, verder", "✏️ 510. Hoe lang denk je nog nodig te hebben?" },
                { "H. Evaluatie laatste acties:", "✏️ 512. Haal ik het?" },
                { "a. Hoe heeft het zo lang kunnen duren?", "✏️ 513. Hoe heeft het zo lang kunnen duren?" },
                { "b. Snellere methodieken mogelijk?", "✏️ 514. Stop timer (4 min)" },
                { "c. Heb ik mijn antwoorden getest voordat ik JW spreek?", "✏️ 515. Significant inzicht gekregen" }
            };

            var phase6Mappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 A. Status = review ontwerp", "🫱🏻‍🫲🏻 601. Status = review ontwerp" },
                { "🫱🏻‍🫲🏻 B. Is de baas aanwezig?", "🫱🏻‍🫲🏻 602. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 C. Meteen meldplicht wanneer de baas aanwezig is", "🫱🏻‍🫲🏻 602F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 D. Na dat de baas weer aanwezig is", "🫱🏻‍🫲🏻 602T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 E. Overleg resultaten voor goedkeuring vervolg", "🫱🏻‍🫲🏻 604. Overleg resultaten voor goedkeuring vervolg" },
                { "🫱🏻‍🫲🏻 F. Goedkeuring?", "🫱🏻‍🫲🏻 606. Goedkeuring?" },
                { "🫱🏻‍🫲🏻 G. De baas bepaald volgende status", "🫱🏻‍🫲🏻 606F. De baas bepaald volgende status en verifieert effort per resterende fase" },
                { "🫱🏻‍🫲🏻 H. Volg de implementatie", "🫱🏻‍🫲🏻 606T. Volg de implementatie" }
            };

            var phase7Mappings = new Dictionary<string, string>
            {
                { "⌨️ Implementatie", "⌨️ 701. Status = implementatie" },
                { "A. Na afronding van elke functie testen (steppend debuggen, check op werking en dekking)", "⌨️ 702. Start timer (ETC)" },
                { "B. Houd ik me aan de eenvoudigste oplossing?", "⌨️ 703. Ontwerp verwerken" },
                { "C. Houd ik me aan de code conventie?", "⌨️ 704. Oplossing implementeren" },
                { "D. Alle IF en ELSE paden getest?", "⌨️ 705. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "E. Confidence level vastgesteld", "⌨️ 706. Stop timer (ETC)" },
                { "F. Vragen stellen bij onduidelijkheden!", "⌨️ 707. Bijzonderheden gezien?" },
                { "G. Evaluatie eerste acties:", "⌨️ 708. Na 2/3 ETC Na 100% ETC Na 150% ETC" },
                { "a. Hoe lang denk je nog nodig te hebben?", "⌨️ 709. Start timer (1 min)" },
                { "b. Past het binnen de 150%? Tijdtrigger gehaald? Zo ja, verder", "⌨️ 710. Hoe lang denk je nog nodig te hebben?" },
                { "H. Evaluatie laatste acties:", "⌨️ 712. Haal ik het?" },
                { "a. Hoe heeft het zo lang kunnen duren?", "⌨️ 713. Hoe heeft het zo lang kunnen duren?" },
                { "b. Snellere methodieken mogelijk?", "⌨️ 714. Stop timer (4 min)" },
                { "c. Heb ik mijn antwoorden getest voordat ik JW spreek?", "⌨️ 715. Significant inzicht gekregen" }
            };

            var phase8Mappings = new Dictionary<string, string>
            {
                { "🔎 Test", "🔎 801. Status = test" },
                { "A. Functioneel testen voor ieder soort haard (indien beschikbaar)", "🔎 802. Start timer (ETC)" },
                { "B. Test compleet uitgevoerd?", "🔎 803. Implementatie verwerken" },
                { "C. Code review van eigen code", "🔎 804. Oplossing testen" },
                { "D. Eigen review/ontwerp review", "🔎 805. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "E. Rekening gehouden met oudere systemen?", "🔎 806. Stop timer (ETC)" },
                { "F. Heeft elke IF een ELSE?", "🔎 807. Bijzonderheden gezien?" },
                { "a. Hoe lang denk je nog nodig te hebben?", "🔎 809. Start timer (1 min)" },
                { "b. Past het binnen de 150%? Tijdtrigger gehaald? Zo ja, verder", "🔎 810. Hoe lang denk je nog nodig te hebben?" },
                { "K. Evaluatie laatste acties:", "🔎 812. Haal ik het?" },
                { "a. Hoe heeft het zo lang kunnen duren?", "🔎 813. Hoe heeft het zo lang kunnen duren?" },
                { "b. Snellere methodieken mogelijk?", "🔎 814. Stop timer (4 min)" },
                { "c. Heb ik mijn antwoorden getest voordat ik JW spreek?", "🔎 815. Significant inzicht gekregen" }
            };

            var phase9Mappings = new Dictionary<string, string>
            {
                { "⚠️ Meldplicht", "⚠️ 901. Status = meldplicht" },
                { "⚠️ Status = meldplicht", "⚠️ 901. Status = meldplicht" },
                { "A. Start timer (1 min)", "⚠️ 902. Start timer (1 min)" },
                { "B. Stel vervolgplan op", "⚠️ 903. Stel vervolgplan op" },
                { "C. Stop timer (1 min)", "⚠️ 904. Stop timer (1 min)" },
                { "D. Is de baas aanwezig?", "⚠️ 905. Is de baas aanwezig?" },
                { "E. Meteen meldplicht wanneer de baas aanwezig is", "⚠️ 905F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "F. Na dat de baas weer aanwezig is", "⚠️ 905T. Na dat de baas weer aanwezig is" },
                { "G. Overleg vervolgplan", "⚠️ 906. Overleg vervolgplan" },
                { "H. De baas bepaald volgende status en verifieert effort per resterende fase", "⚠️ 907. De baas bepaald volgende status en verifieert effort per resterende fase" }
            };

            var phase10Mappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 Review Final", "🫱🏻‍🫲🏻 1001. Status = review final" },
                { "A. Demonstratie van de oplossing geven", "🫱🏻‍🫲🏻 1004. Demonstratie van de oplossing geven" },
                { "B. Review met Jeroen uitvoeren", "🫱🏻‍🫲🏻 1005. Review met Jeroen uitvoeren" }
            };

            var phase11Mappings = new Dictionary<string, string>
            {
                { "⚙️ Special action", "⚙️ 1101. Status = special action" },
                { "⚙️ Status = special action", "⚙️ 1101. Status = special action" },
                { "A. Start timer (ETC)", "⚙️ 1102. Start timer (ETC)" },
                { "B. Intake verwerken", "⚙️ 1103. Intake verwerken" },
                { "C. Oplossing implementeren", "⚙️ 1104. Oplossing implementeren" },
                { "D. Evalueer of de deliverables correct en volledig zijn afgerond", "⚙️ 1105. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "E. Stop timer (ETC)", "⚙️ 1106. Stop timer (ETC)" },
                { "F. Bijzonderheden gezien?", "⚙️ 1107. Bijzonderheden gezien?" },
                { "G. Volg de meldplicht", "⚙️ 1107T. Volg de meldplicht" },
                { "H. Volg de review special action", "⚙️ 1107F. Volg de review special action" },
                { "I. Na 2/3 ETC Na 100% ETC Na 150% ETC", "⚙️ 1108. Na 2/3 ETC Na 100% ETC Na 150% ETC" },
                { "J. Start timer (1 min)", "⚙️ 1109. Start timer (1 min)" },
                { "K. Hoe lang denk je nog nodig te hebben?", "⚙️ 1110. Hoe lang denk je nog nodig te hebben?" },
                { "L. sa: m.", "⚙️ 1111. sa: m." },
                { "M. Haal ik het?", "⚙️ 1112. Haal ik het?" },
                { "N. Stop timer (1 min)", "⚙️ 1112T. Stop timer (1 min)" },
                { "O. Stop timer (1 min) Start timer (4 min)", "⚙️ 1112F. Stop timer (1 min) Start timer (4 min)" },
                { "P. Hoe heeft het zo lang kunnen duren?", "⚙️ 1113. Hoe heeft het zo lang kunnen duren?" },
                { "Q. Stop timer (4 min)", "⚙️ 1114. Stop timer (4 min)" },
                { "R. Significant inzicht gekregen", "⚙️ 1115. Significant inzicht gekregen" }
            };

            var phase12Mappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 A. Status = review special action", "🫱🏻‍🫲🏻 1201. Status = review special action" },
                { "🫱🏻‍🫲🏻 B. Is de baas aanwezig?", "🫱🏻‍🫲🏻 1202. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 C. Meteen meldplicht wanneer de baas aanwezig is", "🫱🏻‍🫲🏻 1202F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 D. Na dat de baas weer aanwezig is", "🫱🏻‍🫲🏻 1202T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 E. Overleg resultaten voor goedkeuring vervolg", "🫱🏻‍🫲🏻 1204. Overleg resultaten voor goedkeuring vervolg" },
                { "🫱🏻‍🫲🏻 F. De baas bepaald volgende status", "🫱🏻‍🫲🏻 1206. De baas bepaald volgende status en verifieert effort per resterende phase" }
            };

            var phase1CheckMappings = new Dictionary<string, string>
            {
                { "⏱️ 1. Jira-uitvoerder is medewerker", "⏱️ 101. Status = intake" },
                { "⏱️ 2. Jira-status is Toegewezen", "⏱️ 101. Status = intake" },
                { "⏱️ 3. Jira-status wordt Intake", "⏱️ 101. Status = intake" },
                { "⏱️ 4. Start > Klok > menu > Timer", "⏱️ 102. Start timer" },
                { "⏱️ 5. Start 5 min. > Op voorgrond behouden", "⏱️ 102. Start timer" },
                { "⏱️ 6. Jira-stopwatch wordt gestart", "⏱️ 102. Start timer" },
                { "⏱️ 7. Ik heb de oplossingsrichting geschreven", "⏱️ 103. Oplossingsrichting:" },
                { "⏱️ 8. Ik heb de deliverable specificatie geschreven", "⏱️ 104. Deliverable specificatie:" },
                { "⏱️ 9. Ik heb de initiële schatting geschreven", "⏱️ 105. Initiële schatting:" },
                { "⏱️ 10. Tijdtrigger na 5 minuten opvolgen", "⏱️ 106. Stop timer" },
                { "⏱️ 11. Pauze > Opnieuw instellen > Terug naar volledige weergave", "⏱️ 106. Stop timer" },
                { "⏱️ 12. Klok minimaliseren", "⏱️ 106. Stop timer" },
                { "⏱️ 13. Naar volgende flowchartpagina navigeren", "⏱️ 107. Bijzonderheden gezien?" }
            };

            var phase2CheckMappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 1. Jira-uitvoerder is medewerker", "🫱🏻‍🫲🏻 201. Status = review aanpak" },
                { "🫱🏻‍🫲🏻 2. Jira-status is Intake", "🫱🏻‍🫲🏻 201. Status = review aanpak" },
                { "🫱🏻‍🫲🏻 3. Jira-status wordt Review aanpak", "🫱🏻‍🫲🏻 201. Status = review aanpak" },
                { "🫱🏻‍🫲🏻 4. Baas is mondeling beschikbaar?", "🫱🏻‍🫲🏻 202. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 5. Baas is schriftelijk beschikbaar", "🫱🏻‍🫲🏻 202F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 6. Aanwezigheid monitoren", "🫱🏻‍🫲🏻 202F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 7. Baas is mondeling beschikbaar", "🫱🏻‍🫲🏻 202T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 8. Overleg resultaten voor goedkeurig vervolg", "🫱🏻‍🫲🏻 204. Bespreek oplossingsrichting, deliverables en ETC’s" },
                { "🫱🏻‍🫲🏻 9. Resultaten zijn goedgekeurd?", "🫱🏻‍🫲🏻 206. Goedkeuring?" },
                { "🫱🏻‍🫲🏻 10. Terug naar vorige flowchartpagina navigeren", "🫱🏻‍🫲🏻 206F. De baas bepaald volgende status" },
                { "🫱🏻‍🫲🏻 11. Naar volgende flowchartpagina navigeren", "🫱🏻‍🫲🏻 206T. Volg de analyse" }
            };

            var phase3CheckMappings = new Dictionary<string, string>
            {
                { "🔬 A. Aanleiding of probleem (BUGS) helder?", "🔬 302. Start timer (ETC)" },
                { "🔬 B. Is het issue een Bug? (Bug(s) reproduceren!)", "🔬 303. Intake verwerken" },
                { "🔬 C. Mogelijke oplossingen bedenken (minimaal 2, bestaande templates gebruiken!)", "🔬 304. Oplossing analyseren" },
                { "🔬 D. Beste oplossing kiezen (KISS), onderbouwen waarom deze de voorkeur heeft", "🔬 305. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "🔬 E. Evaluatie eerste acties:", "🔬 310. Hoe lang denk je nog nodig te hebben?" },
                { "🔬 F. Evaluatie laatste acties:", "🔬 313. Hoe heeft het zo lang kunnen duren?" }
            };

            var phase5CheckMappings = new Dictionary<string, string>
            {
                { "✏️ A. Concept gekozen, eventueel met pseudocode of direct implementatie", "✏️ 502. Start timer (ETC)" },
                { "✏️ B. Implementatie van het gekozen template (op de kop af?)", "✏️ 503. Analyse verwerken" },
                { "✏️ C. Confidence level vastgesteld", "✏️ 504. Oplossing ontwerpen" },
                { "✏️ D. Is de pseudocode juist?", "✏️ 505. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "✏️ E. Zit alles op de juiste plek? Zo niet, bespreken og terug!", "✏️ 506. Stop timer (ETC)" },
                { "✏️ E. Zit alles op de juiste plek? Zo niet, bespreken en terug!", "✏️ 506. Stop timer (ETC)" },
                { "✏️ G. Evaluatie eerste acties:", "✏️ 510. Hoe lang denk je nog nodig te hebben?" },
                { "✏️ H. Evaluatie laatste acties:", "✏️ 513. Hoe heeft het zo lang kunnen duren?" }
            };

            var phase6CheckMappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 1. Jira-uitvoerder is medewerker", "🫱🏻‍🫲🏻 601. Status = review ontwerp" },
                { "🫱🏻‍🫲🏻 2. Jira-status is Ontwerp", "🫱🏻‍🫲🏻 601. Status = review ontwerp" },
                { "🫱🏻‍🫲🏻 3. Jira-status wordt Review ontwerp", "🫱🏻‍🫲🏻 601. Status = review ontwerp" },
                { "🫱🏻‍🫲🏻 4. Baas is mondeling beschikbaar?", "🫱🏻‍🫲🏻 602. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 5. Baas is schriftelijk beschikbaar", "🫱🏻‍🫲🏻 602F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 6. Aanwezigheid monitoren", "🫱🏻‍🫲🏻 602F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 7. Baas is mondeling beschikbaar", "🫱🏻‍🫲🏻 602T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 8. Ik mail naar baas", "🫱🏻‍🫲🏻 604. Overleg resultaten voor goedkeuring vervolg" },
                { "🫱🏻‍🫲🏻 9. Resultaten zijn goedgekeurd?", "🫱🏻‍🫲🏻 606. Goedkeuring?" },
                { "🫱🏻‍🫲🏻 10. Terug naar vorige flowchartpagina navigeren", "🫱🏻‍🫲🏻 606F. De baas bepaald volgende status en verifieert effort per resterende fase" },
                { "🫱🏻‍🫲🏻 11. Naar volgende flowchartpagina navigeren", "🫱🏻‍🫲🏻 606T. Volg de implementatie" }
            };

            var phase7CheckMappings = new Dictionary<string, string>
            {
                { "⌨️ A. Na afronding van elke functie testen (steppend debuggen, check op werking en dekking)", "⌨️ 702. Start timer (ETC)" },
                { "⌨️ B. Houd ik me aan de eenvoudigste oplossing?", "⌨️ 703. Ontwerp verwerken" },
                { "⌨️ C. Houd ik me aan de code conventie?", "⌨️ 704. Oplossing implementeren" },
                { "⌨️ D. Alle IF en ELSE paden getest?", "⌨️ 705. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "⌨️ E. Confidence level vastgesteld", "⌨️ 706. Stop timer (ETC)" },
                { "⌨️ F. Vragen stellen bij onduidelijkheden!", "⌨️ 707. Bijzonderheden gezien?" },
                { "⌨️ G. Evaluatie eerste acties:", "⌨️ 710. Hoe lang denk je nog nodig te hebben?" },
                { "⌨️ H. Evaluatie laatste acties:", "⌨️ 713. Hoe heeft het zo lang kunnen duren?" }
            };

            var phase8CheckMappings = new Dictionary<string, string>
            {
                { "🔎 A. Functioneel testen voor ieder soort haard (indien beschikbaar)", "🔎 802. Start timer (ETC)" },
                { "🔎 B. Test compleet uitgevoerd?", "🔎 803. Implementatie verwerken" },
                { "🔎 C. Code review van eigen code", "🔎 804. Oplossing testen" },
                { "🔎 D. Eigen review/ontwerp review", "🔎 805. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "🔎 E. Rekening gehouden met oudere systemen?", "🔎 806. Stop timer (ETC)" },
                { "🔎 F. Heeft elke IF een ELSE?", "🔎 807. Bijzonderheden gezien?" },
                { "🔎 K. Evaluatie laatste acties:", "🔎 813. Hoe heeft het zo lang kunnen duren?" }
            };

            var phase9CheckMappings = new Dictionary<string, string>
            {
                { "⚠️ A. Is de baas aanwezig?", "⚠️ 905. Is de baas aanwezig?" },
                { "⚠️ B. Meteen meldplicht wanneer de baas aanwezig is", "⚠️ 905F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "⚠️ C. Na dat de baas weer aanwezig is", "⚠️ 905T. Na dat de baas weer aanwezig is" },
                { "⚠️ D. Overleg vervolgplan", "⚠️ 906. Overleg vervolgplan" },
                { "⚠️ E. De baas bepaald volgende status en verifieert effort per resterende fase", "⚠️ 907. De baas bepaald volgende status en verifieert effort per resterende fase" }
            };

            var phase10CheckMappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 A. Demonstratie van de oplossing geven", "🫱🏻‍🫲🏻 1004. Demonstratie van de oplossing geven" },
                { "🫱🏻‍🫲🏻 B. Review met Jeroen uitvoeren", "🫱🏻‍🫲🏻 1005. Review met Jeroen uitvoeren" }
            };

            var phase11CheckMappings = new Dictionary<string, string>
            {
                { "⚙️ A. Start timer (ETC)", "⚙️ 1102. Start timer (ETC)" },
                { "⚙️ B. Intake verwerken", "⚙️ 1103. Intake verwerken" },
                { "⚙️ C. Oplossing implementeren", "⚙️ 1104. Oplossing implementeren" },
                { "⚙️ D. Evalueer of de deliverables correct en volledig zijn afgerond", "⚙️ 1105. Evalueer of de deliverables correct en volledig zijn afgerond" },
                { "⚙️ E. Stop timer (ETC)", "⚙️ 1106. Stop timer (ETC)" },
                { "⚙️ F. Bijzonderheden gezien?", "⚙️ 1107. Bijzonderheden gezien?" },
                { "⚙️ I. Na 2/3 ETC Na 100% ETC Na 150% ETC", "⚙️ 1108. Na 2/3 ETC Na 100% ETC Na 150% ETC" },
                { "⚙️ K. Hoe lang denk je nog nodig te hebben?", "⚙️ 1110. Hoe lang denk je nog nodig te hebben?" },
                { "⚙️ P. Hoe heeft het zo lang kunnen duren?", "⚙️ 1113. Hoe heeft het zo lang kunnen duren?" }
            };

            var phase12CheckMappings = new Dictionary<string, string>
            {
                { "🫱🏻‍🫲🏻 1. Jira-uitvoerder is medewerker", "🫱🏻‍🫲🏻 1201. Status = review special action" },
                { "🫱🏻‍🫲🏻 2. Jira-status is Special action", "🫱🏻‍🫲🏻 1201. Status = review special action" },
                { "🫱🏻‍🫲🏻 3. Jira-status wordt Review special action", "🫱🏻‍🫲🏻 1201. Status = review special action" },
                { "🫱🏻‍🫲🏻 4. Baas is mondeling beschikbaar?", "🫱🏻‍🫲🏻 1202. Is de baas aanwezig?" },
                { "🫱🏻‍🫲🏻 5. Baas is schriftelijk beschikbaar", "🫱🏻‍🫲🏻 1202F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 6. Aanwezigheid monitoren", "🫱🏻‍🫲🏻 1202F. Meteen meldplicht wanneer de baas aanwezig is" },
                { "🫱🏻‍🫲🏻 7. Baas is mondeling beschikbaar", "🫱🏻‍🫲🏻 1202T. Na dat de baas weer aanwezig is" },
                { "🫱🏻‍🫲🏻 8. Overleg resultaten voor goedkeuring vervolg", "🫱🏻‍🫲🏻 1204. Overleg resultaten voor goedkeuring vervolg" },
                { "🫱🏻‍🫲🏻 10. De baas bepaald volgende status en verifieert effort per resterende fase", "🫱🏻‍🫲🏻 1206. De baas bepaald volgende status en verifieert effort per resterende phase" }
            };

            PatchFileHeadings(actielijstPath, phase2Mappings, "🫱🏻‍🫲🏻 A. Status = review aanpak");
            PatchFileHeadings(actielijstPath, phase3Mappings, "🔬 A. Status = analyse");
            PatchFileHeadings(actielijstPath, phase4Mappings, "🫱🏻‍🫲🏻 A. Status = review analyse");
            PatchFileHeadings(actielijstPath, phase5Mappings, "✏️ Ontwerp");
            PatchFileHeadings(actielijstPath, phase6Mappings, "🫱🏻‍🫲🏻 A. Status = review ontwerp");
            PatchFileHeadings(actielijstPath, phase7Mappings, "⌨️ Implementatie");
            PatchFileHeadings(actielijstPath, phase8Mappings, "🔎 Test");
            PatchFileHeadings(actielijstPath, phase9Mappings, "⚠️ Meldplicht");
            PatchFileHeadings(actielijstPath, phase10Mappings, "🫱🏻‍🫲🏻 Review Final");
            PatchFileHeadings(actielijstPath, phase11Mappings, "⚙️ Special action");
            PatchFileHeadings(actielijstPath, phase12Mappings, "🫱🏻‍🫲🏻 A. Status = review special action");
            PatchFileHeadings(checklijstPath, phase1CheckMappings, "⏱️ 1. Jira-uitvoerder is medewerker");
            PatchFileHeadings(checklijstPath, phase2CheckMappings, "🫱🏻‍🫲🏻 1. Jira-uitvoerder is medewerker");
            PatchFileHeadings(checklijstPath, phase3CheckMappings, "🔬 A. Aanleiding of probleem (BUGS) helder?");
            PatchFileHeadings(checklijstPath, phase5CheckMappings, "✏️ A. Concept gekozen, eventueel met pseudocode of direct implementatie");
            PatchFileHeadings(checklijstPath, phase6CheckMappings, "🫱🏻‍🫲🏻 1. Jira-uitvoerder is medewerker");
            PatchFileHeadings(checklijstPath, phase7CheckMappings, "⌨️ A. Na afronding van elke functie testen (steppend debuggen, check op werking en dekking)");
            PatchFileHeadings(checklijstPath, phase8CheckMappings, "🔎 A. Functioneel testen voor ieder soort haard (indien beschikbaar)");
            PatchFileHeadings(checklijstPath, phase9CheckMappings, "⚠️ A. Is de baas aanwezig?");
            PatchFileHeadings(checklijstPath, phase10CheckMappings, "🫱🏻‍🫲🏻 A. Demonstratie van de oplossing geven");
            PatchFileHeadings(checklijstPath, phase11CheckMappings, "⚙️ A. Start timer (ETC)");
            PatchFileHeadings(checklijstPath, phase12CheckMappings, "🫱🏻‍🫲🏻 1. Jira-uitvoerder is medewerker");
        }

        private static void PatchFileHeadings(string filePath, Dictionary<string, string> mappings, string startMarker)
        {
            if (!File.Exists(filePath)) return;

            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                bool inPhase = false;

                foreach (var p in body.Descendants<Paragraph>())
                {
                    var sb = new StringBuilder();
                    var textElements = p.Descendants<Text>().ToList();
                    foreach (var t in textElements)
                    {
                        sb.Append(t.Text);
                    }
                    string txt = sb.ToString().Trim();

                    if (string.IsNullOrEmpty(txt)) continue;

                    if (txt.Contains(startMarker) || (startMarker == "🔬" && txt.StartsWith("🔬")))
                    {
                        inPhase = true;
                    }
                    else if (inPhase)
                    {
                        foreach (var em in Emojis)
                        {
                            if (em != "🔬" && txt.StartsWith(em, StringComparison.Ordinal) && (txt.Contains("Status =") || txt.Contains("Ontwerp") || txt.Contains("Test") || txt.Contains("Review Final")) && !txt.Contains(startMarker))
                            {
                                inPhase = false;
                                break;
                            }
                        }
                    }

                    if (inPhase && mappings.ContainsKey(txt))
                    {
                        string newText = mappings[txt];
                        if (textElements.Count > 0)
                        {
                            Console.WriteLine("C# Patching {0} -> {1}", txt, newText);
                            textElements[0].Text = newText;
                            for (int i = 1; i < textElements.Count; i++)
                            {
                                textElements[i].Text = string.Empty;
                            }
                        }
                    }
                }
                doc.MainDocumentPart.Document.Save();
            }
        }
    }
}
