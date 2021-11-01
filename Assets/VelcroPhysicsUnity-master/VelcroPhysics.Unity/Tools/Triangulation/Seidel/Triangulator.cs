﻿using System;
using System.Collections.Generic;

namespace VelcroPhysics.Tools.Triangulation.Seidel
{
    internal class Triangulator
    {
        // Initialize trapezoidal map and query structure
        private Trapezoid _boundingBox;

        private List<Edge> _edgeList;
        private QueryGraph _queryGraph;
        private float _sheer = 0.001f;
        private TrapezoidalMap _trapezoidalMap;

        private List<MonotoneMountain> _xMonoPoly;

        // Trapezoid decomposition list
        public List<Trapezoid> Trapezoids;

        public List<List<Point>> Triangles;

        public Triangulator(List<Point> polyLine, float sheer)
        {
            _sheer = sheer;
            Triangles = new List<List<Point>>();
            Trapezoids = new List<Trapezoid>();
            _xMonoPoly = new List<MonotoneMountain>();
            _edgeList = InitEdges(polyLine);
            _trapezoidalMap = new TrapezoidalMap();
            _boundingBox = _trapezoidalMap.BoundingBox(_edgeList);
            _queryGraph = new QueryGraph(Sink.Isink(_boundingBox));

            Process();
        }

        // Build the trapezoidal map and query graph
        private void Process()
        {
            foreach (var edge in _edgeList)
            {
                var traps = _queryGraph.FollowEdge(edge);

                // Remove trapezoids from trapezoidal Map
                foreach (var t in traps)
                {
                    _trapezoidalMap.Map.Remove(t);

                    var cp = t.Contains(edge.P);
                    var cq = t.Contains(edge.Q);
                    Trapezoid[] tList;

                    if (cp && cq)
                    {
                        tList = _trapezoidalMap.Case1(t, edge);
                        _queryGraph.Case1(t.Sink, edge, tList);
                    }
                    else if (cp && !cq)
                    {
                        tList = _trapezoidalMap.Case2(t, edge);
                        _queryGraph.Case2(t.Sink, edge, tList);
                    }
                    else if (!cp && !cq)
                    {
                        tList = _trapezoidalMap.Case3(t, edge);
                        _queryGraph.Case3(t.Sink, edge, tList);
                    }
                    else
                    {
                        tList = _trapezoidalMap.Case4(t, edge);
                        _queryGraph.Case4(t.Sink, edge, tList);
                    }

                    // Add new trapezoids to map
                    foreach (var y in tList) _trapezoidalMap.Map.Add(y);
                }

                _trapezoidalMap.Clear();
            }

            // Mark outside trapezoids
            foreach (var t in _trapezoidalMap.Map) MarkOutside(t);

            // Collect interior trapezoids
            foreach (var t in _trapezoidalMap.Map)
                if (t.Inside)
                {
                    Trapezoids.Add(t);
                    t.AddPoints();
                }

            // Generate the triangles
            CreateMountains();
        }

        // Build a list of x-monotone mountains
        private void CreateMountains()
        {
            foreach (var edge in _edgeList)
                if (edge.MPoints.Count > 2)
                {
                    var mountain = new MonotoneMountain();

                    // Sorting is a perfromance hit. Literature says this can be accomplised in
                    // linear time, although I don't see a way around using traditional methods
                    // when using a randomized incremental algorithm

                    // Insertion sort is one of the fastest algorithms for sorting arrays containing 
                    // fewer than ten elements, or for lists that are already mostly sorted.

                    var points = new List<Point>(edge.MPoints);
                    points.Sort((p1, p2) => p1.X.CompareTo(p2.X));

                    foreach (var p in points)
                        mountain.Add(p);

                    // Triangulate monotone mountain
                    mountain.Process();

                    // Extract the triangles into a single list
                    foreach (var t in mountain.Triangles) Triangles.Add(t);

                    _xMonoPoly.Add(mountain);
                }
        }

        // Mark the outside trapezoids surrounding the polygon
        private void MarkOutside(Trapezoid t)
        {
            if (t.Top == _boundingBox.Top || t.Bottom == _boundingBox.Bottom)
                t.TrimNeighbors();
        }

        // Create segments and connect end points; update edge event pointer
        private List<Edge> InitEdges(List<Point> points)
        {
            var edges = new List<Edge>();

            for (var i = 0; i < points.Count - 1; i++) edges.Add(new Edge(points[i], points[i + 1]));
            edges.Add(new Edge(points[0], points[points.Count - 1]));
            return OrderSegments(edges);
        }

        private List<Edge> OrderSegments(List<Edge> edgeInput)
        {
            // Ignore vertical segments!
            var edges = new List<Edge>();

            foreach (var e in edgeInput)
            {
                var p = ShearVTransform(e.P);
                var q = ShearVTransform(e.Q);

                // Point p must be to the left of point q
                if (p.X > q.X)
                    edges.Add(new Edge(q, p));
                else if (p.X < q.X) edges.Add(new Edge(p, q));
            }

            // Randomized triangulation improves performance
            // See Seidel's paper, or O'Rourke's book, p. 57 
            Shuffle(edges);
            return edges;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Prevents any two distinct endpoints from lying on a common vertical line, and avoiding
        // the degenerate case. See Mark de Berg et al, Chapter 6.3
        private Point ShearVTransform(Point point)
        {
            return new Point(point.X + _sheer * point.Y, point.Y);
        }
    }
}