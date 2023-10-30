using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using static Autodesk.Revit.DB.SpecTypeId;
using Reference = Autodesk.Revit.DB.Reference;

namespace SAMBIMdemo
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //Selection sel = uidoc.Selection;
            IList<Reference> r;
            ElementInLinkSelectionFilter<Pipe> filter = new ElementInLinkSelectionFilter<Pipe>(doc);
            filter.AddAllowedCategory(BuiltInCategory.OST_PipeAccessory);
            // filter.AddAllowedType<PipeAccessory>();
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
                        // fa = doc.GetElement(a.LinkedElementId);
                    }
                }
                //var link = doc.GetElement(fa.Id) as RevitLinkInstance;
                //var linkDoc = link.GetLinkDocument();
                Transaction targetTrans = new Transaction(doc);
                targetTrans.Start("Copy Linked Piping");
                CopyPasteOptions copyOptions = new CopyPasteOptions();
                //copyOptions.SetDuplicateTypeNamesHandler(new CopyUseDestination());
                //Document linkDoc = doc.GetElement(eids[0]).Document;
                ElementTransformUtils.CopyElements(filter.LinkedDocument, eids, doc, null, copyOptions);
                doc.Regenerate();
                targetTrans.Commit();

                return Result.Succeeded;
            }

            return Result.Cancelled;
            //TaskDialog.Show("Picked", e.Name);

            // // Reference linkRef = sel.PickObject(ObjectType.Element, "Select a link instance first.");
            // // RevitLinkInstance linkInstance = doc.GetElement(linkRef.ElementId) as RevitLinkInstance;

            ////  if (linkInstance is RevitLinkInstance)
            // // {
            //      CategorySelectionFilter selectionFilter = new CategorySelectionFilter();
            //      IList<Reference> selectedIds = sel.PickObjects(ObjectType.LinkedElement, selectionFilter, "Select linked elements of a specific category and press Finish.");
            //      foreach(Reference conduit in selectedIds)
            //      {
            //          Element e = doc.GetElement(conduit);
            //          Parameter conduitSize = e.LookupParameter("Length");
            //      Debug.WriteLine(e.Name + " : " + conduitSize.AsString());
            //      }
            // // List<Element> selectedElements = new List<Element>();
            //      //foreach (ElementId id in selectedIds)
            //      //{
            //      //    Element element = doc.GetElement(id);
            //      //    selectedElements.Add(element);
            //      //}

            // Now you have the selected linked elements of the specified category in the 'selectedElements' list.
            // You can perform further operations with them as needed.

            // TaskDialog.Show("Result", $"Selected {selectedElements.Count} linked elements of the specified category.");
            //  return Result.Succeeded;
            // }
            // else
            //  {
            //     TaskDialog.Show("Error", "Please select a Revit link instance.");
            //     return Result.Failed;
            // }

        }
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
            if (ePipeLink.Category.Name.ToUpper().Contains("PIPE")!=false)
            {
                return true;
            }
            return false;
        }
    }

}
