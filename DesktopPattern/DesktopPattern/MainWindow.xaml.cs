using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
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

namespace DesktopPattern
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //file paths to the packages
        string pathToOutputPackage = @"C:\LaurenCDrive\TL\Readiness\Trainings\RT_Offline_Workflows\MMPKs\Map.mmpk";
        string pathToUnpackedPackage = "";

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }


        private void Initialize()
        {
            Map map = new Map(Basemap.CreateTopographic());

            MyMapView.Map = map;
        }

        private async void OpenMMPK1()
        {
            // Mobile map package to open directly from a package or an unpacked folder.
            MobileMapPackage mobileMapPackage;

            // Check whether the mobile map package supports direct read.
            bool isDirectReadSupported = await MobileMapPackage.IsDirectReadSupportedAsync(pathToOutputPackage);
            if (isDirectReadSupported)
            {
                // If it does, create the mobile map package directly from the .mmpk file.
                mobileMapPackage = await MobileMapPackage.OpenAsync(pathToOutputPackage);
            }
            else
            {
                // Otherwise, unpack the mobile map package file into a directory.
                await MobileMapPackage.UnpackAsync(pathToOutputPackage, pathToUnpackedPackage);

                // Create the mobile map package from the unpack directory.
                mobileMapPackage = await MobileMapPackage.OpenAsync(pathToUnpackedPackage);
            }

            // Make sure there is at least one map, then add the first map in the package to the map view.
            if (mobileMapPackage.Maps.Count > 0)
            {
                Map myMap = mobileMapPackage.Maps.First();
                MyMapView.Map = myMap;
            }
        }

        private void OpenMMPK_Click(object sender, RoutedEventArgs e)
        {
            OpenMMPK1();
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
