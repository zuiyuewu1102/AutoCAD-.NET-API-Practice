using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chap13
{
    public class CircleJig : EntityJig
    {
        private Point3d m_CenterPt;
        private double m_Radius = 100.0;
        public CircleJig(Vector3d normal):base(new Circle())
        {
            ((Circle)Entity).Center = m_CenterPt;
            ((Circle)Entity).Normal = normal;
            ((Circle)Entity).Radius = m_Radius;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions optJig = new JigPromptPointOptions("\n请指定圆的圆心或用右键修改半径");
            optJig.Keywords.Add("100");
            optJig.Keywords.Add("200");
            optJig.Keywords.Add("300");
            optJig.UserInputControls = UserInputControls.Accept3dCoordinates;
            PromptPointResult resJig = prompts.AcquirePoint(optJig);
            Point3d curPt = resJig.Value;
            if(resJig.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }
            if(resJig.Status == PromptStatus.Keyword)
            {
                switch (resJig.StringResult)
                {
                    case "100":
                        m_Radius = 100.0;
                        return SamplerStatus.NoChange;
                    case "200":
                        m_Radius = 200;
                        return SamplerStatus.NoChange;
                    case "300":
                        m_Radius = 300;
                        return SamplerStatus.NoChange;
                }
            }
            if (m_CenterPt != curPt)
            {
                m_CenterPt = curPt;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }

        protected override bool Update()
        {
            ((Circle)Entity).Center = m_CenterPt;
            ((Circle)Entity).Radius = m_Radius;
            return true;
        }

        public Entity GetEntity()
        {
            return Entity;
        }
    }
}
