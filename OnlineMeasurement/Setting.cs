using HalconDotNet;
using HslCommunication.Secs.Types;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OnlineMeasurement
{
    //一个车型下的所有参数
    public class Car
    {
        public string CarName;
        public Car(string carName)
        {
            CarName = carName;
        }
        /// <summary>
        /// 相机名-车型参数
        /// </summary>
        public Dictionary<string, CarSetting> car = new Dictionary<string, CarSetting>();

        public bool Load()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName;
            if (!Load(basePath))
            {
                //string basePath_bak = AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName + "_bak\\";
                //if (!Load(basePath_bak))
                //{
                return false;
                //}
                //else
                //{
                //    CopyDirectory(basePath_bak, basePath);
                //}
            }
            return true;
        }
        bool Load(string basePath)
        {
            bool result = true;
            if (Directory.Exists(basePath))
            {
                string[] strings = Directory.GetDirectories(basePath);
                if (strings.Length > 0)
                {
                    foreach (var item in strings)//L、R
                    {
                        var carSetting = new CarSetting();
                        if (!carSetting.Load(item))
                        {
                            result = false;
                        }
                        string camName = Path.GetFileNameWithoutExtension(item);
                        if (car.ContainsKey(camName))
                        {
                            car[camName] = carSetting;
                        }
                        else
                        {
                            car.Add(camName, carSetting);
                        }
                    }
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public bool LoadGeneralSet()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName;
            if (!LoadGeneralSet(basePath))
            {
                //string basePath_bak = AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName + "_bak\\";
                //if (!Load(basePath_bak))
                //{
                return false;
                //}
                //else
                //{
                //    CopyDirectory(basePath_bak, basePath);
                //}
            }
            return true;
        }
        bool LoadGeneralSet(string basePath)
        {
            bool result = true;
            if (Directory.Exists(basePath))
            {
                string[] strings = Directory.GetDirectories(basePath);
                if (strings.Length > 0)
                {
                    foreach (var item in strings)//L、R
                    {
                        var carSetting = new CarSetting();
                        if (!carSetting.LoadGeneralSet(item))
                        {
                            result = false;
                        }
                        string camName = Path.GetFileNameWithoutExtension(item);
                        if (car.ContainsKey(camName))
                        {
                            car[camName] = carSetting;
                        }
                        else
                        {
                            car.Add(camName, carSetting);
                        }
                    }
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public bool Save()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName;
            foreach (var item in car.Keys)//L、R
            {
                string path = basePath + "\\" + item;
                if (!car[item].Save(path))
                {
                    result = false;
                }
            }

            //if (result)
            //{
            //    CopyDirectory(basePath, AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName + "_bak");
            //}
            return result;
        }
        public bool SaveGeneralSet()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName;
            foreach (var item in car.Keys)//L、R
            {
                string path = basePath + "\\" + item;
                if (!car[item].SaveGeneralSet(path))
                {
                    result = false;
                }
            }

            //if (result)
            //{
            //    CopyDirectory(basePath, AppDomain.CurrentDomain.BaseDirectory + "Data\\Car\\" + CarName + "_bak");
            //}
            return result;
        }

        private void CopyDirectory(string sourcePath, string destPath)
        {
            string floderName = Path.GetFileName(sourcePath);
            DirectoryInfo di = Directory.CreateDirectory(Path.Combine(destPath, floderName));
            string[] files = Directory.GetFileSystemEntries(sourcePath);

            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    CopyDirectory(file, di.FullName);
                }
                else
                {
                    File.Copy(file, Path.Combine(di.FullName, Path.GetFileName(file)), true);
                }
            }
        }
    }
    //车型下一个相机的参数
    public class CarSetting
    {
        /// <summary>
        /// 常规参数
        /// </summary>
        public Dictionary<int, GeneralSet> gSets = new Dictionary<int, GeneralSet>();

        /// <summary>
        /// 匹配识别模板
        /// </summary>
        public Dictionary<int, HNCCModel> Models0 = new Dictionary<int, HNCCModel>();
        /// <summary>
        /// 匹配识别模板
        /// </summary>
        public Dictionary<int, HShapeModel> ShapeModels0 = new Dictionary<int, HShapeModel>();
        /// <summary>
        /// 匹配识别模板
        /// </summary>
        public Dictionary<int, HNCCModel> Models1 = new Dictionary<int, HNCCModel>();
        /// <summary>
        /// 匹配识别模板
        /// </summary>
        public Dictionary<int, HNCCModel> Models2 = new Dictionary<int, HNCCModel>();

        //搜索区域
        public Dictionary<int, HRegion> rectangle0 = new Dictionary<int, HRegion>();
        public Dictionary<int, HRegion> rectangle1 = new Dictionary<int, HRegion>();
        public Dictionary<int, HRegion> rectangle2 = new Dictionary<int, HRegion>();

        public Dictionary<int, PointF> modeCenter0 = new Dictionary<int, PointF>();
        public Dictionary<int, PointF> modeCenter1 = new Dictionary<int, PointF>();
        public Dictionary<int, PointF> modeCenter2 = new Dictionary<int, PointF>();

        /// <summary>
        /// 基座标转车身矩阵
        /// </summary>
        public HHomMat3D robot2Car;

        public bool Load(string basePath)
        {

            bool result2 = LoadGeneralSet(basePath);

            bool result5 = true;
            try
            {
                string modelBasePath = basePath + "\\Model\\";
                if (Directory.Exists(modelBasePath))
                {
                    string[] shapeModelPaths = Directory.GetFiles(modelBasePath, "*.shm");
                    if (shapeModelPaths.Length > 0)
                    {
                        for (int i = 0; i < shapeModelPaths.Length; i++)
                        {
                            string[] names = Path.GetFileNameWithoutExtension(shapeModelPaths[i]).Split('_');
                            if (names.Length == 2 && int.TryParse(names[0], out int key))
                            {
                                string kind = names[1];
                                if (kind == "0")
                                {
                                    if (ShapeModels0.ContainsKey(key))
                                    {
                                        if (ShapeModels0[key] != null)
                                        {
                                            ShapeModels0[key].Dispose();
                                        }
                                        ShapeModels0[key] = new HShapeModel(shapeModelPaths[i]);
                                    }
                                    else
                                    {
                                        ShapeModels0.Add(key, new HShapeModel(shapeModelPaths[i]));
                                    }
                                }
                            }
                        }
                    }

                    string[] modelPaths = Directory.GetFiles(modelBasePath, "*.ncc");
                    if (modelPaths.Length > 0)
                    {
                        for (int i = 0; i < modelPaths.Length; i++)
                        {
                            string[] names = Path.GetFileNameWithoutExtension(modelPaths[i]).Split('_');
                            if (names.Length == 2 && int.TryParse(names[0], out int key))
                            {
                                string kind = names[1];
                                if (kind == "0")
                                {
                                    if (Models0.ContainsKey(key))
                                    {
                                        if (Models0[key] != null)
                                        {
                                            Models0[key].Dispose();
                                        }
                                        Models0[key] = new HNCCModel(modelPaths[i]);
                                    }
                                    else
                                    {
                                        Models0.Add(key, new HNCCModel(modelPaths[i]));
                                    }
                                }
                                else if (kind == "1")
                                {
                                    if (Models1.ContainsKey(key))
                                    {
                                        if (Models1[key] != null)
                                        {
                                            Models1[key].Dispose();
                                        }
                                        Models1[key] = new HNCCModel(modelPaths[i]);
                                    }
                                    else
                                    {
                                        Models1.Add(key, new HNCCModel(modelPaths[i]));
                                    }
                                }
                                else if (kind == "2")
                                {
                                    if (Models2.ContainsKey(key))
                                    {
                                        if (Models2[key] != null)
                                        {
                                            Models2[key].Dispose();
                                        }
                                        Models2[key] = new HNCCModel(modelPaths[i]);
                                    }
                                    else
                                    {
                                        Models2.Add(key, new HNCCModel(modelPaths[i]));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //如果检测点数不为零，则初始化失败
                        if (gSets.Count != 0)
                        {
                            result5 = false;
                        }
                    }

                    string[] txtPaths = Directory.GetFiles(modelBasePath, "*.txt");
                    if (txtPaths.Length > 0)
                    {
                        for (int i = 0; i < txtPaths.Length; i++)
                        {
                            string[] names = Path.GetFileNameWithoutExtension(txtPaths[i]).Split('_');
                            if (names.Length == 2 && int.TryParse(names[0], out int key))
                            {
                                string kind = names[1];
                                if (kind == "0")
                                {
                                    using (StreamReader reader = new StreamReader(txtPaths[i], Encoding.UTF8))
                                    {
                                        string[] strings = reader.ReadLine().Split(',');
                                        if (strings.Length == 4 && double.TryParse(strings[0], out double row1) && double.TryParse(strings[1], out double column1)
                                            && double.TryParse(strings[2], out double row2) && double.TryParse(strings[3], out double column2))
                                        {
                                            new HImage("byte", (int)(column2 + 1), (int)(row2 + 1)).Dispose();//类似于画布，要能覆盖下面创建的区域
                                            if (rectangle0.ContainsKey(key))
                                            {
                                                rectangle0[key] = new HRegion(row1, column1, row2, column2);
                                            }
                                            else
                                            {
                                                rectangle0.Add(key, new HRegion(row1, column1, row2, column2));
                                            }
                                        }

                                        string[] printFs = reader.ReadLine().Split(',');
                                        if (printFs.Length == 2 && double.TryParse(printFs[0], out double row) && double.TryParse(printFs[1], out double column))
                                        {
                                            if (modeCenter0.ContainsKey(key))
                                            {
                                                modeCenter0[key] = new PointF((float)column, (float)row);
                                            }
                                            else
                                            {
                                                modeCenter0.Add(key, new PointF((float)column, (float)row));
                                            }
                                        }
                                    }
                                }
                                else if (kind == "1")
                                {
                                    using (StreamReader reader = new StreamReader(txtPaths[i], Encoding.UTF8))
                                    {
                                        string[] strings = reader.ReadLine().Split(',');
                                        if (strings.Length == 4 && double.TryParse(strings[0], out double row1) && double.TryParse(strings[1], out double column1)
                                            && double.TryParse(strings[2], out double row2) && double.TryParse(strings[3], out double column2))
                                        {
                                            new HImage("byte", (int)(column2 + 1), (int)(row2 + 1)).Dispose();//类似于画布，要能覆盖下面创建的区域
                                            if (rectangle1.ContainsKey(key))
                                            {
                                                rectangle1[key] = new HRegion(row1, column1, row2, column2);
                                            }
                                            else
                                            {
                                                rectangle1.Add(key, new HRegion(row1, column1, row2, column2));
                                            }
                                        }

                                        string[] printFs = reader.ReadLine().Split(',');
                                        if (printFs.Length == 2 && double.TryParse(printFs[0], out double row) && double.TryParse(printFs[1], out double column))
                                        {
                                            if (modeCenter1.ContainsKey(key))
                                            {
                                                modeCenter1[key] = new PointF((float)column, (float)row);
                                            }
                                            else
                                            {
                                                modeCenter1.Add(key, new PointF((float)column, (float)row));
                                            }
                                        }
                                    }
                                }
                                else if (kind == "2")
                                {
                                    using (StreamReader reader = new StreamReader(txtPaths[i], Encoding.UTF8))
                                    {
                                        string[] strings = reader.ReadLine().Split(',');
                                        if (strings.Length == 4 && double.TryParse(strings[0], out double row1) && double.TryParse(strings[1], out double column1)
                                            && double.TryParse(strings[2], out double row2) && double.TryParse(strings[3], out double column2))
                                        {
                                            new HImage("byte", (int)(column2 + 1), (int)(row2 + 1)).Dispose();//类似于画布，要能覆盖下面创建的区域
                                            if (rectangle2.ContainsKey(key))
                                            {
                                                rectangle2[key] = new HRegion(row1, column1, row2, column2);
                                            }
                                            else
                                            {
                                                rectangle2.Add(key, new HRegion(row1, column1, row2, column2));
                                            }
                                        }

                                        string[] printFs = reader.ReadLine().Split(',');
                                        if (printFs.Length == 2 && double.TryParse(printFs[0], out double row) && double.TryParse(printFs[1], out double column))
                                        {
                                            if (modeCenter2.ContainsKey(key))
                                            {
                                                modeCenter2[key] = new PointF((float)column, (float)row);
                                            }
                                            else
                                            {
                                                modeCenter2.Add(key, new PointF((float)column, (float)row));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //如果检测点数不为零，则初始化失败
                        if (gSets.Count != 0)
                        {
                            result5 = false;
                        }
                    }
                }
                else
                {
                    //如果检测点数不为零，则初始化失败
                    if (gSets.Count != 0)
                    {
                        result5 = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result5 = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            bool result6 = false;
            try
            {
                string path = basePath + "\\基座标转车身矩阵";
                if (File.Exists(path))
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open))
                    {
                        robot2Car = HHomMat3D.Deserialize(stream);
                        result6 = true;
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            return result2 && result5 && result6;
        }

        public bool LoadGeneralSet(string basePath)
        {

            bool result2 = false;
            try
            {
                string gPath = basePath + "\\Setting.xml";
                if (File.Exists(gPath))
                {
                    List<GeneralSet> listgSets = new List<GeneralSet>();
                    XmlSerializer xmlgSets = new XmlSerializer(listgSets.GetType());
                    using (FileStream stream = new FileStream(gPath, FileMode.OpenOrCreate))
                    {
                        listgSets = (List<GeneralSet>)xmlgSets.Deserialize(stream);
                    }
                    if (listgSets != null)
                    {
                        gSets = listgSets.ToDictionary(n => { return n.key; });
                        result2 = true;
                    }
                }
                //else
                //{
                //    gSets.Add(0, new GeneralSet());
                //    Save(basePath);
                //}
            }
            catch (Exception ex)
            {
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            return result2;
        }

        public bool Save(string basePath)
        {
            bool result = SaveGeneralSet(basePath);
            try
            {
                //string modelBasePath = basePath + "\\Model\\";
                //if (!Directory.Exists(modelBasePath))
                //{
                //    Directory.CreateDirectory(modelBasePath);
                //}
                //foreach (var item in Models0.Keys)
                //{
                //    string name = item.ToString("00");
                //    string modelPath = modelBasePath + name + "_0.ncc";
                //    Models0[item].WriteNccModel(modelPath);
                //}
                //foreach (var item in Models1.Keys)
                //{
                //    string name = item.ToString("00");
                //    string modelPath = modelBasePath + name + "_1.ncc";
                //    Models1[item].WriteNccModel(modelPath);
                //}
                //foreach (var item in Models2.Keys)
                //{
                //    string name = item.ToString("00");
                //    string modelPath = modelBasePath + name + "_2.ncc";
                //    Models2[item].WriteNccModel(modelPath);
                //}
                //foreach (var item in rectangle0.Keys)
                //{
                //    string name = item.ToString("00");
                //    string txtPath = modelBasePath + name + "_0.txt";
                //    using (StreamWriter writer = new StreamWriter(txtPath, false, Encoding.UTF8))
                //    {
                //        rectangle0[item].SmallestRectangle1(out int row1, out int column1, out int row2, out int column2);
                //        writer.WriteLine($"{row1},{column1},{row2},{column2}");
                //        writer.WriteLine($"{modeCenter0[item].Y},{modeCenter0[item].X}");                       
                //    }
                //}
                //foreach (var item in rectangle1.Keys)
                //{
                //    string name = item.ToString("00");
                //    string txtPath = modelBasePath + name + "_1.txt";
                //    using (StreamWriter writer = new StreamWriter(txtPath, false, Encoding.UTF8))
                //    {
                //        rectangle1[item].SmallestRectangle1(out int row1, out int column1, out int row2, out int column2);
                //        writer.WriteLine($"{row1},{column1},{row2},{column2}");
                //        writer.WriteLine($"{modeCenter1[item].Y},{modeCenter1[item].X}");
                //    }
                //}
                //foreach (var item in rectangle2.Keys)
                //{
                //    string name = item.ToString("00");
                //    string txtPath = modelBasePath + name + "_2.txt";
                //    using (StreamWriter writer = new StreamWriter(txtPath, false, Encoding.UTF8))
                //    {
                //        rectangle2[item].SmallestRectangle1(out int row1, out int column1, out int row2, out int column2);
                //        writer.WriteLine($"{row1},{column1},{row2},{column2}");
                //        writer.WriteLine($"{modeCenter2[item].Y},{modeCenter2[item].X}");
                //    }
                //}

                if (robot2Car != null)
                {
                    string path = basePath + "\\基座标转车身矩阵";
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        robot2Car.Serialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            return result;
        }

        public bool SaveGeneralSet(string basePath)
        {
            bool result = true;
            try
            {
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                List<GeneralSet> listgSets = gSets.Values.OrderBy(n => { return n.key; }).ToList();//排序
                string fPath = basePath + "\\Setting.xml";
                XmlSerializer xml = new XmlSerializer(listgSets.GetType());
                using (FileStream stream = new FileStream(fPath, FileMode.Create))
                {
                    xml.Serialize(stream, listgSets);
                }
            }
            catch (Exception ex)
            {
                result = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            return result;
        }

        //public bool Find0PxPy(HImage hImage, int index, out HTuple row, out HTuple column, out double dy, out double dx, bool bShow, out HImage hImageShow)
        //{
        //    if (rectangle0.ContainsKey(index) && Models0.ContainsKey(index) && gSets.ContainsKey(index))
        //    {
        //        HImage hImageReduced = hImage.ReduceDomain(rectangle0[index]);
        //        hImageReduced.FindNccModel(Models0[index], -10 * Math.PI / 180, 20 * Math.PI / 180, 0.5, 1, 0.5, "true", 0, out row, out column, out HTuple angle, out HTuple score);
        //        if (score.Length == 1 && score.D >= gSets[index].score0)
        //        {
        //            dy = row.D - modeCenter0[index].Y;
        //            dx = column.D - modeCenter0[index].X;

        //            hImageShow = bShow ? GetShowHIamge(hImage, rectangle0[index], modeCenter0[index].Y, modeCenter0[index].X, row, column) : null;

        //            return true;
        //        }
        //    }
        //    row = new HTuple();
        //    column = new HTuple();
        //    dy = 0;
        //    dx = 0;
        //    hImageShow = null;
        //    return false;
        //}



        //public bool Find1PxPy(HImage hImage, int index, out HTuple row, out HTuple column, out double dy, out double dx)
        //{
        //    if (rectangle1.ContainsKey(index) && Models1.ContainsKey(index) && gSets.ContainsKey(index))
        //    {
        //        HImage hImageReduced = hImage.ReduceDomain(rectangle1[index]);
        //        hImageReduced.FindNccModel(Models1[index], -10 * Math.PI / 180, 20 * Math.PI / 180, 0.5, 1, 0.5, "true", 0, out row, out column, out HTuple angle, out HTuple score);
        //        if (score.Length == 1 && score.D >= gSets[index].score1)
        //        {
        //            dy = row.D - modeCenter1[index].Y;
        //            dx = column.D - modeCenter1[index].X;
        //            return true;
        //        }
        //    }
        //    row = new HTuple();
        //    column = new HTuple();
        //    dy = 0;
        //    dx = 0;
        //    return false;
        //}
        //public bool Find2PxPy(HImage hImage, int index, out HTuple row, out HTuple column, out double dy, out double dx)
        //{
        //    if (rectangle2.ContainsKey(index) && Models2.ContainsKey(index) && gSets.ContainsKey(index))
        //    {
        //        HImage hImageReduced = hImage.ReduceDomain(rectangle2[index]);
        //        hImageReduced.FindNccModel(Models2[index], -10 * Math.PI / 180, 20 * Math.PI / 180, 0.5, 1, 0.5, "true", 0, out row, out column, out HTuple angle, out HTuple score);
        //        if (score.Length == 1 && score.D >= gSets[index].score2)
        //        {
        //            dy = row.D - modeCenter2[index].Y;
        //            dx = column.D - modeCenter2[index].X;
        //            return true;
        //        }
        //    }
        //    row = new HTuple();
        //    column = new HTuple();
        //    dy = 0;
        //    dx = 0;
        //    return false;
        //}
        public bool FindPxPy(HImage map, HRegion hRegion, HNCCModel hNCCModel, double score, double modeCenterX, double modeCenterY, ref float Px, ref float Py, out double dx, out double dy)
        {
            HImage hImageReduced = map.ReduceDomain(hRegion);
            hImageReduced.FindNccModel(hNCCModel, -10 * Math.PI / 180, 20 * Math.PI / 180, 0.5, 1, 0.5, "true", 0, out HTuple row, out HTuple column, out HTuple angle, out HTuple hscore);
            if (hscore.Length == 1 && hscore.D >= score)
            {
                dy = row.D - modeCenterY;
                dx = column.D - modeCenterX;
                Px = (float)column.D;
                Py = (float)row.D;
                return true;
            }
            dy = 0;
            dx = 0;
            return false;
        }
        public bool FindPxPy(HImage map, HRegion hRegion, HShapeModel hShapeModel, double score, double modeCenterX, double modeCenterY, ref float Px, ref float Py, out double dx, out double dy)
        {
            HImage hImageReduced = map.ReduceDomain(hRegion);
            //hImageReduced.FindShapeModel(hShapeModel, -0.1, 0.2, 0.5, 1, 0.5, "least_squares", 0, 0.9, out HTuple row, out HTuple column, out HTuple angle, out HTuple hscore);
            hImageReduced.FindScaledShapeModel(hShapeModel, 0, 0, 0.99, 1.1, 0.5, 1, 0.5, "least_squares", 0, 0.9, out HTuple row, out HTuple column, out HTuple angle, out HTuple scaled, out HTuple hscore);
            if (hscore.Length == 1 && hscore.D >= score)
            {
                dy = row.D - modeCenterY;
                dx = column.D - modeCenterX;
                Px = (float)column.D;
                Py = (float)row.D;
                return true;
            }
            dy = 0;
            dx = 0;
            return false;
        }
        public HImage GetShowHIamge(HImage hImage, HRegion hRegion, double modeCenterX, double modeCenterY, double px, double py, HShapeModel hShapeModel)
        {
            double w = 1;
            //模板十字架
            HRegion hRegionX = new HRegion();
            hRegionX.GenRectangle2(modeCenterY, modeCenterX, 0, 20, w);
            HRegion hRegionY = new HRegion();
            hRegionY.GenRectangle2(modeCenterY, modeCenterX, 0, w, 20);
            HRegion hRegionXY = hRegionY.Union2(hRegionX);
            //匹配十字架
            HRegion hRegionL = new HRegion();
            hRegionL.GenRectangle2(py, px, 0, 20, w);
            HRegion hRegionH = new HRegion();
            hRegionH.GenRectangle2(py, px, 0, w, 20);
            HRegion hRegionLH = hRegionH.Union2(hRegionL);
            //匹配模板
            HHomMat2D hHomMat2D = new HHomMat2D();
            hHomMat2D.VectorAngleToRigid(0, 0, 0, py, px, 0);
            HRegion hRegionCont = hShapeModel.GetShapeModelContours(1).AffineTransContourXld(hHomMat2D).GenRegionContourXld("margin");
            //联合所有区域
            HRegion hRegionAll = hRegion.ConcatObj(hRegionXY).ConcatObj(hRegionLH).ConcatObj(hRegionCont);
            //喷涂
            HImage hImage0 = hImage.PaintRegion(hRegionAll, 0d, "margin");
            HImage hImage1 = hImage0.PaintRegion(hRegionLH, 255d, "margin").PaintRegion(hRegionCont, 255d, "margin");//red
            HImage hImage2 = hImage0.PaintRegion(hRegionXY, 255d, "margin");//green
            HImage hImage3 = hImage0.PaintRegion(hRegion, 255d, "margin");//blue

            return hImage1.Compose3(hImage2, hImage3);
        }
        public HImage GetShowHIamge(HImage hImage, HRegion hRegion, double modeCenterX, double modeCenterY, double px, double py)
        {
            double w = 1;
            //模板十字架
            HRegion hRegionX = new HRegion();
            hRegionX.GenRectangle2(modeCenterY, modeCenterX, 0, 20, w);
            HRegion hRegionY = new HRegion();
            hRegionY.GenRectangle2(modeCenterY, modeCenterX, 0, w, 20);
            HRegion hRegionXY = hRegionY.Union2(hRegionX);
            //匹配十字架
            HRegion hRegionL = new HRegion();
            hRegionL.GenRectangle2(py, px, 0, 20, w);
            HRegion hRegionH = new HRegion();
            hRegionH.GenRectangle2(py, px, 0, w, 20);
            HRegion hRegionLH = hRegionH.Union2(hRegionL);
            //联合所有区域
            HRegion hRegion0XY = hRegion.ConcatObj(hRegionXY);
            HRegion hRegion0XYLH = hRegion0XY.ConcatObj(hRegionLH);
            //喷涂
            HImage hImage0 = hImage.PaintRegion(hRegion0XYLH, 0d, "margin");
            HImage hImage1 = hImage0.PaintRegion(hRegionLH, 255d, "margin");
            HImage hImage2 = hImage0.PaintRegion(hRegionXY, 255d, "margin");
            HImage hImage3 = hImage0.PaintRegion(hRegion, 255d, "margin");

            return hImage1.Compose3(hImage2, hImage3);
        }
        public HImage GetShowHIamge(HImage hImage, HRegion hRegion1, double modeCenterX1, double modeCenterY1, double px1, double py1, HRegion hRegion2, double modeCenterX2, double modeCenterY2, double px2, double py2)
        {
            double w = 1;
            //搜索区域
            HRegion hRegion = hRegion1.ConcatObj(hRegion2);

            //模板十字架
            HRegion hRegionX1 = new HRegion();
            hRegionX1.GenRectangle2(modeCenterY1, modeCenterX1, 0, 20, w);
            HRegion hRegionY1 = new HRegion();
            hRegionY1.GenRectangle2(modeCenterY1, modeCenterX1, 0, w, 20);
            HRegion hRegionXY1 = hRegionY1.Union2(hRegionX1);

            HRegion hRegionX2 = new HRegion();
            hRegionX2.GenRectangle2(modeCenterY2, modeCenterX2, 0, 20, w);
            HRegion hRegionY2 = new HRegion();
            hRegionY2.GenRectangle2(modeCenterY2, modeCenterX2, 0, w, 20);
            HRegion hRegionXY2 = hRegionY2.Union2(hRegionX2);

            HRegion hRegionXY = hRegionXY1.Union2(hRegionXY2);

            //匹配十字架
            HRegion hRegionL1 = new HRegion();
            hRegionL1.GenRectangle2(py1, px1, 0, 20, w);
            HRegion hRegionH1 = new HRegion();
            hRegionH1.GenRectangle2(py1, px1, 0, w, 20);
            HRegion hRegionLH1 = hRegionH1.Union2(hRegionL1);

            HRegion hRegionL2 = new HRegion();
            hRegionL2.GenRectangle2(py2, px2, 0, 20, w);
            HRegion hRegionH2 = new HRegion();
            hRegionH2.GenRectangle2(py2, px2, 0, w, 20);
            HRegion hRegionLH2 = hRegionH2.Union2(hRegionL2);

            HRegion hRegionLH = hRegionLH1.Union2(hRegionLH2);

            //联合所有区域
            HRegion hRegion0XY = hRegion.ConcatObj(hRegionXY);
            HRegion hRegion0XYLH = hRegion0XY.ConcatObj(hRegionLH);
            //喷涂
            HImage hImage0 = hImage.PaintRegion(hRegion0XYLH, 0d, "margin");
            HImage hImage1 = hImage0.PaintRegion(hRegionLH, 255d, "margin");
            HImage hImage2 = hImage0.PaintRegion(hRegionXY, 255d, "margin");
            HImage hImage3 = hImage0.PaintRegion(hRegion, 255d, "margin");

            return hImage1.Compose3(hImage2, hImage3);
        }
    }

    [Serializable]
    public class GeneralSet
    {
        public int key;//
        public int sleepTime = 1000;//延时拍照
        public string type = "孔";//类型
        public bool isBase;//是否重建坐标点
        public float X, Y, Z;//数模坐标
        public float offsetX = 0, offsetY = 0, offsetZ = 0;//补偿坐标
        public float minDX = -1.5f, minDY = -1.5f, minDZ = -1.5f;//下限
        public float maxDX = 1.5f, maxDY = 1.5f, maxDZ = 1.5f;//上限
        public int ledExposure = 4000, lightExposure = 600;//曝光

        public double score0 = 0.5, score1 = 0.5, score2 = 0.5;//匹配分数
        public double pX, pY, pZ, pRX, pRY, pRZ;//拍照点位姿坐标
    }

    [Serializable]
    public class OtherSet
    {
        public bool isSaveImage = true;
        public string imagePath = "D:\\image";

        public bool Load()
        {
            bool result2 = false;
            try
            {
                string gPath = "Data\\OtherSet.xml";
                if (File.Exists(gPath))
                {
                    OtherSet set = null;
                    XmlSerializer xmlgSets = new XmlSerializer(this.GetType());
                    using (FileStream stream = new FileStream(gPath, FileMode.OpenOrCreate))
                    {
                        set = (OtherSet)xmlgSets.Deserialize(stream);
                    }
                    if (set != null)
                    {
                        this.isSaveImage = set.isSaveImage;
                        this.imagePath = set.imagePath;
                        result2 = true;
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            return result2;
        }
        public bool Save()
        {
            bool result = true;
            try
            {
                string basePath = "Data";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string fPath = basePath + "\\OtherSet.xml";
                XmlSerializer xml = new XmlSerializer(this.GetType());
                using (FileStream stream = new FileStream(fPath, FileMode.Create))
                {
                    xml.Serialize(stream, this);
                }
            }
            catch (Exception ex)
            {
                result = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            return result;
        }
    }

    public class CamSetting
    {
        string CamName;
        public HImage mapImage; //用于畸变矫正
        DataTable dtZ = null;   //这里没有做光平面标定，而是做了表格查询插值拟合的操作，来计算激光的z值
        Dictionary<float, HHomMat2D> XY = new Dictionary<float, HHomMat2D>(); //这里也没有直接根据z值来直接计算实际相机坐标的xy，而是通过z值来查询xy的转换矩阵

        public HPose toolInCam = null;
        public HHomMat3D cam2Tool = null;
        //public HHomMat3D cam2Tool = new HHomMat3D();

        HCamPar camParIn = null; //相机内参
        HCamPar camParOut = null; //相机内参做了去畸变处理

        HPose LightInCam = null;    //光面标定pose
        HHomMat3D LightToCam = null;//光面标定矩阵

        public CamSetting(string camName)
        {
            CamName = camName;
        }

        public bool Load(string basePath)
        {
            //读取相机内参
            bool result = true;
            try
            {
                string filePath = basePath + "\\camparam.cal";
                if (File.Exists(filePath))
                {

                    HOperatorSet.ReadCamPar(filePath, out HTuple hv_CamParIn);
                    //HOperatorSet.ChangeRadialDistortionCamPar("adaptive", hv_CamParIn, new HTuple(0, 0, 0, 0, 0), out HTuple hv_CamParOut);
                    HOperatorSet.ChangeRadialDistortionCamPar("adaptive", hv_CamParIn, new HTuple(0), out HTuple hv_CamParOut);
                    mapImage = new HImage();

                    // 1. 转换
                    camParIn = new HCamPar(hv_CamParIn);
                    camParOut = new HCamPar(hv_CamParOut);

                    mapImage.GenRadialDistortionMap(camParIn, camParOut, "bilinear");
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            //读取光面标定矩阵
            bool result5 = true;
            try
            {
                string paramPath = basePath + "\\LightInCam.dat";
                if (File.Exists(paramPath))
                {
                    var hWorldPose = new HPose();
                    hWorldPose.ReadPose(paramPath);
                    LightInCam = hWorldPose;
                    LightToCam = LightInCam.PoseToHomMat3d();
                }
                else
                {
                    result5 = false;
                    File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + "文件不存在" + "\r\n\r\n");

                }
            }
            catch (Exception ex)
            {
                result5 = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            // 内存文件转换
            bool result2 = true;
            try
            {
                string filePath = basePath + "\\camparam.cal";
                string saveLightPath = basePath + "\\lightImage.tiff";
                string saveWorldPath = basePath + "\\camImage.tiff";

                if (File.Exists(filePath))
                {

                    HOperatorSet.ReadCamPar(filePath, out HTuple hv_CamParIn);
                    // 1. 转换
                    camParIn = new HCamPar(hv_CamParIn);


                    OLM.TransformImageLight(camParIn[6].I, camParIn[7].I, mapImage, camParIn, LightInCam, out HObject ho_LightMapped);

                    OLM.TransformImageWorld(camParIn[6].I, camParIn[7].I, mapImage, camParIn, LightInCam,LightToCam,out HObject worldImageMapped);

                    HOperatorSet.WriteImage(ho_LightMapped, "tiff", 0, saveLightPath);
                    HOperatorSet.WriteImage(worldImageMapped, "tiff", 0, saveWorldPath);


                }
                else
                {
                    result2 = false;
                }
            }
            catch (Exception ex)
            {
                result2 = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }

            //读取手眼标定矩阵
            bool result4 = true;
            try
            {
                //string filePath = basePath + "\\相机转工具矩阵";
                //if (File.Exists(filePath))
                //{
                //    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                //    {
                //        cam2Tool = HHomMat3D.Deserialize(stream);
                //    }
                //}
                string filePath = basePath + "\\ToolInCam.dat";
                if (File.Exists(filePath))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        var hWorldPose = new HPose();
                        hWorldPose.ReadPose(filePath);
                        toolInCam = hWorldPose;
                        //这里要先求逆，再转换
                        cam2Tool = toolInCam.PoseInvert().PoseToHomMat3d();
                    }
                }
                else
                {
                    result4 = false;
                }
            }
            catch (Exception ex)
            {
                result4 = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }
            //return result && result2 && result3 && result4 & result5;

            return result && result4 & result5;

        }

        //public bool Save(string basePath)
        //{
        //    bool result = false;

        //    return result;
        //}

        /// <summary>
        /// 输入激光像素坐标，输出物理高度
        /// </summary>
        /// <param name="Px"></param>
        /// <param name="Py"></param>
        /// <returns></returns>
        public bool GetZ(float Px, float Py, out float z)
        {
            z = 0;

            try
            {
                DataRow[] rows;

                //Stopwatch stopwatch1 = Stopwatch.StartNew();
                float x1 = (float)dtZ.Compute("Max(x)", $"x <= {Px}");
                //long a = stopwatch1.ElapsedMilliseconds;
                rows = dtZ.Select($"x = {x1} AND y <= {Py}", "y DESC");
                //long b = stopwatch1.ElapsedMilliseconds-a;
                float y11 = (float)rows[0]["y"];
                float z11 = (float)rows[0]["z"];
                //long c = stopwatch1.ElapsedMilliseconds - b;
                rows = dtZ.Select($"x = {x1} AND y >= {Py}", "y ASC");
                //long d = stopwatch1.ElapsedMilliseconds-c;
                float y12 = (float)rows[0]["y"];
                float z12 = (float)rows[0]["z"];
                float z1 = y12 == y11 ? z11 : (Py - y11) / (y12 - y11) * (z12 - z11) + z11;

                float x2 = (float)dtZ.Compute("Min(x)", $"x >= {Px}");
                rows = dtZ.Select($"x = {x2} AND y <= {Py}", "y DESC");
                float y21 = (float)rows[0]["y"];
                float z21 = (float)rows[0]["z"];
                rows = dtZ.Select($"x = {x2} AND y >= {Py}", "y ASC");
                float y22 = (float)rows[0]["y"];
                float z22 = (float)rows[0]["z"];
                float z2 = y22 == y21 ? z21 : (Py - y21) / (y22 - y21) * (z22 - z21) + z21;

                z = x2 == x1 ? z1 : (Px - x1) / (x2 - x1) * (z2 - z1) + z1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// 输入激光像素坐标，输出物理高度
        /// </summary>
        /// <param name="Px"></param>
        /// <param name="Py"></param>
        /// <returns></returns>
        public bool GetZFromLight(float Px, float Py, out float z)
        {
            z = 0;
            try
            {
                HTuple hx, hy;
                //相机光平面坐标,注意这里输入的xy，和输出是反过来的
                camParOut.ImagePointsToWorldPlane(LightInCam, Py, Px, "m", out hx, out hy);
                HTuple hz = new HTuple(new double[hx.Length]);
                //转相机坐标系
                HTuple camX = LightToCam.AffineTransPoint3d(hx, hy, hz, out HTuple camY, out HTuple camZ);
                z = (float)camZ.D;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }
        /// <summary>
        /// 输入像素坐标与物理高度z，输出物理坐标xy
        /// </summary>
        /// <param name="Px"></param>
        /// <param name="Py"></param>
        /// <param name="z"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool GetXY(float Px, float Py, float z, out float x, out float y)
        {
            x = 0;
            y = 0;
            var list = XY.Keys.OrderBy(n => { return n; }).ToList();
            if (z >= list[0])
            {
                for (int i = 1; i < list.Count(); i++)
                {
                    if (z < list[i])
                    {
                        var Qx1 = XY[list[i - 1]].AffineTransPoint2d(Px, Py, out double Qy1);
                        var Qx2 = XY[list[i]].AffineTransPoint2d(Px, Py, out double Qy2);
                        x = (float)(z == list[i - 1] ? Qx1 : (z - list[i - 1]) / (list[i] - list[i - 1]) * (Qx2 - Qx1) + Qx1);
                        y = (float)(z == list[i - 1] ? Qy1 : (z - list[i - 1]) / (list[i] - list[i - 1]) * (Qy2 - Qy1) + Qy1);
                        //break;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 输入像素坐标与物理高度z，输出物理坐标xy
        /// </summary>
        /// <param name="Px"></param>
        /// <param name="Py"></param>
        /// <param name="z"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool GetXYFromLight(float Px, float Py, float z, out float x, out float y)
        {
            x = 0;
            y = 0;
            try
            {

                HTuple hv_sy = new HTuple(), hv_sx = new HTuple();
                HTuple hv_cy = new HTuple(), hv_cx = new HTuple();
                HTuple hv_focus = new HTuple();

                get_cam_par_data(camParOut, "sx", out hv_sx);
                get_cam_par_data(camParOut, "sy", out hv_sy);
                get_cam_par_data(camParOut, "cx", out hv_cx);
                get_cam_par_data(camParOut, "cy", out hv_cy);
                get_cam_par_data(camParOut, "focus", out hv_focus);

                // halcon 内参转 opencv 内参
                double fx = hv_focus.D / hv_sx.D;
                double fy = hv_focus.D / hv_sy.D;
                double cx = hv_cx.D;
                double cy = hv_cy.D;

                //// 直接用 opencv 内参
                //double fx = 7445.531567;
                //double fy = 7446.254570;
                //double cx = 1508.3623970;
                //double cy = 1055.696547;

                //根据z计算xy
                x = (float)((Px - cx) * z / fx);
                y = (float)((Py - cy) * z / fy);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public void get_cam_par_data(HTuple hv_CameraParam, HTuple hv_ParamName, out HTuple hv_ParamValue)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_CameraType = new HTuple(), hv_CameraParamNames = new HTuple();
            HTuple hv_Index = new HTuple(), hv_ParamNameInd = new HTuple();
            HTuple hv_I = new HTuple();
            // Initialize local and output iconic variables 
            hv_ParamValue = new HTuple();
            try
            {
                //get_cam_par_data returns in ParamValue the value of the
                //parameter that is given in ParamName from the tuple of
                //camera parameters that is given in CameraParam.
                //
                //Get the parameter names that correspond to the
                //elements in the input camera parameter tuple.
                hv_CameraType.Dispose(); hv_CameraParamNames.Dispose();
                get_cam_par_names(hv_CameraParam, out hv_CameraType, out hv_CameraParamNames);
                //
                //Find the index of the requested camera data and return
                //the corresponding value.
                hv_ParamValue.Dispose();
                hv_ParamValue = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_ParamName.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    hv_ParamNameInd.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ParamNameInd = hv_ParamName.TupleSelect(
                            hv_Index);
                    }
                    if ((int)(new HTuple(hv_ParamNameInd.TupleEqual("camera_type"))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_ParamValue = hv_ParamValue.TupleConcat(
                                    hv_CameraType);
                                hv_ParamValue.Dispose();
                                hv_ParamValue = ExpTmpLocalVar_ParamValue;
                            }
                        }
                        continue;
                    }
                    hv_I.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_I = hv_CameraParamNames.TupleFind(
                            hv_ParamNameInd);
                    }
                    if ((int)(new HTuple(hv_I.TupleNotEqual(-1))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_ParamValue = hv_ParamValue.TupleConcat(
                                    hv_CameraParam.TupleSelect(hv_I));
                                hv_ParamValue.Dispose();
                                hv_ParamValue = ExpTmpLocalVar_ParamValue;
                            }
                        }
                    }
                    else
                    {
                        throw new HalconException("Unknown camera parameter " + hv_ParamNameInd);
                    }
                }

                hv_CameraType.Dispose();
                hv_CameraParamNames.Dispose();
                hv_Index.Dispose();
                hv_ParamNameInd.Dispose();
                hv_I.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_CameraType.Dispose();
                hv_CameraParamNames.Dispose();
                hv_Index.Dispose();
                hv_ParamNameInd.Dispose();
                hv_I.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_cam_par_names(HTuple hv_CameraParam, out HTuple hv_CameraType,
     out HTuple hv_ParamNames)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_CameraParamAreaScanDivision = new HTuple();
            HTuple hv_CameraParamAreaScanPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricDivision = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanImageSideTelecentricTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanObjectSideTelecentricTiltDivision = new HTuple();
            HTuple hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanHypercentricDivision = new HTuple();
            HTuple hv_CameraParamAreaScanHypercentricPolynomial = new HTuple();
            HTuple hv_CameraParamLinesScanDivision = new HTuple();
            HTuple hv_CameraParamLinesScanPolynomial = new HTuple();
            HTuple hv_CameraParamLinesScanTelecentricDivision = new HTuple();
            HTuple hv_CameraParamLinesScanTelecentricPolynomial = new HTuple();
            HTuple hv_CameraParamAreaScanTiltDivisionLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanTiltPolynomialLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricDivisionLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanTelecentricPolynomialLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy = new HTuple();
            HTuple hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy = new HTuple();
            // Initialize local and output iconic variables 
            hv_CameraType = new HTuple();
            hv_ParamNames = new HTuple();
            try
            {
                //get_cam_par_names returns for each element in the camera
                //parameter tuple that is passed in CameraParam the name
                //of the respective camera parameter. The parameter names
                //are returned in ParamNames. Additionally, the camera
                //type is returned in CameraType. Alternatively, instead of
                //the camera parameters, the camera type can be passed in
                //CameraParam in form of one of the following strings:
                //  - 'area_scan_division'
                //  - 'area_scan_polynomial'
                //  - 'area_scan_tilt_division'
                //  - 'area_scan_tilt_polynomial'
                //  - 'area_scan_telecentric_division'
                //  - 'area_scan_telecentric_polynomial'
                //  - 'area_scan_tilt_bilateral_telecentric_division'
                //  - 'area_scan_tilt_bilateral_telecentric_polynomial'
                //  - 'area_scan_tilt_object_side_telecentric_division'
                //  - 'area_scan_tilt_object_side_telecentric_polynomial'
                //  - 'area_scan_hypercentric_division'
                //  - 'area_scan_hypercentric_polynomial'
                //  - 'line_scan_division'
                //  - 'line_scan_polynomial'
                //  - 'line_scan_telecentric_division'
                //  - 'line_scan_telecentric_polynomial'
                //
                hv_CameraParamAreaScanDivision.Dispose();
                hv_CameraParamAreaScanDivision = new HTuple();
                hv_CameraParamAreaScanDivision[0] = "focus";
                hv_CameraParamAreaScanDivision[1] = "kappa";
                hv_CameraParamAreaScanDivision[2] = "sx";
                hv_CameraParamAreaScanDivision[3] = "sy";
                hv_CameraParamAreaScanDivision[4] = "cx";
                hv_CameraParamAreaScanDivision[5] = "cy";
                hv_CameraParamAreaScanDivision[6] = "image_width";
                hv_CameraParamAreaScanDivision[7] = "image_height";
                hv_CameraParamAreaScanPolynomial.Dispose();
                hv_CameraParamAreaScanPolynomial = new HTuple();
                hv_CameraParamAreaScanPolynomial[0] = "focus";
                hv_CameraParamAreaScanPolynomial[1] = "k1";
                hv_CameraParamAreaScanPolynomial[2] = "k2";
                hv_CameraParamAreaScanPolynomial[3] = "k3";
                hv_CameraParamAreaScanPolynomial[4] = "p1";
                hv_CameraParamAreaScanPolynomial[5] = "p2";
                hv_CameraParamAreaScanPolynomial[6] = "sx";
                hv_CameraParamAreaScanPolynomial[7] = "sy";
                hv_CameraParamAreaScanPolynomial[8] = "cx";
                hv_CameraParamAreaScanPolynomial[9] = "cy";
                hv_CameraParamAreaScanPolynomial[10] = "image_width";
                hv_CameraParamAreaScanPolynomial[11] = "image_height";
                hv_CameraParamAreaScanTelecentricDivision.Dispose();
                hv_CameraParamAreaScanTelecentricDivision = new HTuple();
                hv_CameraParamAreaScanTelecentricDivision[0] = "magnification";
                hv_CameraParamAreaScanTelecentricDivision[1] = "kappa";
                hv_CameraParamAreaScanTelecentricDivision[2] = "sx";
                hv_CameraParamAreaScanTelecentricDivision[3] = "sy";
                hv_CameraParamAreaScanTelecentricDivision[4] = "cx";
                hv_CameraParamAreaScanTelecentricDivision[5] = "cy";
                hv_CameraParamAreaScanTelecentricDivision[6] = "image_width";
                hv_CameraParamAreaScanTelecentricDivision[7] = "image_height";
                hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomial = new HTuple();
                hv_CameraParamAreaScanTelecentricPolynomial[0] = "magnification";
                hv_CameraParamAreaScanTelecentricPolynomial[1] = "k1";
                hv_CameraParamAreaScanTelecentricPolynomial[2] = "k2";
                hv_CameraParamAreaScanTelecentricPolynomial[3] = "k3";
                hv_CameraParamAreaScanTelecentricPolynomial[4] = "p1";
                hv_CameraParamAreaScanTelecentricPolynomial[5] = "p2";
                hv_CameraParamAreaScanTelecentricPolynomial[6] = "sx";
                hv_CameraParamAreaScanTelecentricPolynomial[7] = "sy";
                hv_CameraParamAreaScanTelecentricPolynomial[8] = "cx";
                hv_CameraParamAreaScanTelecentricPolynomial[9] = "cy";
                hv_CameraParamAreaScanTelecentricPolynomial[10] = "image_width";
                hv_CameraParamAreaScanTelecentricPolynomial[11] = "image_height";
                hv_CameraParamAreaScanTiltDivision.Dispose();
                hv_CameraParamAreaScanTiltDivision = new HTuple();
                hv_CameraParamAreaScanTiltDivision[0] = "focus";
                hv_CameraParamAreaScanTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanTiltDivision[2] = "image_plane_dist";
                hv_CameraParamAreaScanTiltDivision[3] = "tilt";
                hv_CameraParamAreaScanTiltDivision[4] = "rot";
                hv_CameraParamAreaScanTiltDivision[5] = "sx";
                hv_CameraParamAreaScanTiltDivision[6] = "sy";
                hv_CameraParamAreaScanTiltDivision[7] = "cx";
                hv_CameraParamAreaScanTiltDivision[8] = "cy";
                hv_CameraParamAreaScanTiltDivision[9] = "image_width";
                hv_CameraParamAreaScanTiltDivision[10] = "image_height";
                hv_CameraParamAreaScanTiltPolynomial.Dispose();
                hv_CameraParamAreaScanTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanTiltPolynomial[0] = "focus";
                hv_CameraParamAreaScanTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanTiltPolynomial[6] = "image_plane_dist";
                hv_CameraParamAreaScanTiltPolynomial[7] = "tilt";
                hv_CameraParamAreaScanTiltPolynomial[8] = "rot";
                hv_CameraParamAreaScanTiltPolynomial[9] = "sx";
                hv_CameraParamAreaScanTiltPolynomial[10] = "sy";
                hv_CameraParamAreaScanTiltPolynomial[11] = "cx";
                hv_CameraParamAreaScanTiltPolynomial[12] = "cy";
                hv_CameraParamAreaScanTiltPolynomial[13] = "image_width";
                hv_CameraParamAreaScanTiltPolynomial[14] = "image_height";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision = new HTuple();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[0] = "focus";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[2] = "tilt";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[3] = "rot";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[4] = "sx";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[5] = "sy";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[6] = "cx";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[7] = "cy";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[8] = "image_width";
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision[9] = "image_height";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[0] = "focus";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[6] = "tilt";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[7] = "rot";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[8] = "sx";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[9] = "sy";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[10] = "cx";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[11] = "cy";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[12] = "image_width";
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial[13] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[0] = "magnification";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[2] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[3] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[4] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[5] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[6] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[7] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[8] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision[9] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[0] = "magnification";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[6] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[7] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[8] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[9] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[10] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[11] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[12] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial[13] = "image_height";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision = new HTuple();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[0] = "magnification";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[1] = "kappa";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[2] = "image_plane_dist";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[3] = "tilt";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[4] = "rot";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[5] = "sx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[6] = "sy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[7] = "cx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[8] = "cy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[9] = "image_width";
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision[10] = "image_height";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial = new HTuple();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[0] = "magnification";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[1] = "k1";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[2] = "k2";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[3] = "k3";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[4] = "p1";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[5] = "p2";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[6] = "image_plane_dist";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[7] = "tilt";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[8] = "rot";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[9] = "sx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[10] = "sy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[11] = "cx";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[12] = "cy";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[13] = "image_width";
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial[14] = "image_height";
                hv_CameraParamAreaScanHypercentricDivision.Dispose();
                hv_CameraParamAreaScanHypercentricDivision = new HTuple();
                hv_CameraParamAreaScanHypercentricDivision[0] = "focus";
                hv_CameraParamAreaScanHypercentricDivision[1] = "kappa";
                hv_CameraParamAreaScanHypercentricDivision[2] = "sx";
                hv_CameraParamAreaScanHypercentricDivision[3] = "sy";
                hv_CameraParamAreaScanHypercentricDivision[4] = "cx";
                hv_CameraParamAreaScanHypercentricDivision[5] = "cy";
                hv_CameraParamAreaScanHypercentricDivision[6] = "image_width";
                hv_CameraParamAreaScanHypercentricDivision[7] = "image_height";
                hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                hv_CameraParamAreaScanHypercentricPolynomial = new HTuple();
                hv_CameraParamAreaScanHypercentricPolynomial[0] = "focus";
                hv_CameraParamAreaScanHypercentricPolynomial[1] = "k1";
                hv_CameraParamAreaScanHypercentricPolynomial[2] = "k2";
                hv_CameraParamAreaScanHypercentricPolynomial[3] = "k3";
                hv_CameraParamAreaScanHypercentricPolynomial[4] = "p1";
                hv_CameraParamAreaScanHypercentricPolynomial[5] = "p2";
                hv_CameraParamAreaScanHypercentricPolynomial[6] = "sx";
                hv_CameraParamAreaScanHypercentricPolynomial[7] = "sy";
                hv_CameraParamAreaScanHypercentricPolynomial[8] = "cx";
                hv_CameraParamAreaScanHypercentricPolynomial[9] = "cy";
                hv_CameraParamAreaScanHypercentricPolynomial[10] = "image_width";
                hv_CameraParamAreaScanHypercentricPolynomial[11] = "image_height";
                hv_CameraParamLinesScanDivision.Dispose();
                hv_CameraParamLinesScanDivision = new HTuple();
                hv_CameraParamLinesScanDivision[0] = "focus";
                hv_CameraParamLinesScanDivision[1] = "kappa";
                hv_CameraParamLinesScanDivision[2] = "sx";
                hv_CameraParamLinesScanDivision[3] = "sy";
                hv_CameraParamLinesScanDivision[4] = "cx";
                hv_CameraParamLinesScanDivision[5] = "cy";
                hv_CameraParamLinesScanDivision[6] = "image_width";
                hv_CameraParamLinesScanDivision[7] = "image_height";
                hv_CameraParamLinesScanDivision[8] = "vx";
                hv_CameraParamLinesScanDivision[9] = "vy";
                hv_CameraParamLinesScanDivision[10] = "vz";
                hv_CameraParamLinesScanPolynomial.Dispose();
                hv_CameraParamLinesScanPolynomial = new HTuple();
                hv_CameraParamLinesScanPolynomial[0] = "focus";
                hv_CameraParamLinesScanPolynomial[1] = "k1";
                hv_CameraParamLinesScanPolynomial[2] = "k2";
                hv_CameraParamLinesScanPolynomial[3] = "k3";
                hv_CameraParamLinesScanPolynomial[4] = "p1";
                hv_CameraParamLinesScanPolynomial[5] = "p2";
                hv_CameraParamLinesScanPolynomial[6] = "sx";
                hv_CameraParamLinesScanPolynomial[7] = "sy";
                hv_CameraParamLinesScanPolynomial[8] = "cx";
                hv_CameraParamLinesScanPolynomial[9] = "cy";
                hv_CameraParamLinesScanPolynomial[10] = "image_width";
                hv_CameraParamLinesScanPolynomial[11] = "image_height";
                hv_CameraParamLinesScanPolynomial[12] = "vx";
                hv_CameraParamLinesScanPolynomial[13] = "vy";
                hv_CameraParamLinesScanPolynomial[14] = "vz";
                hv_CameraParamLinesScanTelecentricDivision.Dispose();
                hv_CameraParamLinesScanTelecentricDivision = new HTuple();
                hv_CameraParamLinesScanTelecentricDivision[0] = "magnification";
                hv_CameraParamLinesScanTelecentricDivision[1] = "kappa";
                hv_CameraParamLinesScanTelecentricDivision[2] = "sx";
                hv_CameraParamLinesScanTelecentricDivision[3] = "sy";
                hv_CameraParamLinesScanTelecentricDivision[4] = "cx";
                hv_CameraParamLinesScanTelecentricDivision[5] = "cy";
                hv_CameraParamLinesScanTelecentricDivision[6] = "image_width";
                hv_CameraParamLinesScanTelecentricDivision[7] = "image_height";
                hv_CameraParamLinesScanTelecentricDivision[8] = "vx";
                hv_CameraParamLinesScanTelecentricDivision[9] = "vy";
                hv_CameraParamLinesScanTelecentricDivision[10] = "vz";
                hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                hv_CameraParamLinesScanTelecentricPolynomial = new HTuple();
                hv_CameraParamLinesScanTelecentricPolynomial[0] = "magnification";
                hv_CameraParamLinesScanTelecentricPolynomial[1] = "k1";
                hv_CameraParamLinesScanTelecentricPolynomial[2] = "k2";
                hv_CameraParamLinesScanTelecentricPolynomial[3] = "k3";
                hv_CameraParamLinesScanTelecentricPolynomial[4] = "p1";
                hv_CameraParamLinesScanTelecentricPolynomial[5] = "p2";
                hv_CameraParamLinesScanTelecentricPolynomial[6] = "sx";
                hv_CameraParamLinesScanTelecentricPolynomial[7] = "sy";
                hv_CameraParamLinesScanTelecentricPolynomial[8] = "cx";
                hv_CameraParamLinesScanTelecentricPolynomial[9] = "cy";
                hv_CameraParamLinesScanTelecentricPolynomial[10] = "image_width";
                hv_CameraParamLinesScanTelecentricPolynomial[11] = "image_height";
                hv_CameraParamLinesScanTelecentricPolynomial[12] = "vx";
                hv_CameraParamLinesScanTelecentricPolynomial[13] = "vy";
                hv_CameraParamLinesScanTelecentricPolynomial[14] = "vz";
                //Legacy parameter names
                hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTiltDivisionLegacy = new HTuple();
                hv_CameraParamAreaScanTiltDivisionLegacy[0] = "focus";
                hv_CameraParamAreaScanTiltDivisionLegacy[1] = "kappa";
                hv_CameraParamAreaScanTiltDivisionLegacy[2] = "tilt";
                hv_CameraParamAreaScanTiltDivisionLegacy[3] = "rot";
                hv_CameraParamAreaScanTiltDivisionLegacy[4] = "sx";
                hv_CameraParamAreaScanTiltDivisionLegacy[5] = "sy";
                hv_CameraParamAreaScanTiltDivisionLegacy[6] = "cx";
                hv_CameraParamAreaScanTiltDivisionLegacy[7] = "cy";
                hv_CameraParamAreaScanTiltDivisionLegacy[8] = "image_width";
                hv_CameraParamAreaScanTiltDivisionLegacy[9] = "image_height";
                hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTiltPolynomialLegacy = new HTuple();
                hv_CameraParamAreaScanTiltPolynomialLegacy[0] = "focus";
                hv_CameraParamAreaScanTiltPolynomialLegacy[1] = "k1";
                hv_CameraParamAreaScanTiltPolynomialLegacy[2] = "k2";
                hv_CameraParamAreaScanTiltPolynomialLegacy[3] = "k3";
                hv_CameraParamAreaScanTiltPolynomialLegacy[4] = "p1";
                hv_CameraParamAreaScanTiltPolynomialLegacy[5] = "p2";
                hv_CameraParamAreaScanTiltPolynomialLegacy[6] = "tilt";
                hv_CameraParamAreaScanTiltPolynomialLegacy[7] = "rot";
                hv_CameraParamAreaScanTiltPolynomialLegacy[8] = "sx";
                hv_CameraParamAreaScanTiltPolynomialLegacy[9] = "sy";
                hv_CameraParamAreaScanTiltPolynomialLegacy[10] = "cx";
                hv_CameraParamAreaScanTiltPolynomialLegacy[11] = "cy";
                hv_CameraParamAreaScanTiltPolynomialLegacy[12] = "image_width";
                hv_CameraParamAreaScanTiltPolynomialLegacy[13] = "image_height";
                hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricDivisionLegacy = new HTuple();
                hv_CameraParamAreaScanTelecentricDivisionLegacy[0] = "focus";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[1] = "kappa";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[2] = "sx";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[3] = "sy";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[4] = "cx";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[5] = "cy";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[6] = "image_width";
                hv_CameraParamAreaScanTelecentricDivisionLegacy[7] = "image_height";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy = new HTuple();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[0] = "focus";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[1] = "k1";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[2] = "k2";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[3] = "k3";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[4] = "p1";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[5] = "p2";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[6] = "sx";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[7] = "sy";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[8] = "cx";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[9] = "cy";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[10] = "image_width";
                hv_CameraParamAreaScanTelecentricPolynomialLegacy[11] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[0] = "focus";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[1] = "kappa";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[2] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[3] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[4] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[5] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[6] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[7] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[8] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy[9] = "image_height";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy = new HTuple();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[0] = "focus";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[1] = "k1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[2] = "k2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[3] = "k3";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[4] = "p1";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[5] = "p2";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[6] = "tilt";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[7] = "rot";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[8] = "sx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[9] = "sy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[10] = "cx";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[11] = "cy";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[12] = "image_width";
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy[13] = "image_height";
                //
                //If the camera type is passed in CameraParam
                if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleEqual(
                    1))) != 0)
                {
                    if ((int)(((hv_CameraParam.TupleSelect(0))).TupleIsString()) != 0)
                    {
                        hv_CameraType.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CameraType = hv_CameraParam.TupleSelect(
                                0);
                        }
                        if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricPolynomial);
                            }
                        }
                        else if ((int)((new HTuple(hv_CameraType.TupleEqual("line_scan_division"))).TupleOr(
                            new HTuple(hv_CameraType.TupleEqual("line_scan")))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanPolynomial);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_division"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricDivision);
                            }
                        }
                        else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_polynomial"))) != 0)
                        {
                            hv_ParamNames.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ParamNames = new HTuple();
                                hv_ParamNames[0] = "camera_type";
                                hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricPolynomial);
                            }
                        }
                        else
                        {
                            throw new HalconException(("Unknown camera type '" + hv_CameraType) + "' passed in CameraParam.");
                        }

                        hv_CameraParamAreaScanDivision.Dispose();
                        hv_CameraParamAreaScanPolynomial.Dispose();
                        hv_CameraParamAreaScanTelecentricDivision.Dispose();
                        hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                        hv_CameraParamAreaScanTiltDivision.Dispose();
                        hv_CameraParamAreaScanTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                        hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                        hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                        hv_CameraParamAreaScanHypercentricDivision.Dispose();
                        hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                        hv_CameraParamLinesScanDivision.Dispose();
                        hv_CameraParamLinesScanPolynomial.Dispose();
                        hv_CameraParamLinesScanTelecentricDivision.Dispose();
                        hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                        hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                        hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                        hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                        hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                        hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();

                        return;
                    }
                }
                //
                //If the camera parameters are passed in CameraParam
                if ((int)(((((hv_CameraParam.TupleSelect(0))).TupleIsString())).TupleNot()) != 0)
                {
                    //Format of camera parameters for HALCON 12 and earlier
                    switch ((new HTuple(hv_CameraParam.TupleLength()
                        )).I)
                    {
                        //
                        //Area Scan
                        case 8:
                            //CameraType: 'area_scan_division' or 'area_scan_telecentric_division'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanDivision);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_division";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTelecentricDivisionLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_telecentric_division";
                            }
                            break;
                        case 10:
                            //CameraType: 'area_scan_tilt_division' or 'area_scan_telecentric_tilt_division'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTiltDivisionLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_division";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_bilateral_telecentric_division";
                            }
                            break;
                        case 12:
                            //CameraType: 'area_scan_polynomial' or 'area_scan_telecentric_polynomial'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanPolynomial);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_polynomial";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTelecentricPolynomialLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_telecentric_polynomial";
                            }
                            break;
                        case 14:
                            //CameraType: 'area_scan_tilt_polynomial' or 'area_scan_telecentric_tilt_polynomial'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanTiltPolynomialLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_polynomial";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy);
                                hv_CameraType.Dispose();
                                hv_CameraType = "area_scan_tilt_bilateral_telecentric_polynomial";
                            }
                            break;
                        //
                        //Line Scan
                        case 11:
                            //CameraType: 'line_scan' or 'line_scan_telecentric'
                            if ((int)(new HTuple(((hv_CameraParam.TupleSelect(0))).TupleNotEqual(0.0))) != 0)
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamLinesScanDivision);
                                hv_CameraType.Dispose();
                                hv_CameraType = "line_scan_division";
                            }
                            else
                            {
                                hv_ParamNames.Dispose();
                                hv_ParamNames = new HTuple(hv_CameraParamLinesScanTelecentricDivision);
                                hv_CameraType.Dispose();
                                hv_CameraType = "line_scan_telecentric_division";
                            }
                            break;
                        default:
                            throw new HalconException("Wrong number of values in CameraParam.");
                            break;
                    }
                }
                else
                {
                    //Format of camera parameters since HALCON 13
                    hv_CameraType.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CameraType = hv_CameraParam.TupleSelect(
                            0);
                    }
                    if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            9))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            13))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            9))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            13))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTelecentricPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            11))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_image_side_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            15))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            11))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_bilateral_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            15))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_tilt_object_side_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            9))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("area_scan_hypercentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            13))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamAreaScanHypercentricPolynomial);
                        }
                    }
                    else if ((int)((new HTuple(hv_CameraType.TupleEqual("line_scan_division"))).TupleOr(
                        new HTuple(hv_CameraType.TupleEqual("line_scan")))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanPolynomial);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_division"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            12))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricDivision);
                        }
                    }
                    else if ((int)(new HTuple(hv_CameraType.TupleEqual("line_scan_telecentric_polynomial"))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_CameraParam.TupleLength())).TupleNotEqual(
                            16))) != 0)
                        {
                            throw new HalconException("Wrong number of values in CameraParam.");
                        }
                        hv_ParamNames.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ParamNames = new HTuple();
                            hv_ParamNames[0] = "camera_type";
                            hv_ParamNames = hv_ParamNames.TupleConcat(hv_CameraParamLinesScanTelecentricPolynomial);
                        }
                    }
                    else
                    {
                        throw new HalconException("Unknown camera type in CameraParam.");
                    }
                }

                hv_CameraParamAreaScanDivision.Dispose();
                hv_CameraParamAreaScanPolynomial.Dispose();
                hv_CameraParamAreaScanTelecentricDivision.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivision.Dispose();
                hv_CameraParamAreaScanTiltPolynomial.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanHypercentricDivision.Dispose();
                hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                hv_CameraParamLinesScanDivision.Dispose();
                hv_CameraParamLinesScanPolynomial.Dispose();
                hv_CameraParamLinesScanTelecentricDivision.Dispose();
                hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_CameraParamAreaScanDivision.Dispose();
                hv_CameraParamAreaScanPolynomial.Dispose();
                hv_CameraParamAreaScanTelecentricDivision.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivision.Dispose();
                hv_CameraParamAreaScanTiltPolynomial.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanImageSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltDivision.Dispose();
                hv_CameraParamAreaScanObjectSideTelecentricTiltPolynomial.Dispose();
                hv_CameraParamAreaScanHypercentricDivision.Dispose();
                hv_CameraParamAreaScanHypercentricPolynomial.Dispose();
                hv_CameraParamLinesScanDivision.Dispose();
                hv_CameraParamLinesScanPolynomial.Dispose();
                hv_CameraParamLinesScanTelecentricDivision.Dispose();
                hv_CameraParamLinesScanTelecentricPolynomial.Dispose();
                hv_CameraParamAreaScanTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTiltPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricDivisionLegacy.Dispose();
                hv_CameraParamAreaScanTelecentricPolynomialLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltDivisionLegacy.Dispose();
                hv_CameraParamAreaScanBilateralTelecentricTiltPolynomialLegacy.Dispose();

                throw HDevExpDefaultException;
            }
        }
    }






    [Serializable]
    public class CarID
    {
        public string Name;
        public int ID;
        public CarID() { }
        public CarID(string name, int id)
        {
            this.Name = name;
            this.ID = id;
        }
    }

    partial class FormCarSet
    {
        #region MyRegion
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCarSet));
            this.dgvProductItemIfo = new System.Windows.Forms.DataGridView();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnModifyProductItem = new System.Windows.Forms.Button();
            this.btnAddProductItem = new System.Windows.Forms.Button();
            this.btnDeleteProductItem = new System.Windows.Forms.Button();
            this.txtProductId = new System.Windows.Forms.TextBox();
            this.labelID = new System.Windows.Forms.Label();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.labelCarName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductItemIfo)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvProductItemIfo
            // 
            this.dgvProductItemIfo.AllowUserToAddRows = false;
            this.dgvProductItemIfo.AllowUserToDeleteRows = false;
            this.dgvProductItemIfo.AllowUserToResizeColumns = false;
            this.dgvProductItemIfo.AllowUserToResizeRows = false;
            this.dgvProductItemIfo.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(192)))), ((int)(((byte)(233)))));
            this.dgvProductItemIfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProductItemIfo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column4,
            this.Column1,
            this.Column8});
            this.dgvProductItemIfo.Location = new System.Drawing.Point(12, 12);
            this.dgvProductItemIfo.MultiSelect = false;
            this.dgvProductItemIfo.Name = "dgvProductItemIfo";
            this.dgvProductItemIfo.ReadOnly = true;
            this.dgvProductItemIfo.RowHeadersVisible = false;
            this.dgvProductItemIfo.RowTemplate.Height = 23;
            this.dgvProductItemIfo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProductItemIfo.Size = new System.Drawing.Size(402, 246);
            this.dgvProductItemIfo.TabIndex = 2;
            this.dgvProductItemIfo.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProductItemIfo_CellContentClick);
            // 
            // Column4
            // 
            this.Column4.HeaderText = "序号";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "名称";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "产品ID";
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.btnModifyProductItem);
            this.groupBox1.Controls.Add(this.btnAddProductItem);
            this.groupBox1.Controls.Add(this.btnDeleteProductItem);
            this.groupBox1.Controls.Add(this.txtProductId);
            this.groupBox1.Controls.Add(this.labelID);
            this.groupBox1.Controls.Add(this.txtProductName);
            this.groupBox1.Controls.Add(this.labelCarName);
            this.groupBox1.Location = new System.Drawing.Point(12, 264);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(402, 134);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数信息";
            // 
            // btnModifyProductItem
            // 
            this.btnModifyProductItem.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            //this.btnModifyProductItem.Image = ((System.Drawing.Image)(resources.GetObject("btnModifyProductItem.Image")));
            this.btnModifyProductItem.Location = new System.Drawing.Point(163, 80);
            this.btnModifyProductItem.Name = "btnModifyProductItem";
            this.btnModifyProductItem.Size = new System.Drawing.Size(82, 39);
            this.btnModifyProductItem.TabIndex = 132;
            this.btnModifyProductItem.Text = "修改";
            this.btnModifyProductItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnModifyProductItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnModifyProductItem.UseVisualStyleBackColor = true;
            this.btnModifyProductItem.Click += new System.EventHandler(this.btnModifyProductItem_Click);
            // 
            // btnAddProductItem
            // 
            this.btnAddProductItem.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAddProductItem.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //this.btnAddProductItem.Image = ((System.Drawing.Image)(resources.GetObject("btnAddProductItem.Image")));
            this.btnAddProductItem.Location = new System.Drawing.Point(28, 80);
            this.btnAddProductItem.Name = "btnAddProductItem";
            this.btnAddProductItem.Size = new System.Drawing.Size(82, 39);
            this.btnAddProductItem.TabIndex = 131;
            this.btnAddProductItem.Text = "增加";
            this.btnAddProductItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAddProductItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddProductItem.UseVisualStyleBackColor = true;
            this.btnAddProductItem.Click += new System.EventHandler(this.btnAddProductItem_Click);
            // 
            // btnDeleteProductItem
            // 
            this.btnDeleteProductItem.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeleteProductItem.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //this.btnDeleteProductItem.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteProductItem.Image")));
            this.btnDeleteProductItem.Location = new System.Drawing.Point(297, 80);
            this.btnDeleteProductItem.Name = "btnDeleteProductItem";
            this.btnDeleteProductItem.Size = new System.Drawing.Size(82, 39);
            this.btnDeleteProductItem.TabIndex = 130;
            this.btnDeleteProductItem.Text = "删除";
            this.btnDeleteProductItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDeleteProductItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDeleteProductItem.UseVisualStyleBackColor = true;
            this.btnDeleteProductItem.Click += new System.EventHandler(this.btnDeleteProductItem_Click);
            // 
            // txtProductId
            // 
            this.txtProductId.Location = new System.Drawing.Point(288, 27);
            this.txtProductId.Name = "txtProductId";
            this.txtProductId.Size = new System.Drawing.Size(100, 21);
            this.txtProductId.TabIndex = 3;
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Location = new System.Drawing.Point(229, 30);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(41, 12);
            this.labelID.TabIndex = 2;
            this.labelID.Text = "产品ID";
            // 
            // txtProductName
            // 
            this.txtProductName.Location = new System.Drawing.Point(86, 27);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(100, 21);
            this.txtProductName.TabIndex = 1;
            // 
            // labelCarName
            // 
            this.labelCarName.AutoSize = true;
            this.labelCarName.Location = new System.Drawing.Point(17, 30);
            this.labelCarName.Name = "labelCarName";
            this.labelCarName.Size = new System.Drawing.Size(53, 12);
            this.labelCarName.TabIndex = 0;
            this.labelCarName.Text = "产品名称";
            // 
            // FormCarSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(192)))), ((int)(((byte)(233)))));
            this.ClientSize = new System.Drawing.Size(427, 410);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dgvProductItemIfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            //this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCarSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "产品列表";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCarSet_FormClosing);
            this.Load += new System.EventHandler(this.FormCarSet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductItemIfo)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvProductItemIfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnModifyProductItem;
        private System.Windows.Forms.Button btnAddProductItem;
        private System.Windows.Forms.Button btnDeleteProductItem;
        private System.Windows.Forms.TextBox txtProductId;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.TextBox txtProductName;
        private System.Windows.Forms.Label labelCarName;
        #endregion
    }
    public partial class FormCarSet : Form
    {
        Dictionary<Guid, CarID> Cars = new Dictionary<Guid, CarID>();
        public Dictionary<Guid, CarID> NewCars = new Dictionary<Guid, CarID>();
        bool isAlter = false;//是否修改过参数
        public FormCarSet(Dictionary<Guid, CarID> cars)
        {
            InitializeComponent();
            this.Cars = cars;
            foreach (var item in this.Cars.Keys)
            {
                this.NewCars.Add(item, new CarID(this.Cars[item].Name, this.Cars[item].ID));
            }

        }
        private void FormCarSet_Load(object sender, EventArgs e)
        {
            ShowCurrItem(-1);
        }
        private void btnAddProductItem_Click(object sender, EventArgs e)
        {
            try
            {
                //检验格式
                string productName = txtProductName.Text;
                if (productName == string.Empty)
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_name_could_not_null}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (-1 != productName.IndexOfAny(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }))
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_name_could_contine_char}\\/:*?\"<>|", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txtProductId.Text == string.Empty)
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_id_could_not_null}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int productId = -1;
                if (!int.TryParse(txtProductId.Text, out productId))
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_id_format_is_not_correct}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //检验是否重复
                foreach (var item in NewCars.Values)
                {
                    if (item.Name == productName)
                    {
                        MessageBox.Show($"{Resources.LanguageDic.product_name_already_exist}，{Resources.LanguageDic.could_not_repect_add}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (item.ID == productId)
                    {
                        MessageBox.Show($"{Resources.LanguageDic.product_id_already_exist}，{Resources.LanguageDic.could_not_repect_add}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                //添加
                NewCars.Add(Guid.NewGuid(), new CarID(productName, productId));
                isAlter = true;
                int currRow = NewCars.Count;
                ShowCurrItem(currRow - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnModifyProductItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProductItemIfo.CurrentRow == null)
                {
                    MessageBox.Show($"{Resources.LanguageDic.not_select_change_item}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //检验格式
                string newName = txtProductName.Text;
                if (newName == string.Empty)
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_name_could_not_null}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (-1 != newName.IndexOfAny(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }))
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_name_could_contine_char}\\/:*?\"<>|", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txtProductId.Text == string.Empty)
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_id_could_not_null}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int newId = -1;
                if (!int.TryParse(txtProductId.Text, out newId))
                {
                    MessageBox.Show($"{Resources.LanguageDic.product_id_format_is_not_correct}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //修改前的信息
                int currRow = dgvProductItemIfo.CurrentRow.Index;
                //string oldName = (string)dgvProductItemIfo.Rows[currRow].Cells[1].Value;
                int oldId = (int)dgvProductItemIfo.Rows[currRow].Cells[2].Value;

                //找到要修改的对象
                Guid key = new Guid();
                foreach (var item in NewCars.Keys)
                {
                    if (NewCars[item].ID == oldId)
                    {
                        key = item;
                        break;
                    }
                }

                //检验是否重复
                foreach (var item in NewCars.Keys)
                {
                    if (item == key)
                    {
                        continue;
                    }
                    if (NewCars[item].Name == newName)
                    {
                        MessageBox.Show($"{Resources.LanguageDic.product_name_already_exist}，{Resources.LanguageDic.could_not_repect_add}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (NewCars[item].ID == newId)
                    {
                        MessageBox.Show($"{Resources.LanguageDic.product_id_already_exist}，{Resources.LanguageDic.could_not_repect_add}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                //修改
                NewCars[key].Name = newName;
                NewCars[key].ID = newId;
                isAlter = true;

                ShowCurrItem(currRow);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnDeleteProductItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProductItemIfo.CurrentRow == null)
                {
                    MessageBox.Show($"{Resources.LanguageDic.not_select_delete_item}！", $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int currRow = dgvProductItemIfo.CurrentRow.Index;
                string productName = (string)dgvProductItemIfo.Rows[currRow].Cells[1].Value;

                //找到要删除的对象
                Guid key = new Guid();
                foreach (var item in NewCars.Keys)
                {
                    if (NewCars[item].Name == productName)
                    {
                        key = item;
                        break;
                    }
                }

                //移除
                NewCars.Remove(key);
                isAlter = true;

                ShowCurrItem(currRow - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        private void dgvProductItemIfo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvProductItemIfo.CurrentRow != null)
                {
                    int currRow = dgvProductItemIfo.CurrentRow.Index;
                    string productName = (string)dgvProductItemIfo.Rows[currRow].Cells[1].Value;
                    int productId = (int)dgvProductItemIfo.Rows[currRow].Cells[2].Value;
                    txtProductName.Text = productName;
                    txtProductId.Text = productId.ToString();
                }

            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void ShowCurrItem(int currRowIndex)
        {
            try
            {
                dgvProductItemIfo.Rows.Clear();
                if (NewCars.Count == 0) return;
                foreach (var item in NewCars.Values)
                {

                    int currRow = dgvProductItemIfo.Rows.Add();
                    dgvProductItemIfo.Rows[currRow].Cells[0].Value = currRow + 1;
                    dgvProductItemIfo.Rows[currRow].Cells[1].Value = item.Name;
                    dgvProductItemIfo.Rows[currRow].Cells[2].Value = item.ID;

                }
                if (currRowIndex < NewCars.Count)
                {
                    dgvProductItemIfo.ClearSelection();
                    if (currRowIndex < 0)
                    {
                        dgvProductItemIfo.Rows[0].Selected = true;
                        dgvProductItemIfo.CurrentCell = dgvProductItemIfo.Rows[0].Cells[0];
                    }
                    else
                    {
                        dgvProductItemIfo.Rows[currRowIndex].Selected = true;
                        dgvProductItemIfo.CurrentCell = dgvProductItemIfo.Rows[currRowIndex].Cells[0];

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), $"{Resources.LanguageDic.tip}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void FormCarSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isAlter)
            {
                this.DialogResult = MessageBox.Show($"{Resources.LanguageDic.is_save_para}？", $"{Resources.LanguageDic.tip}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (this.DialogResult == DialogResult.Cancel) e.Cancel = true;

            }
        }

    }

    public class Point3D
    {
        public float X, Y, Z;
        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Point3D(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }
    }

    //数模显示参数
    [Serializable]
    public class ShowPoint
    {
        public string Name = " ";
        public Point Point = new Point();
        public Point Location = new Point();
        public int Connection = 0;
    }
    public class ShowCarXYZ
    {
        public string Name;
        public Dictionary<string, ShowPoint> Points = new Dictionary<string, ShowPoint>();
        public bool Load(string filePath)
        {
            bool result2 = false;
            try
            {
                if (File.Exists(filePath))
                {
                    List<ShowPoint> list = new List<ShowPoint>();
                    XmlSerializer xml = new XmlSerializer(list.GetType());
                    using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        list = (List<ShowPoint>)xml.Deserialize(stream);
                    }
                    if (list != null)
                    {
                        Points = list.ToDictionary(n => { return n.Name; });
                        result2 = true;
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }
            return result2;
        }
        public bool Save(string filePath)
        {
            bool result = true;
            try
            {
                string basePath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                List<ShowPoint> list = Points.Values.ToList();
                if (list.Count() == 0)
                {
                    list.Add(new ShowPoint());
                }
                XmlSerializer xml = new XmlSerializer(list.GetType());
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    xml.Serialize(stream, list);
                }
            }
            catch (Exception ex)
            {
                result = false;
                File.AppendAllText("Error.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + ex.ToString() + "\r\n\r\n");
            }
            return result;
        }
    }
}
