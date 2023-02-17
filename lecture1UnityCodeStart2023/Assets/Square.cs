using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class Square
    {
        private List<List<Vector3>> _squares = new List<List<Vector3>>();
        private List<List<bool>> _onOff = new List<List<bool>>();
        float _thresh = 0.5f;

        private int low;
        private int high;
        private int size;
        private int spacing;
        private float negate;
        private float adjust;

        public Square(int low, int high, int size, int spacing, float negate, float adjust)
        {
            this.low = low;
            this.high = high;
            this.size = size;
            this.spacing = spacing;
            this.negate = negate;
            this.adjust = adjust;
        }

        /// <summary>
        /// Segments the area into squares with size = size/2
        /// Technically makes squares, takes the center point and makes a square from them
        /// </summary>
        public void segmentSquare()
        {
            for (int y = spacing / 2; y <= size + spacing / 2; y += spacing / 2)
            {
                for (int x = spacing / 2; x <= size + (spacing / 2); x += (spacing / 2))
                {
                    //vec2 vec4
                    //vec1 vec3
                    Vector3 vec1 = new Vector3(((x - low) - negate) / adjust, ((y - low) - negate) / adjust, 0);
                    Vector3 vec2 = new Vector3(((x - low) - negate) / adjust, ((y - high) - negate) / adjust, 0);
                    Vector3 vec3 = new Vector3(((x - high) - negate) / adjust, ((y - low) - negate) / adjust, 0);
                    Vector3 vec4 = new Vector3(((x - high) - negate) / adjust, ((y - high) - negate) / adjust, 0);

                    List<Vector3> arr = new List<Vector3>();
                    arr.Add(vec1);
                    arr.Add(vec2);
                    arr.Add(vec3);
                    arr.Add(vec4);
                    List<bool> bools = new List<bool>();
                    bools.Add(false);
                    bools.Add(false);
                    bools.Add(false);
                    bools.Add(false);
                    _squares.Add(arr);
                    _onOff.Add(bools);
                }
            }
        }

        /// <summary>
        /// Draws lines through the squares
        /// </summary>
        public void march()
        {
            meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            mscript.createMeshGeometry(vertices, indices);

            for (int i = 0; i < _squares.Count; i++)
            {
                float[] values = new float[4];
                int k = 0;
                for (int j = 0; j < _squares[0].Count; j++)
                {
                    _onOff[i][j] = false;
                    values[k] = Helper.getDist(_squares[i][j]);
                    if (values[k] < _thresh)
                    {
                        _onOff[i][j] = true;
                    }

                    k++;
                }

                Vector3 p1 = _squares[i][0];
                Vector3 p2 = _squares[i][1];
                Vector3 p3 = _squares[i][2];
                Vector3 p4 = _squares[i][3];

                float v1 = values[0];
                float v2 = values[1];
                float v3 = values[2];
                float v4 = values[3];

                bool b1 = _onOff[i][0];
                bool b2 = _onOff[i][1];
                bool b3 = _onOff[i][2];
                bool b4 = _onOff[i][3];

                List<Vector3> points = new List<Vector3>();

                //p2 p4
                //p1 p3
                if (b1 != b3)
                {
                    Vector3 vec = Helper.getEnd(p1, p3, v1, v3,_thresh);
                    points.Add(vec);
                }

                if (b1 != b2)
                {
                    Vector3 vec = Helper.getEnd(p1, p2, v1, v2,_thresh);
                    points.Add(vec);
                }

                if (b3 != b4)
                {
                    Vector3 vec = Helper.getEnd(p3, p4, v3, v4,_thresh);
                    points.Add(vec);
                }

                if (b2 != b4)
                {
                    Vector3 vec = Helper.getEnd(p2, p4, v2, v4,_thresh);
                    points.Add(vec);
                }

                if (points.Count() == 2)
                {
                    vertices.Add(points[0]);
                    vertices.Add(points[1]);
                    int vert = vertices.Count;
                    indices.Add(vert - 2);
                    indices.Add(vert - 1);
                }
                else if (points.Count > 2)
                {
                    vertices.Add(points[0]);
                    vertices.Add(points[1]);
                    vertices.Add(points[2]);
                    vertices.Add(points[3]);
                    int vert = vertices.Count;
                    indices.Add(vert - 4);
                    indices.Add(vert - 3);
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