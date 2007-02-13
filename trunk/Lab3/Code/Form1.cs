using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Data;
using Gav.Graphics;
using LabExcelReader;


namespace Lab3 {
    public partial class Form1 : Form {

        private string[] titles;
        private object[,] data;

        List<object> regions;

        private DataCube dataCube;
        private MapData mapData;
        private ColorMap colorMap;
        private List<string> stringList;

        // Layers
        MapPolygonLayer polygonLayer;
        MapPolygonBorderLayer borderLayer;
        LabGlyphLayer glyphLayer;
        ChoroplethMap choroMap;

        private Renderer renderer;

        Panel mapPanel;
        Panel pcPanel;

        public Form1() {
            InitializeComponent();

            mapPanel = splitContainer1.Panel1;
            pcPanel = splitContainer1.Panel2;

            // Initialize private variables
            regions = new List<object>();
            renderer = new Renderer(this);
            dataCube = new DataCube();
            stringList = new List<string>();

            colorMap = CreateColorMap();

            // Write code here.
            LoadData();
        }

        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 180.0f);
            //LinearColorMapPart linearMap = new LinearColorMapPart(Color, Color.DarkKhaki);
            map.AddColorMapPart(hsvMap);
            hsvMap.Invalidate();
            map.Invalidate();

            return map;
        }

        private void SetupMapLayers()
        {
            ShapeFileReader shapeReader = new ShapeFileReader();
            mapData = shapeReader.Read("../../../Map/Sweden_municipality.shp",
                "../../../Map/Sweden_municipality.dbf", "../../../Map/Sweden_municipality.shx");

            // Border Layer
            borderLayer = new MapPolygonBorderLayer();
            borderLayer.MapData = mapData;

            // Polygon Layer
            polygonLayer = new MapPolygonLayer();
            polygonLayer.MapData = mapData;
            colorMap.Input = dataCube;
            colorMap.Index = 5;
            polygonLayer.ColorMap = colorMap;

            // Glyph Layer
            glyphLayer = new LabGlyphLayer();
            glyphLayer.ActiveGlyphPositioner = new CenterGlyphPositioner();
            glyphLayer.ActiveGlyphPositioner.MapData = mapData;
            glyphLayer.Input = dataCube;

            // Choropleth Map
            choroMap = new ChoroplethMap();

            // Add layers on the proper order
            choroMap.AddLayer(polygonLayer);
            choroMap.AddLayer(borderLayer);
            choroMap.AddLayer(glyphLayer);
            choroMap.Invalidate();

            renderer.Add(choroMap, mapPanel);
        }

        private void LoadData()
        {
            ExcelReader reader = new ExcelReader();
            reader.GetArrayFromExcel("../../../Data/Data.xls", "2005", out titles, out data);

            // NOTE: don't count with the region attribute
            for (int i = 1; i < titles.GetLength(0); i++)
            {
                stringList.Add(titles[i]);
            }

            object[,] cleanData = new object[data.GetLength(0) - 1, data.GetLength(1)];

            for (int i = 0; i < data.GetLength(1); i++ )
            {
                object o = data[0, i];
                regions.Add( o );
                for(int j = 1; j < data.GetLength(0); j++)
                {
                    cleanData[j - 1, i] = data[j, i];
                }
            }

            dataCube.SetData(cleanData);

            // **** Map Stuff ****
            SetupMapLayers();

            // **** Parallel Coordinates Stuff ****
            ParallelCoordinatesPlot pcPlot = InitializeParallelCoordinatesPlot(pcPanel, 
                dataCube, colorMap);

            polygonLayer.IndexVisibilityHandler = pcPlot.IndexVisibilityHandler;
            glyphLayer.IndexVisibilityHandler = pcPlot.IndexVisibilityHandler;
            pcPlot.FilterChanged += new EventHandler(filterPlot_FilterChanged);
            pcPlot.HeaderClicked += new EventHandler(pcPlot_HeaderClicked);
            
        }

        void pcPlot_HeaderClicked(object sender, EventArgs e)
        {
            int column = ((ParallelCoordinatesPlot)sender).ClickedHeader;
            colorMap.Index = column;
            colorMap.Invalidate();
            choroMap.Invalidate();
        }

        void filterPlot_FilterChanged(object sender, EventArgs e)
        {
            choroMap.Invalidate();
        }

        private ParallelCoordinatesPlot InitializeParallelCoordinatesPlot(Panel panel, 
            IDataCubeProvider<float> data, ColorMap colorMap)
        {
            ParallelCoordinatesPlot filterPlot = new ParallelCoordinatesPlot();

            filterPlot.Input = data;
            filterPlot.Headers = stringList;

            filterPlot.ColorMap = colorMap;

            filterPlot.Enabled = true;

            renderer.Add(filterPlot, panel);

            return filterPlot;
        }

 


    }
}