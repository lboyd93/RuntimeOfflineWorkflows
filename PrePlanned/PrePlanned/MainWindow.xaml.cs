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

namespace PrePlanned
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {

        }
        public void OpenMMPK()
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
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
