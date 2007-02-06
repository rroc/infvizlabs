using System;
using System.Collections.Generic;
using System.Text;

using Gav.Data;

namespace Lab2 {
    class DownsamplingFilter : Filter<float, float>{

        private int samples;

        public DownsamplingFilter(int aSamples)
        {
            this.samples = aSamples;
        }

        private void GenerateRandomList(List<int> xpto, int M, int range)
        {
            Random randomGen = new Random();
            List<int> bag = new List<int>();

            for (int i = 0; i < range; i++)
            {
                bag.Add(i);
            }

            for (int i = 0; i < M; i++)
            {
                int row = randomGen.Next(bag.Count);
                bag.Remove(row);

                xpto.Add(row);
            }
        }

        protected override void ProcessData() {

            // The filter's input data.
            float[, ,] inputData = _input.GetData().Data;

            int M = samples;
            int sizeY = inputData.GetLength(1);
            int sizeX = inputData.GetLength(0);
            float[, ,] outputData = new float[sizeX, M, 1];

            List<int> randomNumbers = new List<int>();
            GenerateRandomList(randomNumbers, M, sizeY);

            int j = 0;

            // Implement the downsampling algorithm here. You can of course split up your code into several methods.
            foreach( int row in randomNumbers )
            {
                // iterate through the columns
                for (int col = 0; col < sizeX; col++)
                {
                    outputData[col, j, 0] = inputData[col, row, 0];
                }
                
                ++j;
            }

            // The output data should be set to the _dataCube.Data array.
            _dataCube.Data = outputData;
            
        }
   
    }
}
