using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace CadastrTest
{
    public partial class MainForm : Form
    {

        private Dictionary<TreeNode, string> contentOfNodes = new Dictionary<TreeNode, string>(); //словарь для хранения содержимого нод
        private TreeNode landRecords, buildRecords, constructionRecords, spatialData, municipalBoundaries, zones;

        private string FormatXml(string xml)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch
            {
                return xml;
            }
        }

        private void saveXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count == 0)
            {
                MessageBox.Show("Не загружен основной файл!");
            }
            else
            {
                try
                {
                    string xml = contentOfNodes[treeView1.SelectedNode];
                    saveFileDialog1.CreatePrompt = true;
                    saveFileDialog1.OverwritePrompt = true;
                    saveFileDialog1.FileName = "cadastr";
                    saveFileDialog1.DefaultExt = "xml";
                    saveFileDialog1.Filter =
                        "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    saveFileDialog1.InitialDirectory =
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    DialogResult result = saveFileDialog1.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string savedXml = FormatXml(xml);
                        File.WriteAllText(saveFileDialog1.FileName, savedXml, Encoding.GetEncoding("utf-8"));
                    }
                }
                catch
                {
                    MessageBox.Show("Выбрана нода, недоступная для сохранения!");
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode.Nodes.Count == 0)
            {
                try
                {
                    string savedXml = contentOfNodes[selectedNode];
                    if (savedXml != null)
                    {
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(savedXml);
                        if (xDoc.DocumentElement.ChildNodes.Count > 0)
                        {
                            XmlNode xRoot = xDoc.DocumentElement.ChildNodes[0];
                            AddNode(selectedNode.Nodes, xRoot, true);
                        }
                    }
                }
                catch { }
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void openXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(openFileDialog1.FileName);
                XmlElement xRoot = xDoc.DocumentElement;

                //считываем xml без пустых нод

                treeView1.Nodes.Clear(); //очистка перед загрузкой
                                         //инициализация переменных
                landRecords = treeView1.Nodes.Add("land_records");
                buildRecords = treeView1.Nodes.Add("build_records");
                constructionRecords = treeView1.Nodes.Add("construction_records");
                spatialData = treeView1.Nodes.Add("spatial_data");
                municipalBoundaries = treeView1.Nodes.Add("municipal_boundaries");
                zones = treeView1.Nodes.Add("zones_and_territories_boundaries");

                if (xDoc.DocumentElement.Name == "land_record" || xDoc.DocumentElement.Name == "build_record" || xDoc.DocumentElement.Name == "construction_record" || xDoc.DocumentElement.Name == "entity_spatial" || xDoc.DocumentElement.Name == "municipal_boundary_record" || xDoc.DocumentElement.Name == "zones_and_territories_record")
                {
                    FindTag(xDoc);
                }
                else
                {
                    foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                    {
                        if (node.Name == "cadastral_blocks")
                        {
                            FindTag(node);
                        }
                    }
                }

            }
        }

        private void AddNode(TreeNodeCollection nodes, XmlNode inXmlNode, bool recursion = false)
        {
            if (inXmlNode.HasChildNodes)
            {
                string text = inXmlNode.Name;
                
                TreeNode newNode = nodes.Add(text);

                if (text == "land_record" || text == "build_record" || text == "construction_record" || text == "entity_spatial" || text == "municipal_boundary_record" || text == "zones_and_territories_record")
                {
                    contentOfNodes.Add(newNode, "<" + text + ">" + inXmlNode.InnerXml + "</" + text + ">");

                    XmlNode cadNode = inXmlNode.SelectSingleNode(".//" + "cad_number");
                    if (cadNode != null)
                    {
                        newNode.Text = cadNode.InnerText;
                    }
                    else
                    {
                        cadNode = inXmlNode.SelectSingleNode(".//" + "reg_numb_border");
                        if (cadNode != null)
                        {
                            newNode.Text = cadNode.InnerText;
                        }
                        else
                        {
                            cadNode = inXmlNode.SelectSingleNode(".//" + "sk_id");
                            if (cadNode != null)
                            {
                                newNode.Text = cadNode.InnerText;
                            }
                        }
                    }   

                }

                if (recursion)
                {
                    XmlNodeList nodeList = inXmlNode.ChildNodes;
                    for (int i = 0; i <= nodeList.Count - 1; i++)
                    {
                        XmlNode xNode = inXmlNode.ChildNodes[i];
                        AddNode(newNode.Nodes, xNode, true);
                    }
                }
            }
            else
            {
                string text = (inXmlNode.OuterXml).Trim();
                TreeNode newNode = nodes.Add(text);
            }
        }

        private void FindTag(XmlNode fatherNode)
        {
            if (fatherNode.HasChildNodes)
            {
                foreach (XmlNode node in fatherNode.ChildNodes)
                {
                    if (node.Name == "land_record")
                    {
                        AddNode(landRecords.Nodes, node);
                    }
                    else if (node.Name == "build_record")
                    {
                        AddNode(buildRecords.Nodes, node);
                    }
                    else if (node.Name == "construction_record")
                    {
                        AddNode(constructionRecords.Nodes, node);
                    }
                    else if (node.Name == "entity_spatial")
                    {
                        AddNode(spatialData.Nodes, node);
                    }
                    else if (node.Name == "municipal_boundary_record")
                    {
                        AddNode(municipalBoundaries.Nodes, node);
                    }
                    else if (node.Name == "zones_and_territories_record")
                    {
                        AddNode(zones.Nodes, node);
                    }
                    else
                    {
                        FindTag(node);
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
