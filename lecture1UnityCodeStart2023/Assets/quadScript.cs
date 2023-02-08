using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

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
    private bool _click = false;
    private bool _clickTri = false;
    float thresh = 0.5f;

    // Use this for initialization
    void Start () {
       
        Slice.initDicom();

        string dicomfilepath = Application.dataPath + @"\..\dicomdata\"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up
        
        int spacing = 16;
        int low = spacing - (spacing / 4);
        int high = spacing / 4;

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        float calcY = (spacing * Mathf.Sqrt(3)) / 2;
        bool flip = false;

        for (float y = (spacing*Mathf.Sqrt(3))/2; y <= 512+spacing/4; y+=(spacing*Mathf.Sqrt(3))/2)
        {
            for (int x = spacing / 2; x <= 512 + (spacing / 2); x += (spacing))
            {
                List<Vector3> tri = new List<Vector3>();
                List<Vector3> tri2 = new List<Vector3>();
                Vector3 vec5 = new Vector3();
                Vector3 vec6 = new Vector3();
                Vector3 vec7 = new Vector3();
                Vector3 vec10 = new Vector3();
                
                if (!flip)
                {
                    //    vec7    vec10
                    //
                    //vec5   vec6
                    vec5 = new Vector3((x - spacing),
                        y - calcY, 0);
                    vec6 = new Vector3(x,
                        y - calcY, 0);
                    vec7 = new Vector3(((x - spacing) + x) / 2f, y, 0);

                    
                    tri.Add(vec5);
                    tri.Add(vec6);
                    tri.Add(vec7);

                    if ((x + spacing / 2) < 512+(spacing/2)){
                        vec10 = new Vector3((x + spacing / 2), y, 0);
                        tri2.Add(vec7);
                        tri2.Add(vec6);
                        tri2.Add(vec10);
                        _triangles.Add(tri2);
                    }
                }else{
                    //vec5    vec6
                    //
                    //   vec7    vec10
                    vec5 = new Vector3((x - spacing),
                        y, 0);
                    vec6 = new Vector3(x,
                        y, 0);
                    vec7 = new Vector3(((x - spacing) + x) / 2f, y - calcY, 0);

                    
                    tri.Add(vec5);
                    tri.Add(vec6);
                    tri.Add(vec7);

                    if ((x + spacing / 2) < 512+(spacing/2)){
                        vec10 = new Vector3((x + spacing / 2), y - calcY, 0);
                        tri2.Add(vec7);
                        tri2.Add(vec6);
                        tri2.Add(vec10);
                        _triangles.Add(tri2);
                    }
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

                Vector3 vec5Mod = new Vector3((vec5.x-256)/512,(vec5.y-256)/512,0);
                Vector3 vec6Mod = new Vector3((vec6.x-256)/512,(vec6.y-256)/512);
                Vector3 vec7Mod = new Vector3((vec7.x-256)/512,(vec7.y-256)/512);
                Vector3 vec10Mod = new Vector3((vec10.x-256)/512,(vec10.y-256)/512);
                
                //Draw triangles
                vertices.Add(vec5Mod);
                vertices.Add(vec6Mod);
                vertices.Add(vec5Mod);
                vertices.Add(vec7Mod);
                vertices.Add(vec6Mod);
                vertices.Add(vec7Mod);
                vertices.Add(vec7Mod);
                vertices.Add(vec10Mod);
                vertices.Add(vec6Mod);
                vertices.Add(vec10Mod);
                indices.Add(vertices.Count-10);
                indices.Add(vertices.Count-9);
                indices.Add(vertices.Count-8);
                indices.Add(vertices.Count-7);
                indices.Add(vertices.Count-6);
                indices.Add(vertices.Count-5);
                indices.Add(vertices.Count-4);
                indices.Add(vertices.Count-3);
                indices.Add(vertices.Count-2);
                indices.Add(vertices.Count-1);
            }

            flip = !flip;
        }
        
        for (int y = spacing/2; y <= 512+spacing/2; y+= spacing/2)
        {
            for (int x = spacing / 2; x <= 512 + (spacing / 2); x += (spacing/2))
            {
                //Make squares
                Vector3 vec1 = new Vector3((x - low), (y - low),0);
                Vector3 vec2 = new Vector3((x - low), (y - high),0);
                Vector3 vec3 = new Vector3((x - high), (y - low),0);
                Vector3 vec4 = new Vector3((x - high), (y - high),0);
                
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
                
                /*
                vertices.Add(vec1);
                vertices.Add(vec2);
                vertices.Add(vec1);
                vertices.Add(vec3);
                vertices.Add(vec3);
                vertices.Add(vec4);
                vertices.Add(vec2);
                vertices.Add(vec4);
                indices.Add(vertices.Count-8);
                indices.Add(vertices.Count-7);
                indices.Add(vertices.Count-6);
                indices.Add(vertices.Count-5);
                indices.Add(vertices.Count-4);
                indices.Add(vertices.Count-3);
                indices.Add(vertices.Count-2);
                indices.Add(vertices.Count-1);
                */
            }
        }

        _slices = processSlices(dicomfilepath);     // loads slices from the folder above
        setTexture(_slices[0]);                     // shows the first slice

        //  gets the mesh object and uses it to create a diagonal line
        meshScript mscript = GameObject.Find("GameObjectMesh2").GetComponent<meshScript>();
        /*
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        vertices.Add(new Vector3(-0.5f,-0.5f,0));
        vertices.Add(new Vector3(0.5f,0.5f,0));
        indices.Add(0);
        indices.Add(1);
        */
        mscript.createMeshGeometry(vertices, indices);
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
            march(texture);
            _click = false;
        }else if (_clickTri)
        {
            marchingTriangles(texture);
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
        thresh = val;
        _click = true;
        setTexture(_slices[0]);
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

    public void march(Texture2D texture)
    {
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        mscript.createMeshGeometry(vertices, indices);

        for (int i = 0; i < _squares.Count; i++)
        {
            float[] colors = new float[4];
            int k = 0;
            for (int j = 0; j < _squares[0].Count; j++)
            {
                colors[k] = texture.GetPixel((int) (_squares[i][j].x),
                    (int) (_squares[i][j].y)).r;
                if (colors[k] < thresh)
                {
                    _onOff[i][j] = true;
                }
                k++;
            }
            
            Vector3 p1 = _squares[i][0];
            Vector3 p2 = _squares[i][1];
            Vector3 p3 = _squares[i][2];
            Vector3 p4 = _squares[i][3];

            float c1 = colors[0];
            float c2 = colors[1];
            float c3 = colors[2];
            float c4 = colors[3];

            bool b1 = _onOff[i][0];
            bool b2 = _onOff[i][1];
            bool b3 = _onOff[i][2];
            bool b4 = _onOff[i][3];

            List<Vector3> points = new List<Vector3>();

            //p2 p4
            //p1 p3
            if (b1 != b3)
            {
                Vector3 vec = getEnd(p1,p3,c1,c3,thresh);
                points.Add(vec);
                
            }

            if (b1 != b2)
            {
                Vector3 vec = getEnd(p1,p2,c1,c2,thresh);
                points.Add(vec);
            }

            if (b3 != b4)
            {
                Vector3 vec = getEnd(p3,p4,c3,c4,thresh);
                points.Add(vec);
            }

            if (b2 != b4)
            {
                Vector3 vec = getEnd(p2,p4,c2,c4,thresh);
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
    
    private void marchingTriangles(Texture2D tex)
    {
        
        float thresh = 0.5f;
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        List<Vector3> v = new List<Vector3>();
        List<int> v2 = new List<int>();
        mscript.createMeshGeometry(v, v2);
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < _triangles.Count; i++)
        {
            float[] colors = new float[3];
            int k = 0;
            for (int j = 0; j < _triangles[0].Count; j++)
            {
                colors[k] = tex.GetPixel((int) (_triangles[i][j].x),
                    (int) (_triangles[i][j].y)).r;
                if (colors[k] < thresh)
                {
                    _onOff2[i][j] = true;
                }
                k++;
            }
            
            Vector3 p1 = _triangles[i][0];
            Vector3 p2 = _triangles[i][1];
            Vector3 p3 = _triangles[i][2];

            float c1 = colors[0];
            float c2 = colors[1];
            float c3 = colors[2];

            bool b1 = _onOff2[i][0];
            bool b2 = _onOff2[i][1];
            bool b3 = _onOff2[i][2];

            List<Vector3> points = new List<Vector3>();

            //  p3
            //p1 p2
            if (b1 != b2)
            {
                Vector3 vec = getEnd(p1,p2,c1,c2,thresh);
                points.Add(vec);
                
            }

            if (b1 != b3)
            {
                Vector3 vec = getEnd(p1,p3,c1,c3,thresh);
                points.Add(vec);
            }

            if (b2 != b3)
            {
                Vector3 vec = getEnd(p2,p3,c2,c3,thresh);
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

        }
        mscript.createMeshGeometry(vertices, indices);
    }

    private Vector3 getEnd(Vector3 p1, Vector3 p2,float c1, float c2,float thresh)
    {
        float negate = 256f;
        float adjust = 512f;
        float t = getT(c1,c2,thresh);
        float x;
        float y;
        if (c1 < c2)
        {
            x = (((1-t)*p1.x+t*p2.x)-negate)/adjust;
            y = (((1-t)*p1.y+t*p2.y)-negate)/adjust;
        }
        else
        {
            x = (((1-t)*p2.x+t*p1.x)-negate)/adjust;
            y = (((1-t)*p2.y+t*p1.y)-negate)/adjust;
        }
        Vector3 temp = new Vector3(x,y,0);
        return temp;
    }

    private float getT(float c1, float c2, float iso)
    {
        float v1 = Mathf.Min(c2,c1);
        float v2 = Mathf.Max(c2,c1);
        return (iso - v1) / (v2 - v1);
    }
}
