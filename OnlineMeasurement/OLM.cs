using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineMeasurement
{
    public class OLM
    {

        /// <summary>
        /// 库调用
        /// </summary>
        
        private const string DllName = "dll\\OLM\\OLM_RobotCalib.dll"; // Replace with the actual DLL 
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OLM_RobotCalib_CalibInitKine(string robotDir, string robotNameToSave, string dataDir1, string dataDir2);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OLM_RobotCalib_Init(string robotDir);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OLM_RobotCalib_SetSphereFinder(IntPtr handle, float sphereRadius,
            string sphereSegModelPath, float sphereSegConfThresh,
            float scaleSphereRegion,
            int laserMaxWidth, int laserContrast,
            float laserDBScanEps);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OLM_RobotCalib_OptimizeEyehand(IntPtr handle, string dataDir1, string dataDir2,
            string camXYZMapPath,
            string lightXYZMapPath, string lightInCamPath,
            string toolInCamPath,
            string toolInCamOptmzdPath,
            string ballInBaseSavePath,
            out float biasTranslation, out float biasRotation);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]


        public static extern bool OLM_RobotCalib_OptimizeKineRunTime(IntPtr handle, string dataDir1, string dataDir2,
            string camXYZMapPath,
            string lightXYZMapPath, string lightInCamPath,
            string toolInCamOptmzdPath,
            string ballInBasePath,
            string robotName, string calibedPostfix,
            out float biasFitBalls
);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]


        public static extern bool OLM_RobotCalib_Joint2Cart(IntPtr handle, bool useLastOptKine,
            double jointPoseJ1, double jointPoseJ2, double jointPoseJ3,
            double jointPoseJ4, double jointPoseJ5, double jointPoseJ6,
            out double cartPoseX, out double cartPoseY, out double cartPoseZ,
            out double cartPoseRx, out double cartPoseRy, out double cartPoseRz
);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void OLM_RobotCalib_Free(IntPtr handle);


        /// <summary>
        /// 静态函数
        /// </summary>

        // 图像坐标转光平面再转相机坐标系坐标
        public static void TransformImageWorld(HTuple hv_Width, HTuple hv_Height, HObject ho_Map, HTuple hv_CameraParameters, HTuple hv_LightInCam, HTuple hv_LightToCam, out HObject worldImageMapped)
        {
            HOperatorSet.GenEmptyObj(out worldImageMapped);

            //光面计算
            HTuple hv_X = new HTuple(), hv_Y = new HTuple();


            HImage worldXImage = new HImage();
            HImage worldYImage = new HImage();
            HImage worldZImage = new HImage();
            worldXImage.GenImageConst("real", hv_Width, hv_Height);
            worldYImage.GenImageConst("real", hv_Width, hv_Height);
            worldZImage.GenImageConst("real", hv_Width, hv_Height);

            int W = hv_Width;
            int H = hv_Height;
            int N = W * H;

            // 行号 0…H-1  重复 W 次
            int[] yArr = new int[N];
            for (int k = 0; k < N; k++) yArr[k] = k / W;   // 单层循环，速度已经很快

            // 列号 0…W-1  连续重复 H 次
            int[] xArr = new int[N];
            for (int k = 0; k < N; k++) xArr[k] = k % W;

            HTuple yList = new HTuple(yArr);
            HTuple xList = new HTuple(xArr);


            HOperatorSet.ImagePointsToWorldPlane(hv_CameraParameters, hv_LightInCam, yList,
                xList, "m", out hv_X, out hv_Y);

            HTuple hv_Z = new HTuple(new double[hv_X.Length]);

            HOperatorSet.AffineTransPoint3d(hv_LightToCam, hv_X, hv_Y, hv_Z, out HTuple camX, out HTuple camY,
                out HTuple camZ);

            worldXImage.SetGrayval(yList, xList, (HTuple)(camX));
            worldYImage.SetGrayval(yList, xList, (HTuple)(camY));
            worldZImage.SetGrayval(yList, xList, (HTuple)(camZ));


            HOperatorSet.Compose3(worldXImage, worldYImage, worldZImage, out HObject worldImage);
            //图片矫正
            HOperatorSet.MapImage(worldImage, ho_Map, out worldImageMapped);

        }

        // 图像坐标转光平面

        public static void TransformImageLight(HTuple hv_Width, HTuple hv_Height, HObject ho_Map, HTuple hv_CameraParameters, HTuple hv_LightInCam,  out HObject ho_LightMapped)
        {
            HOperatorSet.GenEmptyObj(out ho_LightMapped);

            //光面计算
            HTuple hv_X = new HTuple(), hv_Y = new HTuple();

            HImage lightXImage = new HImage();
            HImage lightYImage = new HImage();
            lightXImage.GenImageConst("real", hv_Width, hv_Height);
            lightYImage.GenImageConst("real", hv_Width, hv_Height);

            int W = hv_Width;
            int H = hv_Height;
            int N = W * H;

            // 行号 0…H-1  重复 W 次
            int[] yArr = new int[N];
            for (int k = 0; k < N; k++) yArr[k] = k / W;   // 单层循环，速度已经很快

            // 列号 0…W-1  连续重复 H 次
            int[] xArr = new int[N];
            for (int k = 0; k < N; k++) xArr[k] = k % W;


            HTuple yList = new HTuple(yArr);
            HTuple xList = new HTuple(xArr);


            HOperatorSet.ImagePointsToWorldPlane(hv_CameraParameters, hv_LightInCam, yList,
                xList, "m", out hv_X, out hv_Y);

            lightXImage.SetGrayval(yList, xList, (HTuple)(hv_X));
            lightYImage.SetGrayval(yList, xList, (HTuple)(hv_Y));

            HOperatorSet.Compose2(lightXImage, lightYImage, out HObject lightImage);

            //图片矫正
            ho_LightMapped.Dispose();
            HOperatorSet.MapImage(lightImage, ho_Map, out ho_LightMapped);
        }

        /// <summary>
        /// 参数
        /// </summary>
        string m_robotDir = "../config/kuka_kr10_r1420_z";

        string m_dataDir1 = "../test/kuka_kr60_ha/fake_dual/first";
        string m_dataDir2 = "../test/kuka_kr60_ha/fake_dual/second";

        string m_camXYZMapPath = "D:/Projects/P07250114/P07250114_CalibRobot/scripts/camImage.tif";
        string m_lightXYZMapPath = "D:/Projects/P07250114/P07250114_CalibRobot/scripts/lightImage.tif";
        string m_lightInCamPath = "D:/Projects/P07250114/P07250114_CalibRobot/scripts/lightInCam_wt.dat";
        string m_toolInCamPath = "D:/Projects/P07250114/P07250114_CalibRobot/scripts/ToolInCam.dat";

        string m_toolInCamOptmzdPath = "ToolInCamOptmzd.dat";
        string m_ballInBaseSavePath = "ballsInBase.json";
        string m_robotName = "sn1_dual";
        string m_calibedPostfix = "calibed";

        string m_sphereSegModelPath = "../refer_imgs/model.onnx";

        IntPtr intPtr = IntPtr.Zero;

        public string errMsg ="";


        /// <summary>
        /// 函数
        /// </summary>

        public OLM()
        {
        }
        public bool init(string robotDir, string robotName,string sphereSegModelPath)
        {
            // 记录参数
            m_robotDir=robotDir;
            m_robotName=robotName;
            m_sphereSegModelPath = sphereSegModelPath;

            // 初始化机器人
            intPtr = OLM_RobotCalib_Init(m_robotDir);

            // 设置参数
            float sphereRadius = 15.0f;
            float sphereSegConfThresh = 0.3f;

            float scaleSphereRegion = 0.95f;
            int laserMaxWidth = 25;
            int laserContrast = 80;
            float laserDBScanEps = 5.0f;
            bool ret = OLM_RobotCalib_SetSphereFinder(intPtr, sphereRadius,
                m_sphereSegModelPath, sphereSegConfThresh,
                scaleSphereRegion,
                laserMaxWidth, laserContrast,
                laserDBScanEps
            );
            if (!ret)
            {
                errMsg = "参数设置失败";
                return false;
            }
            return true;
        }

        public bool calib( string dataDir1, string dataDir2, string camXYZMapPath, string lightXYZMapPath, string lightInCamPath,
                string toolInCamPath)
        {
            //参数更新
            m_dataDir1 = dataDir1;
            m_dataDir2 = dataDir2;
            m_camXYZMapPath = camXYZMapPath;
            m_lightXYZMapPath = lightXYZMapPath;
            m_lightInCamPath = lightInCamPath;
            m_toolInCamPath = toolInCamPath;


            if (intPtr == IntPtr.Zero)
            {
                errMsg = "请先初始化机器人";
                return false;
            }
            // 

            //判断是否第一次执行，看一下要不要部署
            string[] urdfFiles = Directory.GetFiles(m_robotDir, "*.urdf", SearchOption.AllDirectories);

            if (urdfFiles.Count() == 0)
            {
                errMsg = "不存在机器人urdf文件";
                return false;
            }

            if (!File.Exists(Path.ChangeExtension(urdfFiles[0], $".{m_robotName}")))
            {
                bool ret2 = OLM_RobotCalib_CalibInitKine(m_robotDir, m_robotName, m_dataDir1, m_dataDir2);

                if (!ret2)
                {
                    errMsg = "机器人部署失败";
                    return false;
                }
            }



            //判断是否需要优化手眼标定
            m_toolInCamOptmzdPath = Path.GetDirectoryName(m_toolInCamPath) + "ToolInCamOptmzd.dat";
            m_ballInBaseSavePath = Path.GetDirectoryName(m_toolInCamPath) + "ballsInBase.json";
            if (!File.Exists(m_toolInCamOptmzdPath) || !File.Exists(m_ballInBaseSavePath))
            {
                float biasTranslation, biasRotation;
                bool ret2 = OLM_RobotCalib_OptimizeEyehand(intPtr, dataDir1, dataDir2,
                    m_camXYZMapPath, m_lightXYZMapPath, m_lightInCamPath,
                    m_toolInCamPath, m_toolInCamOptmzdPath, m_ballInBaseSavePath,
                    out biasTranslation, out biasRotation);

                Console.WriteLine($"biasTranslation: {biasTranslation:F3} mm, biasRotation: {biasRotation:F3} deg\n");
                if (!ret2)
                {
                    errMsg = "优化手眼标定失败";
                    return false;
                }
            }


            //温漂标定
            float biasFitBalls;

            bool ret = OLM_RobotCalib_OptimizeKineRunTime(intPtr, m_dataDir1, m_dataDir2,
                m_camXYZMapPath, m_lightXYZMapPath, m_lightInCamPath,
                m_toolInCamOptmzdPath, m_ballInBaseSavePath,
                m_robotName, m_calibedPostfix,
                out biasFitBalls);

            if (!ret)
            {
                errMsg = "温漂标定失败";
                return false;
            }
            Console.WriteLine($"biasFitBalls: {biasFitBalls:F3} mm\n" );


            return true;
        }

        public bool run(bool isUseOpt,double A1, double A2, double A3, double A4, double A5, double A6,
            out double X, out double Y, out double Z, out double RX, out double RY, out double RZ)
        {
            X=0; Y=0; Z=0; RX=0; RY=0; RZ=0;

            if (intPtr == IntPtr.Zero)
            {

                errMsg = "请先初始化机器人";
                return false;
            }
            return OLM_RobotCalib_Joint2Cart(intPtr, isUseOpt,
                A1, A2, A3, A4, A5, A6,out X, out Y, out Z, out RX, out RY, out RZ);
        }


        ~OLM()
        {
            if (intPtr == IntPtr.Zero)
            {
                return;
            }
            //释放指针
            OLM_RobotCalib_Free(intPtr);
        }


    }
}
