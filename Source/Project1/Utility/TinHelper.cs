using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geometry;

namespace Utility
{
    public static class TinHelper
    {
        /// <summary>
        /// 判读三角形是否具有两条硬边
        /// </summary>
        /// <param name="tri"></param>
        /// <returns>返回非硬边的索引号</returns>
        public static int HasTwoHardEdge(ITinTriangle tri)
        {
            ITinEdge edge0 = tri.Edge[0];
            ITinEdge edge1 = tri.Edge[1];
            ITinEdge edge2 = tri.Edge[2];

            if (HasThreeEdge(tri, esriTinEdgeType.esriTinSoftEdge) || HasThreeEdge(tri, esriTinEdgeType.esriTinHardEdge) || HasThreeEdge(tri, esriTinEdgeType.esriTinRegularEdge))
                return -1;


            if (edge0.Type == esriTinEdgeType.esriTinHardEdge && edge1.Type == esriTinEdgeType.esriTinHardEdge)
                return 2;
            else if (edge1.Type == esriTinEdgeType.esriTinHardEdge && edge2.Type == esriTinEdgeType.esriTinHardEdge)
                return 0;
            else if (edge2.Type == esriTinEdgeType.esriTinHardEdge && edge0.Type == esriTinEdgeType.esriTinHardEdge)
                return 1;
            else
                return -1;
        }

        public static bool HasThreeEdge(ITinTriangle tri, esriTinEdgeType esriTinEdgeType)
        {
            ITinEdge edge0 = tri.Edge[0];
            ITinEdge edge1 = tri.Edge[1];
            ITinEdge edge2 = tri.Edge[2];

            if (edge0.Type == esriTinEdgeType && edge1.Type == esriTinEdgeType && edge2.Type == esriTinEdgeType)
                return true;
            else
                return false;                     
        }

        /// <summary>
        /// 判读三角形是否具有两条规则边
        /// </summary>
        /// <param name="tri"></param>
        /// <returns>返回非规则边的索引号</returns>
        public static int HasTwoRegularEdge(ITinTriangle tri)
        {
            ITinEdge edge0 = tri.Edge[0];
            ITinEdge edge1 = tri.Edge[1];
            ITinEdge edge2 = tri.Edge[2];

            if (edge0.Type == esriTinEdgeType.esriTinHardEdge && edge1.Type == esriTinEdgeType.esriTinHardEdge && edge2.Type == esriTinEdgeType.esriTinHardEdge)
                return -1;
            if (edge0.Type == esriTinEdgeType.esriTinRegularEdge && edge1.Type == esriTinEdgeType.esriTinRegularEdge && edge2.Type == esriTinEdgeType.esriTinRegularEdge)
                return -1;
            if (edge0.Type == esriTinEdgeType.esriTinSoftEdge && edge1.Type == esriTinEdgeType.esriTinSoftEdge && edge2.Type == esriTinEdgeType.esriTinSoftEdge)
                return -1;

            if (edge0.Type == esriTinEdgeType.esriTinRegularEdge && edge1.Type == esriTinEdgeType.esriTinRegularEdge)
                return 2;
            else if (edge1.Type == esriTinEdgeType.esriTinRegularEdge && edge2.Type == esriTinEdgeType.esriTinRegularEdge)
                return 0;
            else if (edge2.Type == esriTinEdgeType.esriTinRegularEdge && edge0.Type == esriTinEdgeType.esriTinRegularEdge)
                return 1;
            else
                return -1;
        }

        /// <summary>
        /// 生成连接几何点到三角形边中点的线
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static IPolyline ConnectPt2MidOfEdge(ITinEdge edge, IPoint pt)
        {
            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl);

            IPoint pt2 = new PointClass();
            pt2.Z = (edge.ToNode.Z + edge.FromNode.Z) / 2;
            pt2.PutCoords((edge.ToNode.X + edge.FromNode.X) / 2, (edge.ToNode.Y + edge.FromNode.Y) / 2);

            pl.FromPoint = pt;
            pl.ToPoint = pt2;

            return pl;
        }

        /// <summary>
        /// 构造节点到边的中点的线
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static IPolyline ConnectNode2Edge(ITinNode node, ITinEdge edge)
        {
            IPoint pt = GeometryHelper.CreatePoint(node.X, node.Y, node.Z);

            return ConnectPt2MidOfEdge(edge, pt);
        }

        /// <summary>
        /// 根据三角形边构造几何线
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static IPolyline CreatePolylineFromTinEdge(ITinEdge edge)
        {
            IPoint pt1 = new PointClass();
            IPoint pt2 = new PointClass();

            pt1.X = edge.FromNode.X;
            pt1.Y = edge.FromNode.Y;
            pt1.Z = edge.FromNode.Z;
            pt2.X = edge.ToNode.X;
            pt2.Y = edge.ToNode.Y;
            pt2.Z = edge.ToNode.Z;

            object missing = Type.Missing;
            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl as IGeometry);
            pl.FromPoint = pt1;
            pl.ToPoint = pt2;

            return pl;
        }

        /// <summary>
        /// 根据Tin三角形生成一条多义线
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        public static IPolyline CreatePolylineFromTinTri(ITinTriangle tri)
        {
            //if (double.IsNaN(tri.Node[0].Z) || double.IsNaN(tri.Node[1].Z) || double.IsNaN(tri.Node[2].Z))
            //    return null;

            IPoint pt1 = new PointClass();
            IPoint pt2 = new PointClass();
            IPoint pt3 = new PointClass();

            pt1.X = tri.Node[0].X;
            pt1.Y = tri.Node[0].Y;
            pt1.Z = tri.Node[0].Z;
            pt2.X = tri.Node[1].X;
            pt2.Y = tri.Node[1].Y;
            pt2.Z = tri.Node[1].Z;
            pt3.X = tri.Node[2].X;
            pt3.Y = tri.Node[2].Y;
            pt3.Z = tri.Node[2].Z;

            object missing = Type.Missing;
            IPointCollection pc = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pc as IGeometry);
            pc.AddPoint(pt1, missing, missing);
            pc.AddPoint(pt2, missing, missing);
            pc.AddPoint(pt3, missing, missing);
            pc.AddPoint(pt1, missing, missing);

            return pc as IPolyline;
        }

        /// <summary>
        /// 构造一条直线，由两条边的中点连接而成
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <returns></returns>
        public static IPolyline ConnectMidOfEdges(ITinEdge edge1, ITinEdge edge2)
        {
            IPolyline pl = new PolylineClass();
            GeometryHelper.Enable3DGeometry(pl);

            IPoint pt1 = new PointClass();
            pt1.PutCoords((edge1.ToNode.X + edge1.FromNode.X) / 2, (edge1.ToNode.Y + edge1.FromNode.Y) / 2);
            pt1.Z = (edge1.ToNode.Z + edge1.FromNode.Z) / 2;

            IPoint pt2 = new PointClass();
            pt2.PutCoords((edge2.ToNode.X + edge2.FromNode.X) / 2, (edge2.ToNode.Y + edge2.FromNode.Y) / 2);
            pt2.Z = (edge2.ToNode.Z + edge2.FromNode.Z) / 2;

            pl.FromPoint = pt1;
            pl.ToPoint = pt2;

            return pl;
        }

        public static bool IsLevelTriangle(ITinTriangle tri)
        {
            if (tri.Node[0].Z == tri.Node[1].Z && tri.Node[1].Z == tri.Node[2].Z)
                return true;
            else
                return false;
        }
    }
}
