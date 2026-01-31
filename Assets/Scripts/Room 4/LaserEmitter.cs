using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEmitter : MonoBehaviour
{
    [Header("Beam")]
    public float maxDistance = 40f;
    public int maxBounces = 6;
    public float beamWidth = 0.02f;
    public Color beamColor = Color.cyan;

    [Header("Layers")]
    public LayerMask hitMask = ~0;         
    public LayerMask mirrorMask;           

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.startWidth = lr.endWidth = beamWidth;
    }

    void Update() => Simulate();

    void Simulate()
    {
        var points = new List<Vector3>();
        Vector3 pos = transform.position;
        Vector3 dir = transform.forward;

        points.Add(pos);

        for (int i = 0; i < maxBounces; i++)
        {
            if (!Physics.Raycast(pos, dir, out var hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                points.Add(pos + dir * maxDistance);
                break;
            }

            points.Add(hit.point);

            var receiver = hit.collider.GetComponentInParent<LaserReceiver>();
            if (receiver)
            {
                receiver.RegisterHit();
                break;
            }

            bool isMirror = hit.collider.GetComponentInParent<LaserMirror>() != null;
            if (!isMirror && mirrorMask.value != 0)
                isMirror = ((mirrorMask.value & (1 << hit.collider.gameObject.layer)) != 0);

            if (isMirror)
            {
                dir = Vector3.Reflect(dir, hit.normal).normalized;
                pos = hit.point + dir * 0.001f;
                continue;
            }
            break;
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }
}
