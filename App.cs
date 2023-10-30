#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Imaging;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

#endregion

namespace SAMBIMdemo
{
    public static class ToggleCatVar
    {
        public static Category category { get; set; }
    }
   
    internal class App : IExternalApplication
    {
        static readonly string _path = System.Reflection.Assembly.GetExecutingAssembly().Location;
        const string ribbon_TAB = "SAMBIMdemo";
        const string ribbon_ModelPANNEL = "Model Tools";

        public Result OnStartup(UIControlledApplication a)
        {
            // Try to create Ribbion
            #region{
            try // Try the Tab Panels creation
            {
                //Create Ribbon Tab
                CreateRibbonTab(a, ribbon_TAB);
                //Create Always On Ribbion Panels
                #region{
                //RibbonPanel panelGeneral = RibbonPanel(a, ribbon_TAB, ribbon_GeneralPANNEL);
                #endregion}
                //Create Document Type Triggered Pannels and set to invisible by default
                #region{
                RibbonPanel panelModelPannel = RibbonPanel(a, ribbon_TAB, ribbon_ModelPANNEL);
                #endregion}
                //Try Create Buttons
                try
                {
                    //*******************Toggle Scope Boxes********************
                    {
                        PushButtonData toggleScopeBoxesBtn = new PushButtonData("ToggleScopeBoxes", ("Toggle\nScope\nBoxes"), _path, "SAMBIMdemo.ToogleScopeBox");
                        PushButton toggleScopeBoxes = panelModelPannel.AddItem(toggleScopeBoxesBtn) as PushButton;
                        toggleScopeBoxes.ToolTip = "Hide/Unhide Scope Boxes in given View\nWill Enable Temporary View Propetires if a View Template is Active";
                    }
                    //*******************Linked Section Box********************
                    {
                        PushButtonData linkedBoundingBoxBtn = new PushButtonData("LSB", ("Linked\nSection\nBox"), _path, "SAMBIMdemo.LinkedBoundingBox");
                        PushButton linkedBox = panelModelPannel.AddItem(linkedBoundingBoxBtn) as PushButton;
                        linkedBox.ToolTip = "Select linked elements and apply a section box about them in the User's 3D View";
                    }
                    //*******************Toggle Category********************
                    {
                        PushButtonData toggleCatBtn = new PushButtonData("ToggleCat", ("Toggle\nCategory"), _path, "SAMBIMdemo.ToggleCategory");
                        PushButton toggleCat = panelModelPannel.AddItem(toggleCatBtn) as PushButton;
                        toggleCat.ToolTip = "Toggle a Stored Category's visibilty";
                    }
                    //*******************Update Toggle Category UI********************
                    {
                        PushButtonData updateCatBtn = new PushButtonData("UpdateToggleCat", ("Update\nToggle\nCategory"), _path, "SAMBIMdemo.UpdateCatToggle");
                        PushButton updateCat = panelModelPannel.AddItem(updateCatBtn) as PushButton;
                        updateCat.ToolTip = "Update Toggle Catgory Command";
                    }
                }
                catch { TaskDialog.Show("failed at ribbion", "Something Crashed"); }
                return Result.Succeeded;
                }
                #endregion}
                catch { return Result.Failed; }
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
        public static BitmapImage BitmapToImageSource(string name)
        {
            var path = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Debug.WriteLine(path);
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
        //*****************************ribbonPanel()*****************************
        public RibbonPanel RibbonPanel(UIControlledApplication a, string RibbionName, string PanelName)
        {
            RibbonPanel baRibbon = null;
            try { RibbonPanel panel = a.CreateRibbonPanel(RibbionName, PanelName); }
            catch { }
            List<RibbonPanel> panels = a.GetRibbonPanels(RibbionName);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == PanelName)
                { baRibbon = p; break; }
            }
            return baRibbon;
        }
        //*****************************ribbonTab()*****************************
        public void CreateRibbonTab(UIControlledApplication a, string RibbionName)
        {
            try
            {
                a.CreateRibbonTab(RibbionName);
                Debug.WriteLine("Ribbion: " + RibbionName + " Created");
            }
            catch { Debug.WriteLine("Ribbion: " + RibbionName + " Exists already"); }
        }
    }
}
