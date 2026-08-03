using Basler.Pylon;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OnlineMeasurement
{
    public class BaslerCam
    {
        //相机连接的个数
        //public int CameraNumber = CameraFinder.Enumerate().Count;

        //放出一个Camera
        Camera camera = null;

        //basler里用于将相机采集的图像转换成位图
        //PixelDataConverter pxConvert = new PixelDataConverter();

        public BaslerParam Param = new BaslerParam();

        public string Name => camera?.CameraInfo[CameraInfoKey.UserDefinedName];

        public BaslerCam()
        {

        }
        /// <summary>
        /// 打开相机
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            try
            {
                if (camera == null)
                {
                    camera = new Camera();

                    //运行模式
                    camera.CameraOpened += Configuration.AcquireContinuous;//自由模式

                    //断开连接事件
                    camera.ConnectionLost += Camera_ConnectionLost;

                    //打开相机
                    camera.Open();

                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
        /// <summary>
        /// 打开相机
        /// </summary>
        /// <returns></returns>
        public bool Open(string IDName)
        {
            try
            {
                if (camera == null)
                {
                    foreach (ICameraInfo item in CameraFinder.Enumerate())
                    {
                        if (item[CameraInfoKey.UserDefinedName] == IDName.ToString())
                        {
                            camera = new Camera(item);

                            //运行模式
                            camera.CameraOpened += Configuration.AcquireContinuous;//自由模式

                            //断开连接事件
                            camera.ConnectionLost += Camera_ConnectionLost;

                            //打开相机
                            camera.Open();

                            return true;
                        }
                    }
                }
                else
                {
                    if (IDName.ToString() == camera.Parameters[PLGigECamera.DeviceUserID].GetValue())
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void Camera_ConnectionLost(object sender, EventArgs e)//断开连接事件
        {
            camera.StreamGrabber.Stop();
            Close();
        }

        /// <summary>
        /// 获取曝光
        /// </summary>
        /// <returns>获取成功返回曝光值，获取失败返回-1</returns>
        public double GetExposure()
        {
            if (camera != null)
            {
                try
                {
                    return camera.Parameters[PLGigECamera.ExposureTimeAbs].GetValue();
                }
                catch
                {
                    return -1;
                }
            }
            return -1;
        }

        /// <summary>
        /// 设置相机曝光
        /// </summary>
        /// <param name="time">要设置的曝光值，范围10-840000</param>
        public bool SetExposure(double time)
        {
            if (camera != null)
            {
                try
                {
                    camera.Parameters[PLGigECamera.ExposureTimeAbs].SetValue(time);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取相机频率
        /// </summary>
        /// <returns>获取成功返回频率值，获取失败返回-1</returns>
        public double GetHz()
        {
            try
            {
                if (camera != null)
                {
                    return camera.Parameters[PLGigECamera.AcquisitionFrameRateAbs].GetValue();
                }
            }
            catch { }
            return -1;
        }

        /// <summary>
        /// 设置相机频率
        /// </summary>
        /// <param name="hz">要设置的频率值，范围0.15-1000000</param>
        public bool SetHz(double hz)
        {
            if (camera != null)
            {
                try
                {
                    camera.Parameters[PLGigECamera.AcquisitionFrameRateEnable].SetValue(true);
                    camera.Parameters[PLGigECamera.AcquisitionFrameRateAbs].SetValue(hz);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置相机输出图像尺寸
        /// </summary>
        /// <param name="width">图像的宽度</param>
        /// <param name="height">图像的高度</param>
        /// <returns></returns>
        public bool SetSize(long width, long height)
        {
            if (camera != null)
            {
                try
                {
                    if (width > camera.Parameters[PLGigECamera.WidthMax].GetValue())
                    {
                        width = camera.Parameters[PLGigECamera.WidthMax].GetValue();
                    }
                    if (height > camera.Parameters[PLGigECamera.HeightMax].GetValue())
                    {
                        height = camera.Parameters[PLGigECamera.HeightMax].GetValue();
                    }
                    camera.Parameters[PLGigECamera.Width].SetValue(width);
                    camera.Parameters[PLGigECamera.Height].SetValue(height);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public Size GetSize()
        {
            Size s = new Size(-1, -1);
            if (camera != null)
            {
                try
                {
                    s.Width = (int)camera.Parameters[PLGigECamera.Width].GetValue();
                    s.Height = (int)camera.Parameters[PLGigECamera.Height].GetValue();
                }
                catch { }
            }
            return s;
        }

        /// <summary>
        /// 设置相机输出图像偏移
        /// </summary>
        /// <param name="offsetX">图像的X偏移</param>
        /// <param name="offsetY">图像的Y偏移</param>
        /// <returns></returns>
        public bool SetOffset(long offsetX, long offsetY)
        {
            if (camera != null)
            {
                try
                {
                    if (offsetX > camera.Parameters[PLGigECamera.WidthMax].GetValue() - camera.Parameters[PLGigECamera.Width].GetValue())
                    {
                        offsetX = camera.Parameters[PLGigECamera.WidthMax].GetValue() - camera.Parameters[PLGigECamera.Width].GetValue();
                    }
                    else if (offsetX < 0)
                    {
                        offsetX = 0;
                    }
                    if (offsetY > camera.Parameters[PLGigECamera.HeightMax].GetValue() - camera.Parameters[PLGigECamera.Height].GetValue())
                    {
                        offsetY = camera.Parameters[PLGigECamera.HeightMax].GetValue() - camera.Parameters[PLGigECamera.Height].GetValue();
                    }
                    else if (offsetY < 0)
                    {
                        offsetY = 0;
                    }
                    camera.Parameters[PLGigECamera.OffsetX].SetValue(offsetX);
                    camera.Parameters[PLGigECamera.OffsetY].SetValue(offsetY);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public Point GetOffset()
        {
            Point s = new Point(0, 0);
            if (camera != null)
            {
                try
                {
                    s.X = (int)camera.Parameters[PLGigECamera.OffsetX].GetValue();
                    s.Y = (int)camera.Parameters[PLGigECamera.OffsetY].GetValue();
                }
                catch { }
            }
            return s;
        }

        public HImage OneShotByGray(double left, double top, double right, double down, byte grayMin, byte grayMax, out double outGray)
        {
            outGray = -1;
            if (camera != null)
            {
                if (camera.StreamGrabber.IsGrabbing)
                {
                    camera.StreamGrabber.Stop();
                    Thread.Sleep(100);
                }
                HImage hImage = null;

                bool bRun = true;
                int count = 0;

                double exposureMin = 35;
                double exposureMax = 573440;
                long Width = camera.Parameters[PLGigECamera.Width].GetValue();
                long Height = camera.Parameters[PLGigECamera.Height].GetValue();
                double col1 = Width * left;
                double col2 = Width * right;
                double row1 = Height * top;
                double row2 = Height * down;

                HRegion hRegion = new HRegion(row1, col1, row2, col2);

                while (bRun)
                {
                    count++;
                    camera.StreamGrabber.Start(1);
                    while (camera.StreamGrabber.IsGrabbing)
                    {
                        // Wait for an image and then retrieve it. A timeout of 5000 ms is used.
                        IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);

                        using (grabResult)
                        {
                            // Image grabbed successfully?
                            if (grabResult.GrabSucceeded)
                            {

                                hImage = GrabResult2HImage(grabResult);
                                double gray = hImage.Intensity(hRegion, out double _);
                                if (gray == 0)
                                {
                                    //HOperatorSet.GenRectangle1(out HObject hObjectRegion, row1, col1, row2, col2);
                                    //HOperatorSet.Intensity(hObjectRegion, hImage, out HTuple tuple, out HTuple _);
                                    //gray = tuple.D;
                                    hRegion?.Dispose();
                                    hRegion = new HRegion(row1, col1, row2, col2);
                                    gray = hImage.Intensity(hRegion, out double _);
                                }
                                if (gray < grayMin)
                                {
                                    exposureMin = GetExposure();
                                    Console.WriteLine(exposureMin + " " + gray + " " + count);
                                    if (exposureMax == 573440)
                                    {

                                        SetExposure(Math.Min(exposureMin * 2, 573440));
                                    }
                                    else
                                    {
                                        SetExposure((exposureMin + exposureMax) / 2);
                                    }
                                }
                                else if (gray > grayMax)
                                {
                                    exposureMax = GetExposure();
                                    Console.WriteLine(exposureMax + " " + gray + " " + count);
                                    if (exposureMin == 35)
                                    {
                                        SetExposure(Math.Max(exposureMax / 2, 10));
                                    }
                                    else
                                    {
                                        SetExposure((exposureMin + exposureMax) / 2);
                                    }
                                }
                                else { bRun = false; }

                                if (exposureMin == exposureMax)
                                {
                                    bRun = false;
                                }
                                outGray = gray;
                            }
                        }
                    }
                }
                hRegion?.Dispose();
                return hImage;
            }
            return null;
        }

        public HImage OneShot()
        {
            if (camera != null)
            {
                if (camera.StreamGrabber.IsGrabbing)
                {
                    camera.StreamGrabber.Stop();
                    Thread.Sleep(100);
                }

                camera.StreamGrabber.Start(1);
                while (camera.StreamGrabber.IsGrabbing)
                {
                    // Wait for an image and then retrieve it. A timeout of 5000 ms is used.
                    IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
                    HImage hImage = null;
                    using (grabResult)
                    {
                        // Image grabbed successfully?
                        if (grabResult.GrabSucceeded)
                        {
                            hImage = GrabResult2HImage(grabResult);
                        }
                    }
                    return hImage;
                }
            }
            return null;
        }
        public HImage OneShot(double left, double top, double right, double down, out double outGray)
        {
            outGray = -1;
            if (camera != null)
            {
                if (camera.StreamGrabber.IsGrabbing)
                {
                    camera.StreamGrabber.Stop();
                    Thread.Sleep(100);
                }
                HImage hImage = null;
                double col1 = 0, col2 = 0, row1 = 0, row2 = 0;

                camera.StreamGrabber.Start(1);
                while (camera.StreamGrabber.IsGrabbing)
                {
                    // Wait for an image and then retrieve it. A timeout of 5000 ms is used.
                    IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);

                    using (grabResult)
                    {
                        // Image grabbed successfully?
                        if (grabResult.GrabSucceeded)
                        {
                            col1 = grabResult.Width * left;
                            col2 = grabResult.Width * right;
                            row1 = grabResult.Height * top;
                            row2 = grabResult.Height * down;
                            hImage = GrabResult2HImage(grabResult);
                            outGray = hImage.Intensity(new HRegion(row1, col1, row2, col2), out double _);
                        }
                    }
                }
                return hImage;
            }
            return null;
        }



        public void KeepShot(Action<HImage> UseImages)
        {
            if (camera != null)
            {
                if (camera.StreamGrabber.IsGrabbing)
                {
                    camera.StreamGrabber.Stop();
                    Thread.Sleep(100);
                }
                camera.StreamGrabber.Start();
                Thread th = new Thread(() =>
                {
                    while (camera != null && camera.StreamGrabber.IsGrabbing)
                    {
                        // Wait for an image and then retrieve it. A timeout of 5000 ms is used.
                        IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
                        if (grabResult != null)
                        {
                            using (grabResult)
                            {
                                // Image grabbed successfully?
                                if (grabResult.GrabSucceeded)
                                {
                                    // Access the image data.
                                    HImage hImage = GrabResult2HImage(grabResult);
                                    UseImages(hImage);
                                }
                            }
                        }
                    };
                });
                th.Start();
            }
        }

        public void Stop()
        {
            if (camera != null)
            {
                camera.StreamGrabber.Stop();
            }
        }

        /// <summary>
        /// 关闭相机
        /// </summary>
        public void Close()
        {
            if (camera != null)
            {
                if (camera.StreamGrabber.IsGrabbing)
                {
                    camera.StreamGrabber.Stop();
                }
                while (camera.StreamGrabber.IsGrabbing)
                {
                    Thread.Sleep(7);
                }
                camera.Close();

                //camera.Dispose();
                camera = null;
            }
        }

        //将相机抓取到的图像转换成HImage图
        HImage GrabResult2HImage(IGrabResult grabResult)
        {
            if (grabResult.PixelTypeValue == PixelType.Mono8)
            {
                HImage image = new HImage("byte", grabResult.Width, grabResult.Height, grabResult.PixelDataPointer);
                //HImage himage = image.RotateImage(180.0, "constant");
                //image.Dispose();
                return image;
            }
            else if (grabResult.PixelTypeValue == PixelType.Mono12)
            {
                HImage image = new HImage("uint2", grabResult.Width, grabResult.Height, grabResult.PixelDataPointer);
                //HImage himage = image.RotateImage(180.0, "constant");
                //image.Dispose();
                return image;
            }
            else
            {
                return null;
            }
        }

        public bool SingleTrigger(bool bflag)
        {
            try
            {
                if (bflag)
                {
                    camera?.Parameters[PLGigECamera.AcquisitionMode].SetValue(PLGigECamera.AcquisitionMode.Continuous);//采集模式，连续
                    camera?.Parameters[PLGigECamera.TriggerSelector].SetValue(PLGigECamera.TriggerSelector.FrameStart);//选择信号用途？？？

                    camera?.Parameters[PLGigECamera.TriggerMode].SetValue(PLGigECamera.TriggerMode.On);//触发模式
                    camera?.Parameters[PLGigECamera.TriggerSource].SetValue(PLGigECamera.TriggerSource.Line1);//触发源
                    camera?.Parameters[PLGigECamera.TriggerActivation].SetValue(PLGigECamera.TriggerActivation.RisingEdge);//上升沿
                    camera?.Parameters[PLGigECamera.TimerDelayAbs].SetValue(1000);//触发延时，单位us 
                }
                else
                {
                    camera?.Parameters[PLGigECamera.TriggerMode].SetValue(PLGigECamera.TriggerMode.Off);//触发模式
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool InitSet()
        {
            bool result = true;
            try
            {
                camera?.Parameters[PLGigECamera.PixelFormat].SetValue(PLGigECamera.PixelFormat.Mono8);
            }
            catch
            {
                result = false;
            }
            if (!SingleTrigger(false))
            {
                result = false;
            }
            if (!SetSize(Param.SizeWidth, Param.SizeHeight))
            {
                result = false;
            }
            if (!SetOffset(Param.OffsetX, Param.OffsetY))
            {
                result = false;
            }
            return result;
        }
    }

    enum Trigger
    {
        SoftTrigger = 0,
        s = 1
    }

    [Serializable]
    public class BaslerParam
    {
        public bool Enable = true;
        public int Exposure = 5000;
        public int SizeWidth = 640, SizeHeight = 480;
        public int OffsetX = 0, OffsetY = 0;
        public double LeftX = 0.25, TopY = 0.25, RightX = 0.75, DownY = 0.75;
        public byte GrayMin = 0, GrayMax = 255;
        public string ImageFormat = ".png";

        public bool Load()
        {
            bool result = true;
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\";
            try
            {
                string paramPath = basePath + "BaslerParam.xml";
                if (File.Exists(paramPath))
                {
                    BaslerParam param = null;
                    XmlSerializer xml = new XmlSerializer(this.GetType());
                    using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                    {
                        param = (BaslerParam)xml.Deserialize(stream);
                    }
                    if (param != null)
                    {
                        this.Enable = param.Enable;
                        this.Exposure = param.Exposure;
                        this.SizeWidth = param.SizeWidth;
                        this.SizeHeight = param.SizeHeight;
                        this.OffsetX = param.OffsetX;
                        this.OffsetY = param.OffsetY;
                        this.LeftX = param.LeftX;
                        this.TopY = param.TopY;
                        this.RightX = param.RightX;
                        this.DownY = param.DownY;
                        this.GrayMin = param.GrayMin;
                        this.GrayMax = param.GrayMax;
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
            }
            catch (Exception ex)
            {
                result = false;
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

            if (!result)
            {
                result = true;
                try
                {
                    string paramPath = basePath + "BaslerParam_bak.xml";
                    if (File.Exists(paramPath))
                    {
                        BaslerParam param = null;
                        XmlSerializer xml = new XmlSerializer(this.GetType());
                        using (FileStream stream = new FileStream(paramPath, FileMode.OpenOrCreate))
                        {
                            param = (BaslerParam)xml.Deserialize(stream);
                        }
                        if (param != null)
                        {
                            this.Enable = param.Enable;
                            this.Exposure = param.Exposure;
                            this.SizeWidth = param.SizeWidth;
                            this.SizeHeight = param.SizeHeight;
                            this.OffsetX = param.OffsetX;
                            this.OffsetY = param.OffsetY;
                            this.LeftX = param.LeftX;
                            this.TopY = param.TopY;
                            this.RightX = param.RightX;
                            this.DownY = param.DownY;
                            this.GrayMin = param.GrayMin;
                            this.GrayMax = param.GrayMax;

                            File.Copy(paramPath, basePath + "BaslerParam.xml", true);
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
                }
                catch (Exception ex)
                {
                    result = false;
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }

            return result;
        }
        public bool Save()
        {
            bool result = true;
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string BaslerParamPath = basePath + "BaslerParam.xml";
                XmlSerializer xml = new XmlSerializer(this.GetType());
                using (FileStream stream = new FileStream(BaslerParamPath, FileMode.Create))
                {
                    xml.Serialize(stream, this);
                }
                File.Copy(BaslerParamPath, basePath + "BaslerParam_bak.xml", true);
            }
            catch (Exception ex) { result = false; System.Windows.Forms.MessageBox.Show(ex.ToString()); }
            return result;
        }
    }
}
