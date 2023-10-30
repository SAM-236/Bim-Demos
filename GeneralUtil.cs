using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Reference = Autodesk.Revit.DB.Reference;
using System;
using Autodesk.Revit.Attributes;
using System.Threading;

namespace SAMBIMdemo
{
    public class GeneralUtil
    { 
        public static bool ToggleScopeBoxes(Document doc)
            {
                //Toggle the Scope Box (Volumne of Intrest) Visibility in a view
                //Tool Checks if a Template for the View settings is active
                //  if a template is active engage Temportay View Settings before toggling the Scope Boxes
                //      This is done to not edit View Templates that can affect multiple views
                View aview = doc.ActiveView;
                ElementId scopeBoxId = new ElementId(BuiltInCategory.OST_VolumeOfInterest);
                //Also known as new ElementId(Convert.ToInt64(-2006000) for int64
                //new ElementId(-2006000) for int32 if working in revit 23 or 24
                if (aview.ViewTemplateId.Value != -1)
                {
                    if (aview.CanEnableTemporaryViewPropertiesMode() != false)
                    {
                        if (aview.IsTemporaryViewPropertiesModeEnabled() != true)
                        {
                            aview.EnableTemporaryViewPropertiesMode(aview.ViewTemplateId);
                            if (aview.GetCategoryHidden(scopeBoxId) != false)
                            {
                                aview.SetCategoryHidden(scopeBoxId, false);
                                return true;
                            }
                            else { aview.SetCategoryHidden(scopeBoxId, true); return true; }
                        }
                        else
                        {
                            aview.DisableTemporaryViewMode(TemporaryViewMode.TemporaryViewProperties);
                            return true;
                        }
                    }
                }
                else
                {
                    if (aview.GetCategoryHidden(scopeBoxId) == false) { aview.SetCategoryHidden(scopeBoxId, true); }
                    else { aview.SetCategoryHidden(scopeBoxId, false); }
                }
                return false;
            }
        public static bool ToggleCat(Document doc, Category cat)
        {
            //Toggle Catgeory Visibility in a view stored in the app
            //Tool Checks if a Template for the View settings is active
            //  if a template is active engage Temportay View Settings before toggling the category
            //      This is done to not edit View Templates that can affect multiple views
            View aview = doc.ActiveView;
            ElementId catId = cat.Id;
            //Also known as new ElementId(Convert.ToInt64(-2006000) for int64
            //new ElementId(-2006000) for int32 if working in revit 23 or 24
            if (aview.CanCategoryBeHidden(catId) != false)
            {
                if (aview.ViewTemplateId.Value != -1)
                {
                    if (aview.CanEnableTemporaryViewPropertiesMode() != false)
                    {
                        if (aview.IsTemporaryViewPropertiesModeEnabled() != true)
                        {
                            aview.EnableTemporaryViewPropertiesMode(aview.ViewTemplateId);
                            if (aview.GetCategoryHidden(catId) != false)
                            {
                                aview.SetCategoryHidden(catId, false);
                                return true;
                            }
                            else { aview.SetCategoryHidden(catId, true); return true; }
                        }
                        else
                        {
                            aview.DisableTemporaryViewMode(TemporaryViewMode.TemporaryViewProperties);
                            return true;
                        }
                    }
                }
                else
                {
                    if (aview.GetCategoryHidden(catId) == false) { aview.SetCategoryHidden(catId, true); }
                    else { aview.SetCategoryHidden(catId, false); }
                }
                return false;
            }
            else { TaskDialog.Show("Toggle Categor Fail", "Toggle Cagory in View Failed.\nThis Catgory can not behiden in this type of View"); return false; }
            
        }
        public static Autodesk.Revit.DB.Workset GetWorksetByName(Document doc, string name)
            {
                //Tool to Get workset by a Human Readable Name
                IList<Workset> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
                Workset wOut = null;
                foreach (Workset w in worksets)
                {
                    if (w.Name == name) { wOut = w; return w; }
                }
                if (wOut == null) { return null; }
                else { return wOut; }
            }
            public static bool IsModelingView(Document document)
            {
                //Check if the active view is for Modeling
                //Will return true for Floor Plan, 3D Views, Sections, Elevations, Ceiling and Engineering Plans
                ViewType vtype = document.ActiveView.ViewType;
                if (vtype == ViewType.FloorPlan || vtype == ViewType.ThreeD || vtype == ViewType.Section ||
                    vtype == ViewType.Elevation || vtype == ViewType.CeilingPlan || vtype == ViewType.EngineeringPlan)
                { return true; }
                else { return false; }
            }
            public static Result CopySelectedLinkedPipe(UIDocument uidoc)
            {
                //Tool to Select Pipe Items in a linked file and copy them into the current Revit Model to allow for tools like SysQue to convert the pipe to fabrication items without copying all the elements at once.
                //  allows for user to control the pace of the conversion without opening the linked model external to the active UI doc. 
                Document doc = uidoc.Document;
                IList<Reference> r;
                ElementInLinkSelectionFilter<Pipe> filter = new ElementInLinkSelectionFilter<Pipe>(doc);
                filter.AddAllowedCategory(BuiltInCategory.OST_PipeAccessory);
                filter.AddAllowedCategory(BuiltInCategory.OST_PipeFitting);
                try
                {
                    r = uidoc.Selection.PickObjects(ObjectType.LinkedElement, filter, "Please pick a room in current project or linked model");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
                if (r != null)
                {
                    List<Element> e = new List<Element>();
                    List<ElementId> eids = new List<ElementId>();
                    Element fa = null;
                    foreach (Reference a in r)
                    {
                        if (filter.LastCheckedWasFromLink)
                        {
                            e.Add(filter.LinkedDocument.GetElement(a.LinkedElementId));
                            eids.Add(a.LinkedElementId);
                        }
                    }
                    Transaction targetTrans = new Transaction(doc);
                    targetTrans.Start("Copy Linked Piping");
                    CopyPasteOptions copyOptions = new CopyPasteOptions();
                    ElementTransformUtils.CopyElements(filter.LinkedDocument, eids, doc, null, copyOptions);
                    doc.Regenerate();
                    targetTrans.Commit();
                    return Result.Succeeded;
                }
                return Result.Cancelled;
            }
            // Custom selection filter to filter linked elements by a given category
            internal sealed class CategorySelectionFilter : ISelectionFilter
            {
                public bool AllowElement(Element elem)
                {
                    if (elem.Category.Name == "Walls")
                        return true;
                    return false;
                }

                public bool AllowReference(Reference reference, XYZ position)
                {
                    return false;
                }
            }
            public class ElementInLinkSelectionFilter<T> : ISelectionFilter where T : Element
            {
                private Document _doc;
                private List<BuiltInCategory> _allowedCategories;

                public ElementInLinkSelectionFilter(Document doc)
                {
                    _doc = doc;
                    _allowedCategories = new List<BuiltInCategory>();
                    AddAllowedCategory(GetCategoryForType<T>());
                }

                public void AddAllowedCategory(BuiltInCategory category)
                {
                    _allowedCategories.Add(category);
                }

                private BuiltInCategory GetCategoryForType<TElement>() where TElement : Element
                {
                    if (typeof(TElement) == typeof(Pipe))
                    {
                        return BuiltInCategory.OST_PipeCurves;
                    }
                    else
                    {
                        // Throw an exception if the type is not supported
                        throw new NotSupportedException($"Element type '{typeof(TElement).Name}' is not supported.");
                    }
                }

                public Document LinkedDocument { get; private set; } = null;

                public bool LastCheckedWasFromLink
                {
                    get { return null != LinkedDocument; }
                }

                public bool AllowElement(Element e)
                {
                    return true;
                }

                public bool AllowReference(Reference r, XYZ p)
                {
                    LinkedDocument = null;

                    Element e = _doc.GetElement(r);

                    if (e is RevitLinkInstance)
                    {
                        RevitLinkInstance li = e as RevitLinkInstance;
                        LinkedDocument = li.GetLinkDocument();
                        e = LinkedDocument.GetElement(r.LinkedElementId);
                    }

                    return IsCategoryAllowed(e);
                }

                private bool IsCategoryAllowed(Element element)
                {
                    BuiltInCategory category = (BuiltInCategory)element.Category?.Id.IntegerValue;
                    return _allowedCategories.Contains(category);
                }
            }
            public class CopyUseDestination : IDuplicateTypeNamesHandler
            {
                public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
                {
                    return DuplicateTypeAction.UseDestinationTypes;
                }
            }
            public class PipeSelectionFilter : ISelectionFilter
            {
                Autodesk.Revit.DB.Document doc = null;
                public PipeSelectionFilter(Document document)
                {
                    doc = document;
                }

                public bool AllowElement(Element elem)
                {
                    return true;
                }
                public bool AllowReference(Autodesk.Revit.DB.Reference reference, XYZ point)
                {
                    RevitLinkInstance revitlinkinstance = doc.GetElement(reference) as RevitLinkInstance;
                    Autodesk.Revit.DB.Document docLink = revitlinkinstance.GetLinkDocument();
                    Element ePipeLink = docLink.GetElement(reference.LinkedElementId);
                    if (ePipeLink.Category.Name.ToUpper().Contains("PIPE") != false)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
}