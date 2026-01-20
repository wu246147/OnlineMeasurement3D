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
        HeartBeat,
        Readily,
        Running,
        Result,
        Check_Finish,
        Acq_Finish,
        Check_Point_Result,
        Shielding_Vision,
        Feedback_Car_Model = 256,
        Feedback_Check_Point_NO,
        X = 1024,
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
        Start,
        Reset ,
        Readed ,
        Empty_Run,
        Arrive_Photo_Spot,
        End_of_Check_Points,

        Car_Model = 256,
        Pallet_Number,
        Batch_Number,
        Check_Point_NO,

        Car_NO = 1536,
    }

}
