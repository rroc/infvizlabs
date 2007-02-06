using System;
using System.Collections.Generic;
using System.Text;

using Gav.Data;

namespace Lab2 {
    class KMeansFilter : Filter<float, float> {

        private List<List<int>> clusters;
        private List<List<float>> clusterMeans;
        private int nClusters;
        private bool randomClustersReady;

        public KMeansFilter(int aNClusters)
        {
            clusters = new List<List<int>>();
            clusterMeans = new List<List<float>>();
            for (int i = 0; i < aNClusters; i++)
            {
                clusters.Add( new List<int>() );
                clusterMeans.Add(new List<float>());
            }

            

            nClusters = aNClusters;
            randomClustersReady = false;
        }

        private void GenerateRandomList(int totalElements)
        {
            int samplesPerCluster = (int)Math.Ceiling((double)totalElements / (double)nClusters);
            Random randomGen = new Random();
            List<int> bag = new List<int>();

            for (int i = 0; i < totalElements; i++)
            {
                bag.Add(i);
            }

            int currentList = 0;
            while( bag.Count != 0 )
            {
                int rand = randomGen.Next(bag.Count);
                int row = bag[rand];
                bag.Remove(row);
               
                clusters[currentList].Add( row );

                if ( clusters[currentList].Count >= samplesPerCluster )
                {
                    currentList++;
                }
            }

            randomClustersReady = true;
        }

        private void NormalizeData(float[, ,] normalizedData)
        {
            List<float> max, min;
            _input.GetData().GetAllColumnMaxMin(out max, out min);
            int sizeY = _input.GetData().Data.GetLength(1);
            int sizeX = _input.GetData().Data.GetLength(0);

            for (int i = 0; i < sizeX; i++)
            {
                // iterate through the columns
                for (int j = 0; j < sizeY; j++)
                {
                    normalizedData[i, j, 0] = (_input.GetData().Data[i, j, 0] - min[i]) / (max[i] - min[i]);
                }
            }
        }

        private float DistanceItemMean(List<float> mean, int rowNumber, float[, ,] rows)
        {
            float sum = 0.0f;
            int col = 0;
            foreach (float value in mean)
            {
                float columnValue = rows[col++, rowNumber, 0];
                float element = value - columnValue;
                sum += element * element;
            }

            return sum;
        }

        protected override void ProcessData() {

            // The filter's input data.
            float[, ,] inputData = _input.GetData().Data;

            int columnCount = _input.GetData().Data.GetLength(0);
            int rowCount = _input.GetData().Data.GetLength(1);

            float[, ,] normalizedData = new float[columnCount, rowCount, 1];
            NormalizeData(normalizedData);

            // Implement the KMeans clustering algorithm here. You can of course split up your code into several methods.
            if( ! randomClustersReady )
            {
                GenerateRandomList(rowCount);

                int clusterCount = 0;
                
                // Generate first means
                foreach (List<int> cluster in clusters)
                {
                    for (int i = 0; i < columnCount; i++ )
                    {
                        float columnMean = GetColumnMean(cluster, normalizedData, i);

                        // for the current cluster, add the mean for that column
                        clusterMeans[clusterCount].Add(columnMean);
                        //sum += columnSum;
                    }
                    clusterCount++;
                }
            }

            // The output data should be set to the _dataCube.Data array.

        }

        private float GetColumnMean(List<int> cluster, float[, ,] inputData, int column)
        {
            float sum = 0.0f;
            foreach (int row in cluster)
            {
                float value = inputData[column, row, 0];
                sum += value;
            }

            return sum / cluster.Count;

            //int len = inputData.GetLength( 1 );
            //float sum = 0.0f;
            //for (int i = 0; i < len; i++)
            //{
            //    sum += inputData[column, i, 0];
            //}

            //return sum / len;
        }

    }
}
