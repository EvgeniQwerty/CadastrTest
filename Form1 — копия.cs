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

namespace CadastrTest
{
    public partial class Form1 : Form
    {
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
                    var sr = new StreamReader(openFileDialog1.FileName);
                    XmlTextReader reader = null;
                    TreeNode node, prevNode, landRecords, buildRecords, constructionRecords, spatialData, municipalBoundaries, zones;
                    bool findCadNumber = false;

                    //try
                    //{
                        //считываем xml без пустых нод
                        reader = new XmlTextReader(sr);
                        reader.WhitespaceHandling = WhitespaceHandling.None;

                        treeView1.Nodes.Clear(); //очистка перед загрузкой
                        landRecords = treeView1.Nodes.Add("land_records");
                        buildRecords = treeView1.Nodes.Add("build_records");
                        constructionRecords = treeView1.Nodes.Add("construction_records");
                        spatialData = treeView1.Nodes.Add("spatial_data");
                        municipalBoundaries = treeView1.Nodes.Add("municipal_boundaries");
                        zones = treeView1.Nodes.Add("zones_and_territories_boundaries");
                        prevNode = landRecords;


                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    switch (reader.Name)
                                    {
                                        case "land_record":
                                            prevNode = landRecords;
                                            break;
                                        case "cad_number":
                                            findCadNumber = true;
                                            break;
                                        case "build_record":
                                            prevNode = buildRecords;
                                            break;
                                        case "construction_record":
                                            prevNode = constructionRecords;
                                            break;
                                        case "entity_spatial":
                                            prevNode = spatialData;
                                            break;
                                        case "sk_id":
                                            findCadNumber = true;
                                            break;
                                        case "municipal_boundary_record":
                                            prevNode = municipalBoundaries;
                                            break;
                                        case "reg_numb_border":
                                            findCadNumber = true;
                                            break;
                                        case "zones_and_territories_record":
                                            prevNode = zones;
                                            break;
                                    }
                                    break;
                                case XmlNodeType.Text:
                                    if (findCadNumber)
                                    {
                                        findCadNumber = false;
                                        prevNode.Nodes.Add(reader.Value);
                                    }
                                    break;
                                case XmlNodeType.EntityReference:
                                   //Console.Write(reader.Name);
                                    break;
                                case XmlNodeType.EndElement:
                                    //Console.Write("</{0}>", reader.Name);
                                    break;

                            }
                        }
                    //}
                    //catch
                    //{
                    //    MessageBox.Show("Выбран неверный файл!");
                    //}
                    //finally
                    //{
                        if (reader != null)
                            reader.Close();
                    //}
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Ошибка доступа.\n\nСообщение об ошибке: {ex.Message}\n\n" +
                    $"Детали:\n\n{ex.StackTrace}");
                }
            }
        }
    }
}
