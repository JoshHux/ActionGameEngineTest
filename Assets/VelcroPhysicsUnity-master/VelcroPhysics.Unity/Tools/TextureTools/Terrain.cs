﻿using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.PolygonManipulation;
using VelcroPhysics.Tools.Triangulation.TriangulationBase;

namespace VelcroPhysics.Tools.TextureTools
{
    /// <summary>
    /// Simple class to maintain a VTerrain. It can keep track
    /// </summary>
    public class VTerrain
    {
        /// <summary>
        /// Generated bodies.
        /// </summary>
        private List<Body>[,] _bodyMap;

        private AABB _dirtyArea;
        private float _localHeight;

        private float _localWidth;

        /// <summary>
        /// Point cloud defining the VTerrain.
        /// </summary>
        private sbyte[,] _VTerrainMap;

        private Vector2 _topLeft;
        private int _xnum;
        private int _ynum;

        /// <summary>
        /// Points per cell.
        /// </summary>
        public int CellSize;

        /// <summary>
        /// Center of VTerrain in world units.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Decomposer to use when regenerating VTerrain. Can be changed on the fly without consequence.
        /// Note: Some decomposerers are unstable.
        /// </summary>
        public TriangulationAlgorithm Decomposer;

        /// <summary>
        /// Height of VTerrain in world units.
        /// </summary>
        public float Height;

        /// <summary>
        /// Number of iterations to perform in the Marching Squares algorithm.
        /// Note: More then 3 has almost no effect on quality.
        /// </summary>
        public int Iterations = 2;

        /// <summary>
        /// Points per each world unit used to define the VTerrain in the point cloud.
        /// </summary>
        public int PointsPerUnit;

        /// <summary>
        /// Points per sub cell.
        /// </summary>
        public int SubCellSize;

        /// <summary>
        /// Width of VTerrain in world units.
        /// </summary>
        public float Width;

        /// <summary>
        /// World to manage VTerrain in.
        /// </summary>
        public World World;

        /// <summary>
        /// Creates a new VTerrain.
        /// </summary>
        /// <param name="world">The World</param>
        /// <param name="area">The area of the VTerrain.</param>
        public VTerrain(World world, AABB area)
        {
            World = world;
            Width = area.Width;
            Height = area.Height;
            Center = area.Center;
        }

        /// <summary>
        /// Creates a new VTerrain
        /// </summary>
        /// <param name="world">The World</param>
        /// <param name="position">The position (center) of the VTerrain.</param>
        /// <param name="width">The width of the VTerrain.</param>
        /// <param name="height">The height of the VTerrain.</param>
        public VTerrain(World world, Vector2 position, float width, float height)
        {
            World = world;
            Width = width;
            Height = height;
            Center = position;
        }

