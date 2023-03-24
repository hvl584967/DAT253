using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using DefaultNamespace;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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

    public Material material;
    public Material materialBack;
    private int _sliderX = 256;
    private int _sliderY = 256;
    private float _size = 0.5f;
    private bool _click = false;
    private bool _clickTri = false;
    private bool _clickTet = false;
    private Tetraeder _tetra;
    private Square _sq;
    private Triangle _tri;
    float _thresh = 0.5f;
    private float _sliderImg = 0f;
    private ushort[] _points;
    private int _height;
    private int _spacing = 4;

    // Use this for initialization
    void Start () {
       
        Slice.initDicom();

        string dicomfilepath = Application.dataPath + @"\..\dicomdata\"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up

        int size = 512;
        
        int low = _spacing - (_spacing / 4);
        int high = _spacing / 4;
        float negate = 256f;
        float adjust = 512f;

        _height = 355;
        int width = 512;
        int depth = 512;

        //_tri = new Triangle(size,_spacing,negate,adjust);
        //_tri.segmentTriangle();
        //_sq = new Square(low, high, size, _spacing, negate, adjust);
        //_sq.segmentSquare();
        _tetra = new Tetraeder(low,high,_height,width,depth,_spacing,material,materialBack);
        _tetra.segmentTetraeder();

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
            float val = pixelval(new Vector2(x, y), xdim, pixels);
            float v = (val-_minIntensity) / _maxIntensity;      // maps [_minIntensity,_maxIntensity] to [0,1] , i.e.  _minIntensity to black and _maxIntensity to white
            texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
        }

        if (_click||_clickTet||_clickTri)
        {
            for (int y = 0; y < ydim; y++)
            for (int x = 0; x < xdim; x++)
            {
                
                var rToRGB = Mathf.Sqrt(Mathf.Pow((_sliderX-x)/_size,2)+Mathf.Pow((_sliderY-y)/_size,2))/362;
                
                float val = pixelval(new Vector2(x, y), xdim, pixels);
                float v = (val-_minIntensity) / _maxIntensity;      // maps [_minIntensity,_maxIntensity] to [0,1] , i.e.  _minIntensity to black and _maxIntensity to white
                texture.SetPixel(x, y, new UnityEngine.Color(rToRGB, rToRGB, rToRGB));
            }
        }

        LineRenderer rend = new LineRenderer();

        texture.filterMode = FilterMode.Point;  // nearest neigbor interpolation is used.  (alternative is FilterMode.Bilinear)
        texture.Apply();  // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;
    }
    
    ushort pixelval(Vector2 p, int xdim, ushort[] pixels)
    {
        return pixels[(int)p.x + (int)p.y * xdim];
    }

    public void sliceShow(float val)
    {
        _sliderImg = val;
        setTexture(_slices[(int)(_sliderImg*_slices.Length)]);
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

    public void sliceSizeSliderChange(float val)
    {
        _thresh = val;
        if (_click)
        {
            _sq.setThresh(_thresh);
            _sq.march();
        }

        if (_clickTri)
        {
            _tri.setThresh(_thresh);
            _tri.march();
        }

        if (_clickTet)
        {
            _tetra.setThresh(_thresh);
            _tetra.marchingTetraeder(new ushort[]{});
        }
    }
    
    public void button1Pushed()
    {
        _sq.march();
        _click = true;
        print("button1Pushed"); 
    }

    public void button2Pushed()
    {
        _tri.march();
        _clickTri = true;
        print("button2Pushed"); 
    }
    
    public void button3Pushed()
    {
        getValues();
        _tetra.marchingTetraeder(_points);
        _clickTet = !_clickTet;
        print("button3Pushed");
    }

    private void getValues()
    {
        _points = _points ?? _slices
            .SelectMany(s => s.getPixels())
            .ToArray();
    }
}