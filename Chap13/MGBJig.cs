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
    public class MGBJig : EntityJig
    {
        public double m_t;
        private Polyline m_polyline;
        private Point3d m_peakPt;
        private Point2d[] m_pts = new Point2d[4];

        public MGBJig(double t,Point3d spt,Point3d ept) : base(new Polyline())
        {
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //m_mt = ed.CurrentUserCoordinateSystem;
            m_t = t;
            m_pts[0] = new Point2d(spt.X, spt.Y);
            m_pts[1] = new Point2d(ept.X, ept.Y);
            ((Polyline)Entity).AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
            ((Polyline)Entity).AddVertexAt(1, Point2d.Origin, 0.0, 0.0, 0.0);
            ((Polyline)Entity).AddVertexAt(2, Point2d.Origin, 0.0, 0.0, 0.0);
            ((Polyline)Entity).AddVertexAt(3, Point2d.Origin, 0.0, 0.0, 0.0);
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions optJig = new JigPromptPointOptions("\n请指定木工板的方向");
            PromptPointResult resJig = prompts.AcquirePoint(optJig);
            Point3d curPt = resJig.Value;
            if (resJig.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }
            if (m_peakPt != curPt)
            {
                m_peakPt = curPt;
                Vector2d vec1 = m_pts[1] - m_pts[0];
                Vector2d vec2 = new Point2d(m_peakPt.X, m_peakPt.Y) - m_pts[0];
                if(vec2.Angle > vec1.Angle)
                {
                    m_pts[2] = PolarPoint(m_pts[1], vec1.Angle + Rad2Ang(90.0), m_t);
                    m_pts[3] = PolarPoint(m_pts[0], vec1.Angle + Rad2Ang(90.0), m_t);
                }
                else
                {
                    m_pts[2] = PolarPoint(m_pts[1], vec1.Angle - Rad2Ang(90.0), m_t);
                    m_pts[3] = PolarPoint(m_pts[0], vec1.Angle - Rad2Ang(90.0), m_t);
                }
                return SamplerStatus.OK;
            }
            else
                return SamplerStatus.NoChange;
        }
        private Point2d PolarPoint(Point2d basePt, double angle, double dis)
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
        public Entity GetEntity()
        {
            return Entity;
        }

        protected override bool Update()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ((Polyline)Entity).SetPointAt(0, m_pts[0]);
            ((Polyline)Entity).SetPointAt(1, m_pts[1]);
            ((Polyline)Entity).SetPointAt(2, m_pts[2]);
            ((Polyline)Entity).SetPointAt(3, m_pts[3]);
            ((Polyline)Entity).Normal = Vector3d.ZAxis;
            ((Polyline)Entity).Elevation = 0.0;
            ((Polyline)Entity).Closed = true;
            return true;
        }
    }
}
