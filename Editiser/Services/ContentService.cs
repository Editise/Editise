using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO.Packaging;
using DocumentFormat.OpenXml.Math;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using RunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml;

namespace Editiser.Services
{
    public class ContentService : IContentService
    {
        private Dictionary<string, Content> _blocks { get; set; } = new();
        public Dictionary<string, Content> Blocks
        {
            get
            {
                ContentCheck();
                return _blocks;
            }
            set
            {
                _blocks = value;
            }
        }
        public bool DevMode { get; set; } = false;
        private DateTime NextRefresh = DateTime.Now.AddDays(-1);

        public ContentService()
        {
            if (NextRefresh < DateTime.Now)
            {
                Console.WriteLine("fetching content");
                ContentCheck();
            }
        }

        private void ContentCheck()
        {
            if (NextRefresh > DateTime.Now) return;
            NextRefresh = DateTime.Now.AddDays(30);
            DirectoryInfo d = new DirectoryInfo(@"Docs"); //Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.docx"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                if (file.Name.Substring(0, 2) != "~$")
                {
                    AddDoc(file);
                }
            }
        }

        private void AddDoc(FileInfo file)
        {
            Console.WriteLine("singleton open doc  ......");
            string ActiveTag = "";
            try
            {
                using (Stream stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {
                        // Open a WordprocessingDocument based on a package in read only mode
                        using (WordprocessingDocument wordDocument =
                            WordprocessingDocument.Open(stream, false))
                        {
                            Body body = wordDocument.MainDocumentPart.Document.Body;

                            Console.WriteLine("parsing doc  ......");
                            //    List<DocumentFormat.OpenXml.OpenXmlElement> paragraphs = body.Where(p => p.ToString() == "DocumentFormat.OpenXml.Wordprocessing.Paragraph").ToList();
                            List<Paragraph> paragraphs = body.OfType<Paragraph>().ToList();
                            //List<Table> table = body.OfType<Table>().ToList();
                            foreach (var item in body)
                            {
                                var tableRowForConc = string.Empty;
                                if (item is Table)
                                {
                                    Table table = item as Table;
                                    if (table != null)
                                    {
                                        foreach (var row in table)
                                        {
                                            TableRow tableRow = row as TableRow;
                                            var tableDatas = string.Empty;
                                            if (tableRow != null)
                                            {
                                                foreach (var cell in tableRow)
                                                {
                                                    TableCell tableCell = cell as TableCell;
                                                    if (tableCell != null)
                                                    {
                                                        tableDatas += "<td>" + tableCell.InnerText + "</td>";
                                                    }
                                                }

                                            }
                                            tableRowForConc += "<tr>" + tableDatas + "</tr>";
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(tableRowForConc))
                                    {
                                        tableRowForConc = "<table class='table text-white'>" + tableRowForConc + "</table>";
                                        if (_blocks[file.Name].ContainsKey(ActiveTag))
                                        {
                                            _blocks[file.Name][ActiveTag] += tableRowForConc;

                                        }
                                        else
                                        {
                                            _blocks[file.Name][ActiveTag] = tableRowForConc;
                                        }
                                        //appendPara += "\n" + tableRowForConc;
                                    }
                                }

                                if (item is Paragraph)
                                {
                                    Paragraph para = item as Paragraph;
                                    if (para.ParagraphProperties != null &&
                                         para.ParagraphProperties.ParagraphStyleId != null &&
                                         para.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading1"))
                                    {
                                        ActiveTag = para.InnerText.Trim();
                                    }
                                    else if (!string.IsNullOrEmpty(ActiveTag))
                                    {
                                        if (!_blocks.ContainsKey(file.Name))
                                        {
                                            _blocks.Add(file.Name, new Content());
                                        }
                                        if (_blocks[file.Name].ContainsKey(ActiveTag))
                                        {
                                            _blocks[file.Name][ActiveTag] += ParaHtml(para, wordDocument);
                                            _blocks[file.Name][ActiveTag] = _blocks[file.Name][ActiveTag].Replace("</ul><ul>", ""); // remove un necessary UL dividers
                                        }
                                        else
                                        {
                                            _blocks[file.Name][ActiveTag] = ParaHtml(para, wordDocument);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"singleton parsing doc failed  ......{ex.ToString()}");
                    }
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"singleton parsing doc failed  ......{ex.ToString()}");
            }
        }

        private string ParaHtml(Paragraph para, WordprocessingDocument wordDocument)
        {
            string retval = string.Empty;
            string hasImage = string.Empty;
            string prefix = string.Empty;
            string suffix = string.Empty;
            if (para.ParagraphProperties != null &&
     para.ParagraphProperties.ParagraphStyleId != null &&
     !string.IsNullOrEmpty(para.ParagraphProperties.ParagraphStyleId.Val.Value))
            {
                if (para.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading2"))
                {
                    prefix = "<h2>";
                    suffix = "</h2>";
                } else if (para.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading3"))
                {
                    prefix = "<h3>";
                    suffix = "</h3>";
                }
                else if (para.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading4"))
                {
                    prefix = "<h4>";
                    suffix = "</h4>";
                }
                else if (para.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading5"))
                {
                    prefix = "<h5>";
                    suffix = "</h5>";
                }
                
            }



            bool isList = IsList(para);
            foreach (Run run in para.Descendants<Run>())
            {
                string base64 = string.Empty;
                RunProperties props = run.RunProperties;
                XmlDocument runDocument = new XmlDocument();
                runDocument.LoadXml(run.OuterXml.ToString());
                var blips = runDocument.GetElementsByTagName("a:blip");
                if (blips.Count > 0)
                {
                    hasImage = "class='word-image'";
                    foreach (XmlElement blip in blips)
                    {
                        var drawingAttr = blip.Attributes["r:embed"].Value;
                        ImagePart img = (ImagePart)wordDocument.MainDocumentPart.GetPartById(drawingAttr);
                        var imgStream = img.GetStream();
                        byte[] bytes;
                        using (var memoryStream = new MemoryStream())
                        {
                            imgStream.CopyTo(memoryStream);
                            bytes = memoryStream.ToArray();
                        }
                        string strbytes = Convert.ToBase64String(bytes);
                        base64 += !string.IsNullOrEmpty(strbytes) ? $"<img src='data:{img.ContentType};base64,{strbytes}' />" : string.Empty;
                    }
                }
                XmlDocument parentDocument = new XmlDocument();
                parentDocument.LoadXml(run.Parent.OuterXml);
                XmlNodeList links = parentDocument.GetElementsByTagName("w:hyperlink");
                string url = string.Empty;
                bool external = false;
                if (links.Count > 0)
                {
                    XmlNode link = links[0]; // even if there were more elemnets we can only handle 1
                    string linkRef = link.Attributes["r:id"].Value;
                    var lnkUri = wordDocument.MainDocumentPart?.HyperlinkRelationships?.FirstOrDefault(l => l.Id == linkRef)?.Uri;
                    external = lnkUri.IsAbsoluteUri;
                    url = lnkUri.ToString();
                }
                string target = external ? "target='_new'" : "";
                string chunk = !string.IsNullOrEmpty(url) ? $"<a href='{url}' {target}>{ApplyTextFormatting(run) + base64}</a>" : ApplyTextFormatting(run) + base64;
                //retval += isList ? $"<li>{chunk}</li>" : chunk;
                retval += chunk;
            }
            if (string.IsNullOrEmpty(retval)) return string.Empty;
            return isList ? $"<ul><li>{retval}</li></ul>" : $"<p {hasImage}>{prefix}{retval}{suffix}</p>";
        }
        private string ApplyTextFormatting(Run run)
        {
            string content = run.InnerText;
            if (content.Contains("Automated Software Testing of mobile apps"))
            {
                Console.WriteLine("Automated Software Testing of mobile apps");
            }
            RunProperties property = run.RunProperties;
            if (property == null) return content;
            StringBuilder buildString = new StringBuilder(content);

            if (property.Bold != null)
            {
                buildString.Insert(0, "<b>");
                buildString.Append("</b>");
            }

            if (property.Italic != null)
            {
                buildString.Insert(0, "<i>");
                buildString.Append("</i>");
            }

            if (property.Underline != null)
            {
                buildString.Insert(0, "<u>");
                buildString.Append("</u>");
            }

            if (property.Color != null && property.Color.Val != null)
            {
                buildString.Insert(0, "<span style=\"color: #" + property.Color.Val + "\">");
                buildString.Append("</span>");
            }

            if (property.Highlight != null && property.Highlight.Val != null)
            {
                buildString.Insert(0, "<span style=\"background-color: " + property.Highlight.Val + "\">");
                buildString.Append("</span>");
            }

            if (property.Strike != null)
            {
                buildString.Insert(0, "<s>");
                buildString.Append("</s>");
            }
            return buildString.ToString();
        }
        private bool IsList(Paragraph para)
        {

            XmlDocument paraDocument = new XmlDocument();
            paraDocument.LoadXml(para.OuterXml.ToString());
            var pStyles = paraDocument.GetElementsByTagName("w:pStyle");
            foreach (XmlElement pStyle in pStyles)
            {
                var listval = pStyle?.Attributes["w:val"]?.Value;
                if (!string.IsNullOrEmpty(listval) && listval == "ListParagraph") return true;
            }
            return false;
        }
        public void ClearCache()
        {
            NextRefresh = DateTime.Now.AddDays(-1);
            _blocks = new();
            ContentCheck();
        }
        public bool BlockPresent(List<(string Docname, string BlockName)> testBlocks)
        {
            if (_blocks.Count == 0) return false;
            foreach (var testBlock in testBlocks)
            {
                if (_blocks.ContainsKey(testBlock.Docname) && _blocks[testBlock.Docname].ContainsKey(testBlock.BlockName)) return true;
            }
            return false;
        }
    }
    public class Content : Dictionary<string, string>
    {

    }

}

