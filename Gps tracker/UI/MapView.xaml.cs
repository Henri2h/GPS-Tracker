﻿using System;
using System.AppCore;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Console = System.AppCore.Console;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Gps_tracker.UI
{
    public sealed partial class MapView : UserControl
    {
        private readonly int PointMax = 150;

        public MapView()
        {
            this.InitializeComponent();
            // place the mapServiceToken here, you can get one at https://www.bingmapsportal.com/
            if (AppCore.Core.Settings.MapServiceToken != null) MapControl.MapServiceToken = AppCore.Core.Settings.MapServiceToken;
            MapControl.MapServiceToken = "dIQYRjm1oGFEfWPNnTmx~GRofurcHYDuU4uJtNG1C6Q~AhcpDsCLAmjtPskvs3dCm3TMl2Hhawxmy66H6cGFAmkUcOFou7gYl0xbTzzit0Id";

        }

        public void CenterMap(Geopoint point)
        {
            MapControl.Center = point;
            MapControl.ZoomLevel = 16;
        }

        public void SetUIMapSegement(BasicGeoposition start, BasicGeoposition end)
        {
            Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline()
            {
                Path = new Geopath(new List<BasicGeoposition>() { start, end }),

                StrokeColor = Colors.Black,
                StrokeThickness = 3,
                StrokeDashed = true
            };
            MapControl.MapElements.Add(mapPolyline);
        }

        public void UpdateMap(point[] points)
        {
            try
            {
                Windows.System.AppMemoryUsageLevel level = Windows.System.MemoryManager.AppMemoryUsageLevel;
                if (level == Windows.System.AppMemoryUsageLevel.Medium || level == Windows.System.AppMemoryUsageLevel.High)
                {
                    Console.WriteLine("Cleaning map :");
                    this.UpdateUIAllMap(points);
                }

                if (points.Length == 1)
                {
                    BasicGeoposition gP = new BasicGeoposition()
                    {
                        Latitude = points[0].latitude,
                        Longitude = points[0].longitude,
                        Altitude = points[0].altitude
                    };

                    Geopoint gPt = new Geopoint(gP);
                    this.CenterMap(gPt);
                }

                if (points.Length > 1)
                {
                    point oldPoint = points[points.Length - 2];
                    point current = points[points.Length - 1];

                    if (oldPoint != null)
                    {
                        SetUIMapSegement(
                            new BasicGeoposition() { Latitude = oldPoint.latitude, Longitude = oldPoint.longitude },
                            new BasicGeoposition() { Latitude = current.latitude, Longitude = current.longitude }
                        );
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Source = "MainPage.updateUIMap";
                ErrorMessage.PrintOut(ex);
            }
        }

        /// <summary>
        /// Refresh all the map
        /// </summary>
        public void UpdateUIAllMap(point[] points)
        {
            MapControl.MapElements.Clear();
            int numberOfPoints = points.Length - 1;
            int distanceBetweenPoint = 1;

            if (numberOfPoints > PointMax)
            {
                Console.WriteLine("Number of point : " + numberOfPoints);
                distanceBetweenPoint = numberOfPoints / PointMax;
            }

            if (distanceBetweenPoint == 0) { distanceBetweenPoint = 1; }

            try
            {
                point oldPoint = null;
                int totalPointCount = 0;
                for (int i = 0; i < numberOfPoints; i += distanceBetweenPoint)
                {
                    totalPointCount++;
                    point pointElement = points[i];

                    if (oldPoint != null)
                    {
                        Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline()
                        {
                            Path = new Geopath(new List<BasicGeoposition>() {
                            new BasicGeoposition() {Latitude=oldPoint.latitude, Longitude=oldPoint.longitude},
                            new BasicGeoposition() {Latitude=pointElement.latitude, Longitude=pointElement.longitude}
                             }),
                            StrokeColor = Colors.Black,
                            StrokeThickness = 3,
                            StrokeDashed = true
                        };
                        MapControl.MapElements.Add(mapPolyline);
                    }

                    oldPoint = pointElement;
                }

                Console.WriteLine("Total point number : " + totalPointCount);

            }
            catch (Exception ex)
            {
                ex.Source = "MainPage.updateUIMap";
                ErrorMessage.PrintOut(ex);
            }
        }

        private void MapControl1_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateUIAllMap(AppCore.Core.GPSLocator.track.ToArray());
        }
    }

}
