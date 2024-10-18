using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace NN
{
    public class YOLOv8OutputReader
    {
        public static float DiscardThreshold = 0.1f;
        protected const int ClassesNum = 20; // Sesuaikan dengan jumlah kelas baru
        const int InputWidth = 640;
        const int InputHeight = 640;

        public IEnumerable<ResultBox> ReadOutput(Tensor output)
        {
            Debug.Log($"Output Tensor dimensions: {output.shape}"); // Log untuk melihat dimensi tensor

            List<ResultBox> resultBoxes = new List<ResultBox>();

            try
            {
                float[,] array = ReadOutputToArray(output);
                if (array != null)
                {
                    Debug.Log($"Converted array dimensions: {array.GetLength(0)} x {array.GetLength(1)}");
                    resultBoxes.AddRange(ReadBoxes(array));
                }
                else
                {
                    Debug.LogError("Error in ReadOutput: Array conversion failed, resulting array is null.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ReadOutput: {ex.Message}");
            }

            return resultBoxes;
        }

        private float[,] ReadOutputToArray(Tensor output)
        {
            int featuresPerBox = ClassesNum + 4; // 20 classes + 4 untuk koordinat box
            int BoxesPerCell = output.flatWidth / featuresPerBox;

            Debug.Log($"Calculated BoxesPerCell: {BoxesPerCell}, features per box: {featuresPerBox}");

            // Pastikan bahwa dimensi tensor sesuai sebelum melakukan reshaping
            if (output.flatWidth % featuresPerBox != 0)
            {
                Debug.LogError($"Mismatch in tensor dimensions. Total elements ({output.flatWidth}) is not divisible by features per box ({featuresPerBox}). Adjust 'ClassesNum' or check model output.");
                throw new InvalidOperationException("Tensor dimensions do not match expected output.");
            }

            // Buat array dengan ukuran yang sesuai
            float[,] array = new float[BoxesPerCell, featuresPerBox];
            var data = output.AsFloats();

            Debug.Log($"Tensor data length: {data.Length}");

            // Salin data dari tensor ke array
            for (int i = 0; i < BoxesPerCell; i++)
            {
                for (int j = 0; j < featuresPerBox; j++)
                {
                    int index = i * featuresPerBox + j;
                    if (index < data.Length)
                    {
                        array[i, j] = data[index];
                    }
                    else
                    {
                        Debug.LogError($"Index {index} out of bounds for data length {data.Length}");
                        return null;
                    }
                }
            }

            return array;
        }

        private IEnumerable<ResultBox> ReadBoxes(float[,] array)
        {
            List<ResultBox> resultBoxes = new List<ResultBox>();

            int boxes = array.GetLength(0);
            int features = array.GetLength(1);
            Debug.Log($"Reading boxes: {boxes} boxes with {features} features each");

            for (int box_index = 0; box_index < boxes; box_index++)
            {
                try
                {
                    ResultBox box = ReadBox(array, box_index);
                    if (box != null)
                        resultBoxes.Add(box);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in ReadBox at index {box_index}: {ex.Message}");
                }
            }

            return resultBoxes;
        }

        protected virtual ResultBox ReadBox(float[,] array, int box)
        {
            int features = array.GetLength(1);
            if (box < 0 || box >= array.GetLength(0))
            {
                Debug.LogError($"Invalid box index: {box}. Array length is {array.GetLength(0)}.");
                return null;
            }

            if (features < ClassesNum + 4)
            {
                Debug.LogError($"Not enough features in array for decoding. Expected at least {ClassesNum + 4}, but got {features}.");
                return null;
            }

            (int highestClassIndex, float highestScore) = DecodeBestBoxIndexAndScore(array, box);

            if (highestScore < DiscardThreshold)
                return null;

            Rect box_rect = DecodeBoxRectangle(array, box);

            return new ResultBox(
                rect: box_rect,
                score: highestScore,
                bestClassIndex: highestClassIndex);
        }

        private (int, float) DecodeBestBoxIndexAndScore(float[,] array, int box)
        {
            const int classesOffset = 4;

            int highestClassIndex = 0;
            float highestScore = 0;

            for (int i = 0; i < ClassesNum; i++)
            {
                float currentClassScore = array[box, i + classesOffset];
                if (currentClassScore > highestScore)
                {
                    highestScore = currentClassScore;
                    highestClassIndex = i;
                }
            }

            return (highestClassIndex, highestScore);
        }

        private Rect DecodeBoxRectangle(float[,] data, int box)
        {
            const int boxCenterXIndex = 0;
            const int boxCenterYIndex = 1;
            const int boxWidthIndex = 2;
            const int boxHeightIndex = 3;

            float centerX = data[box, boxCenterXIndex];
            float centerY = data[box, boxCenterYIndex];
            float width = data[box, boxWidthIndex];
            float height = data[box, boxHeightIndex];

            float xMin = Mathf.Max(0, centerX - width / 2);
            float yMin = Mathf.Max(0, centerY - height / 2);
            var rect = new Rect(xMin, yMin, width, height);

            rect.xMax = Mathf.Min(rect.xMax, InputWidth);
            rect.yMax = Mathf.Min(rect.yMax, InputHeight);

            return rect;
        }
    }
}
