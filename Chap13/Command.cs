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
    }
}
