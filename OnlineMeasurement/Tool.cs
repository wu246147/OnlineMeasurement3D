using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OnlineMeasurement;

namespace BaslerCamera
{
    public class Tool
    {


        //坐标系转换
        #region
        public static void xyz2zyx(HTuple hv_r1, HTuple hv_r2, HTuple hv_r3, out HTuple hv_R1_new,
out HTuple hv_R2_new, out HTuple hv_R3_new)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Pose = new HTuple(), hv_PoseOut = new HTuple();
            // Initialize local and output iconic variables 
            hv_R1_new = new HTuple();
            hv_R2_new = new HTuple();
            hv_R3_new = new HTuple();
            hv_Pose.Dispose();
            HOperatorSet.CreatePose(0, 0, 0, hv_r1, hv_r2, hv_r3, "Rp+T", "abg", "point",
                out hv_Pose);
            hv_PoseOut.Dispose();
            HOperatorSet.ConvertPoseType(hv_Pose, "Rp+T", "gba", "point", out hv_PoseOut);
            hv_R1_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R1_new = hv_PoseOut.TupleSelect(
                    3);
            }
            hv_R2_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R2_new = hv_PoseOut.TupleSelect(
                    4);
            }
            hv_R3_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R3_new = hv_PoseOut.TupleSelect(
                    5);
            }

            hv_Pose.Dispose();
            hv_PoseOut.Dispose();

            return;
        }

        public static void xyz2zyz(HTuple hv_r1, HTuple hv_r2, HTuple hv_r3, out HTuple hv_R1_new,
            out HTuple hv_R2_new, out HTuple hv_R3_new)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Pose = new HTuple(), hv_HomMat3DXYZ = new HTuple();
            // Initialize local and output iconic variables 
            hv_R1_new = new HTuple();
            hv_R2_new = new HTuple();
            hv_R3_new = new HTuple();
            hv_Pose.Dispose();
            HOperatorSet.CreatePose(0, 0, 0, hv_r1, hv_r2, hv_r3, "Rp+T", "abg", "point",
                out hv_Pose);
            hv_HomMat3DXYZ.Dispose();
            HOperatorSet.PoseToHomMat3d(hv_Pose, out hv_HomMat3DXYZ);
            //ZYZ
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R1_new.Dispose();
                HOperatorSet.TupleAtan2(hv_HomMat3DXYZ.TupleSelect(6), hv_HomMat3DXYZ.TupleSelect(
                    2), out hv_R1_new);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R2_new.Dispose();
                HOperatorSet.TupleAtan2(((1 - (((hv_HomMat3DXYZ.TupleSelect(10))).TuplePow(2)))).TupleSqrt()
                    , hv_HomMat3DXYZ.TupleSelect(10), out hv_R2_new);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R3_new.Dispose();
                HOperatorSet.TupleAtan2(hv_HomMat3DXYZ.TupleSelect(9), -(hv_HomMat3DXYZ.TupleSelect(
                    8)), out hv_R3_new);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                {
                    HTuple
                      ExpTmpLocalVar_R1_new = hv_R1_new.TupleDeg()
                        ;
                    hv_R1_new.Dispose();
                    hv_R1_new = ExpTmpLocalVar_R1_new;
                }
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                {
                    HTuple
                      ExpTmpLocalVar_R2_new = hv_R2_new.TupleDeg()
                        ;
                    hv_R2_new.Dispose();
                    hv_R2_new = ExpTmpLocalVar_R2_new;
                }
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                {
                    HTuple
                      ExpTmpLocalVar_R3_new = hv_R3_new.TupleDeg()
                        ;
                    hv_R3_new.Dispose();
                    hv_R3_new = ExpTmpLocalVar_R3_new;
                }
            }

            hv_Pose.Dispose();
            hv_HomMat3DXYZ.Dispose();

            return;
        }

        public static void zyx2xyz(HTuple hv_r1, HTuple hv_r2, HTuple hv_r3, out HTuple hv_R1_new,
            out HTuple hv_R2_new, out HTuple hv_R3_new)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Pose = new HTuple(), hv_PoseOut = new HTuple();
            // Initialize local and output iconic variables 
            hv_R1_new = new HTuple();
            hv_R2_new = new HTuple();
            hv_R3_new = new HTuple();
            hv_Pose.Dispose();
            HOperatorSet.CreatePose(0, 0, 0, hv_r1, hv_r2, hv_r3, "Rp+T", "gba", "point",
                out hv_Pose);
            hv_PoseOut.Dispose();
            HOperatorSet.ConvertPoseType(hv_Pose, "Rp+T", "abg", "point", out hv_PoseOut);
            hv_R1_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R1_new = hv_PoseOut.TupleSelect(
                    3);
            }
            hv_R2_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R2_new = hv_PoseOut.TupleSelect(
                    4);
            }
            hv_R3_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R3_new = hv_PoseOut.TupleSelect(
                    5);
            }

            hv_Pose.Dispose();
            hv_PoseOut.Dispose();

            return;
        }

        public static void zyx2zyz(HTuple hv_r1, HTuple hv_r2, HTuple hv_r3, out HTuple hv_R1_new,
            out HTuple hv_R2_new, out HTuple hv_R3_new)
        {


            // Initialize local and output iconic variables 
            hv_R1_new = new HTuple();
            hv_R2_new = new HTuple();
            hv_R3_new = new HTuple();
            hv_R1_new.Dispose(); hv_R2_new.Dispose(); hv_R3_new.Dispose();
            zyx2xyz(hv_r1, hv_r2, hv_r3, out hv_R1_new, out hv_R2_new, out hv_R3_new);
            {
                HTuple ExpTmpOutVar_0; HTuple ExpTmpOutVar_1; HTuple ExpTmpOutVar_2;
                xyz2zyz(hv_R1_new, hv_R2_new, hv_R3_new, out ExpTmpOutVar_0, out ExpTmpOutVar_1,
                    out ExpTmpOutVar_2);
                hv_R1_new.Dispose();
                hv_R1_new = ExpTmpOutVar_0;
                hv_R2_new.Dispose();
                hv_R2_new = ExpTmpOutVar_1;
                hv_R3_new.Dispose();
                hv_R3_new = ExpTmpOutVar_2;
            }


            return;
        }

        public static void zyz2xyz(HTuple hv_r1, HTuple hv_r2, HTuple hv_r3, out HTuple hv_R1_new,
            out HTuple hv_R2_new, out HTuple hv_R3_new)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_HomMat3DIdentity = new HTuple();
            HTuple hv_HomMat3DRotate = new HTuple(), hv_HomMat3DRotate1 = new HTuple();
            HTuple hv_HomMat3DRotate2 = new HTuple(), hv_Pose5 = new HTuple();
            HTuple hv_PoseXYZ = new HTuple();
            // Initialize local and output iconic variables 
            hv_R1_new = new HTuple();
            hv_R2_new = new HTuple();
            hv_R3_new = new HTuple();
            hv_HomMat3DIdentity.Dispose();
            HOperatorSet.HomMat3dIdentity(out hv_HomMat3DIdentity);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_HomMat3DRotate.Dispose();
                HOperatorSet.HomMat3dRotateLocal(hv_HomMat3DIdentity, hv_r1.TupleRad(), "z",
                    out hv_HomMat3DRotate);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_HomMat3DRotate1.Dispose();
                HOperatorSet.HomMat3dRotateLocal(hv_HomMat3DRotate, hv_r2.TupleRad(), "y", out hv_HomMat3DRotate1);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_HomMat3DRotate2.Dispose();
                HOperatorSet.HomMat3dRotateLocal(hv_HomMat3DRotate1, hv_r3.TupleRad(), "z", out hv_HomMat3DRotate2);
            }
            hv_Pose5.Dispose();
            HOperatorSet.HomMat3dToPose(hv_HomMat3DRotate2, out hv_Pose5);
            hv_PoseXYZ.Dispose();
            HOperatorSet.ConvertPoseType(hv_Pose5, "Rp+T", "abg", "point", out hv_PoseXYZ);

            hv_R1_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R1_new = hv_PoseXYZ.TupleSelect(
                    3);
            }
            hv_R2_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R2_new = hv_PoseXYZ.TupleSelect(
                    4);
            }
            hv_R3_new.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_R3_new = hv_PoseXYZ.TupleSelect(
                    5);
            }

            hv_HomMat3DIdentity.Dispose();
            hv_HomMat3DRotate.Dispose();
            hv_HomMat3DRotate1.Dispose();
            hv_HomMat3DRotate2.Dispose();
            hv_Pose5.Dispose();
            hv_PoseXYZ.Dispose();

            return;
        }

        public static void zyz2zyx(HTuple hv_r1, HTuple hv_r2, HTuple hv_r3, out HTuple hv_R1_new,
            out HTuple hv_R2_new, out HTuple hv_R3_new)
        {


            // Initialize local and output iconic variables 
            hv_R1_new = new HTuple();
            hv_R2_new = new HTuple();
            hv_R3_new = new HTuple();
            hv_R1_new.Dispose(); hv_R2_new.Dispose(); hv_R3_new.Dispose();
            zyz2xyz(hv_r1, hv_r2, hv_r3, out hv_R1_new, out hv_R2_new, out hv_R3_new);
            {
                HTuple ExpTmpOutVar_0; HTuple ExpTmpOutVar_1; HTuple ExpTmpOutVar_2;
                xyz2zyx(hv_R1_new, hv_R2_new, hv_R3_new, out ExpTmpOutVar_0, out ExpTmpOutVar_1,
                    out ExpTmpOutVar_2);
                hv_R1_new.Dispose();
                hv_R1_new = ExpTmpOutVar_0;
                hv_R2_new.Dispose();
                hv_R2_new = ExpTmpOutVar_1;
                hv_R3_new.Dispose();
                hv_R3_new = ExpTmpOutVar_2;
            }


            return;
        }


        #endregion


        ///PoseType 0是xyz，1是zyx，2是zyz,这里是用halcon实现
        public static int transformCartPose2(double RX, double RY, double RZ, int originCarPoseType,
              ref double transformRX, ref double transformRY, ref double transformRZ, int dstCarPoseType)
        {
            if (originCarPoseType == 0)
            {
                if (dstCarPoseType == 0)
                {
                    transformRX = RX; transformRY = RY; transformRZ = RZ;
                }
                else if (dstCarPoseType == 1)
                {
                    xyz2zyx(RX, RY, RZ, out HTuple hvR1, out HTuple hvR2, out HTuple hvR3);

                    transformRX = hvR1.D;
                    transformRY = hvR2.D;
                    transformRZ = hvR3.D;

                }
                else
                {
                    xyz2zyz(RX, RY, RZ, out HTuple hvR1, out HTuple hvR2, out HTuple hvR3);

                    transformRX = hvR1.D;
                    transformRY = hvR2.D;
                    transformRZ = hvR3.D;
                }
            }
            else if (originCarPoseType == 1)
            {
                if (dstCarPoseType == 0)
                {
                    zyx2xyz(RX, RY, RZ, out HTuple hvR1, out HTuple hvR2, out HTuple hvR3);

                    transformRX = hvR1.D;
                    transformRY = hvR2.D;
                    transformRZ = hvR3.D;
                }
                else if (dstCarPoseType == 1)
                {
                    transformRX = RX; transformRY = RY; transformRZ = RZ;

                }
                else
                {
                    zyx2zyz(RX, RY, RZ, out HTuple hvR1, out HTuple hvR2, out HTuple hvR3);

                    transformRX = hvR1.D;
                    transformRY = hvR2.D;
                    transformRZ = hvR3.D;
                }
            }
            else
            {
                if (dstCarPoseType == 0)
                {
                    zyz2xyz(RX, RY, RZ, out HTuple hvR1, out HTuple hvR2, out HTuple hvR3);

                    transformRX = hvR1.D;
                    transformRY = hvR2.D;
                    transformRZ = hvR3.D;
                }
                else if (dstCarPoseType == 1)
                {
                    zyz2zyx(RX, RY, RZ, out HTuple hvR1, out HTuple hvR2, out HTuple hvR3);

                    transformRX = hvR1.D;
                    transformRY = hvR2.D;
                    transformRZ = hvR3.D;
                }
                else
                {
                    transformRX = RX; transformRY = RY; transformRZ = RZ;

                }
            }


            return 0;
        }


        //pose姿态以点云形式保存
        #region
        // =========================================================
        //  单个 pose
        // =========================================================

        public static void WritePoseAxes(HPose pose, string plyPath, double axisLength = 50.0)
        {
            WriteMultiplePoseAxes(new[] { pose }, plyPath, axisLength);
        }

        // =========================================================
        //  多个 pose（每个 pose 都是 X红 Y绿 Z蓝）
        // =========================================================

        public static void WriteMultiplePoseAxes(HPose[] poses, string plyPath, double axisLength = 50.0)
        {
            var allX = new List<double>();
            var allY = new List<double>();
            var allZ = new List<double>();
            var allR = new List<byte>();
            var allG = new List<byte>();
            var allB = new List<byte>();

            foreach (HPose pose in poses)
            {
                HHomMat3D homMat = pose.PoseToHomMat3d();

                // 局部坐标系下生成轴线点
                var lx = new List<double>();
                var ly = new List<double>();
                var lz = new List<double>();
                var lr = new List<byte>();
                var lg = new List<byte>();
                var lb = new List<byte>();

                // X轴 = 红
                AddAxis(lx, ly, lz, lr, lg, lb,
                        1, 0, 0, 0, 1, 0, 0, 0, 1, axisLength, 255, 50, 50);

                // Y轴 = 绿
                AddAxis(lx, ly, lz, lr, lg, lb,
                        0, 1, 0, 1, 0, 0, 0, 0, 1, axisLength, 50, 255, 50);

                // Z轴 = 蓝
                AddAxis(lx, ly, lz, lr, lg, lb,
                        0, 0, 1, 1, 0, 0, 0, 1, 0, axisLength, 50, 120, 255);

                // 原点 = 白色小球
                AddOrigin(lx, ly, lz, lr, lg, lb, axisLength * 0.04);

                // 局部 → 世界
                HTuple wx, wy, wz;
                wx = homMat.AffineTransPoint3d(
                    ToHTuple(lx), ToHTuple(ly), ToHTuple(lz),
                     out wy, out wz);

                for (int i = 0; i < lx.Count; i++)
                {
                    allX.Add(wx[i].D);
                    allY.Add(wy[i].D);
                    allZ.Add(wz[i].D);
                    allR.Add(lr[i]);
                    allG.Add(lg[i]);
                    allB.Add(lb[i]);
                }
            }

            SaveAsPly(allX, allY, allZ, allR, allG, allB, plyPath);
        }

        // =========================================================
        //  点云 + 多个 pose
        // =========================================================

        public static void WriteCloudWithPoses(
            List<List<double>> pointList,
            HPose[] poses,
            string plyPath,
            double axisLength = 50.0,
            bool colorByHeight = false)
        {
            var allX = new List<double>();
            var allY = new List<double>();
            var allZ = new List<double>();
            var allR = new List<byte>();
            var allG = new List<byte>();
            var allB = new List<byte>();

            // ---- 点云 ----
            double minZ = double.MaxValue, maxZ = double.MinValue;
            if (colorByHeight)
                foreach (var p in pointList) { minZ = Math.Min(minZ, p[2]); maxZ = Math.Max(maxZ, p[2]); }
            double zRange = maxZ - minZ;

            foreach (var p in pointList)
            {
                allX.Add(p[0]); allY.Add(p[1]); allZ.Add(p[2]);
                if (colorByHeight && zRange > 1e-9)
                {
                    HeightColor((p[2] - minZ) / zRange, out byte r, out byte g, out byte b);
                    allR.Add(r); allG.Add(g); allB.Add(b);
                }
                else
                {
                    allR.Add(200); allG.Add(200); allB.Add(200);
                }
            }

            // ---- 各 pose 坐标轴 ----
            foreach (HPose pose in poses)
            {
                HHomMat3D homMat = pose.PoseToHomMat3d();

                var lx = new List<double>();
                var ly = new List<double>();
                var lz = new List<double>();
                var lr = new List<byte>();
                var lg = new List<byte>();
                var lb = new List<byte>();

                AddAxis(lx, ly, lz, lr, lg, lb, 1, 0, 0, 0, 1, 0, 0, 0, 1, axisLength, 255, 50, 50);
                AddAxis(lx, ly, lz, lr, lg, lb, 0, 1, 0, 1, 0, 0, 0, 0, 1, axisLength, 50, 255, 50);
                AddAxis(lx, ly, lz, lr, lg, lb, 0, 0, 1, 1, 0, 0, 0, 1, 0, axisLength, 50, 120, 255);
                AddOrigin(lx, ly, lz, lr, lg, lb, axisLength * 0.04);

                HTuple wx, wy, wz;
                wx = homMat.AffineTransPoint3d(ToHTuple(lx), ToHTuple(ly), ToHTuple(lz), out wy, out wz);

                for (int i = 0; i < lx.Count; i++)
                {
                    allX.Add(wx[i].D); allY.Add(wy[i].D); allZ.Add(wz[i].D);
                    allR.Add(lr[i]); allG.Add(lg[i]); allB.Add(lb[i]);
                }
            }

            SaveAsPly(allX, allY, allZ, allR, allG, allB, plyPath);
        }

        // =========================================================
        //  内部工具
        // =========================================================

        private static void AddAxis(
            List<double> lx, List<double> ly, List<double> lz,
            List<byte> cr, List<byte> cg, List<byte> cb,
            double dirX, double dirY, double dirZ,
            double p1X, double p1Y, double p1Z,
            double p2X, double p2Y, double p2Z,
            double length, byte r, byte g, byte b)
        {
            double spacing = 0.3;
            double headLen = length * 0.15;
            double headR = length * 0.05;
            double headBase = length - headLen;
            int steps = Math.Max(1, (int)(length / spacing));

            for (int i = 0; i <= steps; i++)
            {
                double t = length * i / steps;
                double px = dirX * t, py = dirY * t, pz = dirZ * t;

                lx.Add(px); ly.Add(py); lz.Add(pz);
                cr.Add(r); cg.Add(g); cb.Add(b);

                if (t >= headBase)
                {
                    double ratio = (t - headBase) / headLen;
                    double radius = headR * (1.0 - ratio);
                    for (int c = 0; c < 8; c++)
                    {
                        double angle = 2.0 * Math.PI * c / 8;
                        double dr = radius * Math.Cos(angle);
                        double ds = radius * Math.Sin(angle);
                        lx.Add(px + p1X * dr + p2X * ds);
                        ly.Add(py + p1Y * dr + p2Y * ds);
                        lz.Add(pz + p1Z * dr + p2Z * ds);
                        cr.Add(r); cg.Add(g); cb.Add(b);
                    }
                }
            }
        }

        private static void AddOrigin(
            List<double> lx, List<double> ly, List<double> lz,
            List<byte> cr, List<byte> cg, List<byte> cb, double radius)
        {
            int seg = 8;
            for (int i = 0; i <= seg; i++)
            {
                double theta = Math.PI * i / seg;
                for (int j = 0; j < seg * 2; j++)
                {
                    double phi = 2.0 * Math.PI * j / (seg * 2);
                    lx.Add(radius * Math.Sin(theta) * Math.Cos(phi));
                    ly.Add(radius * Math.Sin(theta) * Math.Sin(phi));
                    lz.Add(radius * Math.Cos(theta));
                    cr.Add(255); cg.Add(255); cb.Add(255);
                }
            }
        }

        private static void SaveAsPly(
            List<double> xs, List<double> ys, List<double> zs,
            List<byte> rs, List<byte> gs, List<byte> bs, string path)
        {
            HTuple hv_OM3D;
            HOperatorSet.GenObjectModel3dFromPoints(
                ToHTuple(xs), ToHTuple(ys), ToHTuple(zs), out hv_OM3D);

            HOperatorSet.SetObjectModel3dAttribMod(hv_OM3D, "red", "byte", ToHTupleInt(rs));
            HOperatorSet.SetObjectModel3dAttribMod(hv_OM3D, "green", "byte", ToHTupleInt(gs));
            HOperatorSet.SetObjectModel3dAttribMod(hv_OM3D, "blue", "byte", ToHTupleInt(bs));

            HOperatorSet.WriteObjectModel3d(hv_OM3D, "ply", path, new HTuple(), new HTuple());
            HOperatorSet.ClearObjectModel3d(hv_OM3D);
        }

        // ---- 安全转换（避免 byte[] 报错） ----

        private static HTuple ToHTuple(List<double> list)
        {
            return new HTuple(list.ToArray());
        }

        private static HTuple ToHTupleInt(List<byte> list)
        {
            int[] arr = new int[list.Count];
            for (int i = 0; i < list.Count; i++) arr[i] = list[i];
            return new HTuple(arr);
        }

        private static void HeightColor(double t, out byte r, out byte g, out byte b)
        {
            t = Math.Max(0, Math.Min(1, t));
            if (t < 0.25) { r = 0; g = (byte)(t * 4 * 255); b = 255; }
            else if (t < 0.5) { r = 0; g = 255; b = (byte)((0.5 - t) * 4 * 255); }
            else if (t < 0.75) { r = (byte)((t - 0.5) * 4 * 255); g = 255; b = 0; }
            else { r = 255; g = (byte)((1.0 - t) * 4 * 255); b = 0; }
        }

        #endregion

    }



    /// <summary>
    /// 手眼标定修正 + base→数模 映射
    ///
    /// 数据说明:
    ///   N 个位姿, 每位姿观测 1 个目标点
    ///   point3dBase[i]  → 第i个位姿下, 经错误手眼转换后的基座坐标
    ///   point3dRobot[i] → 第i个目标点对应的数模坐标 (一一对应)
    ///   poses[i]        → 第i个位姿的 HPose (工具在基座系中的位姿)
    ///   cam2ToolWrong   → 当前错误的 cam→tool 矩阵
    ///
    /// 原理:
    ///   正确关系: cad2base · P_cad = T_tool2base · cam2tool · P_cam
    ///   1. 用错误手眼逆推恢复相机坐标 (可精确恢复)
    ///   2. 交替优化: 固定cam2tool求cad2base → 固定cad2base求cam2tool
    ///   每步都是标准 Kabsch 刚体配准, 保证单调收敛
    /// </summary>
    public class HandEyeCalibrator
    {
        #region ========== 内部数学结构 ==========

        struct V3
        {
            public double x, y, z;
            public V3(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
            public static V3 operator +(V3 a, V3 b) => new V3(a.x + b.x, a.y + b.y, a.z + b.z);
            public static V3 operator -(V3 a, V3 b) => new V3(a.x - b.x, a.y - b.y, a.z - b.z);
            public static V3 operator *(double s, V3 a) => new V3(s * a.x, s * a.y, s * a.z);
            public double Norm() => Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>4×4 齐次矩阵</summary>
        class M4
        {
            public double[,] v = new double[4, 4];
            public M4() { }
            public static M4 Identity()
            {
                var m = new M4();
                m.v[0, 0] = m.v[1, 1] = m.v[2, 2] = m.v[3, 3] = 1;
                return m;
            }
            public M4 Mul(M4 b)
            {
                var r = new M4();
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        for (int k = 0; k < 4; k++)
                            r.v[i, j] += v[i, k] * b.v[k, j];
                return r;
            }
            /// <summary>刚体逆 [R^T, -R^T·t]</summary>
            public M4 Inv()
            {
                var r = new M4();
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        r.v[i, j] = v[j, i];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        r.v[i, 3] -= v[j, i] * v[j, 3];
                r.v[3, 3] = 1;
                return r;
            }
            public V3 Apply(V3 p) => new V3(
                v[0, 0] * p.x + v[0, 1] * p.y + v[0, 2] * p.z + v[0, 3],
                v[1, 0] * p.x + v[1, 1] * p.y + v[1, 2] * p.z + v[1, 3],
                v[2, 0] * p.x + v[2, 1] * p.y + v[2, 2] * p.z + v[2, 3]);
            public double[,] To3x4()
            {
                var r = new double[3, 4];
                for (int i = 0; i < 3; i++) for (int j = 0; j < 4; j++) r[i, j] = v[i, j];
                return r;
            }
        }

        #endregion

        #region ========== 3×3 矩阵运算 ==========

        static double[,] M3Mul(double[,] a, double[,] b)
        {
            var c = new double[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        c[i, j] += a[i, k] * b[k, j];
            return c;
        }
        static double[,] M3T(double[,] a)
        {
            var c = new double[3, 3];
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) c[i, j] = a[j, i];
            return c;
        }
        static double Det3(double[,] m) =>
            m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) -
            m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
            m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

        #endregion

        #region ========== Jacobi 特征值分解 (3×3 对称矩阵) ==========

        static void JacobiEig3(double[,] sIn, out double[] vals, out double[,] vecs)
        {
            var a = (double[,])sIn.Clone();
            vecs = new double[3, 3];
            vecs[0, 0] = vecs[1, 1] = vecs[2, 2] = 1;

            for (int iter = 0; iter < 100; iter++)
            {
                // 找最大非对角元素
                int p = 0, q = 1;
                double mx = Math.Abs(a[0, 1]);
                if (Math.Abs(a[0, 2]) > mx) { p = 0; q = 2; mx = Math.Abs(a[0, 2]); }
                if (Math.Abs(a[1, 2]) > mx) { p = 1; q = 2; mx = Math.Abs(a[1, 2]); }
                if (mx < 1e-15) break;

                double theta = Math.Abs(a[p, p] - a[q, q]) < 1e-15
                    ? Math.PI / 4
                    : 0.5 * Math.Atan2(2 * a[p, q], a[p, p] - a[q, q]);
                double c = Math.Cos(theta), sn = Math.Sin(theta);

                double app = a[p, p], aqq = a[q, q], apq = a[p, q];
                a[p, p] = c * c * app + 2 * c * sn * apq + sn * sn * aqq;
                a[q, q] = sn * sn * app - 2 * c * sn * apq + c * c * aqq;
                a[p, q] = a[q, p] = 0;
                for (int r = 0; r < 3; r++)
                {
                    if (r == p || r == q) continue;
                    double arp = a[r, p], arq = a[r, q];
                    a[r, p] = a[p, r] = c * arp + sn * arq;
                    a[r, q] = a[q, r] = -sn * arp + c * arq;
                }
                for (int r = 0; r < 3; r++)
                {
                    double vp = vecs[r, p], vq = vecs[r, q];
                    vecs[r, p] = c * vp + sn * vq;
                    vecs[r, q] = -sn * vp + c * vq;
                }
            }
            vals = new[] { a[0, 0], a[1, 1], a[2, 2] };
        }

        #endregion

        #region ========== SVD 3×3 ==========

        static void SVD3(double[,] m, out double[,] U, out double[] s, out double[,] V)
        {
            JacobiEig3(M3Mul(M3T(m), m), out double[] ev, out V);

            int[] idx = { 0, 1, 2 };
            for (int i = 0; i < 2; i++)
                for (int j = i + 1; j < 3; j++)
                    if (ev[idx[j]] > ev[idx[i]]) { int t = idx[i]; idx[i] = idx[j]; idx[j] = t; }

            var Vo = new double[3, 3];
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) Vo[j, i] = V[j, idx[i]];
            V = Vo;

            s = new double[3];
            for (int i = 0; i < 3; i++) s[i] = Math.Sqrt(Math.Max(0, ev[idx[i]]));

            U = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                if (s[i] > 1e-10)
                    for (int r = 0; r < 3; r++)
                    {
                        double sum = 0;
                        for (int c = 0; c < 3; c++) sum += m[r, c] * V[c, i];
                        U[r, i] = sum / s[i];
                    }
            }
            if (s[2] < 1e-10)
            {
                U[0, 2] = U[1, 0] * U[2, 1] - U[2, 0] * U[1, 1];
                U[1, 2] = U[2, 0] * U[0, 1] - U[0, 0] * U[2, 1];
                U[2, 2] = U[0, 0] * U[1, 1] - U[1, 0] * U[0, 1];
            }
        }

        #endregion

        #region ========== Kabsch 刚体配准 ==========

        /// <summary>求解 tgt ≈ R·src + t (需 ≥3 个不共线点)</summary>
        static M4 Kabsch(V3[] src, V3[] tgt)
        {
            int n = src.Length;
            V3 cs = new V3(), ct = new V3();
            for (int i = 0; i < n; i++) { cs = cs + src[i]; ct = ct + tgt[i]; }
            cs = (1.0 / n) * cs; ct = (1.0 / n) * ct;

            double[,] H = new double[3, 3];
            for (int i = 0; i < n; i++)
            {
                V3 a = src[i] - cs, b = tgt[i] - ct;
                H[0, 0] += a.x * b.x; H[0, 1] += a.x * b.y; H[0, 2] += a.x * b.z;
                H[1, 0] += a.y * b.x; H[1, 1] += a.y * b.y; H[1, 2] += a.y * b.z;
                H[2, 0] += a.z * b.x; H[2, 1] += a.z * b.y; H[2, 2] += a.z * b.z;
            }

            SVD3(H, out var U, out var sv, out var V);
            double d = Det3(M3Mul(V, M3T(U)));
            double[,] dd = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, d > 0 ? 1 : -1 } };
            double[,] R = M3Mul(M3Mul(V, dd), M3T(U));

            double[] csa = { cs.x, cs.y, cs.z };
            double[] t = { ct.x, ct.y, ct.z };
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    t[i] -= R[i, j] * csa[j];

            var T = new M4();
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) T.v[i, j] = R[i, j];
            T.v[0, 3] = t[0]; T.v[1, 3] = t[1]; T.v[2, 3] = t[2]; T.v[3, 3] = 1;
            return T;
        }

        #endregion

        #region ========== Halcon 转换 (兼容所有版本) ==========

        /// <summary>HPose → 4×4</summary>
        static M4 PoseToM4(HPose pose)
        {
            HHomMat3D hm = pose.PoseToHomMat3d();
            return HomMat3DToM4(hm);
        }

        /// <summary>HHomMat3D → 4×4 (用 AffineTransPoint3d 提取, 不依赖 HomMat3dToDouble)</summary>
        static M4 HomMat3DToM4(HHomMat3D hm)
        {
            HTuple tx = hm.AffineTransPoint3d(0, 0, 0,  out HTuple ty, out HTuple tz);
            HTuple ax = hm.AffineTransPoint3d(1, 0, 0,  out HTuple ay, out HTuple az);
            HTuple bx = hm.AffineTransPoint3d(0, 1, 0,  out HTuple by, out HTuple bz);
            HTuple cx = hm.AffineTransPoint3d(0, 0, 1,  out HTuple cy, out HTuple cz);

            var m = new M4();
            m.v[0, 0] = ax - tx; m.v[0, 1] = bx - tx; m.v[0, 2] = cx - tx; m.v[0, 3] = tx;
            m.v[1, 0] = ay - ty; m.v[1, 1] = by - ty; m.v[1, 2] = cy - ty; m.v[1, 3] = ty;
            m.v[2, 0] = az - tz; m.v[2, 1] = bz - tz; m.v[2, 2] = cz - tz; m.v[2, 3] = tz;
            m.v[3, 3] = 1;
            return m;
        }

        #endregion

        #region ========== 结果类 ==========

        public class CalibrationResult
        {
            /// <summary>修正后的 cam→tool [3×4]</summary>
            public double[,] Cam2ToolMatrix;
            /// <summary>base→数模 映射 [3×4]</summary>
            public double[,] Base2CadMatrix;
            /// <summary>数模→base 映射 [3×4]</summary>
            public double[,] Cad2BaseMatrix;
            /// <summary>各点误差 (mm)</summary>
            public double[] PointErrors;
            /// <summary>手眼修正旋转 (度)</summary>
            public double CorrectionRotDeg;
            /// <summary>手眼修正平移 (mm)</summary>
            public double CorrectionTransMm;
        }

        #endregion

        #region ========== Solve 相机坐标版 ==========

        /// <summary>
        /// 直接从相机坐标求解手眼标定 + base→cad映射 (不需要旧手眼矩阵)
        ///
        /// 原理:
        ///   Y · P_cad_j = G_i · X · P_cam_i_j
        ///   X = cam2tool,  Y = cad2base
        ///   交替 Kabsch 求解
        /// </summary>
        /// <param name="point3dCam">相机坐标 (N个, 与point3dRobot一一对应)</param>
        /// <param name="point3dRobot">数模坐标 (N个)</param>
        /// <param name="poses">拍摄位姿 (N个 HPose)</param>
        /// <param name="outlierThreshold">离群点剔除阈值, 0=不剔除</param>
        public static CalibrationResult SolveFromCamera(
            Point3D[] point3dCam,
            Point3D[] point3dRobot,
            HPose[] poses,
            double outlierThreshold = 0)
        {
            int N = point3dCam.Length;

            if (N != point3dRobot.Length || N != poses.Length)
                throw new ArgumentException(
                    $"数量不一致: cam={N} robot={point3dRobot.Length} poses={poses.Length}");
            if (N < 3)
                throw new ArgumentException($"至少需要3组对应点, 当前仅{N}组");

            // ===== 数据转换 =====
            V3[] cad = new V3[N], cam = new V3[N];
            for (int i = 0; i < N; i++)
            {
                cad[i] = new V3(point3dRobot[i].X, point3dRobot[i].Y, point3dRobot[i].Z);
                cam[i] = new V3(point3dCam[i].X, point3dCam[i].Y, point3dCam[i].Z);
            }

            M4[] G = new M4[N];
            M4[] Gi = new M4[N];
            for (int i = 0; i < N; i++)
            {
                G[i] = PoseToM4(poses[i]);
                Gi[i] = G[i].Inv();
            }

            // ===== 内部求解 (不需要恢复相机坐标, 直接用 cam) =====
            M4 SolveSubset(int[] idx)
            {
                int n = idx.Length;
                M4 X = M4.Identity(); // cam2tool
                M4 Y = M4.Identity(); // cad2base
                double prevErr = double.MaxValue;

                for (int iter = 0; iter < 500; iter++)
                {
                    // 固定 X, 求 Y: cad → G·X·cam
                    V3[] tY = new V3[n];
                    for (int k = 0; k < n; k++)
                        tY[k] = G[idx[k]].Mul(X).Apply(cam[idx[k]]);
                    Y = Kabsch(SubSet(cad, idx), tY);

                    // 固定 Y, 求 X: cam → G^(-1)·Y·cad
                    V3[] tX = new V3[n];
                    for (int k = 0; k < n; k++)
                        tX[k] = Gi[idx[k]].Mul(Y).Apply(cad[idx[k]]);
                    X = Kabsch(SubSet(cam, idx), tX);

                    // 收敛判断
                    double maxE = 0;
                    for (int k = 0; k < n; k++)
                    {
                        double d = (Y.Apply(cad[idx[k]]) - G[idx[k]].Mul(X).Apply(cam[idx[k]])).Norm();
                        if (d > maxE) maxE = d;
                    }
                    if (maxE < 1e-9 || Math.Abs(prevErr - maxE) < 1e-14) break;
                    prevErr = maxE;
                }
                return X;
            }

            // ===== 离群点剔除 =====
            int[] allIdx = new int[N];
            for (int i = 0; i < N; i++) allIdx[i] = i;
            int[] activeIdx = allIdx;

            if (outlierThreshold > 0)
            {
                var keepList = new List<int>(allIdx);
                Console.WriteLine($"[离群点剔除] 阈值={outlierThreshold:F6}");

                for (int round = 0; round < 10; round++)
                {
                    if (keepList.Count < 3) break;
                    int[] curIdx = keepList.ToArray();
                    M4 Xcur = SolveSubset(curIdx);

                    // 同时求 Y 用于评估误差
                    M4 Ycur = M4.Identity();
                    {
                        V3[] tY = new V3[curIdx.Length];
                        for (int k = 0; k < curIdx.Length; k++)
                            tY[k] = G[curIdx[k]].Mul(Xcur).Apply(cam[curIdx[k]]);
                        Ycur = Kabsch(SubSet(cad, curIdx), tY);
                    }

                    // 找最差点
                    double worstErr = 0;
                    int worstK = -1;
                    for (int k = 0; k < curIdx.Length; k++)
                    {
                        double d = (Ycur.Apply(cad[curIdx[k]]) - G[curIdx[k]].Mul(Xcur).Apply(cam[curIdx[k]])).Norm();
                        if (d > worstErr) { worstErr = d; worstK = k; }
                    }

                    Console.WriteLine($"  轮{round}: 点数={curIdx.Length}, 最大误差={worstErr:F6}, 最差点=点{curIdx[worstK]}");

                    if (worstErr <= outlierThreshold) { Console.WriteLine($"  → 全部在阈值内"); break; }

                    Console.WriteLine($"  → 剔除 点{curIdx[worstK]} (误差={worstErr:F6})");
                    keepList.RemoveAt(worstK);
                }
                activeIdx = keepList.ToArray();
                Console.WriteLine($"  最终保留 {activeIdx.Length}/{N} 个点");
            }

            // ===== 最终求解 =====
            M4 Xfinal = SolveSubset(activeIdx);
            M4 Yfinal = M4.Identity();
            {
                int n = activeIdx.Length;
                V3[] tY = new V3[n];
                for (int k = 0; k < n; k++)
                    tY[k] = G[activeIdx[k]].Mul(Xfinal).Apply(cam[activeIdx[k]]);
                Yfinal = Kabsch(SubSet(cad, activeIdx), tY);
            }

            M4 B2C = Yfinal.Inv();

            // 各点误差
            double[] errors = new double[N];
            for (int i = 0; i < N; i++)
            {
                V3 pCad = B2C.Apply(G[i].Mul(Xfinal).Apply(cam[i]));
                errors[i] = (pCad - cad[i]).Norm();
            }

            // 打印
            Console.WriteLine("\n========== 手眼标定结果 (相机坐标输入) ==========");
            Console.WriteLine($"  使用点数: {activeIdx.Length}/{N}");
            Console.WriteLine($"  各点误差:");
            double maxErr = 0, sumErr = 0;
            for (int i = 0; i < N; i++)
            {
                string flag = (outlierThreshold > 0 && errors[i] > outlierThreshold) ? " ★离群" : "";
                Console.WriteLine($"    点{i}: {errors[i]:F6}{flag}");
                if (errors[i] > maxErr) maxErr = errors[i];
                sumErr += errors[i];
            }
            Console.WriteLine($"  平均误差: {sumErr / N:F6}  最大误差: {maxErr:F6}");
            Console.WriteLine("\n  修正后 cam2Tool (3×4):");
            PrintM34(Xfinal.To3x4());
            Console.WriteLine("\n  base→cad (3×4):");
            PrintM34(B2C.To3x4());

            return new CalibrationResult
            {
                Cam2ToolMatrix = Xfinal.To3x4(),
                Base2CadMatrix = B2C.To3x4(),
                Cad2BaseMatrix = Yfinal.To3x4(),
                PointErrors = errors,
                CorrectionRotDeg = 0,
                CorrectionTransMm = 0
            };
        }

        #endregion

        #region ========== 相机坐标 → 基座坐标 ==========

        /// <summary>
        /// 用正确的手眼矩阵, 将相机坐标系的点转换为基座坐标
        ///
        /// P_base = tool2base · cam2tool · P_cam
        /// </summary>
        /// <param name="point3dCam">相机坐标 (N个)</param>
        /// <param name="poses">对应位姿 (N个 HPose)</param>
        /// <param name="cam2ToolMatrix">正确的手眼矩阵 [3×4]</param>
        /// <returns>基座坐标 (N个)</returns>
        public static Point3D[] CamToBase(
            Point3D[] point3dCam,
            HPose[] poses,
            double[,] cam2ToolMatrix)
        {
            int N = point3dCam.Length;
            if (N != poses.Length)
                throw new ArgumentException($"点数({N})与位姿数({poses.Length})不一致");

            M4 X = M4From3x4(cam2ToolMatrix);
            Point3D[] result = new Point3D[N];

            for (int i = 0; i < N; i++)
            {
                M4 G = PoseToM4(poses[i]);
                V3 pCam = new V3(point3dCam[i].X, point3dCam[i].Y, point3dCam[i].Z);
                V3 pBase = G.Mul(X).Apply(pCam);
                result[i] = new Point3D(pBase.x, pBase.y, pBase.z);
            }
            return result;
        }

        /// <summary>
        /// 用正确的手眼矩阵, 将相机坐标系的点一步转换为数模坐标
        ///
        /// P_cad = base2cad · tool2base · cam2tool · P_cam
        /// </summary>
        /// <param name="point3dCam">相机坐标 (N个)</param>
        /// <param name="poses">对应位姿 (N个 HPose)</param>
        /// <param name="result">SolveFromCamera 或 Solve 返回的标定结果</param>
        /// <returns>数模坐标 (N个)</returns>
        public Point3D[] CamToCad(
            List<Point3D> point3dCam,
            HPose[] poses,
            CalibrationResult result)
        {
            int N = point3dCam.Count;
            if (N != poses.Length)
                throw new ArgumentException($"点数({N})与位姿数({poses.Length})不一致");

            M4 X = M4From3x4(result.Cam2ToolMatrix);
            M4 B2C = M4From3x4(result.Base2CadMatrix);
            Point3D[] ret = new Point3D[N];

            for (int i = 0; i < N; i++)
            {
                M4 G = PoseToM4(poses[i]);
                V3 pCam = new V3(point3dCam[i].X, point3dCam[i].Y, point3dCam[i].Z);
                V3 pBase = G.Mul(X).Apply(pCam);
                V3 pCad = B2C.Apply(pBase);
                ret[i] = new Point3D(pCad.x, pCad.y, pCad.z);
            }
            return ret;
        }

        #endregion

        #region ========== 主求解入口 ==========

        /// <param name="point3dBase">错误的基座坐标 (N个, 与point3dRobot一一对应)</param>
        /// <param name="point3dRobot">数模坐标 (N个)</param>
        /// <param name="poses">拍摄位姿 (N个 HPose, 每个位姿对应一个点)</param>
        /// <param name="cam2ToolWrong">当前错误的 cam→tool 手眼矩阵</param>
        public static CalibrationResult Solve(
            Point3D[] point3dBase,
            Point3D[] point3dRobot,
            HPose[] poses,
            HHomMat3D cam2ToolWrong)
        {
            int N = point3dBase.Length;

            // ===== 校验 =====
            if (N != point3dRobot.Length || N != poses.Length)
                throw new ArgumentException(
                    $"数量不一致: base={N} robot={point3dRobot.Length} poses={poses.Length}");
            if (N < 3)
                throw new ArgumentException($"至少需要3组对应点(不共线), 当前仅{N}组");

            // ===== 数据转换 =====
            V3[] cad = new V3[N], bas = new V3[N];
            for (int i = 0; i < N; i++)
            {
                cad[i] = new V3(point3dRobot[i].X, point3dRobot[i].Y, point3dRobot[i].Z);
                bas[i] = new V3(point3dBase[i].X, point3dBase[i].Y, point3dBase[i].Z);
            }

            M4[] G = new M4[N];    // tool→base
            M4[] Gi = new M4[N];   // base→tool
            for (int i = 0; i < N; i++)
            {
                G[i] = PoseToM4(poses[i]);
                Gi[i] = G[i].Inv();
            }

            M4 Xw = HomMat3DToM4(cam2ToolWrong);
            M4 Xwi = Xw.Inv();

            // ===== Step 1: 恢复相机坐标 =====
            // P_cam = cam2Tool_wrong^(-1) · base2tool · P_base
            // 即使手眼矩阵有误, 这一步能精确恢复相机坐标 (因为错误的正反变换互相抵消)
            V3[] cam = new V3[N];
            for (int i = 0; i < N; i++)
                cam[i] = Xwi.Mul(Gi[i]).Apply(bas[i]);

            // ===== Step 2: 交替 Kabsch 求解 =====
            //
            // 目标: Y·cad_i = G_i·X·cam_i   (Y=cad2base, X=cam2tool)
            //
            // 交替:
            //   固定X → 求Y: Y映射 cad_i → G_i·X·cam_i   (Kabsch)
            //   固定Y → 求X: X映射 cam_i → G_i^(-1)·Y·cad_i (Kabsch)

            M4 X = M4.Identity(); // cam2tool
            M4 Y = M4.Identity(); // cad2base

            double prevErr = double.MaxValue;
            int converged = -1;

            for (int iter = 0; iter < 500; iter++)
            {
                // --- 固定 X, 求 Y ---
                V3[] tY = new V3[N];
                for (int i = 0; i < N; i++)
                    tY[i] = G[i].Mul(X).Apply(cam[i]);
                Y = Kabsch(cad, tY);

                // --- 固定 Y, 求 X ---
                V3[] tX = new V3[N];
                for (int i = 0; i < N; i++)
                    tX[i] = Gi[i].Mul(Y).Apply(cad[i]);
                X = Kabsch(cam, tX);

                // --- 收敛判断 ---
                double maxE = 0;
                for (int i = 0; i < N; i++)
                {
                    double d = (Y.Apply(cad[i]) - G[i].Mul(X).Apply(cam[i])).Norm();
                    if (d > maxE) maxE = d;
                }

                if (maxE < 1e-9) { converged = iter; break; }
                if (Math.Abs(prevErr - maxE) < 1e-14) { converged = iter; break; }
                prevErr = maxE;
            }

            // ===== Step 3: 计算结果 =====
            M4 Xcorrect = X;
            M4 B2C = Y.Inv(); // base→cad
            M4 C2B = Y;       // cad→base

            // 手眼修正量
            M4 delta = Xcorrect.Mul(Xwi);
            double cosA = Math.Max(-1, Math.Min(1,
                (delta.v[0, 0] + delta.v[1, 1] + delta.v[2, 2] - 1) / 2));
            double corrRot = Math.Acos(cosA) * 180.0 / Math.PI;
            double corrTrans = Math.Sqrt(
                delta.v[0, 3] * delta.v[0, 3] +
                delta.v[1, 3] * delta.v[1, 3] +
                delta.v[2, 3] * delta.v[2, 3]);

            // 各点误差
            double[] errors = new double[N];
            for (int i = 0; i < N; i++)
            {
                V3 pCad = B2C.Apply(bas[i]);
                errors[i] = (pCad - cad[i]).Norm();
            }

            // ===== Step 4: 输出报告 =====
            Console.WriteLine("========== 手眼标定结果 ==========");
            Console.WriteLine($"  位姿数: {N}, 迭代: {converged}");
            Console.WriteLine($"  手眼修正量: 旋转={corrRot:F4}° 平移={corrTrans:F4}mm");
            Console.WriteLine($"  base→cad 误差:");
            double maxErr = 0, sumErr = 0;
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine($"    点{i}: {errors[i]:F4}mm");
                if (errors[i] > maxErr) maxErr = errors[i];
                sumErr += errors[i];
            }
            Console.WriteLine($"  平均误差: {sumErr / N:F4}mm  最大误差: {maxErr:F4}mm");

            // 打印矩阵
            Console.WriteLine("\n  修正后 cam2Tool (3×4):");
            PrintM34(Xcorrect.To3x4());
            Console.WriteLine("\n  base→cad (3×4):");
            PrintM34(B2C.To3x4());

            return new CalibrationResult
            {
                Cam2ToolMatrix = Xcorrect.To3x4(),
                Base2CadMatrix = B2C.To3x4(),
                Cad2BaseMatrix = C2B.To3x4(),
                PointErrors = errors,
                CorrectionRotDeg = corrRot,
                CorrectionTransMm = corrTrans
            };
        }


        /// <summary>
        /// 将错误的基座坐标，通过旧手眼矩阵+位姿+新手眼矩阵，修正为正确的基座坐标
        ///
        /// 原理:
        ///   P_cam = cam2tool_wrong^(-1) · tool2base^(-1) · P_base_wrong
        ///   P_base_correct = tool2base · cam2tool_correct · P_cam
        ///   等价于:
        ///   P_base_correct = tool2base · Δ · tool2base^(-1) · P_base_wrong
        ///   其中 Δ = cam2tool_correct · cam2tool_wrong^(-1)
        ///
        /// 注意: 修正量与位姿有关 (同一个错误点在不同位姿下修正不同)
        /// </summary>
        /// <param name="point3dBase">错误的基座坐标 (N个)</param>
        /// <param name="poses">对应的拍摄位姿 (N个 HPose)</param>
        /// <param name="cam2ToolWrong">旧的(错误的)手眼矩阵</param>
        /// <param name="result">Solve 返回的标定结果</param>
        /// <returns>修正后的基座坐标 (N个)</returns>
        public static Point3D[] CorrectBasePoints(
            Point3D[] point3dBase,
            HPose[] poses,
            HHomMat3D cam2ToolWrong,
            CalibrationResult result)
        {
            int N = point3dBase.Length;
            if (N != poses.Length)
                throw new ArgumentException($"点数({N})与位姿数({poses.Length})不一致");

            // --- 转换输入 ---
            M4 Xw = HomMat3DToM4(cam2ToolWrong);
            M4 Xwi = Xw.Inv();
            M4 Xc = M4From3x4(result.Cam2ToolMatrix);

            // 预算修正矩阵 Δ = X_correct · X_wrong^(-1)
            // (可以不用, 直接走完整链路更清晰)

            Point3D[] corrected = new Point3D[N];

            for (int i = 0; i < N; i++)
            {
                M4 G = PoseToM4(poses[i]);       // tool→base
                M4 Gi = G.Inv();                  // base→tool

                // Step 1: 恢复相机坐标 (旧矩阵)
                V3 pBase = new V3(point3dBase[i].X, point3dBase[i].Y, point3dBase[i].Z);
                V3 pCam = Xwi.Mul(Gi).Apply(pBase);

                // Step 2: 用新矩阵投影回基座坐标
                V3 pCorrect = G.Mul(Xc).Apply(pCam);

                corrected[i] = new Point3D(pCorrect.x, pCorrect.y, pCorrect.z);
            }

            return corrected;
        }

        /// <summary>
        /// 直接从错误的基座坐标映射到数模坐标 (一步到位)
        /// 等价于 CorrectBasePoints + Base2Cad, 但效率更高
        ///
        /// P_cad = cad2base^(-1) · tool2base · X_correct · X_wrong^(-1) · tool2base^(-1) · P_base_wrong
        /// </summary>
        /// <param name="point3dBase">错误的基座坐标 (N个)</param>
        /// <param name="poses">对应的拍摄位姿 (N个 HPose)</param>
        /// <param name="cam2ToolWrong">旧的(错误的)手眼矩阵</param>
        /// <param name="result">Solve 返回的标定结果</param>
        /// <returns>对应的数模坐标 (N个)</returns>
        public Point3D[] BaseToCad(
            List<Point3D> point3dBase,
            HPose[] poses,
            HHomMat3D cam2ToolWrong,
            CalibrationResult result)
        {
            int N = point3dBase.Count;
            if (N != poses.Length)
                throw new ArgumentException($"点数({N})与位姿数({poses.Length})不一致");

            M4 Xw = HomMat3DToM4(cam2ToolWrong);
            M4 Xwi = Xw.Inv();
            M4 Xc = M4From3x4(result.Cam2ToolMatrix);
            M4 B2C = M4From3x4(result.Base2CadMatrix);

            Point3D[] cadPoints = new Point3D[N];

            for (int i = 0; i < N; i++)
            {
                M4 G = PoseToM4(poses[i]);
                M4 Gi = G.Inv();

                // 完整链路: base_wrong → tool → cam → tool → base_correct → cad
                V3 pBase = new V3(point3dBase[i].X, point3dBase[i].Y, point3dBase[i].Z);
                V3 pCam = Xwi.Mul(Gi).Apply(pBase);
                V3 pBaseCorrect = G.Mul(Xc).Apply(pCam);
                V3 pCad = B2C.Apply(pBaseCorrect);

                cadPoints[i] = new Point3D(pCad.x, pCad.y, pCad.z);
            }

            return cadPoints;
        }


        #endregion

        #region ========== 工具方法 ==========

        static V3[] SubSet(V3[] arr, int[] idx)
        {
            var r = new V3[idx.Length];
            for (int i = 0; i < idx.Length; i++) r[i] = arr[idx[i]];
            return r;
        }

        /// <summary>3×4 double[,] → M4</summary>
        static M4 M4From3x4(double[,] m)
        {
            var r = M4.Identity();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                    r.v[i, j] = m[i, j];
            return r;
        }

        /// <summary>3×4 矩阵变换点</summary>
        public static void TransPoint(double[,] m, double x, double y, double z,
                                       out double ox, out double oy, out double oz)
        {
            ox = m[0, 0] * x + m[0, 1] * y + m[0, 2] * z + m[0, 3];
            oy = m[1, 0] * x + m[1, 1] * y + m[1, 2] * z + m[1, 3];
            oz = m[2, 0] * x + m[2, 1] * y + m[2, 2] * z + m[2, 3];
        }

        /// <summary>尝试将 3×4 矩阵转为 HHomMat3D (如果HTuple构造可用)</summary>
        public static HHomMat3D ToHomMat3D(double[,] m)
        {
            return new HHomMat3D(new HTuple(
                m[0, 0], m[0, 1], m[0, 2], m[0, 3],
                m[1, 0], m[1, 1], m[1, 2], m[1, 3],
                m[2, 0], m[2, 1], m[2, 2], m[2, 3]));
        }

        static void PrintM34(double[,] m)
        {
            for (int i = 0; i < 3; i++)
                Console.WriteLine($"    [{m[i, 0],10:F4} {m[i, 1],10:F4} {m[i, 2],10:F4} {m[i, 3],10:F4}]");
        }

        #endregion
    }

}
