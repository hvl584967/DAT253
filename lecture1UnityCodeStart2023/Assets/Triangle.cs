using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class Triangle
    {
        private List<List<Vector3>> _triangles = new List<List<Vector3>>();
        private List<List<bool>> _onOff2 = new List<List<bool>>();
        float _thresh = 0.5f;

        private int size;
        private int spacing;
        private float negate;
        private float adjust;

        public Triangle(int size, int spacing, float negate, float adjust)
        {
            this.size = size;
            this.spacing = spacing;
            this.negate = negate;
            this.adjust = adjust;
        }
        
        /// <summary>
        /// Segments the area into triangles with sides = size
        /// </summary>
        public void segmentTriangle()
        {
            float calcY = (spacing * Mathf.Sqrt(3)) / 2;
            float flip = 0f;

            for (float y = (spacing * Mathf.Sqrt(3)) / 2; y <= size + spacing / 4; y += (spacing * Mathf.Sqrt(3)) / 2)
            {
                for (int x = spacing / 2; x <= size + (spacing / 2); x += (spacing))
                {
                    List<Vector3> tri = new List<Vector3>();
                    List<Vector3> tri2 = new List<Vector3>();

                    //    vec7    vec10
                    //
                    //vec5   vec6
                    Vector3 vec5 = new Vector3(((x - spacing) - negate) / adjust,
                        ((y - calcY + flip) - negate) / adjust, 0);
                    Vector3 vec6 = new Vector3((x - negate) / adjust, ((y - calcY + flip) - negate) / adjust, 0);
                    Vector3 vec7 = new Vector3(((((x - spacing) + x) / 2f) - negate) / adjust,
                        (y - flip - negate) / adjust, 0);

                    tri.Add(vec5);
                    tri.Add(vec6);
                    tri.Add(vec7);

                    if ((x + spacing / 2) < size + (spacing / 2))
                    {
                        Vector3 vec10 = new Vector3(((x + spacing / 2) - negate) / adjust, (y - flip - negate) / adjust,
                            0);
                        tri2.Add(vec7);
                        tri2.Add(vec6);
                        tri2.Add(vec10);
                        _triangles.Add(tri2);
                    }

                    _triangles.Add(tri);

                    List<bool> b = new List<bool>();
                    b.Add(false);
                    b.Add(false);
                    b.Add(false);
                    List<bool> b2 = new List<bool>();
                    b2.Add(false);
                    b2.Add(false);
                    b2.Add(false);
                    _onOff2.Add(b);
                    _onOff2.Add(b2);
                }

                if (flip == 0)
                {
                    flip = calcY;
                }
                else
                {
                    flip = 0;
                }
            }
        }

        /// <summary>
        /// Draws lines through the triangles
        /// </summary>
        public void march()
        {
            meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            mscript.createMeshGeometry(vertices, indices);

            for (int i = 0; i < _triangles.Count; i++)
            {
                float[] values = new float[3];
                int k = 0;
                for (int j = 0; j < _triangles[0].Count; j++)
                {
                    _onOff2[i][j] = false;
                    values[k] = Helper.getDist(_triangles[i][j]);
                    if (values[k] < _thresh)
                    {
                        _onOff2[i][j] = true;
                    }

                    k++;
                }

                Vector3 p1 = _triangles[i][0];
                Vector3 p2 = _triangles[i][1];
                Vector3 p3 = _triangles[i][2];

                float v1 = values[0];
                float v2 = values[1];
                float v3 = values[2];

                bool b1 = _onOff2[i][0];
                bool b2 = _onOff2[i][1];
                bool b3 = _onOff2[i][2];

                List<Vector3> points = new List<Vector3>();

                //  p3
                //p1 p2
                if (b1 != b2)
                {
                    Vector3 vec = Helper.getEnd(p1, p2, v1, v2,_thresh);
                    points.Add(vec);
                }

                if (b1 != b3)
                {
                    Vector3 vec = Helper.getEnd(p1, p3, v1, v3,_thresh);
                    points.Add(vec);
                }

                if (b2 != b3)
                {
                    Vector3 vec = Helper.getEnd(p2, p3, v2, v3,_thresh);
                    points.Add(vec);
                }

                if (points.Count != 0)
                {
                    vertices.Add(points[0]);
                    vertices.Add(points[1]);
                    int vert = vertices.Count;
                    indices.Add(vert - 2);
                    indices.Add(vert - 1);
                }
            }

            mscript.createMeshGeometry(vertices, indices);
        }

        public void setThresh(float val)
        {
            _thresh = val;
        }
    }
}