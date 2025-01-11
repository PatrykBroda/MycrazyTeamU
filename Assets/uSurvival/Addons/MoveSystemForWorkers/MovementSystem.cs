using System.Collections.Generic;
using UnityEngine;

namespace GFFAddons
{
    public enum PathTypes
    {
        lener, loop
    }

    public class MovementSystem : MonoBehaviour
    {
        public PathTypes pathType;
        public int movementDirection = 1;
        //public int moveingTo = 0;
        public Transform[] pathElements;

        public float timeAwait = 0.5f;
        private bool await = false;

        public void OnDrawGizmos()
        {
            if (pathElements == null || pathElements.Length < 2)
            {
                return;
            }

            for (int i = 1; i < pathElements.Length; i++)
            {
                Gizmos.DrawLine(pathElements[i - 1].position, pathElements[i].position);
            }

            if (pathType == PathTypes.loop)
            {
                Gizmos.DrawLine(pathElements[0].position, pathElements[pathElements.Length - 1].position);
            }
        }

        public IEnumerator<Transform> GetNextPathPoint(int moveingTo)
        {
            if (pathElements == null || pathElements.Length < 1)
            {
                yield break;
            }

            while (true)
            {
                yield return pathElements[moveingTo];

                if (pathElements.Length == 1)
                {
                    continue;
                }

                if (pathType == PathTypes.lener)
                {
                    if (moveingTo <= 0)
                    {
                        movementDirection = 1;

                        //to do
                    }
                    else if (moveingTo >= pathElements.Length - 1)
                    {
                        movementDirection = -1;
                    }
                }

                //yield return new WaitForSeconds(timeAwait);

                moveingTo = moveingTo + movementDirection;

                if (pathType == PathTypes.loop)
                {
                    if (moveingTo >= pathElements.Length)
                    {
                        moveingTo = 0;
                    }

                    if (moveingTo < 0)
                    {
                        moveingTo = pathElements.Length - 1;
                    }
                }
            }
        }
    }
}


