using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Graphics;
using Gav.Data;
using LabExcelReader;

namespace Lab1
{
    public partial class Form1 : Form
    {
        private string [] titles;
        private object[,] data;

        private DataCube dataCube;
        private DataCubeViewer cubeViewer;
        private Renderer renderer;

        private ScatterPlot2D scatterPlot2d;
        private ScatterPlot3D scatterPlot3d;

        private ColorMap colorMap;

        private const int scatterX = 5;
        private const int scatterY = 0;
        private const int scatterZ = 7;

        public Form1()
        {
            InitializeComponent();
            dataCube = new DataCube();
            
            renderer = new Renderer( this );

            colorMap = new ColorMap();

 
        }

        private void InitializeColorMap() 
        { 
            LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 500.0f);
            colorMap.AddColorMapPart(hsvMap);
            colorMap.Input = dataCube;
        }

        private void LoadData() 
        {
            ExcelReader reader = new ExcelReader();
            reader.GetArrayFromExcel( "../../SocialScienceData.xls", "2005", out titles, out data );

            dataCube.SetData(data);

        }

        private void CreateCubeViewer()
        {
            cubeViewer = new DataCubeViewer();

            List<string> stringList = new List<string>();
            for (int i = 0; i < titles.GetLength(0); i++) 
            {
                stringList.Add(titles[i]);
            }
            cubeViewer.Input = dataCube;
            cubeViewer.SetHeaders( stringList );

            renderer.Add(cubeViewer, splitContainer1.Panel2);
            cubeViewer.Invalidate();
        }

        private void CreateScatterPlot2d()
        {
            scatterPlot2d = new ScatterPlot2D();
            
            scatterPlot2d.Input = dataCube;
            scatterPlot2d.Enabled = true;
            scatterPlot2d.AxisIndexX = scatterX;
            scatterPlot2d.AxisIndexY = scatterY;

            scatterPlot2d.AxisXText = titles[scatterX];
            scatterPlot2d.AxisYText = titles[scatterY];


            scatterPlot2d.ColorMap = colorMap;

            scatterPlot2d.SelectedGlyphColor = Color.Black;

            scatterPlot2d.Picked +=new EventHandler<IndexesPickedEventArgs>(PickedScatter);

            renderer.Add(scatterPlot2d, splitContainer2.Panel2);
            scatterPlot2d.Invalidate();
        }

        private void PickedScatter(object sender, IndexesPickedEventArgs e) 
        {
            scatterPlot2d.SetSelectedGlyphs( e.PickedIndexes, false);
            scatterPlot2d.Invalidate();

            scatterPlot3d.SetSelectedGlyphs(e.PickedIndexes, false);
            scatterPlot3d.Invalidate();
        }

        private void CreateScatterPlot3d() 
        {
            scatterPlot3d = new ScatterPlot3D();
            scatterPlot3d.Input = dataCube;
            scatterPlot3d.Enabled = true;

            scatterPlot3d.AxisIndexX = scatterX;
            scatterPlot3d.AxisIndexY = scatterY;
            scatterPlot3d.AxisIndexZ = scatterZ;

            scatterPlot3d.AxisXText = titles[scatterX];
            scatterPlot3d.AxisYText = titles[scatterY];
            scatterPlot3d.AxisZText = titles[scatterZ];

            scatterPlot3d.ColorMap = colorMap;
            scatterPlot3d.SelectedGlyphColor = Color.Black;

            scatterPlot3d.Picked += new EventHandler<IndexesPickedEventArgs>(PickedScatter);
            scatterPlot3d.LightEnabled = true;
            renderer.Add(scatterPlot3d, splitContainer2.Panel1);
            scatterPlot3d.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            InitializeColorMap();

            CreateCubeViewer();
            CreateScatterPlot2d();
            CreateScatterPlot3d();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Run for your life!";
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}