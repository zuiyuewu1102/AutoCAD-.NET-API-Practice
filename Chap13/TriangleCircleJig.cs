using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chap13
{
    public class TriangleCircleJig : EntityJig
    {
        private Point3d m_CenterPt;
        private Point3d m_peakPt;
        private Point2d[] m_pts = new Point2d[3];
        private Matrix3d m_mt = Matrix3d.Identity;
        public ObjectId m_CirId = ObjectId.Null;
        public TriangleCircleJig (Point3d cenPt,ObjectId cirId):base(new Polyline())
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            m_mt = ed.CurrentUserCoordinateSystem;
            ((Polyline)Entity).AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            ((Polyline)Entity).AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            ((Polyline)Entity).AddVertexAt(2, Point2d.Origin, 0.0, 0.0, 0.0);
            m_CenterPt = cenPt;
            m_CirId = cirId;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions optJig = new JigPromptPointOptions("\n请指定等边三角形的一个顶点");
            optJig.Cursor = CursorType.RubberBand;
            optJig.UserInputControls = UserInputControls.Accept3dCoordinates;
            optJig.BasePoint = m_CenterPt.TransformBy(m_mt);
            optJig.UseBasePoint = true;
            PromptPointResult resJig = prompts.AcquirePoint(optJig);
            Point3d curPt = resJig.Value;
            if(resJig.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }
            if (m_peakPt != curPt)
            {
                m_peakPt = curPt;
                Point2d cenPt = new Point2d(m_CenterPt.X, m_CenterPt.Y);
                m_pts[0] = new Point2d(m_CenterPt.TransformBy(m_mt.Inverse()).X, m_peakPt.TransformBy(m_mt.Inverse()).Y);
                double dis = m_pts[0].GetDistanceTo(cenPt);
                Vector2d vec = m_pts[0] - cenPt;
                double ang = vec.Angle;
                m_pts[1] = PolarPoint(cenPt, ang + Rad2Ang(120.0), dis);
                m_pts[2] = PolarPoint(cenPt, ang + Rad2Ang(240.0), dis);
                return SamplerStatus.OK;
            }
            else
                return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ((Polyline)Entity).SetPointAt(0, m_pts[0]);
            ((Polyline)Entity).SetPointAt(1, m_pts[1]);
            ((Polyline)Entity).SetPointAt(2, m_pts[2]);
            ((Polyline)Entity).Normal = Vector3d.ZAxis;
            ((Polyline)Entity).Elevation = 0.0;
            ((Polyline)Entity).TransformBy(m_mt);
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                Circle circle = (Circle)tr.GetObject(m_CirId, OpenMode.ForWrite);
                circle.Center = m_CenterPt.TransformBy(m_mt);
                circle.Normal = m_mt.CoordinateSystem3d.Zaxis;
                double radius = 0.5 * m_pts[0].GetDistanceTo(m_pts[1]) * Math.Tan(Rad2Ang(30.0));
                if(radius > 0.0)
                {
                    circle.Radius = radius;
                }
                tr.Commit();
            }
            return true;
        }

        public Entity GetEntity()
        {
            return Entity;
        }

        private Point2d PolarPoint(Point2d basePt,double angle,double dis)
        {
            double x = basePt[0] + dis * Math.Cos(angle);
            double y = basePt[1] + dis * Math.Sin(angle);
            Point2d point = new Point2d(x, y);
            return point;
        }

        private double Rad2Ang(double angle)
        {
            double rad = angle * Math.PI / 180.0;
            return rad;
        }
    }
}
