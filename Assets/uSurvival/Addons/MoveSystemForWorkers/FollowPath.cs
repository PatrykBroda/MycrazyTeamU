using System.Collections.Generic;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public enum MovementType { moveing, lerping }

    public class FollowPath : MonoBehaviour
    {
        public MovementType type = MovementType.moveing;
        public MovementSystem myPath;
        public float speed = 1;
        public float maxDistance = .1f;
        public float temeAwait = 0.5f;
        public AudioSource source;

        //gff
        public int moveingTo = 0;

        private IEnumerator<Transform> pointInPath;

        private void Start()
        {
            if (myPath == null)
            {
                Debug.Log("Select Path");
                return;
            }

            pointInPath = myPath.GetNextPathPoint(moveingTo);
            pointInPath.MoveNext();

            if (pointInPath.Current == null)
            {
                Debug.Log("Need Points");
                return;
            }

            transform.position = pointInPath.Current.position;
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                source.enabled = true;

                if (pointInPath == null || pointInPath.Current == null)
                {
                    return;
                }

                if (type == MovementType.moveing)
                {
                    transform.position = Vector3.MoveTowards(transform.position, pointInPath.Current.position, Time.deltaTime * speed);
                }
                else if (type == MovementType.lerping)
                {
                    transform.position = Vector3.Lerp(transform.position, pointInPath.Current.position, Time.deltaTime * speed);
                }

                var distanceSqure = (transform.position - pointInPath.Current.position).sqrMagnitude;

                if (distanceSqure < maxDistance * maxDistance)
                {
                    pointInPath.MoveNext();
                }

                transform.LookAt(pointInPath.Current.position);
            }
            else
            {
                source.enabled = false;
            }
        }

        public void LookAtY(Vector3 position)
        {
            transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
        }
    }
}


