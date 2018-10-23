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
            Map map = new Map(Basemap.CreateTopographic());

            MyMapView.Map = map;
        }

        private async void GeneratePreplannedMap()
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            string itemID = "a9081e4d02dc49f68c6202f03bfe302f";

            // Get a web map item using its ID.
            PortalItem webmapItem = await PortalItem.CreateAsync(portal, itemID);

            // Create a map from the web map item.
            Map onlineMap = new Map(webmapItem);

            // Create an OfflineMapTask from the map ...
            OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(onlineMap);
            // ... or a web map portal item.
            //takeMapOfflineTask = await OfflineMapTask.CreateAsync(webmapItem);

            // Get a list of the available preplanned map areas.
            IReadOnlyList<PreplannedMapArea> preplannedMapAreaList = await takeMapOfflineTask.GetPreplannedMapAreasAsync();

            // Loop through all preplanned map areas.
            foreach (PreplannedMapArea mapArea in preplannedMapAreaList)
            {
                // Load the preplanned map area so property values can be read.
                await mapArea.LoadAsync();

                // Get the area of interest (geometry) for this area.
                Geometry aoi = mapArea.AreaOfInterest;

                // Get the portal item for this area, read the title and thumbnail image.
                PortalItem preplannedMapItem = mapArea.PortalItem;
                string mapTitle = preplannedMapItem.Title;
                RuntimeImage areaThumbnail = preplannedMapItem.Thumbnail;
            }


            PreplannedMapArea downloadMapArea = preplannedMapAreaList.First();

            string pathToOutputPackage = @"C:\LaurenCDrive\TL\Readiness\Trainings\RT_Offline_Workflows\PrePlannedMap";

            DownloadPreplannedOfflineMapJob preplannedMapJob = takeMapOfflineTask.DownloadPreplannedOfflineMap(downloadMapArea, pathToOutputPackage);

            // Generate the offline map and download it.
            DownloadPreplannedOfflineMapResult preplannedMapResult = await preplannedMapJob.GetResultAsync();

            if (!preplannedMapResult.HasErrors)
            {
                // Job completed successfully and all content was generated.
                Debug.WriteLine("Map " +
                    preplannedMapResult.MobileMapPackage.Item.Title +
                    " was saved to " +
                    preplannedMapResult.MobileMapPackage.Path);

                // Show the offline map in a MapView.
                MyMapView.Map = preplannedMapResult.OfflineMap;
            }
            else
            {
                // Job is finished but one or more layers or tables had errors.
                if (preplannedMapResult.LayerErrors.Count > 0)
                {
                    // Show layer errors.
                    foreach (var layerError in preplannedMapResult.LayerErrors)
                    {
                        Debug.WriteLine("Error occurred when taking " +
                            layerError.Key.Name +
                            " offline. Error : " +
                            layerError.Value.Message);
                    }
                }
                if (preplannedMapResult.TableErrors.Count > 0)
                {
                    // Show table errors.
                    foreach (var tableError in preplannedMapResult.TableErrors)
                    {
                        Debug.WriteLine("Error occurred when taking " +
                            tableError.Key.TableName +
                            " offline. Error : " +
                            tableError.Value.Message);
                    }
                }
            }
        }

        private void GeneratePreplannedAreas_Click(object sender, RoutedEventArgs e)
        {
            GeneratePreplannedMap();
        }


        // Map initialization logic is contained in MapViewModel.cs
    }
}
