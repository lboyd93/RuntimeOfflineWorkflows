using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace OnDemand
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
            Map map = new Map(Basemap.CreateTopographic());

            MyMapView.Map = map;
        }

        private void OnDemand_Click(object sender, RoutedEventArgs e)
        {
            GenerateOnDemand();
        }

        private Envelope GetAreaOfInterest()
        {
            Envelope area = new Envelope(new MapPoint(-13106757.745708855, 3953831.4127816204, new SpatialReference(102100)), new MapPoint(-12860630.514630707, 4072461.6806801567, new SpatialReference(102100)));
            return area;
        }
        
        private async void GenerateOnDemand()
        {

            string pathToOutputPackage = @"C:\LaurenCDrive\TL\Readiness\Trainings\RT_Offline_Workflows\OnDemandMap";

            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            // Get a web map item using its ID.
            PortalItem webmapItem = await PortalItem.CreateAsync(portal, "4b923ccea5dd405bbb2a789ad3ac101a");

            // Create a map from the web map item.
            Map onlineMap = new Map(webmapItem);

            // Create an OfflineMapTask from the map ...
            OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(onlineMap);
            // ... or a web map portal item.
            takeMapOfflineTask = await OfflineMapTask.CreateAsync(webmapItem);

            await onlineMap.LoadAsync();

            // Create default parameters for the task.
            Envelope areaOfInterest = GetAreaOfInterest();
            GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(areaOfInterest);

            // Limit the maximum scale to 5000 but take all the scales above (use default of 0 as the MinScale).
            parameters.MaxScale = 5000;

            // Set attachment options.
            parameters.AttachmentSyncDirection = AttachmentSyncDirection.Upload;
            parameters.ReturnLayerAttachmentOption = ReturnLayerAttachmentOption.EditableLayers;

            // Request the table schema only (existing features won’t be included).
            parameters.ReturnSchemaOnlyForEditableLayers = true;

            // Update the map title to contain the region.
            parameters.ItemInfo.Title = parameters.ItemInfo.Title + " (Central)";

            // Override the thumbnail with a new image based on the extent.
            RuntimeImage thumbnail = await MyMapView.ExportImageAsync();
            parameters.ItemInfo.Thumbnail = thumbnail;

            // Create the job to generate an offline map, pass in the parameters and a path to store the map package.
            GenerateOfflineMapJob generateMapJob = takeMapOfflineTask.GenerateOfflineMap(parameters, pathToOutputPackage);

            // Generate the offline map and download it.
            GenerateOfflineMapResult offlineMapResult = await generateMapJob.GetResultAsync();

            if (!offlineMapResult.HasErrors)
            {
                // Job completed successfully and all content was generated.
                Debug.WriteLine("Map " +
                    offlineMapResult.MobileMapPackage.Item.Title +
                    " was saved to " +
                    offlineMapResult.MobileMapPackage.Path);

                // Show the offline map in a MapView.
                MyMapView.Map = offlineMapResult.OfflineMap;
            }
            else
            {
                // Job is finished but one or more layers or tables had errors.
                if (offlineMapResult.LayerErrors.Count > 0)
                {
                    // Show layer errors.
                    foreach (var layerError in offlineMapResult.LayerErrors)
                    {
                        Debug.WriteLine("Error occurred when taking " +
                            layerError.Key.Name +
                            " offline. Error : " +
                            layerError.Value.Message);
                    }
                }
                if (offlineMapResult.TableErrors.Count > 0)
                {
                    // Show table errors.
                    foreach (var tableError in offlineMapResult.TableErrors)
                    {
                        Debug.WriteLine("Error occurred when taking " +
                            tableError.Key.TableName +
                            " offline. Error : " +
                            tableError.Value.Message);
                    }
                }
            }

            OfflineMapCapabilities results = await takeMapOfflineTask.GetOfflineMapCapabilitiesAsync(parameters);
            if (results.HasErrors)
            {
                // Handle possible errors with layers
                foreach (var layerCapability in results.LayerCapabilities)
                {
                    if (!layerCapability.Value.SupportsOffline)
                    {
                        Debug.WriteLine(layerCapability.Key.Name + " cannot be taken offline. Error : " + layerCapability.Value.Error.Message);
                    }
                }

                // Handle possible errors with tables
                foreach (var tableCapability in results.TableCapabilities)
                {
                    if (!tableCapability.Value.SupportsOffline)
                    {
                        Debug.WriteLine(tableCapability.Key.TableName + " cannot be taken offline. Error : " + tableCapability.Value.Error.Message);
                    }
                }
            }
            else
            {
                // All layers and tables can be taken offline!
                MessageBox.Show("All layers are good to go!");
            }

            // Create a mobile map package from an unpacked map package folder.
            MobileMapPackage offlineMapPackage = await MobileMapPackage.OpenAsync(pathToOutputPackage);

            // Set the title from the package metadata to the UI
            

            // Get the map from the package and set it to the MapView
            var map = offlineMapPackage.Maps.First();
            MyMapView.Map = map;

        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
