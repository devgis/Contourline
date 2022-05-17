using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace Utility
{
    public static class MapHelper
    {
        public static void AddFeatureClass2Map(IFeatureClass fc, IMap map)
        {
            IFeatureLayer layer = new FeatureLayerClass();
            layer.FeatureClass = fc;
            layer.Name = fc.AliasName;
            map.AddLayer(layer);
        }

        public static void AddTin2Map(ITin tin, IMap iMap, string name)
        {
            if (tin != null)
            {
                ITinLayer layer = new TinLayerClass();
                layer.Dataset = tin;
                layer.Name = name;
                iMap.AddLayer(layer);
            }
        }

    }
}
