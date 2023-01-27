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
    private Vector3[][] _squares = new Vector3[1024][];
    private bool[][] _onOff = new bool[1024][];
    private bool _click = false;

    // Use this for initialization
    void Start () {
       
        Slice.initDicom();

        string dicomfilepath = Application.dataPath + @"\..\dicomdata\"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up

        int spot = 0;
        int spacing = 16;
        
        for (int y = (-256 + spacing); y <= 256; y+=spacing)
        {
            for (int x = (-256 + spacing); x <= 256; x+=spacing)
            {
                Vector3 vec1 = new Vector3(x - (spacing/4), y - (spacing/4),0);
                Vector3 vec2 = new Vector3(x - (spacing/4), y - (spacing/2),0);
                Vector3 vec3 = new Vector3(x - (spacing/2), y - (spacing/4),0);
                Vector3 vec4 = new Vector3(x - (spacing/2), y - (spacing/2),0);
                Vector3[] arr = new Vector3[4];
                arr[0] = vec1;
                arr[1] = vec2;
                arr[2] = vec3;
                arr[3] = vec4;
                bool[] bools = new bool[4];
                bools[0] = false;
                bools[1] = false;
                bools[2] = false;
                bools[3] = false;
                _squares[spot] = arr;
                _onOff[spot] = bools;
                spot++;
            }
        }

        _slices = processSlices(dicomfilepath);     // loads slices from the folder above
        setTexture(_slices[0]);                     // shows the first slice

        //  gets the mesh object and uses it to create a diagonal line
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        vertices.Add(new Vector3(-0.5f,-0.5f,0));
        vertices.Add(new Vector3(0.5f,0.5f,0));
        indices.Add(0);
        indices.Add(1);
        
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
        _size = val;
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
        print("button2Pushed"); 
    }

    public void march(Texture2D texture)
    {
        //Points go in opposite of desired direction
        float thresh = 0.5f;
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < _squares.Length; i++)
        {
            for (int j = 0; j < _squares[0].Length; j++)
            {
                if (texture.GetPixel((int)_squares[i][j].x, (int)_squares[i][j].y).r > thresh)
                {
                    _onOff[i][j] = true;
                }
            }

            Vector3 p1 = _squares[i][0];
            Vector3 p2 = _squares[i][1];
            Vector3 p3 = _squares[i][2];
            Vector3 p4 = _squares[i][3];

            bool b1 = _onOff[i][0];
            bool b2 = _onOff[i][1];
            bool b3 = _onOff[i][2];
            bool b4 = _onOff[i][3];

            Vector3[] points = new Vector3[4];
            int pos = 0;
            
            //p2 p4
            //p1 p3

            if (b1 != b3)
            {
                Vector3 vec = new Vector3(((p1.x + p3.x)/2)/512,p3.y/512,0);
                points[pos] = vec;
                pos++;
            }

            if (b1 != b2)
            {
                Vector3 vec = new Vector3(p2.x/512,((p2.y + p1.y)/2)/512,0);
                points[pos] = vec;
                pos++;
            }
            
            if (b2 != b4)
            {
                Vector3 vec = new Vector3(((p2.x+p4.x)/2)/512,p4.y/512,0);
                points[pos] = vec;
                pos++;
            }
            
            if (b3 != b4)
            {
                Vector3 vec = new Vector3(p4.x/512,((p4.y + p3.y)/2)/512,0);
                points[pos] = vec;
                pos++;
            }

            if (points[3] == new Vector3(0,0,0))
            {
                vertices.Add(points[0]);
                vertices.Add(points[1]);
                int vert = vertices.Count;
                indices.Add(vert-2);
                indices.Add(vert-1);
            }
            else if (points[3] != new Vector3(0,0,0))
            {
                vertices.Add(points[0]);
                vertices.Add(points[1]);
                vertices.Add(points[2]);
                vertices.Add(points[3]);
                int vert = vertices.Count;
                indices.Add(vert-4);
                indices.Add(vert-3);
                indices.Add(vert-2);
                indices.Add(vert-1);
            }
            
        }
        mscript.createMeshGeometry(vertices, indices);
    }

}
