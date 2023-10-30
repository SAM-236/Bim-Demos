using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using Autodesk.Revit.ApplicationServices;

namespace SAMBIMdemo
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ToogleScopeBox : IExternalCommand
    {//Command to toggle scope boxes using General Utility
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (GeneralUtil.IsModelingView(doc) == true)
            {
                Transaction transaction1 = new Transaction(doc);
                transaction1.SetName("Toggle Scope Boxes");
                transaction1.Start();
                GeneralUtil.ToggleScopeBoxes(doc);
                transaction1.Commit();
                return Result.Succeeded;
            }
            else { return Result.Cancelled; }
        }
    }
    [TransactionAttribute(TransactionMode.Manual)]
    public class LinkedBoundingBox : IExternalCommand
    {
        //Create a Bounding Box about a Selection of Linked Elements
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            // attempt to log view async
            //Thread thread1 = new Thread(() => DataLogging.FireAwayAsync(uiapp, "Linked Bounding Box"));
            //thread1.Start();
            if (GeneralUtil.IsModelingView(doc) == true)
            {
                //Make List of Min and Max Points of Elements Seelected from Linked Model
                double Min_X = new double();
                double Min_Y = new double();
                double Min_Z = new double();
                double Max_X = new double();
                double Max_Y = new double();
                double Max_Z = new double();

                List<Double> Max_XList = new List<Double>();
                List<Double> Min_XList = new List<Double>();
                List<Double> Max_YList = new List<Double>();
                List<Double> Min_YList = new List<Double>();
                List<Double> Max_ZList = new List<Double>();
                List<Double> Min_ZList = new List<Double>();

                string name = uiapp.Application.Username;
                string viewName = "{3D - " + name + "}";

                //Find Views
                FilteredElementCollector ThreeDViews = new FilteredElementCollector(doc).OfClass(typeof(View3D));

                Boolean doesViewExist = new Boolean();
                List<string> NameList = new List<string>();

                foreach (Element v in ThreeDViews)
                {
                    NameList.Add(v.Name.ToString());
                }
                // Look for default user view 3d if not there make it
                if (NameList.Contains(viewName)) { doesViewExist = true; }
                else { doesViewExist = false; }

                try
                {
                    // select linked elements (must click finish button in Revit UI to end selection)
                    // selection is filtered by only picked link
                    List<Reference> R = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.LinkedElement, "Select Linked Elements to Set Section Box About").ToList();
                    if (R.Count > 0)
                    {
                        List<Element> elist = new List<Element>();
                        foreach (Reference r in R)
                        {
                            // find xyz min max points of elements
                            Element e = (doc.GetElement(r));
                            Document linkedDoc = (e as RevitLinkInstance).GetLinkDocument();
                            Element eLinked = linkedDoc.GetElement(r.LinkedElementId);
                            BoundingBoxXYZ box = eLinked.get_BoundingBox(doc.ActiveView);
                            Max_X = box.Max.X;
                            Max_Y = box.Max.Y;
                            Max_Z = box.Max.Z;

                            Min_X = box.Min.X;
                            Min_Y = box.Min.Y;
                            Min_Z = box.Min.Z;

                            Max_XList.Add(Max_X);
                            Min_XList.Add(Min_X);
                            Max_YList.Add(Max_Y);
                            Min_YList.Add(Min_Y);
                            Max_ZList.Add(Max_Z);
                            Min_ZList.Add(Min_Z);
                        };

                        //Find Max XYZ and Min XYZ point from list of points
                        double X_Max = Max_XList.Max();
                        double X_Min = Min_XList.Min();
                        double Y_Max = Max_YList.Max();
                        double Y_Min = Min_YList.Min();
                        double Z_Max = Max_ZList.Max();
                        double Z_Min = Min_ZList.Min();
                        XYZ Max = new XYZ(X_Max, Y_Max, Z_Max);
                        XYZ Min = new XYZ(X_Min, Y_Min, Z_Min);
                        //Setup a bounding box about min and max XYZ
                        BoundingBoxXYZ box1 = new BoundingBoxXYZ();
                        box1.Min = Min;
                        box1.Max = Max;
                        //Set View and Set Section Box to match the MIN MAX XYZ
                        if (doesViewExist == true)
                        {
                            Element ExistingView = new FilteredElementCollector(doc)
                             .OfCategory(BuiltInCategory.OST_Views)
                             .First(x => x.Name == viewName);

                            using (Transaction t = new Transaction(doc, "Crop View"))
                            {
                                t.Start();
                                (ExistingView as View3D).IsSectionBoxActive = true;
                                (ExistingView as View3D).SetSectionBox(box1);
                                t.Commit();
                                uidoc.ActiveView = (ExistingView as View3D);
                            }
                        }
                        else { TaskDialog.Show("No User 3D View", "No user 3D View exists\n Please create one by clicking the 3D View button \n EX: {3D View - %YourAutodeskName%}"); }
                    }
                    else { }
                }
                catch
                { TaskDialog.Show("Linked Section Box", "Failed to Create Section Box About Objects"); }
                return Result.Succeeded;
            }
            else { return Result.Cancelled; }
        }
    }
    [TransactionAttribute(TransactionMode.Manual)]
    public class ToggleCategory : IExternalCommand
    {//Command to toggle scope boxes using General Utility
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (GeneralUtil.IsModelingView(doc) == true || ToggleCatVar.category != null)
            {
                Transaction transaction = new Transaction(doc);
                transaction.SetName("Toggle Category");
                transaction.Start();
                GeneralUtil.ToggleCat(doc,ToggleCatVar.category);
                transaction.Commit();
                return Result.Succeeded;
            }
            else { return Result.Cancelled; }
        }
    }
    [TransactionAttribute(TransactionMode.Manual)]
    public class UpdateCatToggle: IExternalCommand
    {//Command to update the toggle catgory command catgory
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Transaction transaction = new Transaction(doc);
            ToggleCategoryUI toggleCategoryUI = new ToggleCategoryUI(doc, ToggleCatVar.category)
            {
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };
            toggleCategoryUI.ShowDialog();
            transaction.SetName("Update Category Toggle");
            transaction.Start();
            transaction.Commit();
            return Result.Succeeded;
        }
    }
}