using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject legPrefab;

    [Header("LegsNum")][Range(2,6)]
    public int LegNum = 2;

    private Vector3 lastPosition;

    List<Leg> legsGroup = new List<Leg>();

    [Header("Radius Of Foot")]
    public float footMoveRadius = 0.3f;
    public float footMoveRadiusOffsetRate = 0.1f;

    [Header("Leg")]
    public float legLength = 0.5f;
    public float legRootOffsetFromBody = 0.3f;
    public float legHeight = 1f;

    [Header("Shaking Animation")]
    public float shakingAmplitude = 0.15f;
    public float shakingFrequency = 10f;

    void Start()
    {
        if(legsGroup.Count != 0)
        {
            foreach(Leg leg in legsGroup)
            {
                Destroy(leg.gameObject);
            }
            legsGroup.Clear();
        }
        Initialize();
    }

    private void OnValidate()
    {
        //Initialize();
    }

    public void Initialize()
    {
        float legApartRadius = 360 / LegNum;

        while (LegNum > legsGroup.Count)
        {
            Leg leg = Instantiate(legPrefab, this.transform).AddComponent<Leg>();
            legsGroup.Add(leg);
        }
        while (LegNum < legsGroup.Count)
        {
            Leg leg = legsGroup[legsGroup.Count - 1];
            legsGroup.RemoveAt(legsGroup.Count - 1);
            Destroy(leg.gameObject);
        }

        float startLegRadius = LegNum % 2 == 0 ? legApartRadius / 2 : 0;
        for (int i = 0; i < LegNum; i++)
        {
            Vector3 dir = Quaternion.AngleAxis(startLegRadius + i * legApartRadius - 180, Vector3.up) * Vector3.right;

            legsGroup[i].Initialize(dir, footMoveRadius, legLength, legHeight, legLength + legRootOffsetFromBody, shakingAmplitude, shakingFrequency);

            legsGroup[i].UpdateMyLocalPosition(legRootOffsetFromBody * dir);

            legsGroup[i].mFootWorldPos = legsGroup[i].GetFootCenterPos();
            legsGroup[i].compareFootMoveRadius = footMoveRadius + footMoveRadiusOffsetRate * i;
        }
    }

    void Update()
    {
        Vector3 movedir = (this.transform.position - lastPosition).normalized;
        foreach (Leg leg in legsGroup)
        {
            Vector3 currentFootPos = leg.mFootWorldPos;

            float dist = Vector3.Distance(leg.GetFootCenterPos(), currentFootPos);
            if (dist > leg.compareFootMoveRadius)
            {  
                leg.UpdateFootWorldPosition(movedir);
                lastPosition = this.transform.position;
            }

            //Handle rotation
            leg.legdir = transform.rotation * leg.initialDir;

        }
    }
}
