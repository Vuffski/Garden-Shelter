using UnityEngine;

namespace Naninovel.Samples
{
    public class Spinner : MonoBehaviour
    {
        public Vector3 Speed;

        private void OnDisable ()
        {
            transform.localRotation = Quaternion.identity;
        }

        private void Update ()
        {
            transform.Rotate(Speed * UnityEngine.Time.deltaTime);
        }
    }
}
