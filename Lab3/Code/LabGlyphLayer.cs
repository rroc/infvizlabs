using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Gav.Data;
using Gav.Graphics;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace Lab3 {
    public class LabGlyphLayer : MapGlyphLayer {

        private CustomVertex.PositionOnly[] _verts;
        private CustomVertex.PositionOnly[] _starVertices;
        private CustomVertex.PositionOnly[] _boxVertices;

        private bool _inited;
        private List<AxisMap> _axisMaps;

        private IDataCubeProvider<float> _input;

        private ColorMap colorMap;

        public IDataCubeProvider<float> Input {
            get { return _input; }
            set { _input = value; }
        }

        private IndexVisibilityHandler _visibilityHandler;

        public IndexVisibilityHandler IndexVisibilityHandler
        {
            get { return _visibilityHandler; }
            set { _visibilityHandler = value; }
        }
	

        private void CreateBoxVerts()
        {
            // Start points
            _boxVertices = new CustomVertex.PositionOnly[5];

            _boxVertices[0].Position = new Vector3(-1f, 1f, 0);
            _boxVertices[1].Position = new Vector3(1f, 1f, 0);
            _boxVertices[2].Position = new Vector3(1f, -1f, 0);
            _boxVertices[3].Position = new Vector3(-1f, -1f, 0);
            _boxVertices[4].Position = new Vector3(-1f, 1f, 0);
        }

        private void CreateStartVerts(int r)
        {
            // Start points
            _starVertices = new CustomVertex.PositionOnly[6];

            _starVertices[0].Position = new Vector3(0, 0, 0);
            _starVertices[1].Position = new Vector3(0, _axisMaps[0].MappedValues[r], 0);
            _starVertices[2].Position = new Vector3(_axisMaps[1].MappedValues[r], 0, 0);
            _starVertices[3].Position = new Vector3(0, -_axisMaps[2].MappedValues[r], 0);
            _starVertices[4].Position = new Vector3(-_axisMaps[3].MappedValues[r], 0, 0);
            _starVertices[5].Position = new Vector3(0, _axisMaps[0].MappedValues[r], 0);

            //_starVertices[0].Position = new Vector3(0, 0.5f, 0);
            //_starVertices[1].Position = new Vector3(0.5f, 0, 0);
            //_starVertices[2].Position = new Vector3(0, -0.5f, 0);
            //_starVertices[3].Position = new Vector3(-0.5f, 0, 0);
            //_starVertices[4].Position = new Vector3(0, 0.5f, 0);
        }

        // Creates the verts (positions) used for the two glyph triangles. 
        private void CreateVerts(Microsoft.DirectX.Direct3D.Device device) {

            // Creates the verts array. Two triangles is stored in the array, three positions per triangle. 
            _verts = new CustomVertex.PositionOnly[6];

            //_verts[0].Position = new Vector3(-0.5f, 0.5f, 0);
            //_verts[1].Position = new Vector3(0.5f, 0.5f, 0);
            //_verts[2].Position = new Vector3(-0.5f, -0.5f, 0);
            //_verts[3].Position = new Vector3(-0.5f, -0.5f, 0);
            //_verts[4].Position = new Vector3(0.5f, 0.5f, 0);
            //_verts[5].Position = new Vector3(0.5f, -0.5f, 0);
        }

        // This method is called everytime the map is rendered. 
        protected override void InternalRender() {

            // If the input is null we cannot render.
            if (_input == null) {
                return;
            }

            // If the glyph is not inited, call InternalInit. 
            if (!_inited) {
                InternalInit(_device);
                if (!_inited) {
                    return;
                }
            }

            _device.RenderState.CullMode = Cull.None;

            Color c = Color.Black;

            // Create material to enable glyph coloring. 
            Material _material = new Material();
            _material.Emissive = c;
            _material.Diffuse = c;

            // Tells the device ("graphics card") to use the created material.
            _device.Material = _material;

            // Get the mapped values from the first axis map. This axis map is connected to the first column in the data cube.
            // The index corresponds to the column in the data cube.
            float[] mappedValues = _axisMaps[0].MappedValues;

            // Tells the device that the next primitives to draw are of type CustomVertex.PositionOnly.
            _device.VertexFormat = CustomVertex.PositionOnly.Format;

            // Loops through the regions in the map.
            for (int i = 0; i < _input.GetData().GetAxisLength(Axis.Y); i++) {

                // Resets the world transform.  
                _device.Transform.World = _layerWorldMatrix;

                // Scales the object according to the mapped values. 
                //_device.Transform.World *= Matrix.Scaling(new Vector3(1f + mappedValues[i] * 4.0f, 1f + mappedValues[i] * 4.0f, 1));
                //_device.Transform.World *= Matrix.Scaling(new Vector3(3, 3, 1));

                // If a glyph positioner (a class that moves the glyphs to the correct position) is set, use it. 
                if (ActiveGlyphPositioner != null) {
                    //Gets the position for the glyph with index i.
                    Vector2 pos = ActiveGlyphPositioner.GetPosition(i);
                    // Translates the world transform. 
                    _device.Transform.World *= Matrix.Translation(
                        pos.X,
                        pos.Y,
                        0
                        );
                }

                CreateStartVerts( i );

                // Render one glyph. 
                //_device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, _verts);
                c = colorMap.GetColor(i);
                _material.Emissive = c;
                _material.Diffuse = c;

                _device.Material = _material;

                if (IndexVisibilityHandler.GetVisibility(i))
                {
                    _device.DrawUserPrimitives(PrimitiveType.TriangleFan, 4, _starVertices);
                    _device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, _boxVertices);
                }
            }
        }

        // This method is called once when the glyph is inited. 
        protected override void InternalInit(Device device) {

            if (_input == null) {
                return;
            }

            CreateAxisMaps();
            CreateVerts(device);
            CreateBoxVerts();

            colorMap = CreateColorMap();
            colorMap.Input = _input.GetData();
            colorMap.Index = 4;

            _inited = true;
        }

        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            LinearColorMapPart linearMap = new LinearColorMapPart(Color.Black, Color.White);
            map.AddColorMapPart(linearMap);
            linearMap.Invalidate();
            map.Invalidate();

            return map;
        }



        // Creates one axismap per column in the data set. 
        private void CreateAxisMaps() {
            _axisMaps = new List<AxisMap>();

            for (int i = 0; i < _input.GetData().GetAxisLength(Axis.X); i++) {
                AxisMap axisMap = new AxisMap();
                axisMap.Input = _input;
                axisMap.Index = i;
                axisMap.DoMapping();
                _axisMaps.Add(axisMap);
            }
        }

        protected override void InternalInvalidate() { }
    }
}