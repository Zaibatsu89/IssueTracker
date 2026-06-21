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
        private static readonly string[] Emojis = { "⏱️", "🤝🏻", "🔬", "✏️", "⌨️", "🔎", "⚠️", "⚙️" };
        private const string VariationSelector = "\uFE0F";

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
            new PhaseInfo { Number = 2, Name = "Review aanpak", DrawioFilename = "2 Issue Tracker review aanpak.drawio", Emoji = "🤝🏻", SectionKeyword = "review aanpak" },
            new PhaseInfo { Number = 3, Name = "Analyse", DrawioFilename = "3 Issue Tracker analyse.drawio", Emoji = "🔬", SectionKeyword = "analyse" },
            new PhaseInfo { Number = 4, Name = "Review analyse", DrawioFilename = "4 Issue Tracker review analyse.drawio", Emoji = "🤝🏻", SectionKeyword = "review analyse" },
            new PhaseInfo { Number = 5, Name = "Ontwerp", DrawioFilename = "5 Issue Tracker ontwerp.drawio", Emoji = "✏️", SectionKeyword = "Ontwerp" },
            new PhaseInfo { Number = 6, Name = "Review ontwerp", DrawioFilename = "6 Issue Tracker review ontwerp.drawio", Emoji = "🤝🏻", SectionKeyword = "review ontwerp" },
            new PhaseInfo { Number = 7, Name = "Implementatie", DrawioFilename = "7 Issue Tracker implementatie.drawio", Emoji = "⌨️", SectionKeyword = "Implementatie" },
            new PhaseInfo { Number = 8, Name = "Test", DrawioFilename = "8 Issue Tracker test.drawio", Emoji = "🔎", SectionKeyword = "Test" },
            new PhaseInfo { Number = 9, Name = "Meldplicht", DrawioFilename = "9 Issue Tracker meldplicht.drawio", Emoji = "⚠️", SectionKeyword = "meldplicht" },
            new PhaseInfo { Number = 10, Name = "Review final", DrawioFilename = "10 Issue Tracker review final.drawio", Emoji = "🤝🏻", SectionKeyword = "Review Final" },
            new PhaseInfo { Number = 11, Name = "Special action", DrawioFilename = "11 Issue Tracker special action.drawio", Emoji = "⚙️", SectionKeyword = "special action" },
            new PhaseInfo { Number = 12, Name = "Review special action", DrawioFilename = "12 Issue Tracker review special action.drawio", Emoji = "🤝🏻", SectionKeyword = "review special action" }
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

                    if (matchingSection != null)
                    {
                        string headingToUse = matchingSection.Heading;
                        if (headingToUse.Length > 0)
                        {
                            headingToUse = char.ToUpper(headingToUse[0]) + headingToUse.Substring(1);
                        }
                        AddHeading2(body, headingToUse, "2E75B6");

                        foreach (var action in matchingSection.Actions)
                        {
                            bool isSubHeader = false;
                            string actionNorm = action.Replace(VariationSelector, "").TrimStart();
                            foreach (var em in Emojis)
                            {
                                string emNorm = em.Replace(VariationSelector, "");
                                if (actionNorm.StartsWith(emNorm, StringComparison.Ordinal))
                                {
                                    isSubHeader = true;
                                    break;
                                }
                            }

                            if (isSubHeader)
                            {
                                AddParagraph(body, action, bold: true, colorHex: "1F497D", leftIndent: 180);

                                var checks = FindChecksForAction(action, checkGroups, phase.Emoji, phase.SectionKeyword);
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
                                AddParagraph(body, action, leftIndent: 360);

                                var checks = FindChecksForAction(action, checkGroups, phase.Emoji, phase.SectionKeyword);
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
            string currentSectionKeyword = null;

            foreach (var p in paragraphs)
            {
                bool hasEmoji = false;
                string pNorm = p.Replace(VariationSelector, "").TrimStart();
                string pEmoji = "";
                foreach (var em in Emojis)
                {
                    string emNorm = em.Replace(VariationSelector, "");
                    if (pNorm.StartsWith(emNorm, StringComparison.Ordinal))
                    {
                        hasEmoji = true;
                        pEmoji = emNorm;
                        pNorm = pNorm.Substring(emNorm.Length).TrimStart();
                        break;
                    }
                }

                bool isHeading = false;
                if (hasEmoji)
                {
                    var match = Regex.Match(pNorm, @"^(\d+)");
                    if (match.Success)
                    {
                        int num = int.Parse(match.Groups[1].Value);
                        if (num % 100 == 1)
                        {
                            isHeading = true;
                            int phaseNum = num / 100;
                            var matchedPhase = Phases.FirstOrDefault(ph => ph.Number == phaseNum);
                            if (matchedPhase != null && matchedPhase.SectionKeyword != currentSectionKeyword)
                            {
                                currentSectionKeyword = matchedPhase.SectionKeyword;
                                currentSection = new PhaseSection { Heading = currentSectionKeyword };
                                sections.Add(currentSection);
                            }
                        }
                    }
                }

                if (currentSection != null && !isHeading)
                {
                    if (p.ToLowerInvariant().Contains("status ="))
                    {
                        continue;
                    }
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
                    string lowerP = p.ToLowerInvariant();
                    PhaseInfo matchedPhase = null;
                    var codeMatch = Regex.Match(p, @"\b\d{3,4}\b");
                    if (codeMatch.Success)
                    {
                        int phaseNum = int.Parse(codeMatch.Value) / 100;
                        matchedPhase = Phases.FirstOrDefault(ph => ph.Number == phaseNum && ph.Emoji == "🤝🏻");
                    }

                    if (matchedPhase == null)
                    {
                        matchedPhase = Phases.FirstOrDefault(ph => 
                            ph.Emoji == "🤝🏻" && lowerP.Contains(ph.Name.ToLowerInvariant())
                        );
                    }

                    if (matchedPhase != null)
                    {
                        currentSubPhase = matchedPhase.SectionKeyword;
                    }

                    bool isHandshake = p.StartsWith("🤝🏻", StringComparison.Ordinal);

                    currentGroup = new CheckGroup
                    {
                        Heading = p,
                        SubPhase = isHandshake ? currentSubPhase : string.Empty
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

        private static bool IsEmojiMatch(string heading, string phaseEmoji)
        {
            if (heading.StartsWith(phaseEmoji, StringComparison.Ordinal)) return true;

            bool isHeadingHandshake = heading.StartsWith("🤝🏻", StringComparison.Ordinal);
            bool isPhaseHandshake = phaseEmoji == "🤝🏻";

            return isHeadingHandshake && isPhaseHandshake;
        }

        private static List<string> FindChecksForAction(string action, List<CheckGroup> checkGroups, string phaseEmoji, string sectionKeyword)
        {
            string normAction = Normalize(action);
            if (string.IsNullOrEmpty(normAction)) return new List<string>();

            if (normAction == "ontwerpverwerken")
            {
                return new List<string>
                {
                    "Stappenplan op basis van de opdrachtbeschrijving opstellen",
                    "Verifiëren of de stappen onderdeel van de opdracht zijn"
                };
            }

            foreach (var cg in checkGroups)
            {
                // 1. Must match the phase emoji prefix
                if (!IsEmojiMatch(cg.Heading, phaseEmoji)) continue;

                // 2. If it's a review phase, must match the specific review sub-phase keyword!
                if (phaseEmoji == "🤝🏻")
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

                    if (!string.IsNullOrEmpty(action))
                    {
                        string trimmedAction = action.Trim();
                        if (trimmedAction.Length > 0)
                        {
                            char lastChar = trimmedAction[trimmedAction.Length - 1];
                            if (char.IsLetter(lastChar) && lastChar != 'T' && lastChar != 'F')
                            {
                                Console.WriteLine($"[VALIDATIE WAARSCHUWING] Overtreding bedrijfsregel in '{Path.GetFileName(filePath)}': Action ID '{action}' eindigt met een ongeldige letter. Alleen T of F is toegestaan.");
                            }
                        }
                    }

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
    }
}
