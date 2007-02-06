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

namespace Lab2 {
    public partial class Form1 : Form {

        // private attributes
        private string[] titles;
        private object[,] data;

        private DataCube dataCube;
        private Renderer renderer;
        //private ParallelCoordinatesPlot downSamplingFilterPlot;
        private DownsamplingFilter downSamplingFilter;
        private KMeansFilter kMeansFilter;

        Panel downSamplingPanel;
        Panel originalDataPanel;
        Panel kMeansPanel;
        
        public Form1() {
            InitializeComponent();

            dataCube = new DataCube();
            renderer = new Renderer(this);
            //colorMap = new ColorMap();
            //InitializeColorMap();

            LoadData();
            
            /************************************************************************/
            /* PASS TO A FUNCTION                                                  */
            /************************************************************************/
            downSamplingFilter = new DownsamplingFilter( 50 );
            downSamplingFilter.Input = dataCube;

            kMeansFilter = new KMeansFilter(2);
            kMeansFilter.Input = dataCube;
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    CreateColorMap
        // FullName:  Lab2.Form1.CreateColorMap
        // Access:    private 
        // Returns:   Gav.Data.ColorMap
        //////////////////////////////////////////////////////////////////////////
        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            //LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 500.0f);
            LinearColorMapPart linearMap = new LinearColorMapPart(Color.Aquamarine, Color.DarkKhaki);
            map.AddColorMapPart(linearMap);
            linearMap.Invalidate();
            map.Invalidate();

            return map;
            //colorMap.Input = dataCube;
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    InitializeParallelCoordinatesPlot
        // FullName:  Lab2.Form1.InitializeParallelCoordinatesPlot
        // Access:    private 
        // Returns:   Gav.Graphics.ParallelCoordinatesPlot
        // Parameter: Panel panel
        // Parameter: IDataCubeProvider<float> filter
        //////////////////////////////////////////////////////////////////////////
        private ParallelCoordinatesPlot InitializeParallelCoordinatesPlot(Panel panel, IDataCubeProvider<float> filter)
        {
            ParallelCoordinatesPlot filterPlot = new ParallelCoordinatesPlot();

            List<string> stringList = new List<string>();
            for (int i = 0; i < titles.GetLength(0); i++)
            {
                stringList.Add(titles[i]);
            }
            filterPlot.Input = filter;
            filterPlot.Headers = stringList;
            
            ColorMap colorMap = CreateColorMap();
            colorMap.Input = filter;
            filterPlot.ColorMap = colorMap;
            
            filterPlot.Enabled = true;

            renderer.Add(filterPlot, panel);
            //filterPlot.Invalidate();

            return filterPlot;
        }

        private void LoadData()
        {
            ExcelReader reader = new ExcelReader();
            reader.GetArrayFromExcel("../../../cars.xls", "Sheet1", out titles, out data);

            dataCube.SetData(data);
        }

        private void Form1_Load(object sender, EventArgs e) {

            // Panels in the split containers. 
            downSamplingPanel = splitContainer1.Panel1;
            originalDataPanel = splitContainer2.Panel1;
            kMeansPanel = splitContainer2.Panel2;
            
            // Write your code here.
            InitializeParallelCoordinatesPlot(originalDataPanel, dataCube);
            ParallelCoordinatesPlot plot = InitializeParallelCoordinatesPlot(downSamplingPanel, 
                downSamplingFilter);

            InitializeParallelCoordinatesPlot(kMeansPanel, kMeansFilter);

            List<float> max, min;
            dataCube.GetData().GetAllColumnMaxMin(out max, out min);
            int columns = dataCube.GetData().Data.GetLength(0);
            for (int i = 0; i < columns; i++)
            {
                plot.SetMin(i, min[i]);
                plot.SetMax(i, max[i]);
            }

            
        }
    }
}