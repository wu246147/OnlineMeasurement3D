using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMeasurement.IO
{

    /// <summary>
    /// bool(0~255),ushort(256~511),short(512~767),uint(768~1023),int(1024~1279),float(1280~1535), string(1536~1791)
    /// </summary>
    public enum DO
    {
        //共用：
        HeartBeat,                  //心跳
        Readily,                    //准备好
        Running,                    //运行中、检测中
        //Result,                     //检测结果，是以bool数组保存，这里只记录第一位
        ResultOK,                   //检测OK结果，是以bool数组保存，这里只记录第一位
        ResultNG,                   //检测NG结果，是以bool数组保存，这里只记录第一位
        All_Check_Finish,           //所有检测完毕、检测结果可读

        Feedback_Car_Finish,
        //独立：
        Point_Check_Finish,         //当前点检测完毕
        Check_Point_Result,         //当前检测点结果
        Shielding_Vision,           //屏蔽视觉

        //共用：
        Feedback_Car_Model = 256,   //反馈车型
        Feedback_Car_NO,            //反馈车号、连番号

        //独立：
        Feedback_Check_Point_NO,    //反馈检测点编号

        X,  //第13位为“0”时，数值为“+”；第13位为“1”时，数值为“-” 精度0.01mm 用   DataConvert来转换一下
        Y,
        Z,

        Dx,
        Dy,
        Dz,
}
    /// <summary>
    /// bool(0~255),ushort(256~511),short(512~767),uint(768~1023),int(1024~1279),float(1280~1535), string(1536~1791)
    /// </summary>
    public enum DI
    {
        //共用：
        Reading_Car_Info,           //读取车型、连番号中
        Check_Start,                //检测开始中
        Calibration_Start,          //校准开始中
        Readed,                     //结果已读
        Reset ,                     //重置
        Empty_Run,                  //空运行



        //独立：
        Arrive_Photo_Spot,          //到达拍照点
        End_of_Check_Points,        //末点
        Check_Point_NO,             //二进制形式读取，默认4位数，最大15
        Calibration_Point_NO,       //二进制形式读取，默认5位数，最大31
        Calibration_Moving,

        //共用：
        Car_Model = 256,            //车型
        Pallet_Number,              //托盘号
        Batch_Number,               //连番号
        Car_NO,                     //车号\

        //独立：

    }

}
