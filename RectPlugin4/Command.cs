using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Xml.Linq;

[assembly: CommandClass(typeof(RectPlugin4.Command))]

namespace RectPlugin4
{
    public class Command
    {
        [CommandMethod("DrawRect")]
        public void DrawRect()
        { string xmlPath = @"C:\Users\shimp\source\repos\SampleXML.xml";


            if (!File.Exists(xmlPath))
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nxml file not found");
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(xmlPath);

                var items = doc.Descendants("item").ToList();

                double width = Convert.ToDouble(items.First(i =>  i.Element("CHARC").Value == "Width").Element("VALUE").Value);

                double depth = Convert.ToDouble(items.First(i => i.Element("CHARC").Value == "Depth").Element("VALUE").Value);

                string[] pickPoint = items.First(i => i.Element("CHARC").Value == "PickPoint").Element("VALUE").Value.Split(',');

                double x = Convert.ToDouble(pickPoint[0]);
                double y = Convert.ToDouble(pickPoint[1]);
                double z = Convert.ToDouble(pickPoint[2]);

                Point3d basePoint = new Point3d(x, y, z);

                Document docAutoCAD = Application.DocumentManager.MdiActiveDocument;
                Database db = docAutoCAD.Database;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    // Define the rectangle corners
                    Point2d p1 = new Point2d(x, y);
                    Point2d p2 = new Point2d(x + width, y);
                    Point2d p3 = new Point2d(x + width, y + depth);
                    Point2d p4 = new Point2d(x, y + depth);

                    Polyline rect = new Polyline(4);
                    rect.AddVertexAt(0, p1, 0, 0, 0);
                    rect.AddVertexAt(1, p2, 0, 0, 0);
                    rect.AddVertexAt(2, p3, 0, 0, 0);
                    rect.AddVertexAt(3, p4, 0, 0, 0);
                    rect.Closed = true;

                    btr.AppendEntity(rect);
                    tr.AddNewlyCreatedDBObject(rect, true);

                    tr.Commit();
                }

                docAutoCAD.Editor.WriteMessage("\nRectangle Created Successfully");

            }
            catch (System.Exception ex)
            {

                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nErrror: {ex.Message}");


            }
        }
    }
}
