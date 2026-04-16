using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public Gun gun;
    public MountPoint[] mountPoints;
    public float detectionRadius = 25f;
    private Transform target;

    public float maxDistance = 20f;
    public LayerMask obstacleMask;

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!target) return;

        var dashLineSize = 2f;

        foreach (var mountPoint in mountPoints)
        {
            var hardpoint = mountPoint.transform;
            var from = Quaternion.AngleAxis(-mountPoint.angleLimit / 2, hardpoint.up) * hardpoint.forward;
            var projection = Vector3.ProjectOnPlane(target.position - hardpoint.position, hardpoint.up);

            // projection line
            Handles.color = Color.white;
            Handles.DrawDottedLine(target.position, hardpoint.position + projection, dashLineSize);

            // do not draw target indicator when out of angle
            if (Vector3.Angle(hardpoint.forward, projection) > mountPoint.angleLimit / 2) return;

            // target line
            Handles.color = Color.red;
            Handles.DrawLine(hardpoint.position, hardpoint.position + projection);

            // range line
            Handles.color = Color.green;
            Handles.DrawWireArc(hardpoint.position, hardpoint.up, from, mountPoint.angleLimit, projection.magnitude);
            Handles.DrawSolidDisc(hardpoint.position + projection, hardpoint.up, .5f);
        }
#endif
    }

    Transform FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player.transform;
            }
        }

        return closest;
    }

    bool CanSeeTarget(Transform origin)
    {
        Vector3 dir = target.position - origin.position;
        float distance = dir.magnitude;

        if (distance > maxDistance)
            return false;

        dir.Normalize();

        if (Physics.Raycast(origin.position, dir, out RaycastHit hit, maxDistance, obstacleMask))
        {
            // Only valid if we hit the player first
            if (!hit.transform.CompareTag("Player"))
                return false;
        }

        return true;
    }

    void Update()
    {
        // Find a target if we don't have one
        if (!target)
        {
            target = FindTarget();
            return;
        }

        // If target is too far or invalid, drop it
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > detectionRadius)
        {
            target = null;
            return;
        }

        var aimed = true;

        foreach (var mountPoint in mountPoints)
        {
            var hardpoint = mountPoint.transform;

            if (!CanSeeTarget(hardpoint))
            {
                aimed = false;
                continue;
            }

            if (!mountPoint.Aim(target.position))
            {
                aimed = false;
            }
        }

        if (aimed)
        {
            gun.Fire();
        }
    }
}
