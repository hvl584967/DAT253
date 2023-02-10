using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

public class quadScript : MonoBehaviour {

    // Dicom har et "levende" dictionary som leses fra xml ved initDicom
    // slices må sorteres, og det basert på en tag, men at pixeldata lesing er en separat operasjon, derfor har vi nullpeker til pixeldata
    // dicomfile lagres slik at fil ikke må leses enda en gang når pixeldata hentes
    
    // member variables of quadScript, accessible from any function
    Slice[] _slices;
    int _numSlices;
    int _minIntensity;
    int _maxIntensity;
    //int _iso;

    private int _sliderX = 256;
    private int _sliderY = 256;
    private float _size = 0.5f;
    private List<List<Vector3>> _squares = new List<List<Vector3>>();
    private List<List<bool>> _onOff = new List<List<bool>>();
    private List<List<Vector3>> _triangles = new List<List<Vector3>>();
    private List<List<bool>> _onOff2 = new List<List<bool>>();
    private List<List<Vector3>> _tetraeder = new List<List<Vector3>>();
    private List<List<bool>> _onOff3 = new List<List<bool>>();
    private bool _click = false;
    private bool _clickTri = false;
    float _thresh = 0.5f;

    // Use this for initialization
    void Start () {
       
        Slice.initDicom();

        string dicomfilepath = Application.dataPath + @"\..\dicomdata\"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up

        int size = 512;
        int spacing = 32;
        int low = spacing - (spacing / 4);
        int high = spacing / 4;
        float negate = 256f;
        float adjust = 512f;

        segmentTriangle(size,spacing,negate,adjust);
        segmentSquare(low,high,size,spacing,negate,adjust);
        segmentTetraeder(low,high,size,spacing,negate,adjust);

        _slices = processSlices(dicomfilepath);     // loads slices from the folder above
        setTexture(_slices[0]);                     // shows the first slice

        //  gets the mesh object and uses it to create a diagonal line
        //meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        /*
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        vertices.Add(new Vector3(-0.5f,-0.5f,0));
        vertices.Add(new Vector3(0.5f,0.5f,0));
        indices.Add(0);
        indices.Add(1);
        */
        //mscript.createMeshGeometry(vertices, indices);
    }

    Slice[] processSlices(string dicomfilepath)
    {
        string[] dicomfilenames = Directory.GetFiles(dicomfilepath, "*.IMA");

        _numSlices =  dicomfilenames.Length;

        Slice[] slices = new Slice[_numSlices];

        float max = -1;
        float min = 99999;
        for (int i = 0; i < _numSlices; i++)
        {
            string filename = dicomfilenames[i];
            slices[i] = new Slice(filename);
            SliceInfo info = slices[i].sliceInfo;
            if (info.LargestImagePixelValue > max) max = info.LargestImagePixelValue;
            if (info.SmallestImagePixelValue < min) min = info.SmallestImagePixelValue;
            // Del dataen på max før den settes inn i tekstur
            // alternativet er å dele på 2^dicombitdepth,  men det ville blitt 4096 i dette tilfelle

        }
        print("Number of slices read:" + _numSlices);
        print("Max intensity in all slices:" + max);
        print("Min intensity in all slices:" + min);

        _minIntensity = (int)min;
        _maxIntensity = (int)max;
        //_iso = 0;

        Array.Sort(slices);
        
        return slices;
    }

    void setTexture(Slice slice)
    {
        int xdim = slice.sliceInfo.Rows;
        int ydim = slice.sliceInfo.Columns;

        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false);     // garbage collector will tackle that it is new'ed 

        ushort[] pixels = slice.getPixels();
        
        for (int y = 0; y < ydim; y++)
            for (int x = 0; x < xdim; x++)
            {
                var rToRGB = Mathf.Sqrt(Mathf.Pow((_sliderX-x)/_size,2)+Mathf.Pow((_sliderY-y)/_size,2))/362;
                
                float val = pixelval(new Vector2(x, y), xdim, pixels);
                float v = (val-_minIntensity) / _maxIntensity;      // maps [_minIntensity,_maxIntensity] to [0,1] , i.e.  _minIntensity to black and _maxIntensity to white
                texture.SetPixel(x, y, new UnityEngine.Color(rToRGB, rToRGB, rToRGB));
            }

        LineRenderer rend = new LineRenderer();

        if (_click)
        {
            march();
            _click = false;
        }else if (_clickTri)
        {
            marchingTriangles();
            _clickTri = false;
        }

