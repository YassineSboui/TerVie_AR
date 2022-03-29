using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

// -----------------------------------------------------------
public class PlaceManager : MonoBehaviour
{
    // --------Variables 
    public GameObject placementIndicator;

    [SerializeField]
    private ARSessionOrigin aRSessionOrigin;

    [SerializeField]
    private ARRaycastManager aRRaycastManager;

    private Pose placementPose;

    private bool placementPoseIsValid = false;

    [SerializeField]
    TrackableType trackableType = TrackableType.Planes;

    public GameObject ObjectToPlace;

    public GameObject Circle;

    public GameObject tree;

    public GameObject Space;

    public GameObject MulchCircle;

    public GameObject DemieLunecircle;

    GameObject limits;

    GameObject place;

    GameObject treeSpace;

    GameObject Spacetokeep;

    GameObject MulchPlace;

    public int MethodNumber;

    LineRenderer lineRenderer;

    List<Vector3> PointsList = new List<Vector3>();

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (this.PointsList.Count == 2)
        {
            this.lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
            this.lineRenderer.tag = "Clone";
            this.lineRenderer.startColor = Color.black;
            this.lineRenderer.endColor = Color.black;
            this.lineRenderer.startWidth = 0.01f;
            this.lineRenderer.endWidth = 0.01f;
            this.lineRenderer.positionCount = 0;
            this.lineRenderer.useWorldSpace = true;
            this.lineRenderer.loop = true;
        }
        foreach (Touch touch in Input.touches)
        {
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }
        }
        var circles = GameObject.FindGameObjectsWithTag("Circle");
        if (
            placementPoseIsValid &&
            Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began
        &&
        circles.Length == 0
        )
        {
            PlaceObject();

        }
    }

    public void HandleInputData(int val)
    {
        this.MethodNumber = val;


    }



    // Reset ALl-----------------------------------------------------------
    public void ClearALL()

    {
        var clones = GameObject.FindGameObjectsWithTag("Clone");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }
        var circles = GameObject.FindGameObjectsWithTag("Circle");
        foreach (var circle in circles)
        {
            Destroy(circle);
        }
        this.PointsList.Clear();

        this.lineRenderer.positionCount = 0;
        this.lineRenderer.SetPositions(this.PointsList.ToArray());
    }

    public void Commencer()

    {


        if (this.MethodNumber == 0)
        {
            Zai();
        }
        if (this.MethodNumber == 1)
        {
            Mulch();
        }
        if (this.MethodNumber == 2)
        {
            FixationBiologique();
        }
        if (this.MethodNumber == 3)
        {
            DemieLune();
        }
    }

    public void Mulch()
    {
        float Maxdistance = 0;

        float north = this.PointsList[0].z;
        float south = this.PointsList[0].z;
        float est = this.PointsList[0].x;
        float west = this.PointsList[0].x;
        foreach (var item in this.PointsList)
        {
            if (item.x < west) west = item.x;
            if (item.x > est) est = item.x;
            if (item.z > north) north = item.z;
            if (item.z < south) south = item.z;
        }
        float CentreX = (est + west) / 2;
        float CentreY = (north + south) / 2;
        var PointOrigine = new Vector2(CentreX, CentreY);
        Debug.Log(PointOrigine);
        foreach (var item in this.PointsList)
        {
            var e = new Vector2(item.x, item.z);
            float dist = Vector2.Distance(PointOrigine, e);
            if (dist > Maxdistance)
            {
                Debug.Log(e);
                Maxdistance = dist;
            }
        }
        Debug.Log(Maxdistance);
        var clones = GameObject.FindGameObjectsWithTag("Clone");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }
        if (Maxdistance > 0)
        {
            treeSpace =
                Instantiate(tree,
                new Vector3(CentreX, placementPose.position.y, CentreY),
                placementPose.rotation) as
                GameObject;
            treeSpace.transform.localScale =
                new Vector3(Maxdistance, Maxdistance, Maxdistance);

            Spacetokeep =
                Instantiate(Space,
                new Vector3(CentreX, placementPose.position.y, CentreY),
                placementPose.rotation) as
                GameObject;
            Spacetokeep.transform.localScale =
                new Vector3(Maxdistance + 0.15f,
                    Maxdistance + 0.15f,
                    Maxdistance + 0.15f);

            MulchPlace =
                Instantiate(MulchCircle,
                new Vector3(CentreX, placementPose.position.y, CentreY),
                placementPose.rotation) as
                GameObject;
            MulchPlace.transform.localScale =
                new Vector3(Maxdistance + 0.35f,
                    Maxdistance + 0.35f,
                    Maxdistance + 0.35f);
        }
        this.PointsList.Clear();
        this.lineRenderer.positionCount = 0;
        this.lineRenderer.SetPositions(this.PointsList.ToArray());
        Maxdistance = 0;
    }

    // show where to plant (Zai methode) -----------------------------------------------------------
    public void Zai()
    {
        var clones = GameObject.FindGameObjectsWithTag("Clone");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }

        //surface de cercle
        var CircleArea = 0.15 * 0.15 * Mathf.PI;

        //surface de polygone
        var AllArea = SuperficieIrregularPolygon();
        var nb = AllArea / CircleArea;

        float north = this.PointsList[0].z;
        float south = this.PointsList[0].z;
        float est = this.PointsList[0].x;
        float west = this.PointsList[0].x;
        foreach (var item in this.PointsList)
        {
            if (item.x < west) west = item.x;
            if (item.x > est) est = item.x;
            if (item.z > north) north = item.z;
            if (item.z < south) south = item.z;
        }
        float CentreX = (est + west) / 2;
        float CentreY = (north + south) / 2;

        float i = 0;
        if (nb > 1)
        {
            while (CentreY + i < north)
            {
                float right = 0;
                while (CentreX + right < est)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX + right,
                            0,
                            CentreY + i),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(Circle,
                        new Vector3(CentreX + right,
                            placementPose.position.y,
                            CentreY + i),
                        placementPose.rotation);
                        right = right + 1.1f;
                    }
                    else
                    {
                        right = right + 0.01f;
                    }
                }
                float left = 1.1f;
                while (CentreX - left > west)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX - left,
                            0,
                            CentreY + i),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(Circle,
                        new Vector3(CentreX - left,
                            placementPose.position.y,
                            CentreY + i),
                        placementPose.rotation);
                        left = left + 1.1f;
                    }
                    else
                    {
                        left = left + 0.01f;
                    }
                }
                i = i + 1.1f;
            }

            float j = 1.1f;
            while (CentreY - j > south)
            {
                float right = 0;
                while (CentreX + right < est)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX + right,
                            0,
                            CentreY - j),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(Circle,
                        new Vector3(CentreX + right,
                            placementPose.position.y,
                            CentreY - j),
                        placementPose.rotation);
                        right = right + 1.1f;
                    }
                    else
                    {
                        right = right + 0.01f;
                    }
                }
                float left = 1.1f;
                while (CentreX - left > west)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX - left,
                            0,
                            CentreY - j),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(Circle,
                        new Vector3(CentreX - left,
                            placementPose.position.y,
                            CentreY - j),
                        placementPose.rotation);
                        left = left + 1.1f;
                    }
                    else
                    {
                        left = left + 0.01f;
                    }
                }
                j = j + 1.1f;
            }
            this.PointsList.Clear();
        }
    }

    // show where to plant (Demie-Lune methode)
    public void DemieLune()
    {
        var clones = GameObject.FindGameObjectsWithTag("Clone");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }

        //surface de cercle
        var CircleArea = 2 * Mathf.PI;

        //surface de polygone
        var AllArea = SuperficieIrregularPolygon();
        var nb = AllArea / CircleArea;
        float north = this.PointsList[0].z;
        float south = this.PointsList[0].z;
        float est = this.PointsList[0].x;
        float west = this.PointsList[0].x;
        foreach (var item in this.PointsList)
        {
            if (item.x < west) west = item.x;
            if (item.x > est) est = item.x;
            if (item.z > north) north = item.z;
            if (item.z < south) south = item.z;
        }
        float CentreX = (est + west) / 2;
        float CentreY = (north + south) / 2;

        float i = 0;
        if (nb > 1)
        {
            while (CentreY + i < north)
            {
                float right = 0;
                while (CentreX + right < est)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX + right,
                            0,
                            CentreY + i),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(DemieLunecircle,
                        new Vector3(CentreX + right,
                            placementPose.position.y,
                            CentreY + i),
                        placementPose.rotation);

                        right = right + 8f;
                    }
                    else
                    {
                        right = right + 0.01f;
                    }
                }
                float left = 8f;
                while (CentreX - left > west)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX - left,
                            0,
                            CentreY + i),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(DemieLunecircle,
                        new Vector3(CentreX - left,
                            placementPose.position.y,
                            CentreY + i),
                        placementPose.rotation);

                        left = left + 8f;
                    }
                    else
                    {
                        left = left + 0.01f;
                    }
                }
                i = i + 8f;
            }

            float j = 8f;
            while (CentreY - j > south)
            {
                float right = 0;
                while (CentreX + right < est)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX + right,
                            0,
                            CentreY - j),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(DemieLunecircle,
                        new Vector3(CentreX + right,
                            placementPose.position.y,
                            CentreY - j),
                        placementPose.rotation);

                        right = right + 8f;
                    }
                    else
                    {
                        right = right + 0.01f;
                    }
                }
                float left = 8f;
                while (CentreX - left > west)
                {
                    if (
                        InsidePolygon(new Vector3(CentreX - left,
                            0,
                            CentreY - j),
                        this.PointsList.ToArray())
                    )
                    {
                        Instantiate(DemieLunecircle,
                        new Vector3(CentreX - left,
                            placementPose.position.y,
                            CentreY - j),
                        placementPose.rotation);

                        left = left + 8f;
                    }
                    else
                    {
                        left = left + 0.01f;
                    }
                }
                j = j + 8f;
            }
            this.PointsList.Clear();
        }
    }

    // show where to plant (FixationBiologique methode)
    public void FixationBiologique()
    {
        var clones = GameObject.FindGameObjectsWithTag("Clone");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }

        //surface de cercle
        var CircleArea = 0.2 * 0.2 * Mathf.PI;

        //surface de polygone
        var AllArea = SuperficieIrregularPolygon();
        var nb = AllArea / CircleArea;

        float north = this.PointsList[0].z;
        float south = this.PointsList[0].z;
        float est = this.PointsList[0].x;
        float west = this.PointsList[0].x;
        foreach (var item in this.PointsList)
        {
            if (item.x < west) west = item.x;
            if (item.x > est) est = item.x;
            if (item.z > north) north = item.z;
            if (item.z < south) south = item.z;
        }
        // float CentreX = (est + west) / 2;
        // float CentreY = (north + south) / 2;

        float i = 0;
        if (nb > 1)
        {
            while (south + i < north)
            {
                float right = 0;
                while (west + right < est)
                {
                    if (
                        InsidePolygon(new Vector3(west + right,
                            0,
                            south + i),
                        this.PointsList.ToArray())
                    )
                    {
                        treeSpace =
                            Instantiate(Circle,
                            new Vector3(west + right,
                                placementPose.position.y,
                                south + i),
                            placementPose.rotation) as
                            GameObject;
                        treeSpace.transform.localScale =
                            new Vector3(1.25f, 1.25f, 1.25f);
                        right = right + 5;
                    }
                    else
                    {
                        right = right + 0.01f;
                    }
                }

                i = i + 5;
            }
            this.PointsList.Clear();
        }
    }

    // -----------------------------------------------------------
    void PlaceObject()
    {
        limits =
            Instantiate(ObjectToPlace,
            placementPose.position,
            placementPose.rotation) as
            GameObject;
        this.PointsList.Add(placementPose.position);
        this.lineRenderer.positionCount = this.PointsList.Count;
        this.lineRenderer.SetPositions(this.PointsList.ToArray());
    }

    // Area Of Polygon-----------------------------------------------------------
    float SuperficieIrregularPolygon()
    {
        float temp = 0;
        int i = 0;
        for (; i < this.PointsList.Count; i++)
        {
            if (i != this.PointsList.Count - 1)
            {
                float mulA = this.PointsList[i].x * this.PointsList[i + 1].z;
                float mulB = this.PointsList[i + 1].x * this.PointsList[i].z;
                temp = temp + (mulA - mulB);
            }
            else
            {
                float mulA = this.PointsList[i].x * this.PointsList[0].z;
                float mulB = this.PointsList[0].x * this.PointsList[i].z;
                temp = temp + (mulA - mulB);
            }
        }
        temp *= 0.4f;
        temp = Mathf.Abs(temp);
        return Mathf.Abs(temp);
    }

    // check if a point is inside a given polygon-----------------------------------------------------------
    bool InsidePolygon(Vector3 point, Vector3[] polygon)
    {
        Vector2 PointOrigine = new Vector2(point.x, point.z);
        float somme = 0;
        for (int i = 0; i < (polygon.Length) - 1; i++)
        {
            Vector2 PointA = new Vector2(polygon[i].x, polygon[i].z);
            Vector2 PointB = new Vector2(polygon[i + 1].x, polygon[i + 1].z);

            Vector2 side1 = PointA - PointOrigine;
            Vector2 side2 = PointB - PointOrigine;
            somme = somme + Vector2.Angle(side1, side2);
        }

        Vector2 Pointa =
            new Vector2(polygon[polygon.Length - 1].x,
                polygon[polygon.Length - 1].z);
        Vector2 Pointb = new Vector2(polygon[0].x, polygon[0].z);

        somme =
            somme + Vector2.Angle(Pointa - PointOrigine, Pointb - PointOrigine);
        return ((somme == 360) || (somme == -360));
    }

    // Update placement indicator position-----------------------------------------------------------
    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator
                .transform
                .SetPositionAndRotation(placementPose.position,
                placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    // Update Camera view center-----------------------------------------------------------s
    private void UpdatePlacementPose()
    {
        var screenCenter =
            aRSessionOrigin
                .camera
                .ViewportToScreenPoint(new Vector3(0.4f, 0.4f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, trackableType);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = aRSessionOrigin.camera.transform.forward;
            var cameraBearing =
                new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}
