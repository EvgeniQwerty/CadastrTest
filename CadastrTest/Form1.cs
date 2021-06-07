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
    public partial class Form1 : Form
    {

        private Dictionary<TreeNode, string> contentOfNodes = new Dictionary<TreeNode, string>(); //словарь для хранения содержимого нод

        public Form1()
        {
            InitializeComponent();
        }

        private void openXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
             if (openFileDialog1.ShowDialog() == DialogResult.OK)
             {
                try
                {
                    //как только попадаем в головную ноду, начинаем запись стринга. Как только попадаем в его закрывающую часть, заканчиваем писать в стринг. Ещё делаем привязку к ноде (с помощью словаря)
                    var sr = new StreamReader(openFileDialog1.FileName);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.Async = false; //без асинхронной загрузки/чтения
                    XmlReader reader = XmlReader.Create(sr, settings);
                    TreeNode addedNode, prevNode, landRecords, buildRecords, constructionRecords, spatialData, municipalBoundaries, zones;
                    bool foundCadNumber = false; //переменная для идентификации тэга-идентификатора (cad_number, sk_id, reg_numb_border)
                    string textXml = ""; //переменная, в которую записывается xml, для последующего показа в расшифровке (отправляется в словарь contentOfNodes)
                    bool isSk_id = false; //переменная для идентификации, является ли тэг sk_id идентификатором или нет
                    string prevTag = "land_record"; //переменная для сохранения предыдушего тэга
                    bool isRightES = false; //переменная для идентификации, является ли тэг entity_spatial идентификатором или нет

                    try
                    {
                        //считываем xml без пустых нод

                        treeView1.Nodes.Clear(); //очистка перед загрузкой
                        //инициализация переменных
                        landRecords = treeView1.Nodes.Add("land_records");
                        buildRecords = treeView1.Nodes.Add("build_records");
                        constructionRecords = treeView1.Nodes.Add("construction_records");
                        spatialData = treeView1.Nodes.Add("spatial_data");
                        municipalBoundaries = treeView1.Nodes.Add("municipal_boundaries");
                        zones = treeView1.Nodes.Add("zones_and_territories_boundaries");
                        prevNode = landRecords;
                        addedNode = landRecords;

                        //цикл считывания
                        while (reader.Read())
                        {
                            //свитч на тип ноды (открывающий тэг, текст, закрывающий тэг)
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    switch (reader.Name)
                                    {
                                        case "land_record":
                                            prevNode = landRecords;
                                            textXml = "<" + reader.Name + ">";
                                            break;
                                        case "cad_number":
                                            if (prevTag != "common_land_cad_number")
                                            {
                                                foundCadNumber = true;
                                            }
                                            textXml += "<" + reader.Name + ">";
                                            break;
                                        case "build_record":
                                            prevNode = buildRecords;
                                            textXml = "<" + reader.Name + ">";
                                            break;
                                        case "construction_record":
                                            prevNode = constructionRecords;
                                            textXml = "<" + reader.Name + ">";
                                            break;
                                        case "entity_spatial":
                                            if (prevTag != "contour" && prevTag != "number_pp")
                                            {
                                                prevNode = spatialData;
                                                textXml = "<" + reader.Name + ">";
                                                isRightES = true;
                                            }
                                            else
                                            {
                                                textXml += "<" + reader.Name + ">";
                                                isRightES = false;
                                            }
                                            break;
                                        case "sk_id":
                                            isSk_id = true;
                                            foundCadNumber = true;
                                            textXml += "<" + reader.Name + ">";
                                            break;
                                        case "municipal_boundary_record":
                                            prevNode = municipalBoundaries;
                                            textXml = "<" + reader.Name + ">";
                                            break;
                                        case "reg_numb_border":
                                            textXml += "<" + reader.Name + ">";
                                            foundCadNumber = true;
                                            break;
                                        case "zones_and_territories_record":
                                            prevNode = zones;
                                            textXml = "<" + reader.Name + ">";
                                            break;
                                        default:
                                            textXml += "<" + reader.Name + ">";
                                            break;
                                    }
                                    prevTag = reader.Name;
                                    break;
                                case XmlNodeType.Text:
                                    textXml += reader.Value;

                                    if (isSk_id)
                                    {
                                        try
                                        {
                                            float value = float.Parse(reader.Value.Replace('.', ','));
                                        }
                                        catch
                                        {
                                            foundCadNumber = false;
                                        }
                                        finally
                                        {
                                            isSk_id = false;
                                        }
                                    }

                                    if (foundCadNumber)
                                    {
                                        foundCadNumber = false;
                                        addedNode = prevNode.Nodes.Add(reader.Value);
                                    }
                                    break;
                                case XmlNodeType.EndElement:
                                switch (reader.Name)
                                {
                                    case "land_record":
                                        textXml += "</" + reader.Name + ">";
                                        contentOfNodes.Add(addedNode, textXml);
                                        break;
                                    case "build_record":
                                        textXml += "</" + reader.Name + ">";
                                        contentOfNodes.Add(addedNode, textXml);
                                        break;
                                    case "construction_record":
                                        textXml += "</" + reader.Name + ">";
                                        contentOfNodes.Add(addedNode, textXml);
                                        break;
                                    case "entity_spatial":
                                        textXml += "</" + reader.Name + ">";
                                        if (isRightES)
                                        { 
                                            contentOfNodes.Add(addedNode, textXml);
                                        }
                                        isRightES = false;
                                        break;
                                    case "municipal_boundary_record": 
                                        textXml += "</" + reader.Name + ">";
                                        contentOfNodes.Add(addedNode, textXml);
                                        break;
                                    case "zones_and_territories_record": 
                                        textXml += "</" + reader.Name + ">";
                                        contentOfNodes.Add(addedNode, textXml);
                                        break;
                                    default:
                                        textXml += "</" + reader.Name + ">";
                                        break;
                                }
                                break;

                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при загрузке файла!");
                    }
                    finally
                    {
                        //закрываем считывание
                        if (reader != null)
                            reader.Close();
                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Ошибка доступа.\n\nСообщение об ошибке: {ex.Message}\n\n" +
                    $"Детали:\n\n{ex.StackTrace}");
                }
            }
        }

        //функция для красивого вывода xml с отступами
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

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                //выбираем ноду с фокусом. Ищем её в словаре. Если находим, в текстбокс выводим данные.
                TreeView selectedNode = (TreeView)sender;
                string result = contentOfNodes[selectedNode.SelectedNode];
                richTextBox1.Text = FormatXml(result);
            }
            catch
            {
                richTextBox1.Text = "";
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Length > 0)
            {

                saveFileDialog1.CreatePrompt = true;
                saveFileDialog1.OverwritePrompt = true;
                saveFileDialog1.FileName = "cadastr";
                saveFileDialog1.DefaultExt = "xml";
                saveFileDialog1.Filter =
                    "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                saveFileDialog1.InitialDirectory =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                DialogResult result = saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName.Length == 0)
                {
                    MessageBox.Show("Заполните имя файла!");
                }

                else if (result == DialogResult.OK)
                {
                    string[] arr_text = richTextBox1.Lines;
                    //richTextBox1.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                    File.WriteAllLines(saveFileDialog1.FileName, arr_text, Encoding.GetEncoding("utf-8"));
                }
            }
            else
            {
                MessageBox.Show("Не выбрана нода для сохранения!");
            }
        }
    }
}