        /// <summary>
        /// Initialize the VTerrain for use.
        /// </summary>
        public void Initialize()
        {
            // find top left of VTerrain in world space
            _topLeft = new Vector2(Center.x - Width * 0.5f, Center.y - -Height * 0.5f);

            // convert the VTerrains size to a point cloud size
            _localWidth = Width * PointsPerUnit;
            _localHeight = Height * PointsPerUnit;

            _VTerrainMap = new sbyte[(int) _localWidth + 1, (int) _localHeight + 1];

            for (var x = 0; x < _localWidth; x++)
            for (var y = 0; y < _localHeight; y++)
                _VTerrainMap[x, y] = 1;

            _xnum = (int) (_localWidth / CellSize);
            _ynum = (int) (_localHeight / CellSize);
            _bodyMap = new List<Body>[_xnum, _ynum];

            // make sure to mark the dirty area to an infinitely small box
            _dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue),
                new Vector2(float.MinValue, float.MinValue));
        }

        /// <summary>
        /// Apply the specified texture data to the VTerrain.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public void ApplyData(sbyte[,] data, Vector2 offset = default(Vector2))
        {
            for (var x = 0; x < data.GetUpperBound(0); x++)
            for (var y = 0; y < data.GetUpperBound(1); y++)
                if (x + offset.x >= 0 && x + offset.x < _localWidth && y + offset.y >= 0 && y + offset.y < _localHeight)
                    _VTerrainMap[(int) (x + offset.x), (int) (y + offset.y)] = data[x, y];

            RemoveOldData(0, _xnum, 0, _ynum);
        }

        /// <summary>
        /// Modify a single point in the VTerrain.
        /// </summary>
        /// <param name="location">World location to modify. Automatically clipped.</param>
        /// <param name="value">-1 = inside VTerrain, 1 = outside VTerrain</param>
        public void ModifyVTerrain(Vector2 location, sbyte value)
        {
            // find local position
            // make position local to map space
            var p = location - _topLeft;

            // find map position for each axis
            p.x = p.x * _localWidth / Width;
            p.y = p.y * -_localHeight / Height;

            if (p.x >= 0 && p.x < _localWidth && p.y >= 0 && p.y < _localHeight)
            {
                _VTerrainMap[(int) p.x, (int) p.y] = value;

                // expand dirty area
                if (p.x < _dirtyArea.LowerBound.x)
                    _dirtyArea.LowerBound.x = p.x;
                if (p.x > _dirtyArea.UpperBound.x)
                    _dirtyArea.UpperBound.x = p.x;

                if (p.y < _dirtyArea.LowerBound.y)
                    _dirtyArea.LowerBound.y = p.y;
                if (p.y > _dirtyArea.UpperBound.y)
                    _dirtyArea.UpperBound.y = p.y;
            }
        }

        /// <summary>
        /// Regenerate the VTerrain.
        /// </summary>
        public void RegenerateVTerrain()
        {
            //iterate effected cells
            var xStart = (int) (_dirtyArea.LowerBound.x / CellSize);
            if (xStart < 0)
                xStart = 0;

            var xEnd = (int) (_dirtyArea.UpperBound.x / CellSize) + 1;
            if (xEnd > _xnum)
                xEnd = _xnum;

            var yStart = (int) (_dirtyArea.LowerBound.y / CellSize);
            if (yStart < 0)
                yStart = 0;

            var yEnd = (int) (_dirtyArea.UpperBound.y / CellSize) + 1;
            if (yEnd > _ynum)
                yEnd = _ynum;

            RemoveOldData(xStart, xEnd, yStart, yEnd);

            _dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue),
                new Vector2(float.MinValue, float.MinValue));
        }

        private void RemoveOldData(int xStart, int xEnd, int yStart, int yEnd)
        {
            for (var x = xStart; x < xEnd; x++)
            for (var y = yStart; y < yEnd; y++)
            {
                //remove old VTerrain object at grid cell
                if (_bodyMap[x, y] != null)
                    for (var i = 0; i < _bodyMap[x, y].Count; i++)
                        World.RemoveBody(_bodyMap[x, y][i]);

                _bodyMap[x, y] = null;

                //generate new one
                GenerateVTerrain(x, y);
            }
        }

        private void GenerateVTerrain(int gx, int gy)
        {
            float ax = gx * CellSize;
            float ay = gy * CellSize;

            var polys = MarchingSquares.DetectSquares(
                new AABB(new Vector2(ax, ay), new Vector2(ax + CellSize, ay + CellSize)), SubCellSize, SubCellSize,
                _VTerrainMap, Iterations, true);
            if (polys.Count == 0)
                return;

            _bodyMap[gx, gy] = new List<Body>();

            // create the scale vector
            var scale = new Vector2(1f / PointsPerUnit, 1f / -PointsPerUnit);

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                // does this need to be negative?
                item.Scale(ref scale);
                item.Translate(ref _topLeft);
                var simplified = SimplifyTools.CollinearSimplify(item);

                var decompPolys = Triangulate.ConvexPartition(simplified, Decomposer);

                foreach (var poly in decompPolys)
                    if (poly.Count > 2)
                        _bodyMap[gx, gy].Add(BodyFactory.CreatePolygon(World, poly, 1));
            }
        }
    }
}