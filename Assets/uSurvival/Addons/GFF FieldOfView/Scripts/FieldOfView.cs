using System;
using System.Collections;
using UnityEngine;
using uSurvival;

namespace uSurvival
{
    public partial class Monster
    {
        public FieldOfView fieldOfView;
    }
}

public class FieldOfView : MonoBehaviour
{
    public Monster entity;

    public float radius = 20;
    [Range(0, 360)]public float angle = 120;
    public float minDist = 3;
    public GameObject viewPosition;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;
    public Entity targetRef;

    private void OnEnable()
    {
        if (viewPosition == null) viewPosition = gameObject;
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    canSeePlayer = true;
                else
                    canSeePlayer = false;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;
    }

    public bool CanSeeTarget(Entity target)
    {
        targetRef = target;

        if (target != null)
        {
            Vector3 directionToTarget = (target.transform.position - viewPosition.transform.position).normalized;

            if (Vector3.Angle(viewPosition.transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (!Physics.Raycast(viewPosition.transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    return true;
                }
            }
        }

        if (entity.target != null && entity.target.Equals(target) && Vector3.Distance(transform.position, target.transform.position) > minDist)
        {
            entity.target = null;
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (entity == null || entity != gameObject.GetComponent<Monster>())
        {
            entity = gameObject.GetComponent<Monster>();
            entity.fieldOfView = this;
        }
    }
#endif
}
