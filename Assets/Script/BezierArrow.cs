using System.Collections.Generic;
using UnityEngine;

public class BezierArrow : MonoBehaviour
{
    private MeshFilter filter = null;
    private Mesh mesh = null;

    public List<Vector3> centerpoint = new List<Vector3>();
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> UV = new List<Vector2>();
    public List<int> Triangles = new List<int>();
    public int stepCount = 101;
    public float curvewidth;
    public bool isupdate;

   
    public int headstepcount = 0;
    public float headlength = 12;
    [Range(0, 1)]
    public float headuvfac = 0;

    public List<Transform> pointlist;
    float totallength = 0;
    float headt = 0;
    Vector3 dirH = Vector3.zero;

    private void Start()
    {
        InitMesh();
    }

    private void Update()
    {
        if(isupdate)
        {
            totallength = 0;
            for (int i = 1; i < pointlist.Count; i++)
            {
                totallength += (pointlist[i].position - pointlist[i - 1].position).magnitude;
            }
            headt = headlength / totallength;
            Refresh();
        }
    }

    private void Refresh()
    {
        UpdateMesh();
        MeshSetup();
    }

    private void InitMesh()
    {
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        filter.mesh = mesh;
        mesh.name = "BezierCurveMesh";
        int curidx = 0;
        for(int i=0;i<stepCount;i++)
        {
            centerpoint.Add(new Vector3(i,0,0));
            Vertices.Add(new Vector3(i,0,-1));
            Vertices.Add(new Vector3(i, 0, 1));
            UV.Add(new Vector2((float)i / stepCount, 0));
            UV.Add(new Vector2((float)i / stepCount, 1));
            if (i != 0)
            {
                Triangles.AddRange(new List<int> { curidx, curidx + 2, curidx + 3 });
                Triangles.AddRange(new List<int> { curidx, curidx + 3, curidx + 1 });
               
                curidx = (i-1) * 2 + 2;
            } 
        }
        Refresh();
    }

    private void UpdateMesh()
    {
        UpdateHeadMesh();
        UpdateBodyMesh();
    }

    private void UpdateHeadMesh()
    {
        Vector3 curvedir = Vector3.zero;
        Vector3 finaldir = (pointlist[pointlist.Count - 1].position - pointlist[0].position).normalized;
        
        for (int i=0;i<headstepcount;i++)
        {
            centerpoint[i]=GetBezierCurve(pointlist,i*headt/(headstepcount-1));
        }
        curvedir = (centerpoint[1] - centerpoint[0]).normalized;
        if(curvedir==finaldir)
        {
            curvedir.y++;
        }
        dirH = new Vector3(0, 0, 1);
        Vertices[0] = centerpoint[0] + dirH * curvewidth;
        Vertices[1] = centerpoint[0] - dirH * curvewidth;
        UV[0] = new Vector2(0, 0);
        UV[1] = new Vector2(0, 1);
        for(int i=1;i<headstepcount;i++)
        {
            curvedir = (centerpoint[i] - centerpoint[i-1]).normalized;
            Vertices[i * 2] = centerpoint[i] + dirH * curvewidth;
            Vertices[i * 2 + 1] = centerpoint[i] - dirH * curvewidth;
            UV[i * 2] =new Vector2(headuvfac * i / headstepcount,0);
            UV[i * 2 + 1] = new Vector2(headuvfac * i / headstepcount, 1);
        }
    }

    private void UpdateBodyMesh()
    {
        Vector3 curvedir = Vector3.zero;
        Vector3 finaldir = (pointlist[pointlist.Count - 1].position - pointlist[0].position).normalized;
        for (int i = headstepcount; i < stepCount; i++)
        {
            centerpoint[i]=GetBezierCurve(pointlist, headt + (1 - headt) * (i - headstepcount) / (stepCount - headstepcount));
        }

        for (int i = headstepcount; i < stepCount; i++)
        {
            curvedir = (centerpoint[i] - centerpoint[i-1]).normalized;
            Vertices[i * 2] = centerpoint[i] + dirH * curvewidth;
            Vertices[i * 2 + 1] = centerpoint[i] - dirH * curvewidth;
            UV[i * 2] = new Vector2(headuvfac+(1-headuvfac)*(i-headstepcount)/ (stepCount-headstepcount), 0);
            UV[i * 2 + 1] = new Vector2(headuvfac + (1 - headuvfac) * (i - headstepcount) / (stepCount-headstepcount), 1);
        }
    }


    private int MyCombination(int upidx,int downidx)
    {
        int upsum = 1;
        int downsum = 1;
        if(downidx==0 || upidx==downidx)
        {
            return 1;
        }
        for(int i=0;i<downidx;i++)
        {
            upsum = upsum * (upidx - i);
            downsum = downidx * (downidx - i);

        }
        return upsum / downsum;
    }

    private Vector3 GetBezierCurve(List<Transform> pointlist,float t)
    {
        int pointcount = pointlist.Count-1;
        Vector3 resultpoint = Vector3.zero;
        for(int i=0;i<= pointcount; i++)
        {
            resultpoint += MyCombination(pointcount, i) *Mathf.Pow((1-t),pointcount-i)* Mathf.Pow(t, i) * pointlist[i].position;
        }
        //Debug.Log(resultpoint);
        return resultpoint;
    }

    private void MeshSetup()
    {
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.uv = UV.ToArray();
    }
}
