using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leg : MonoBehaviour
{

    [Header("Assigned by controller")]
    public float footMoveRadius = 1;
    public float compareFootMoveRadius = 1;
    public float footRadiusOffset = 0;
    public float legLength = 0;
    public float footOffsetFromBody = 0;
    public float legHeight = 2;
    public float shakingAmplitude;
    public float shakingFrequency;

    [HideInInspector]
    public Vector3 legdir;
    [HideInInspector]
    public Vector3 initialDir;

    [HideInInspector]
    public Vector3 mFootWorldPos;

    LineRenderer lineRenderer;

    private float roundTripValue;
    private Vector3 shakingOffset = Vector3.zero;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector3 legRootPos = transform.position;
        Vector3 middlePoint = mFootWorldPos;
        
        float newRoundTripValue = Mathf.PingPong(Time.time * shakingFrequency, 2) - 1;
        if((roundTripValue < 0 && newRoundTripValue > 0) || (roundTripValue > 0 && newRoundTripValue < 0) || shakingOffset == Vector3.zero)
        {
            shakingOffset = Random.onUnitSphere;
        }
        roundTripValue = newRoundTripValue;
        middlePoint += shakingOffset * shakingAmplitude;
        Vector3 footPos = mFootWorldPos - Vector3.up * legHeight;
        Vector3[] points = new Vector3[3] { legRootPos, middlePoint, footPos };

        Vector3[] curvePoints = GetBezierCurvePoints(points);

        lineRenderer.positionCount = curvePoints.Length;
        lineRenderer.SetPositions(curvePoints);
    }
    void OnDrawGizmosSelected()
    {
        // green is leg root
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.transform.position + Vector3.up, 0.1f);
        //red is foot
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(mFootWorldPos - Vector3.up, 0.1f);
        Gizmos.DrawWireSphere(mFootWorldPos - Vector3.up, footMoveRadius);
        // blue is a wheel of foot center
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetFootCenterPos() - Vector3.up * 1.5f, 0.1f);
        Gizmos.DrawWireSphere(transform.parent.transform.position - Vector3.up * 1.5f, footOffsetFromBody);
    }

    public void Initialize(Vector3 dir, float footMoveRadius, float legLength, float legHeight, float footOffsetFromBody, float shakingAmplitude, float shakingFrequency)
    {
        this.initialDir = dir;
        this.legdir = dir;
        this.footMoveRadius = footMoveRadius;
        this.legLength = legLength;
        this.legHeight = legHeight;
        this.footOffsetFromBody = footOffsetFromBody;
        this.shakingAmplitude = shakingAmplitude;
        this.shakingFrequency = shakingFrequency;

    }

    public void UpdateFootWorldPosition(Vector3 movedir)
    {
        mFootWorldPos = GetFootCenterPos() + movedir * (footMoveRadius - 0.01f);
    }

    public void UpdateMyLocalPosition(Vector3 v)
    {
        this.transform.localPosition = v;
    }

    public Vector3 GetFootCenterPos()
    {
        return transform.position + legLength * legdir;
    }

    public Vector3[] GetBezierCurvePoints(Vector3[] points)
    {
        Vector3[] result = new Vector3[10];
        for (int i = 0; i < 10; i++)
        {
            float t = i / 10f;
            result[i] = (1 - t) * (1 - t) * points[0] + 2 * (1 - t) * t * points[1] + t * t * points[2];
        }
        return result;
    }
}
