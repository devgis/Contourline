using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;

namespace Utility
{
    public class GPHelper
    {
        /// <summary>
        /// 执行gp操作
        /// </summary>
        /// <param name="gp"></param>
        /// <param name="igppro"></param>
        /// <param name="tc"></param>
        static public IGeoProcessorResult RunTool(Geoprocessor gp, IGPProcess igppro, ITrackCancel tc)
        {
            gp.OverwriteOutput = true;
            try
            {
                return gp.Execute(igppro, null) as IGeoProcessorResult;
            }
            catch (Exception err)
            {
                throw new Exception(GetMessages(gp), err);
            }
        }

        /// <summary>
        /// 获得gp的信息
        /// </summary>
        /// <param name="gp"></param>
        /// <returns></returns>
        static public string GetMessages(Geoprocessor gp)
        {
            string ms = "";
            if (gp.MessageCount > 0)
            {
                for (int i = 0; i < gp.MessageCount; i++)
                {
                    ms += gp.GetMessage(i);
                }
            }

            return ms;
        }

    }
}
