using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public class Tetraeder
    {
        private Material material;
        private Material materialBack;
        private List<List<Vector3>> _tetraeder = new List<List<Vector3>>();
        private List<List<bool>> _onOff3 = new List<List<bool>>();
        List<Vector3> _vertices;
        List<int> _indices;
        float _thresh = 0.5f;

        private int low;
        private int high;
        private int height;
        private int width;
        private int depth;
        private int spacing;
        private float negatex;
        private float negatey;
        private float negatez;
        private float adjustx;
        private float adjusty;
        private float adjustz;

        public Tetraeder(int low, int high, int height,int width, int depth, int spacing,Material material,Material materialBack)
        {
            this.low = low;
            this.high = high;
            this.height = height;
            this.width = width;
            this.depth = depth;
            this.spacing = spacing;
            negatex = width/2f;
            negatey = height/2f;
            negatez = depth/2f;
            adjustx = width;
            adjusty = height;
            adjustz = depth;
            this.material = material;
            this.materialBack = materialBack;
        }

        /// <summary>
        /// Segments the given area into qubes with size = size
        /// Then segments the qubes into 6 tetrahedrons
        /// </summary>
        public void segmentTetraeder()
        {
            for (int z = spacing / 2; z <= depth + spacing / 2; z += spacing / 2)
            {
                for (int y = spacing / 2; y <= height + spacing / 2; y += spacing / 2)
                {
                    for (int x = spacing / 2; x <= width + spacing / 2; x += spacing / 2)
                    {
                        //Bottom of qube
                        //p010 p110
                        //p000 p100
                        Vector3 p000 = new Vector3(((x - low) - negatex) / adjustx, ((y - low) - negatey) / adjusty,
                            ((z - low) - negatez) / adjustz); //p0
                        Vector3 p010 = new Vector3(((x - low) - negatex) / adjustx, ((y - low) - negatey) / adjusty,
                            ((z - high) - negatez) / adjustz); //p4
                        Vector3 p100 = new Vector3(((x - high) - negatex) / adjustx, ((y - low) - negatey) / adjusty,
                            ((z - low) - negatez) / adjustz); //p1
                        Vector3 p110 = new Vector3(((x - high) - negatex) / adjustx, ((y - low) - negatey) / adjusty,
                            ((z - high) - negatez) / adjustz); //p5

                        //Top of qube
                        //p011 p111
                        //p001 p101
                        Vector3 p001 = new Vector3(((x - low) - negatex) / adjustx, ((y - high) - negatey) / adjusty,
                            ((z - low) - negatez) / adjustz); //p2
                        Vector3 p011 = new Vector3(((x - low) - negatex) / adjustx, ((y - high) - negatey) / adjusty,
                            ((z - high) - negatez) / adjustz); //p6
                        Vector3 p101 = new Vector3(((x - high) - negatex) / adjustx, ((y - high) - negatey) / adjusty,
                            ((z - low) - negatez) / adjustz); //p3
                        Vector3 p111 = new Vector3(((x - high) - negatex) / adjustx, ((y - high) - negatey) / adjusty,
                            ((z - high) - negatez) / adjustz); //p7

                        List<Vector3> tetra1 = new List<Vector3>();
                        List<Vector3> tetra2 = new List<Vector3>();
                        List<Vector3> tetra3 = new List<Vector3>();
                        List<Vector3> tetra4 = new List<Vector3>();
                        List<Vector3> tetra5 = new List<Vector3>();
                        List<Vector3> tetra6 = new List<Vector3>();

                        //4 6 0 7
                        tetra1.Add(p010); tetra1.Add(p011); tetra1.Add(p000); tetra1.Add(p111);

                        //6 0 7 2
                        tetra2.Add(p011); tetra2.Add(p000); tetra2.Add(p111); tetra2.Add(p001);

                        //0 7 2 3
                        tetra3.Add(p000);tetra3.Add(p111); tetra3.Add(p001); tetra3.Add(p101);

                        //4 5 7 0
                        tetra4.Add(p010); tetra4.Add(p110); tetra4.Add(p111); tetra4.Add(p000);

                        //1 7 0 3
                        tetra5.Add(p100); tetra5.Add(p111); tetra5.Add(p000); tetra5.Add(p101);

                        //0 5 7 1
                        tetra6.Add(p000); tetra6.Add(p110); tetra6.Add(p111); tetra6.Add(p100);

                        for (int i = 0; i < 6; i++)
                        {
                            List<bool> t = new List<bool>();
                            t.Add(false);
                            t.Add(false);
                            t.Add(false);
                            t.Add(false);
                            _onOff3.Add(t);
                        }

                        _tetraeder.Add(tetra1);
                        _tetraeder.Add(tetra2);
                        _tetraeder.Add(tetra3);
                        _tetraeder.Add(tetra4);
                        _tetraeder.Add(tetra5);
                        _tetraeder.Add(tetra6);
                        
                    }
                }
            }
            //meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
            //mscript.createMeshGeometry(vertices, indices, MeshTopology.Triangles);
        }

        /// <summary>
        /// Draws triangles 
        /// </summary>
        public void marchingTetraeder(ushort[] cloud)
        {
            meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();

            Mesh mesh = new Mesh();
            Mesh meshBack = new Mesh();

            GameObject gameObject = new GameObject("Mesh",typeof(MeshFilter),typeof(MeshRenderer));
            gameObject.transform.localScale = new Vector3(8,8,8);
            
            GameObject gameObject2 = new GameObject("MeshBack",typeof(MeshFilter),typeof(MeshRenderer));
            gameObject2.transform.localScale = new Vector3(8,8,8);

            _vertices = new List<Vector3>();
            _indices = new List<int>();

            mscript.createMeshGeometry(_vertices, _indices);

            for (int i = 0; i < _tetraeder.Count; i++)
            {
                float[] values = new float[4];
                int k = 0;
                for (int j = 0; j < _tetraeder[0].Count; j++)
                { 
                    _onOff3[i][j] = false;
                    if (cloud == null)
                    {
                        values[k] = Helper.getDist(_tetraeder[i][j]);
                    }
                    else
                    {
                        Vector3 vec = _tetraeder[i][j];
                        int x = (int) (vec.x * width+negatex);
                        int y = (int) (vec.y * height+negatey);
                        int z = (int) (vec.z * depth+negatez);
                        if (vec.x < 0.5 && vec.y < 0.5 && vec.z < 0.5 && vec.x > -0.5 && vec.y > -0.5 && vec.z > -0.5)
                        {
                            //Debug.Log($"x {x} y {y} z {z}");
                            values[k] = cloud[x + width * (z + y * depth)]/2441f;
                        }
                    }
                    
                    if (values[k] < _thresh) 
                        _onOff3[i][j] = true;
                    k++;
                }
                
                Vector3 p1 = _tetraeder[i][0];
                Vector3 p2 = _tetraeder[i][1];
                Vector3 p3 = _tetraeder[i][2];
                Vector3 p4 = _tetraeder[i][3];

                float v1 = values[0];
                float v2 = values[1];
                float v3 = values[2];
                float v4 = values[3];

                bool b1 = _onOff3[i][0];
                bool b2 = _onOff3[i][1];
                bool b3 = _onOff3[i][2];
                bool b4 = _onOff3[i][3];

                List<Vector3> points = new List<Vector3>();

                if (b1 != b2)
                    points.Add(Helper.getEnd(p1, p2, v1, v2,_thresh));

                if (b1 != b3)
                    points.Add(Helper.getEnd(p1, p3, v1, v3,_thresh));

                if (b2 != b3)
                    points.Add(Helper.getEnd(p2, p3, v2, v3,_thresh));

                if (b1 != b4)
                    points.Add(Helper.getEnd(p1, p4, v1, v4,_thresh));

                if (b2 != b4)
                    points.Add(Helper.getEnd(p2, p4, v2, v4,_thresh));

                if (b3 != b4)
                    points.Add(Helper.getEnd(p3, p4, v3, v4,_thresh));

                int len = points.Count;
                if (len == 3)
                {
                    addVertices(points);
                    int value = _vertices.Count;
                    if (normalOut(points[0], points[1], points[2]))
                        addIndices(value-3,value-2,value-1);
                    else
                        addIndices(value-1,value-2,value-3);
                }
                else if (len == 4)
                {
                    addVertices(points);
                    int value = _vertices.Count;
                    if (normalOut(points[0], points[1], points[2]))
                        addIndices(value-4,value-3,value-2);
                    else
                        addIndices(value-2,value-3,value-4);

                    if (normalOut(points[1], points[2], points[3]))
                        addIndices(value-3,value-2,value-1);
                    else
                        addIndices(value-1,value-2,value-3);
                }
            }

            mesh.indexFormat = IndexFormat.UInt32;
            meshBack.indexFormat = IndexFormat.UInt32;
            
            mesh.SetVertices(_vertices);
            meshBack.SetVertices(_vertices);
            mesh.SetIndices(_indices,MeshTopology.Triangles,0);
            _indices.Reverse();
            meshBack.SetIndices(_indices,MeshTopology.Triangles,0);

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = material;
            
            gameObject2.GetComponent<MeshFilter>().mesh = meshBack;
            gameObject2.GetComponent<MeshRenderer>().material = materialBack;

            //mscript.createMeshGeometry(_vertices, _indices, MeshTopology.Triangles);
            mscript.toFile("test.obj", _vertices, _indices);
        }

        /// <summary>
        /// Checks if a triangles normal points away from origin
        /// </summary>
        /// <param name="p1">Corner 1</param>
        /// <param name="p2">Corner 2</param>
        /// <param name="p3">Corner 3</param>
        /// <returns>boolean</returns>
        private bool normalOut(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 U = p2-p1;
            Vector3 V = p3-p1;

            Vector3 vec = Vector3.Cross(U,V);
            float dot = Vector3.Dot(vec, p1);
            bool t;
            if (dot > 0)
                t = true;
            else
                t = false;
            return t;
        }

        public void setThresh(float val)
        {
            _thresh = val;
        }

        private void addIndices(int p1,int p2,int p3)
        {
            _indices.Add(p1);
            _indices.Add(p2);
            _indices.Add(p3);
        }

        /// <summary>
        /// Adds vertices to the list of all relevant vertices
        /// </summary>
        /// <param name="points">Takes a list of new points in one or more triangles</param>
        private void addVertices(List<Vector3> points)
        {
            foreach (var point in points)
            {
                _vertices.Add(point);
            }
        }
    }
}