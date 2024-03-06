using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawParabola : MonoBehaviour
{
    private MeshFilter filter = null;
    private Mesh mesh = null;
    //public Material arrowmaterial;

    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> UV = new List<Vector2>();
    public List<int> Triangles = new List<int>();
    public int stepCount = 100;

    public Transform begintransform;
    public Transform endtransform;
 
    
    [Range(0.1f,20)]
    public float totaltime;
    public float curvewidth;
    public bool isupdate;
    [Range(0, 1)]
    public float headsplitfac = 0;
    public float headmeshlength = 12;
    [Range(0, 1)]
    public float headuvsplitfac = 0;
    private float a = -10;

    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "Parabola";
        filter.mesh = mesh;
        int curidx = 0;
        for (int i = 0; i < stepCount; i++)
        {
            //centerpoint.Add(new Vector3(i, 0, 0));
            Vertices.Add(new Vector3(i, 0, -1));
            Vertices.Add(new Vector3(i, 0, 1));
            UV.Add(new Vector2((float)i / stepCount, 0));
            UV.Add(new Vector2((float)i / stepCount, 1));
            if (i != 0)
            {
                Triangles.AddRange(new List<int> { curidx, curidx + 2, curidx + 3 });
                Triangles.AddRange(new List<int> { curidx, curidx + 3, curidx + 1 });

                curidx = (i - 1) * 2 + 2;
            }
        }
        Refresh();
    }
    // Update is called once per frame
    void Update()
    {
        if(isupdate)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        UpdateMesh();
        MeshSetup();
    }

    private void UpdateMesh()
    {
        Vector3 begintop;
        Vector3 beginbottom;
        Vector3 beginspeed = GetParabolaInitSpeed(begintransform.position, endtransform.position, a, totaltime);
        //Vector3 rotateaxis = Vector3.Normalize(Vector3.Cross(beginspeed, endtransform.position - begintransform.position));
        //Vector3 beginspeedvertical = Quaternion.AngleAxis(90,rotateaxis)*beginspeed;
        Vector3 beginspeedvertical = Vector3.Normalize(Vector3.Cross(beginspeed, endtransform.position - begintransform.position));
        begintop = begintransform.position - curvewidth * beginspeedvertical / beginspeedvertical.magnitude;
        beginbottom = begintransform.position + curvewidth * beginspeedvertical / beginspeedvertical.magnitude;
        Vertices[0]=begintop;
        Vertices[1]=beginbottom;
        UV[0]=new Vector2(0, 0);
        UV[1]=new Vector2(0, 1);
        //int curidx = 0;
        int headcout = (int)(headsplitfac * stepCount);
        float speedH = new Vector3(beginspeed.x, 0, beginspeed.z).magnitude;
        float temptime = headmeshlength / speedH;

        float tempy = beginspeed.y * temptime + 0.5f * a * temptime * temptime;
        float length = new Vector2(headmeshlength, tempy).magnitude;
        float tempx = headmeshlength * headmeshlength / length;
        temptime = tempx / speedH;

        for (int i = 0; i < headcout; i++)
        {
            //float t = (i + 1) * totaltime / stepCount;
            float t = (i + 1) * temptime / headcout;
            Vector3 stepspeed = beginspeed;
            stepspeed.y = beginspeed.y + a * t;
            //Vector3 stepspeedvertical = Quaternion.AngleAxis(90, rotateaxis) * stepspeed;
            Vector3 stepspeedvertical = beginspeedvertical;
            Vector3 stepposition = GetStepPos(begintransform.position, beginspeed, a, t);
            Vector3 steptop = stepposition - curvewidth * stepspeedvertical.normalized;
            Vector3 stepbottom = stepposition + curvewidth * stepspeedvertical.normalized;
            Vertices[i * 2] = steptop;
            Vertices[i * 2 + 1] = stepbottom;
            UV[i * 2] = new Vector2(headuvsplitfac * (float)(i + 1) / headcout, 0);
            UV[i * 2 + 1] = new Vector2(headuvsplitfac * (float)(i + 1) / headcout, 1);
        }


        for (int i = 0; i < stepCount - headcout; i++)
        {
            float t = temptime + (i + 1) * (totaltime - temptime) / (stepCount - headcout);
            Vector3 stepspeed = beginspeed;
            stepspeed.y = beginspeed.y + a * t;
            Vector3 stepspeedvertical = beginspeedvertical;
            Vector3 stepposition = GetStepPos(begintransform.position, beginspeed, a, t);
            Vector3 steptop = stepposition - curvewidth * stepspeedvertical.normalized;
            Vector3 stepbottom = stepposition + curvewidth * stepspeedvertical.normalized;
            Vertices[(i+headcout)*2]=(steptop);
            Vertices[(i+headcout)*2+1]=(stepbottom);
            UV[(i+headcout)*2]=(new Vector2(headuvsplitfac + (1 - headuvsplitfac) * (i - headcout) / (stepCount - headcout), 0));
            UV[(i+headcout)*2+1]=(new Vector2(headuvsplitfac + (1 - headuvsplitfac) * (i - headcout) / (stepCount - headcout), 1));
        }
    }

    private Vector3 GetStepPos(Vector3 beginpos,Vector3 beginspeed,float a, float t)
    {
        Vector3 stepposition = Vector3.zero;
        stepposition.x = begintransform.position.x + beginspeed.x * t;
        stepposition.z = begintransform.position.z + beginspeed.z * t;
        stepposition.y = begintransform.position.y + beginspeed.y * t + 0.5f * a * t * t;
        return stepposition;
    }

    private Vector3 GetParabolaInitSpeed(Vector3 p0, Vector3 p1, float a, float t)
    {
        Vector3 initspeed = Vector3.zero;
        initspeed.x = (p1.x - p0.x) / t;
        initspeed.z = (p1.z - p0.z) / t;
        initspeed.y = (p1.y - p0.y) / t - 0.5f * a * t;
        return initspeed;
    }

    void MeshSetup()
    {
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.uv = UV.ToArray();
    }
}
