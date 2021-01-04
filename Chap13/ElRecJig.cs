using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Chap13
{
    public class ElRecJig : DrawJig
    {
        public Ellipse m_Ellipse;
        private Polyline m_Polyline;
        private Point3d m_Pt1, m_Pt2;
        public ElRecJig(Point3d pt1,Ellipse ellipse,Polyline polyline)
        {
            m_Pt1 = pt1;
            m_Ellipse = ellipse;
            m_Polyline = polyline;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            JigPromptPointOptions optJigPoint = new JigPromptPointOptions("\n请指定矩形的另一角点")
            {
                Cursor = CursorType.Crosshair,
                UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted | UserInputControls.NoNegativeResponseAccepted,
                BasePoint = m_Pt1.TransformBy(mt),
                UseBasePoint = true
            };
            PromptPointResult resJigPoint = prompts.AcquirePoint(optJigPoint);
            Point3d tempPt = resJigPoint.Value;
            if (resJigPoint.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;
            if (m_Pt2 != tempPt)
            {
                m_Pt2 = tempPt;
                Point3d ucsPt2 = m_Pt2.TransformBy(mt.Inverse());
                m_Polyline.Normal = Vector3d.ZAxis;
                m_Polyline.Elevation = 0.0;
                m_Polyline.SetPointAt(0, new Point2d(m_Pt1.X, m_Pt1.Y));
                m_Polyline.SetPointAt(1, new Point2d(ucsPt2.X, m_Pt1.Y));
                m_Polyline.SetPointAt(2, new Point2d(ucsPt2.X, ucsPt2.Y));
                m_Polyline.SetPointAt(3, new Point2d(m_Pt1.X, ucsPt2.Y));
                m_Polyline.TransformBy(mt);
                Point3d cenPt = GeTools.MidPoint(m_Pt1.TransformBy(mt), m_Pt2);
                Point3d majorPt = new Point3d(ucsPt2.X, m_Pt1.Y, 0);
                Vector3d vecX = GeTools.MidPoint(majorPt, ucsPt2).TransformBy(mt) - cenPt;
                try
                {
                    if (Math.Abs(ucsPt2.X - m_Pt1.X) < 0.0000001 | Math.Abs(ucsPt2.Y - m_Pt1.Y) < 0.000001)
                    {
                        m_Ellipse = new Ellipse(Point3d.Origin, Vector3d.ZAxis, new Vector3d(0.00000001, 0, 0), 1.0, 0.0, 0.0);
                    }
                    else
                    {
                        double radiusRatio = Math.Abs((ucsPt2.X - m_Pt1.X) / (ucsPt2.Y - m_Pt1.Y));
                        if (radiusRatio < 1.0)
                        {
                            majorPt = new Point3d(m_Pt1.X, ucsPt2.Y, 0.0);
                            vecX = GeTools.MidPoint(majorPt, ucsPt2).TransformBy(mt) - cenPt;
                        }
                        else
                        {
                            radiusRatio = 1.0 / radiusRatio;
                        }
                        m_Ellipse.Set(cenPt, mt.CoordinateSystem3d.Zaxis, vecX, radiusRatio, 0, 2 * Math.PI);
                    }
                }
                catch { }
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            draw.Geometry.Draw(m_Ellipse);
            draw.Geometry.Draw(m_Polyline);
            return true;
        }
    }
}