        texture.filterMode = FilterMode.Point;  // nearest neigbor interpolation is used.  (alternative is FilterMode.Bilinear)
        texture.Apply();  // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;
    }
    
    ushort pixelval(Vector2 p, int xdim, ushort[] pixels)
    {
        return pixels[(int)p.x + (int)p.y * xdim];
    }


    Vector2 vec2(float x, float y)
    {
        return new Vector2(x, y);
    }


    // Update is called once per frame
    void Update () {
        
      
    }
       
    public void slicePosSliderChange(float val)
    {
        _sliderX =(int) val;
        print("slicePosSliderChange:" + val);
        setTexture(_slices[0]);
    }

    public void sliceIsoSliderChange(float val)
    {
        _sliderY =(int) val;
        print("sliceIsoSliderChange:" + val);
        setTexture(_slices[0]);
        
    }

    public void sliceSizeSliderCahnge(float val)
    {
        //_size = val;
        _thresh = val;
        //_click = true;
        //setTexture(_slices[0]);
        marchingTetraeder();
    }
    
    public void button1Pushed()
    {
        _click = true;
        setTexture(_slices[0]);
        print("button1Pushed"); 
    }

    public void button2Pushed()
    {
        _clickTri = true;
        setTexture(_slices[0]);
        print("button2Pushed"); 
    }
    
    public void button3Pushed()
    {
        marchingTetraeder();
        print("button3Pushed"); 
    }

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
                values[k] = getCircleDist(_squares[i][j]);
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
                Vector3 vec = getEnd(p1,p3,v1,v3);
                points.Add(vec);
                
            }

            if (b1 != b2)
            {
                Vector3 vec = getEnd(p1,p2,v1,v2);
                points.Add(vec);
            }

            if (b3 != b4)
            {
                Vector3 vec = getEnd(p3,p4,v3,v4);
                points.Add(vec);
            }

            if (b2 != b4)
            {
                Vector3 vec = getEnd(p2,p4,v2,v4);
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
    
    private void marchingTriangles()
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
                values[k] = getCircleDist(_triangles[i][j]);
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
                Vector3 vec = getEnd(p1,p2,v1,v2);
                points.Add(vec);
            }

            if (b1 != b3)
            {
                Vector3 vec = getEnd(p1,p3,v1,v3);
                points.Add(vec);
            }

            if (b2 != b3)
            {
                Vector3 vec = getEnd(p2,p3,v2,v3);
                points.Add(vec);
            }

            if (points.Count() != 0)
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

    private Vector3 getEnd(Vector3 p1, Vector3 p2,float v1, float v2)
    {
        float t = getT(v2, v1);
        float x;
        float y;
        if (v1 < v2)
        {
            x = (1-t)*p1.x+t*p2.x;
            y = (1-t)*p1.y+t*p2.y;
        }
        else
        {
            x = (1-t)*p2.x+t*p1.x;
            y = (1-t)*p2.y+t*p1.y;
        }
        Vector3 temp = new Vector3(x,y,0);
        return temp;
    }
    
    private float getT(float v1, float v2)
    {
        float vMax = Mathf.Max(v2,v1);
        float vMin = Mathf.Min(v2,v1);
        return (_thresh-vMin)/(vMax-vMin);
    }

    private float getCircleDist(Vector3 p)
    {
        float f = Mathf.Sqrt(Mathf.Pow(p.x,2)+Mathf.Pow(p.y,2));
        return f;
    }
    
    private float getSphereDist(Vector3 p)
    {
        float f = Mathf.Sqrt(Mathf.Pow(p.x,2)+Mathf.Pow(p.y,2)+Mathf.Pow(p.z,2));
        return f;
    }

    private void segmentTriangle(int size,int spacing,float negate,float adjust)
    {
        float calcY = (spacing * Mathf.Sqrt(3)) / 2;
        float flip = 0f;
        
        for (float y = (spacing*Mathf.Sqrt(3))/2; y <= size+spacing/4; y+=(spacing*Mathf.Sqrt(3))/2)
        {
            for (int x = spacing / 2; x <= size + (spacing / 2); x += (spacing))
            {
                List<Vector3> tri = new List<Vector3>();
                List<Vector3> tri2 = new List<Vector3>();

                //    vec7    vec10
                //
                //vec5   vec6
                Vector3 vec5 = new Vector3(((x - spacing)-negate)/adjust,((y - calcY + flip)-negate)/adjust, 0);
                Vector3 vec6 = new Vector3((x-negate)/adjust,((y - calcY+flip)-negate)/adjust, 0);
                Vector3 vec7 = new Vector3(((((x - spacing) + x) / 2f)-negate)/adjust, (y-flip-negate)/adjust, 0);
                    
                tri.Add(vec5);
                tri.Add(vec6);
                tri.Add(vec7);

                if ((x + spacing / 2) < size+(spacing/2)){
                    Vector3 vec10 = new Vector3(((x + spacing / 2)-negate)/adjust, (y-flip-negate)/adjust, 0);
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

    private void segmentSquare(int low, int high,int size, int spacing,float negate,float adjust)
    {
        for (int y = spacing/2; y <= size+spacing/2; y+= spacing/2)
        {
            for (int x = spacing / 2; x <= size + (spacing / 2); x += (spacing/2))
            {
                //vec2 vec4
                //vec1 vec3
                Vector3 vec1 = new Vector3(((x - low)-negate)/adjust, ((y - low)-negate)/adjust,0);
                Vector3 vec2 = new Vector3(((x - low)-negate)/adjust, ((y - high)-negate)/adjust,0);
                Vector3 vec3 = new Vector3(((x - high)-negate)/adjust, ((y - low)-negate)/adjust,0);
                Vector3 vec4 = new Vector3(((x - high)-negate)/adjust, ((y - high)-negate)/adjust,0);
                
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

    private void segmentTetraeder(int low, int high,int size, int spacing,float negate,float adjust)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        for (int z = spacing/2; z <= size+spacing/2; z += spacing/2)
        {
            for (int y = spacing/2; y <= size+spacing/2; y += spacing/2)
            {
                for (int x = spacing/2; x <= size+spacing/2; x += spacing/2)
                {
                    //Bottom of qube
                    //p010 p110
                    //p000 p100
                    Vector3 p000 = new Vector3(((x - low)-negate)/adjust, ((y - low)-negate)/adjust,((z - low)-negate)/adjust);
                    Vector3 p010 = new Vector3(((x - low)-negate)/adjust, ((y - low)-negate)/adjust,((z - high)-negate)/adjust);
                    Vector3 p100 = new Vector3(((x - high)-negate)/adjust, ((y - low)-negate)/adjust,((z - low)-negate)/adjust);
                    Vector3 p110 = new Vector3(((x - high)-negate)/adjust, ((y - low)-negate)/adjust,((z - high)-negate)/adjust);
                    
                    //Top of qube
                    //p011 p111
                    //p001 p101
                    Vector3 p001 = new Vector3(((x - low)-negate)/adjust, ((y - high)-negate)/adjust,((z - low)-negate)/adjust);
                    Vector3 p011 = new Vector3(((x - low)-negate)/adjust, ((y - high)-negate)/adjust,((z - high)-negate)/adjust);
                    Vector3 p101 = new Vector3(((x - high)-negate)/adjust, ((y - high)-negate)/adjust,((z - low)-negate)/adjust);
                    Vector3 p111 = new Vector3(((x - high)-negate)/adjust, ((y - high)-negate)/adjust,((z - high)-negate)/adjust);

                    List<Vector3> tetra1 = new List<Vector3>();
                    List<Vector3> tetra2 = new List<Vector3>();
                    List<Vector3> tetra3 = new List<Vector3>();
                    List<Vector3> tetra4 = new List<Vector3>();
                    List<Vector3> tetra5 = new List<Vector3>();
                    List<Vector3> tetra6 = new List<Vector3>();
                    
                    tetra1.Add(p000);
                    tetra1.Add(p010);
                    tetra1.Add(p110);
                    tetra1.Add(p111);
                    
                    tetra2.Add(p000);
                    tetra2.Add(p010);
                    tetra2.Add(p011);
                    tetra2.Add(p111);
                    
                    tetra3.Add(p000);
                    tetra3.Add(p001);
                    tetra3.Add(p011);
                    tetra3.Add(p111);
                    
                    tetra4.Add(p000);
                    tetra4.Add(p001);
                    tetra4.Add(p111);
                    tetra4.Add(p101);
                    
                    tetra5.Add(p000);
                    tetra5.Add(p100);
                    tetra5.Add(p110);
                    tetra5.Add(p111);
                    
                    tetra6.Add(p000);
                    tetra6.Add(p100);
                    tetra6.Add(p101);
                    tetra6.Add(p111);

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
                    
                    /*
                    vertices.Add(p000);
                    vertices.Add(p010);
                    vertices.Add(p000);
                    vertices.Add(p100);
                    vertices.Add(p000);
                    vertices.Add(p001);
                    
                    vertices.Add(p010);
                    vertices.Add(p011);
                    vertices.Add(p010);
                    vertices.Add(p110);
                    
                    vertices.Add(p100);
                    vertices.Add(p110);
                    vertices.Add(p100);
                    vertices.Add(p101);
                    
                    vertices.Add(p110);
                    vertices.Add(p111);
                    
                    vertices.Add(p001);
                    vertices.Add(p011);
                    vertices.Add(p001);
                    vertices.Add(p101);
                    
                    vertices.Add(p011);
                    vertices.Add(p111);
                    
                    vertices.Add(p101);
                    vertices.Add(p111);
                    int len = vertices.Count;
                    //24
                    indices.Add(len-24);
                    indices.Add(len-23);
                    indices.Add(len-22);
                    indices.Add(len-21);
                    indices.Add(len-20);
                    indices.Add(len-19);
                    indices.Add(len-18);
                    indices.Add(len-17);
                    indices.Add(len-16);
                    indices.Add(len-15);
                    indices.Add(len-14);
                    indices.Add(len-13);
                    indices.Add(len-12);
                    indices.Add(len-11);
                    indices.Add(len-10);
                    indices.Add(len-9);
                    indices.Add(len-8);
                    indices.Add(len-7);
                    indices.Add(len-6);
                    indices.Add(len-5);
                    indices.Add(len-4);
                    indices.Add(len-3);
                    indices.Add(len-2);
                    indices.Add(len-1);
                    */
                }
            }
        }
        
        //meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        //mscript.createMeshGeometry(vertices, indices);
    }
    
    private void marchingTetraeder()
    {
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        mscript.createMeshGeometry(vertices, indices);

        for (int i = 0; i < _tetraeder.Count; i++)
        {
            float[] values = new float[4];
            int k = 0;
            for (int j = 0; j < _tetraeder[0].Count; j++)
            {
                _onOff3[i][j] = false;
                values[k] = getSphereDist(_tetraeder[i][j]);
                if (values[k] < _thresh)
                {
                    _onOff3[i][j] = true;
                }
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
            {
                Vector3 vec = getEnd3(p1,p2,v1,v2);
                points.Add(vec);
            }

            if (b1 != b3)
            {
                Vector3 vec = getEnd3(p1,p3,v1,v3);
                points.Add(vec);
            }

            if (b2 != b3)
            {
                Vector3 vec = getEnd3(p2,p3,v2,v3);
                points.Add(vec);
            }

            if (b1 != b4)
            {
                Vector3 vec = getEnd3(p1,p4,v1,v4);
                points.Add(vec);
            }

            if (b2 != b4)
            {
                Vector3 vec = getEnd3(p2,p4,v2,v4);
                points.Add(vec);
            }

            if (b3 != b4)
            {
                Vector3 vec = getEnd3(p3,p4,v3,v4);
                points.Add(vec);
            }

            int len = points.Count;
            if (len == 3)
            {
                vertices.Add(points[0]);
                vertices.Add(points[1]);
                vertices.Add(points[2]);
                int val = vertices.Count;
                indices.Add(val-3);
                indices.Add(val-2);
                indices.Add(val-1);
            }else if (len == 4)
            {
                vertices.Add(points[0]);
                vertices.Add(points[1]);
                vertices.Add(points[2]);
                vertices.Add(points[3]);
                int val = vertices.Count;
                indices.Add(val-4);
                indices.Add(val-3);
                indices.Add(val-2);
                indices.Add(val-3);
                indices.Add(val-2);
                indices.Add(val-1);
            }

        }
        mscript.createMeshGeometry(vertices, indices, MeshTopology.Triangles);
    }
    
    private Vector3 getEnd3(Vector3 p1, Vector3 p2,float v1, float v2)
    {
        float t = getT(v2, v1);
        float x;
        float y;
        float z;
        if (v1 < v2)
        {
            x = (1-t)*p1.x+t*p2.x;
            y = (1-t)*p1.y+t*p2.y;
            z = (1-t)*p1.z+t*p2.z;
        }
        else
        {
            x = (1-t)*p2.x+t*p1.x;
            y = (1-t)*p2.y+t*p1.y;
            z = (1-t)*p2.z+t*p1.z;
        }
        Vector3 temp = new Vector3(x,y,z);
        return temp;
    }
}
