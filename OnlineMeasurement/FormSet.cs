using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineMeasurement
{
    public partial class FormSet : Form
    {
        bool isAlter = false;
        Dictionary<string, Car> 车型参数 = new Dictionary<string, Car>();
        OtherSet otherSet = new OtherSet();
        public bool IsSave = false;

        Dictionary<int, string> checkDic = new Dictionary<int, string> {
            {0, "孔"} ,
            {1, "棱"} ,
            {2, "槽"} ,
        };
        Dictionary<string, int> checkDicReversed;

        public FormSet()
        {
            InitializeComponent();

            // 初始化界面
            GeneralFunc.ChangeLanguateFun(typeof(FormSet), this);


            checkDicReversed = checkDic
    .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            comboBox_type.Items.AddRange(new string[] { Resources.LanguageDic.hole, Resources.LanguageDic.edge, Resources.LanguageDic.trough });

            groupBox绿色范围值.ContextMenuStrip = new ContextMenuStrip();
            groupBox绿色范围值.ContextMenuStrip.Opening += (s, e) =>
            {
                ContextMenuStrip contextMenuStrip = (ContextMenuStrip)s;
                if (contextMenuStrip.Items.Count > 0)
                {
                    contextMenuStrip.Items.Clear();
                }
                ToolStripMenuItem tool0 = new ToolStripMenuItem();
                tool0.Text = Resources.LanguageDic.copy_to_all_model;
                tool0.Click += (s0, e0) =>
                {
                    foreach (var item in 车型参数.Values)
                    {
                        foreach (var carSet in item.car.Values)
                        {
                            foreach (var set in carSet.gSets.Values)
                            {
                                Copy绿色范围值(set);
                            }
                        }
                    }
                    contextMenuStrip.Close();
                };
                foreach (var item in 车型参数.Values)
                {
                    ToolStripMenuItem tool1 = new ToolStripMenuItem();
                    tool1.Text = item.CarName;
                    tool1.Click += (s1, e1) =>
                    {
                        ToolStripMenuItem tool = (ToolStripMenuItem)s1;
                        foreach (var carSet in item.car.Values)
                        {
                            foreach (var set in carSet.gSets.Values)
                            {
                                Copy绿色范围值(set);
                            }
                        }
                        contextMenuStrip.Close();
                    };
                    foreach (var camName in item.car.Keys)
                    {
                        ToolStripMenuItem tool2 = new ToolStripMenuItem();
                        tool2.Text = camName;
                        tool2.Click += (s2, e2) =>
                        {
                            ToolStripMenuItem tool = (ToolStripMenuItem)s2;
                            foreach (var set in item.car[camName].gSets.Values)
                            {
                                Copy绿色范围值(set);
                            }
                            contextMenuStrip.Close();
                        };
                        foreach (var set in item.car[camName].gSets.Values)
                        {
                            ToolStripMenuItem tool3 = new ToolStripMenuItem();
                            tool3.Text = set.key.ToString();
                            tool3.Click += (s3, e3) =>
                            {
                                ToolStripMenuItem tool = (ToolStripMenuItem)s3;
                                Copy绿色范围值(set);
                                contextMenuStrip.Close();
                            };
                            tool2.DropDownItems.Add(tool3);
                        }
                        tool1.DropDownItems.Add(tool2);
                    }
                    tool0.DropDownItems.Add(tool1);
                }
                contextMenuStrip.Items.Add(tool0);
            };
        }
        private void Copy绿色范围值(GeneralSet set)
        {
            set.minDX = (float)numericUpDown_minDX.Value;
            set.minDY = (float)numericUpDown_minDY.Value;
            set.minDZ = (float)numericUpDown_minDZ.Value;
            set.maxDX = (float)numericUpDown_maxDX.Value;
            set.maxDY = (float)numericUpDown_maxDY.Value;
            set.maxDZ = (float)numericUpDown_maxDZ.Value;
            isAlter = true;
        }
        private void FormSet_Load(object sender, EventArgs e)
        {
            车型参数.Clear();

            string[] carPaths = Directory.GetDirectories("Data\\Car");
            foreach (var item in carPaths)
            {
                string dirName = Path.GetFileNameWithoutExtension(item);
                string[] strings = dirName.Split('-');
                if (strings.Length == 3 && int.TryParse(strings[0], out int 车型号) && int.TryParse(strings[1], out int 托盘号))
                {
                    Car car = new Car(dirName);
                    car.LoadGeneralSet();
                    车型参数.Add(dirName, car);
                }
            }

            treeView1.Nodes.Clear();
            var node1 = treeView1.Nodes.Add(Resources.LanguageDic.root_node);
            foreach (var key1 in 车型参数.Keys)
            {
                var node2 = node1.Nodes.Add(key1);//10-1-3UG、...
                foreach (var key2 in 车型参数[key1].car.Keys)
                {
                    var node3 = node2.Nodes.Add(key2);//L、R

                    var keys3 = 车型参数[key1].car[key2].gSets.Keys.OrderBy(n => { return n; });//排序
                    foreach (var key3 in keys3)
                    {
                        var node4 = node3.Nodes.Add(key3.ToString());//1、2、3、...
                    }
                }
            }

            otherSet.Load();
            checkBox_isSaveImage.Checked = otherSet.isSaveImage;
            textBox_imagePath.Text = otherSet.imagePath;
            this.checkBox_isSaveImage.CheckedChanged += new System.EventHandler(this.checkBox_isSaveImage_CheckedChanged);
            this.textBox_imagePath.TextChanged += new System.EventHandler(this.textBox_imagePath_TextChanged);
        }

        private void EnableUpData()
        {
            numericUpDownSleepTime.ValueChanged += UpData;
            comboBox_type.TextChanged += UpData;
            checkBox_isBase.CheckedChanged += UpData;
            numericUpDown_ledExposure.ValueChanged += UpData;
            numericUpDown_lightExposure.ValueChanged += UpData;
            numericUpDown_score0.ValueChanged += UpData;
            numericUpDown_score1.ValueChanged += UpData;
            numericUpDown_score2.ValueChanged += UpData;
            numericUpDown_X.ValueChanged += UpData;
            numericUpDown_Y.ValueChanged += UpData;
            numericUpDown_Z.ValueChanged += UpData;
            numericUpDown_minDX.ValueChanged += UpData;
            numericUpDown_minDY.ValueChanged += UpData;
            numericUpDown_minDZ.ValueChanged += UpData;
            numericUpDown_maxDX.ValueChanged += UpData;
            numericUpDown_maxDY.ValueChanged += UpData;
            numericUpDown_maxDZ.ValueChanged += UpData;
            numericUpDown_offsetX.ValueChanged += UpData;
            numericUpDown_offsetY.ValueChanged += UpData;
            numericUpDown_offsetZ.ValueChanged += UpData;
            numericUpDown_pX.ValueChanged += UpData;
            numericUpDown_pY.ValueChanged += UpData;
            numericUpDown_pZ.ValueChanged += UpData;
            numericUpDown_pRX.ValueChanged += UpData;
            numericUpDown_pRY.ValueChanged += UpData;
            numericUpDown_pRZ.ValueChanged += UpData;
        }
        private void DisenableUpData()
        {
            numericUpDownSleepTime.ValueChanged -= UpData;
            comboBox_type.TextChanged -= UpData;
            checkBox_isBase.CheckedChanged -= UpData;
            numericUpDown_ledExposure.ValueChanged -= UpData;
            numericUpDown_lightExposure.ValueChanged -= UpData;
            numericUpDown_score0.ValueChanged -= UpData;
            numericUpDown_score1.ValueChanged -= UpData;
            numericUpDown_score2.ValueChanged -= UpData;
            numericUpDown_X.ValueChanged -= UpData;
            numericUpDown_Y.ValueChanged -= UpData;
            numericUpDown_Z.ValueChanged -= UpData;
            numericUpDown_minDX.ValueChanged -= UpData;
            numericUpDown_minDY.ValueChanged -= UpData;
            numericUpDown_minDZ.ValueChanged -= UpData;
            numericUpDown_maxDX.ValueChanged -= UpData;
            numericUpDown_maxDY.ValueChanged -= UpData;
            numericUpDown_maxDZ.ValueChanged -= UpData;
            numericUpDown_offsetX.ValueChanged -= UpData;
            numericUpDown_offsetY.ValueChanged -= UpData;
            numericUpDown_offsetZ.ValueChanged -= UpData;
            numericUpDown_pX.ValueChanged -= UpData;
            numericUpDown_pY.ValueChanged -= UpData;
            numericUpDown_pZ.ValueChanged -= UpData;
            numericUpDown_pRX.ValueChanged -= UpData;
            numericUpDown_pRY.ValueChanged -= UpData;
            numericUpDown_pRZ.ValueChanged -= UpData;
        }

        private void UpData(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Level != 3)
            {
                return;
            }
            isAlter = true;
            var set = 车型参数[treeView1.SelectedNode.Parent.Parent.Text].car[treeView1.SelectedNode.Parent.Text].gSets[int.Parse(treeView1.SelectedNode.Text)];

            set.sleepTime = (int)numericUpDownSleepTime.Value;

            if (checkDic.ContainsKey(comboBox_type.SelectedIndex))
            {
                set.type = checkDic[comboBox_type.SelectedIndex];
            }
            else
            {
                //set.type = comboBox_type.Text;
            }

            set.isBase = checkBox_isBase.Checked;
            set.ledExposure = (int)numericUpDown_ledExposure.Value;
            set.lightExposure = (int)numericUpDown_lightExposure.Value;
            set.score0 = (double)numericUpDown_score0.Value;
            set.score1 = (double)numericUpDown_score1.Value;
            set.score2 = (double)numericUpDown_score2.Value;
            set.X = (float)numericUpDown_X.Value;
            set.Y = (float)numericUpDown_Y.Value;
            set.Z = (float)numericUpDown_Z.Value;
            set.minDX = (float)numericUpDown_minDX.Value;
            set.minDY = (float)numericUpDown_minDY.Value;
            set.minDZ = (float)numericUpDown_minDZ.Value;
            set.maxDX = (float)numericUpDown_maxDX.Value;
            set.maxDY = (float)numericUpDown_maxDY.Value;
            set.maxDZ = (float)numericUpDown_maxDZ.Value;
            set.offsetX = (float)numericUpDown_offsetX.Value;
            set.offsetY = (float)numericUpDown_offsetY.Value;
            set.offsetZ = (float)numericUpDown_offsetZ.Value;
            set.pX = (double)numericUpDown_pX.Value;
            set.pY = (double)numericUpDown_pY.Value;
            set.pZ = (double)numericUpDown_pZ.Value;
            set.pRX = (double)numericUpDown_pRX.Value;
            set.pRY = (double)numericUpDown_pRY.Value;
            set.pRZ = (double)numericUpDown_pRZ.Value;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 3)
            {
                DisenableUpData();

                panel1.Visible = true;
                var set = 车型参数[e.Node.Parent.Parent.Text].car[e.Node.Parent.Text].gSets[int.Parse(e.Node.Text)];
                label_Node.Text = e.Node.Parent.Parent.Text + "/" + e.Node.Parent.Text + "/" + e.Node.Text;

                numericUpDownSleepTime.Value = set.sleepTime;
                if (checkDicReversed.ContainsKey(set.type))
                {
                    comboBox_type.SelectedIndex = checkDicReversed[set.type];
                }
                else 
                {
                    comboBox_type.Text = set.type;
                }
                checkBox_isBase.Checked = set.isBase;
                numericUpDown_ledExposure.Value = set.ledExposure;
                numericUpDown_lightExposure.Value = (int)set.lightExposure;
                numericUpDown_score0.Value = (decimal)set.score0;
                numericUpDown_score1.Value = (decimal)set.score1;
                numericUpDown_score2.Value = (decimal)set.score2;
                numericUpDown_X.Value = (decimal)set.X;
                numericUpDown_Y.Value = (decimal)set.Y;
                numericUpDown_Z.Value = (decimal)set.Z;
                numericUpDown_minDX.Value = (decimal)set.minDX;
                numericUpDown_minDY.Value = (decimal)set.minDY;
                numericUpDown_minDZ.Value = (decimal)set.minDZ;
                numericUpDown_maxDX.Value = (decimal)set.maxDX;
                numericUpDown_maxDY.Value = (decimal)set.maxDY;
                numericUpDown_maxDZ.Value = (decimal)set.maxDZ;
                numericUpDown_offsetX.Value = (decimal)set.offsetX;
                numericUpDown_offsetY.Value = (decimal)set.offsetY;
                numericUpDown_offsetZ.Value = (decimal)set.offsetZ;
                numericUpDown_pX.Value = (decimal)set.pX;
                numericUpDown_pY.Value = (decimal)set.pY;
                numericUpDown_pZ.Value = (decimal)set.pZ;
                numericUpDown_pRX.Value = (decimal)set.pRX;
                numericUpDown_pRY.Value = (decimal)set.pRY;
                numericUpDown_pRZ.Value = (decimal)set.pRZ;

                EnableUpData();
            }
            else
            {
                panel1.Visible = false;
            }
        }

        private void FormSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isAlter)
            {
                DialogResult dialogResult = MessageBox.Show($"{Resources.LanguageDic.is_save_para}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    foreach (var key1 in 车型参数.Keys)
                    {
                        if (!车型参数[key1].SaveGeneralSet())
                        {
                            MessageBox.Show($"{Resources.LanguageDic.save}{key1}{Resources.LanguageDic.fail}！！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                            return;
                        }
                    }
                    otherSet.Save();
                    isAlter = false;
                    IsSave = true;
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Level == 0)//根节点
            {
                FormName frm = new FormName();
                List<string> list = new List<string>();
                foreach (TreeNode item in treeView1.SelectedNode.Nodes)
                {
                    list.Add(item.Text);
                }
                frm.InNames = list.ToArray();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    车型参数.Add(frm.NameValue, new Car(frm.NameValue));//10-1-3UG、...
                    treeView1.SelectedNode.Nodes.Add(frm.NameValue);
                    isAlter = true;
                }
            }
            if (treeView1.SelectedNode.Level == 1)//10-1-3UG、...
            {
                FormName frm = new FormName();
                List<string> list = new List<string>();
                foreach (TreeNode item in treeView1.SelectedNode.Nodes)
                {
                    list.Add(item.Text);
                }
                frm.InNames = list.ToArray();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    车型参数[treeView1.SelectedNode.Text].car.Add(frm.NameValue, new CarSetting());//L、R
                    treeView1.SelectedNode.Nodes.Add(frm.NameValue);
                    isAlter = true;
                }
            }
            if (treeView1.SelectedNode.Level == 2)//L、R
            {
                FormName frm = new FormName();
                List<string> list = new List<string>();
                foreach (TreeNode item in treeView1.SelectedNode.Nodes)
                {
                    list.Add(item.Text);
                }
                frm.InNames = list.ToArray();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    if (int.TryParse(frm.NameValue, out int i))
                    {
                        GeneralSet set = new GeneralSet();
                        set.key = i;
                        车型参数[treeView1.SelectedNode.Parent.Text].car[treeView1.SelectedNode.Text].gSets.Add(i, set);
                        treeView1.SelectedNode.Nodes.Add(frm.NameValue);
                        isAlter = true;
                    }
                    else
                    {
                        MessageBox.Show($"{Resources.LanguageDic.need_to_input_num}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Level == 0)//根节点
            {
                //车型参数.Clear();
            }
            if (treeView1.SelectedNode.Level == 1)//10-1-3UG、...
            {
                if (车型参数.Remove(treeView1.SelectedNode.Text))
                {
                    treeView1.SelectedNode.Remove();
                    isAlter = true;
                }
            }
            if (treeView1.SelectedNode.Level == 2)//L、R
            {
                if (车型参数[treeView1.SelectedNode.Parent.Text].car.Remove(treeView1.SelectedNode.Text))
                {
                    treeView1.SelectedNode.Remove();
                    isAlter = true;
                }
            }
            if (treeView1.SelectedNode.Level == 3)//1、2、3、...
            {
                if (车型参数[treeView1.SelectedNode.Parent.Parent.Text].car[treeView1.SelectedNode.Parent.Text].gSets.Remove(int.Parse(treeView1.SelectedNode.Text)))
                {
                    treeView1.SelectedNode.Remove();
                    isAlter = true;
                }
            }
        }

        private void FormSet_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;//抗锯齿

            GraphicsPath graphicsPath = new GraphicsPath();
            if (control.ClientRectangle.Width > 0 && control.ClientRectangle.Height > 0)
            {
                graphicsPath.AddRectangle(control.ClientRectangle);
                LinearGradientBrush brush = new LinearGradientBrush(control.ClientRectangle, Color.FromArgb(100, 212, 225), Color.FromArgb(100, 162, 225), LinearGradientMode.BackwardDiagonal);
                g.FillPath(brush, graphicsPath);
            }
        }

        private void checkBox_isSaveImage_CheckedChanged(object sender, EventArgs e)
        {
            isAlter = true;
            otherSet.isSaveImage = checkBox_isSaveImage.Checked;
        }

        private void textBox_imagePath_TextChanged(object sender, EventArgs e)
        {
            var chars = Path.GetInvalidPathChars();
            foreach (char c in chars)
            {
                if (textBox_imagePath.Text.Contains(c))
                {
                    MessageBox.Show($"{Resources.LanguageDic.the_path_contains_illegal_characters}" + c);
                    textBox_imagePath.Text = otherSet.imagePath;
                    return;
                }
            }
            isAlter = true;
            otherSet.imagePath = textBox_imagePath.Text;
        }

        private void button_offsetXAdd_Click(object sender, EventArgs e)
        {
            FormValue formValue = new FormValue();
            if (formValue.ShowDialog() == DialogResult.OK)
            {
                numericUpDown_offsetX.Value += (decimal)formValue.Value;
            }
        }

        private void button_offsetYAdd_Click(object sender, EventArgs e)
        {
            FormValue formValue = new FormValue();
            if (formValue.ShowDialog() == DialogResult.OK)
            {
                numericUpDown_offsetY.Value += (decimal)formValue.Value;
            }
        }

        private void button_offsetZAdd_Click(object sender, EventArgs e)
        {
            FormValue formValue = new FormValue();
            if (formValue.ShowDialog() == DialogResult.OK)
            {
                numericUpDown_offsetZ.Value += (decimal)formValue.Value;
            }
        }
    }
}
