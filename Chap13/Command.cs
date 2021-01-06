using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;

namespace Chap13
{
    public class Command
    {
        [CommandMethod("JigCircle")]
        public void JipCircleTest()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            Vector3d normal = mt.CoordinateSystem3d.Zaxis;
            CircleJig circleJig = new CircleJig(normal);
            for(; ; )
            {
                PromptResult resJig = ed.Drag(circleJig);
                if (resJig.Status == PromptStatus.Cancel) return;
                if(resJig.Status == PromptStatus.OK)
                {
                    Tools.AddToModelSpace(db, circleJig.GetEntity());
                    break;
                }
            }

        }

        [CommandMethod("EntityJigTriangleCircle")]

        public void EntityJigTriangleCircleTest()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions optPoint = new PromptPointOptions("\n请指定等边三角形的中心");
            PromptPointResult resPoint = ed.GetPoint(optPoint);
            if (resPoint.Status != PromptStatus.OK) return;
            Point3d cenPt = resPoint.Value;
            Circle circle = new Circle();
            ObjectId cirId = Tools.AddToModelSpace(db, circle);
            TriangleCircleJig triangleCircleJig = new TriangleCircleJig(cenPt, cirId);
            PromptResult resJig = ed.Drag(triangleCircleJig);
            if(resJig.Status == PromptStatus.Cancel)
            {
                using(Transaction tr = db.TransactionManager.StartTransaction())
                {
                    circle = (Circle)tr.GetObject(triangleCircleJig.m_CirId, OpenMode.ForWrite);
                    circle.Erase();
                    tr.Commit();
                }
                return;
            }
            if (resJig.Status == PromptStatus.OK)
            {
                Tools.AddToModelSpace(db, triangleCircleJig.GetEntity());
            }
        }

        [CommandMethod("JigElRec")]
        public void JigElRecTest()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions optPoint = new PromptPointOptions("\n请指定椭圆外切矩形的一个角点");
            PromptPointResult resPoint = ed.GetPoint(optPoint);
            if (resPoint.Status != PromptStatus.OK) return;
            Point3d pt1 = resPoint.Value;
            try
            {
                db.LoadLineTypeFile("DASHED", "acadiso.lin");
            }
            catch { }
            Polyline polyline = new Polyline();
            for (int i = 0; i < 4; i++)
            {
                polyline.AddVertexAt(i, Point2d.Origin, 0.0, 0.0, 0.0);
            }
            polyline.Closed = true;
            polyline.Linetype = "DASHED";
            Ellipse ellipse = new Ellipse(Point3d.Origin, Vector3d.ZAxis, new Vector3d(0.0000001, 0.0, 0.0), 1.0, 0.0, 0.0);
            ElRecJig elRecJig = new ElRecJig(pt1, ellipse, polyline);
            PromptResult resJig = ed.Drag(elRecJig);
            if (resJig.Status == PromptStatus.OK)
            {
                Tools.AddToModelSpace(db, elRecJig.m_Ellipse);
            }
        }

        [CommandMethod("MGB")]
        public void MGB()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            double t = 9.0;
            PromptPointOptions optPoint = new PromptPointOptions("\n请拾取木工板起点坐标");
            PromptPointResult resPoint = ed.GetPoint(optPoint);
            if (resPoint.Status != PromptStatus.OK) return;
            Point3d spt = resPoint.Value;
            PromptPointOptions ppo = new PromptPointOptions("\n请拾取木工板终点坐标");
            ppo.UseBasePoint = true;
            ppo.BasePoint = spt;
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;
            Point3d ept = ppr.Value;
            MGBJig mGBJig = new MGBJig(t, spt, ept);
            PromptResult resJig = ed.Drag(mGBJig);
            if(resJig.Status == PromptStatus.OK)
            {
                Tools.AddToModelSpace(db, mGBJig.GetEntity());
            }
        }
    }
}
