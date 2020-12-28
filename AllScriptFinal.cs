using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllScriptFinal : MonoBehaviour
{
    Mesh startMesh;
   // MeshFilter meshFilter;
    int[] triangles;
    Transform position;

    [HideInInspector]
    public Vector3[] vertices;

    public LineRenderer linePrefab;
    private float[] dist; //distance
    LineRenderer[] faceLines;

    [HideInInspector]
    public Color[] colors;

    //An array of Objects that stores the results of the Resources.LoadAll() method  
    private Object[] objects;
    //Each returned object is converted to a Texture and stored in this array  
    Texture2D[] textures;
    //Texture2D[] current;
    //With this Material object, a reference to the game object Material can be stored  
    private Material goMaterial;
    //An integer to advance frames  
    private int frameCounter = 0;

  

    void Start()
    {
        ////Get a reference to the Material of the game object this script is attached to  
        //this.goMaterial = this.GetComponent<Renderer>().material;

        //Get a reference to the Material of the game object this script is attached to  
        this.goMaterial = this.GetComponent<Renderer>().material;

        //Load all textures found on the Sequence folder, that is placed inside the resources folder  
        this.objects = Resources.LoadAll("Sequence", typeof(Texture2D));

        //Initialize the array of textures with the same size as the objects array  
        this.textures = new Texture2D[objects.Length];

        //Cast each Object to Texture and store the result inside the Textures array  
        for (int i = 0; i < objects.Length; i++)
        {
            this.textures[i] = (Texture2D)this.objects[i];
        }

        startMesh = this.GetComponent<MeshFilter>().mesh;
      //  meshFilter = this.GetComponent<MeshFilter>();
        triangles = startMesh.triangles;
        vertices = startMesh.vertices;

        // build facelines
        int i1, i2, i3;
        Vector3 v1, v2, v3;
        faceLines = new LineRenderer[triangles.Length / 3];
        dist = new float[triangles.Length / 3];
        for (int i = 0; i < triangles.Length / 3; i++)
        {

            i1 = triangles[(i * 3) + 0];
            i2 = triangles[(i * 3) + 1];
            i3 = triangles[(i * 3) + 2];
            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];
          
            Vector3 center = (v1 + v2 + v3) / 3;
          
            LineRenderer line = Instantiate(linePrefab, center, Quaternion.identity);
            line.SetPosition(0, center);
            line.SetWidth(0.03f, 0.015f);
            faceLines[i] = line;
        }
    }

    // Start is called before the first frame update
    /*  void Update()
      {
          // since Update() runs once per frame, we can update the framecounter here.
          frameCounter = (++frameCounter) % textures.Length;

          //Set the material's texture to the current value of the frameCounter variable  
          this.goMaterial.mainTexture = textures[frameCounter];

          CenterOfTriangle(triangles, vertices);

      }
    */
    void Update()
    {
        //Call the 'PlayLoop' method as a coroutine with a 0.04 delay  
        StartCoroutine("PlayLoop", 0.023f);

        //Set the material's texture to the current value of the frameCounter variable  
        this.goMaterial.mainTexture = textures[frameCounter];

        CenterOfTriangle(triangles, vertices);


    }

    //The following methods return a IEnumerator so they can be yielded:  
    //A method to play the animation in a loop  
      IEnumerator PlayLoop(float delay)
      {
          //Wait for the time defined at the delay parameter  
          yield return new WaitForSeconds(delay);

          //Advance one frame  
          frameCounter = (++frameCounter) % textures.Length;

          //Stop this coroutine  
          StopCoroutine("PlayLoop");
      }
    

    public void CenterOfTriangle(int[] triangles, Vector3[] vertices)
    {

        Vector3 center;
        Vector3 normal;

        float H, S, V;
        int i1, i2, i3;
        Vector3 v1, v2, v3;


        for (int i = 0; i < triangles.Length / 3; i++)
        {
            i1 = triangles[(i * 3) + 0];
            i2 = triangles[(i * 3) + 1];
            i3 = triangles[(i * 3) + 2];
          
            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];
            center = (v1 + v2 + v3) / 3;
            normal = GetNormalVectorToTriangle(v1, v2, v3);

            // use barycentric coordinates to interpolate the mesh
            // location to a texture location
            Vector2 uvA = startMesh.uv[triangles[(i * 3) + 0]];
            Vector2 uvB = startMesh.uv[triangles[(i * 3) + 1]];
            Vector2 uvC = startMesh.uv[triangles[(i * 3) + 2]];

            float a, b, c = 0;
            Barycentric(center, v1, v2, v3, out a, out b, out c);

            Vector2 uvP = a * uvA + b * uvB + c * uvC;
            int x = Mathf.FloorToInt(uvP.x * textures[frameCounter].width);
            int y = Mathf.FloorToInt(uvP.y * textures[frameCounter].height);

            // get color of the texture at the position that matches
            // the center of the mesh triangle
            Color color = textures[frameCounter].GetPixel(x, y);

            Color.RGBToHSV(color, out H, out S, out V);


            //dist[i] = Mathf.Gamma(V, 5, 1 / 1.5f) + 0.5f;

             dist[i] = (V * 2.0f) + 0.5f;

            // update the line's end position
            Vector3 endPosition = center + (normal * dist[i]);
            faceLines[i].SetPosition(1, endPosition);

        }
    }

    void Barycentric(Vector3 center, Vector3 A, Vector3 B, Vector3 C, out float a, out float b, out float c)
    {
        Vector3 v0 = B - A, v1 = C - A, v2 = center - A;
        float den = v0.x * v1.y - v1.x * v0.y;
        b = (v2.x * v1.y - v1.x * v2.y) / den;
        c = (v0.x * v2.y - v2.x * v0.y) / den;
        a = 1.0f - b - c;
    }

    public Vector3 GetNormalVectorToTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 v1v2 = v2 - v1;
        Vector3 v2v3 = v3 - v2;
        Vector3 normal = Vector3.Cross(v1v2, v2v3);

        return normal.normalized;

    }
}

