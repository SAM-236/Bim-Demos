using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SAMBIMdemo
{
    /// <summary>
    /// Interaction logic for ToggleCategoryUI.xaml
    /// </summary>
    public partial class ToggleCategoryUI : Window
    {
        public ToggleCategoryUI(Document doc, Category currentToggleCat)
        {
            InitializeComponent();
            var revitCat = doc.Settings.Categories.Cast<Category>().OrderBy(cat => cat.Name).ToList();
            foreach (Category category in revitCat)
            {
                CategoryList.Items.Add(category);
                CategoryList.DisplayMemberPath = "Name";
            }
            CategoryList.SelectedItem = currentToggleCat;
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleCatVar.category = (Category)CategoryList.SelectedItem;
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    
}
