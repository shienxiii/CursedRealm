using ProceduralInterior.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    public static class SpaceGenerator
    {
        public static void CalculateRoomSpaces(Interior interior, int roomIndex)
        {
            if (!interior || roomIndex < 0 || roomIndex >= interior.Rooms.Count) return;

            Room room = interior.Rooms[roomIndex];
            bool[,] grid = ExtractRoomSamples(interior, roomIndex, out int remaining);

            while (remaining > 0)
            {
                int area = GetLargestRectangleFromGrid(grid, out GridSpan rect);

                Debug.Assert(area > 0, "CalculateRoomSpaces() -> GetLargestRectangleFromGrid() returned 0. This should not happen at this stage");

                // set blocks covered by rect to false 
                Vector2Int start = rect.A;
                Vector2Int end = rect.B;

                ArrayOperatorFunctions.MutateBlockOn2DArray(grid, false, start.x, end.x, start.y, end.y);

                remaining -= rect.GetArea();
                rect.A = room.RoomPointToInteriorPoint(rect.A);
                rect.B = room.RoomPointToInteriorPoint(rect.B);
                room.Spaces.Add(rect);
            }

        }

        /// <summary>
        /// Calculate and return the area of the largest rectangle found in the grid
        /// and pass the GridSpan which holds the indexes of making up the rectangle
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="outRect"></param>
        /// <returns></returns>
        public static int GetLargestRectangleFromGrid(bool[,] grid, out GridSpan outRect)
        {
            outRect = default;
            int maxArea = 0;

            if (grid == null || grid.Length == 0) return maxArea;
            int maxX = grid.GetLength(0);
            int maxY = grid.GetLength(1);

            // Evaluating grid using a combination of histogram and Monotonic Stack Algorithm
            int[] hist = new int[maxX];

            // Parsing from Y to X axis to make it easier to mentally visualize
            for (int y = 0; y < maxY; y++)
            {
                // update the histogram on every height(Y)
                for (int x = 0; x < maxX; x++)
                    hist[x] = grid[x, y] ? hist[x] + 1 : 0;

                // evaluate the histogram using Monotonic Stack Algorithm
                Vector2Int rectDim = CalculateLargestRectangleOfHistogram(hist, out int startX);

                if (rectDim.x <= 0 || rectDim.y <= 0)
                    continue;

                // find the starting and end point on the rectangle
                Vector2Int rectStart = new Vector2Int(startX, y - rectDim.y + 1);
                Vector2Int rectEnd = new Vector2Int(startX + rectDim.x - 1, y);

                GridSpan rect = new GridSpan(rectStart, rectEnd);
                int area = rect.GetArea();

                if (area > maxArea)
                {
                    maxArea = area;
                    outRect = rect;
                }
            }

            return maxArea;
        }

        /// <summary>
        /// Takes in and finds the largest rectangle in a histogram as well as the starting index
        /// </summary>
        /// <param name="histogram">the histogram to be evaluated</param>
        /// <param name="startIndex">starting index of the largest rectangle</param>
        /// <returns></returns>
        public static Vector2Int CalculateLargestRectangleOfHistogram(int[] histogram, out int startIndex)
        {
            startIndex = 0;
            if (histogram == null || histogram.Length == 0)
            {
                Debug.Log(string.Format($"check result {histogram == null} {histogram.Length == 0}"));
                return Vector2Int.zero;
            }

            Stack<int> stack = new Stack<int>();
            int maxArea = 0;
            int maxWidth = 0;
            int maxHeight = 0;

            int count = histogram.Length;

            // Iterate through all bars plus an extra virtual boundary (height 0) 
            // to flush out remaining elements in the stack at the end.
            for (int i = 0; i <= count; i++)
            {
                // The height of the current bar (0 if we reached the virtual end)
                int currentHeight = (i == count) ? 0 : histogram[i];

                while (stack.Count > 0 && currentHeight <= histogram[stack.Peek()])
                {
                    int height = histogram[stack.Pop()];
                    int width = stack.Count == 0 ? i : i - stack.Peek() - 1;
                    int area = height * width;

                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxWidth = width;
                        maxHeight = height;
                        startIndex = stack.Count == 0 ? 0 : stack.Peek() + 1;
                    }
                }

                if (i < count)
                    stack.Push(i);

            }

            Vector2Int dimension = new Vector2Int(maxWidth, maxHeight);

            return dimension;
        }

        /// <summary>
        /// Extract a chunk of sample with a specified dimension from an Interior based on
        /// a start point and return them as a 2d array of bool where any samples with that
        /// matches the given roomIndex will be marked true on the array
        /// </summary>
        /// <param name="interior">Interior instance to extract from</param>
        /// <param name="roomIndex">the room index to search for</param>
        /// <param name="matchCount">The dimension of the samples to extract</param>
        /// <returns></returns>
        public static bool[,] ExtractRoomSamples(Interior interior, int roomIndex, out int matchCount)
        {
            matchCount = 0;
            if (interior == null || roomIndex >= interior.Rooms.Count) return null;

            Room room = interior.Rooms[roomIndex];


            // get the grid from interior
            Sample[,] grid = interior.Grid;
            int maxX = grid.GetLength(0);
            int maxY = grid.GetLength(1);

            // create the 2d array of the grid to be extracted
            Vector2Int start = room.Start;
            Vector2Int dim = room.Dimension;
            bool[,] samples = new bool[dim.x, dim.y];

            for (int u = 0; u < dim.x; u++)
            {
                int x = start.x + u;

                if (x >= maxX) break;

                for (int v = 0; v < dim.y; v++)
                {
                    int y = start.y + v;

                    if (y >= maxY) break;

                    if (grid[x, y] != null && grid[x, y].RoomIndex == roomIndex)
                    {
                        samples[u, v] = true;
                        matchCount++;
                    }
                }
            }


            return samples;
        }
    }
}