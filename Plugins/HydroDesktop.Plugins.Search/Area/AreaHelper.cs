﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Projections;
using HydroDesktop.WebServices;
using HydroDesktop.Plugins.Search.Properties;

namespace HydroDesktop.Plugins.Search.Area
{
    static class AreaHelper
    {
        #region Fields

        private static readonly ProjectionInfo _wgs84Projection = ProjectionInfo.FromEsriString(KnownCoordinateSystems.Geographic.World.WGS1984.ToEsriString());

        #endregion

        #region Public methods

        public static IEnumerable<IMapPolygonLayer> GetAllPolygonLayers(Map map)
        {
            if (map == null) throw new ArgumentNullException("map");

            return map.GetAllLayers().OfType<IMapPolygonLayer>();
        }

        public static IEnumerable<IMapPolygonLayer> GetAllSelectedPolygonLayers(Map map)
        {
            if (map == null) throw new ArgumentNullException("map");

            return GetAllPolygonLayers(map)
                  .Where(subLayer => subLayer.IsVisible && 
                         subLayer.IsSelected);
        }

        public static void SelectFirstVisiblePolygonLayer(Map map, bool isWorldTemplate)
        {
            if (map == null) throw new ArgumentNullException("map");

            //special case: world project template
            if (isWorldTemplate)
            {
                map.MapFrame.IsSelected = false;
                foreach (var layer in GetAllPolygonLayers(map).Where(subLayer => subLayer.IsVisible))
                {
                    layer.IsSelected = true;
                    map.Legend.RefreshNodes();
                    break;
                }
            }

            if (map.MapFrame.IsSelected) return; //don't select layers if map frame is already selected

            var hasSelected = GetAllPolygonLayers(map).Where(subLayer => subLayer.IsVisible)
                                                      .Any(item => item.IsSelected);
            if (hasSelected)
                return;

            foreach (var layer in GetAllPolygonLayers(map).Where(subLayer => subLayer.IsVisible).Reverse())
            {
                layer.IsSelected = true;
                map.Legend.RefreshNodes();
                break;
            }
        }

        public static Box ReprojectBoxToWGS84(Box sourceBox, ProjectionInfo sourceProjection)
        {
            if (sourceBox == null) throw new ArgumentNullException("sourceBox");
            if (sourceProjection == null) throw new ArgumentNullException("sourceProjection");

            var xMin = sourceBox.XMin;
            var yMin = sourceBox.YMin;
            var xMax = sourceBox.XMax;
            var yMax = sourceBox.YMax;

            var xy = new[] { xMin, yMin, xMax, yMax };
            Reproject.ReprojectPoints(xy, new double[] { 0, 0 }, sourceProjection, _wgs84Projection, 0, 2);

            xMin = xy[0];
            yMin = xy[1];
            xMax = xy[2];
            yMax = xy[3];
            var rectangle = new Box(xMin, xMax, yMin, yMax);
            return rectangle;
        }

        public static List<IFeature> ReprojectPolygonsToWGS84(FeatureSet polygons)
        {
            if (polygons == null) throw new ArgumentNullException("polygons");
            
            polygons.Reproject(_wgs84Projection);
            return Enumerable.ToList(polygons.Features);
        }

        #endregion
    }
}
