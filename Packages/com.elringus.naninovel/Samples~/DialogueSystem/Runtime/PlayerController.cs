using UnityEngine;
using UnityEngine.InputSystem;

namespace Naninovel.Samples
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Setup")]
        public CharacterController Character;
        public Transform Geometry;

        [Header("Behaviour")]
        public float MoveSpeed = 5f;
        public float DashSpeed = 15f;

        private void Update ()
        {
            var input = 0f;
            if (Keyboard.current.leftArrowKey.isPressed) input = -1f;
            else if (Keyboard.current.rightArrowKey.isPressed) input = 1f;

            if (input != 0)
            {
                var speed = Keyboard.current.shiftKey.isPressed ? DashSpeed : MoveSpeed;
                Character.Move(new Vector3(input, 0, 0) * speed * UnityEngine.Time.deltaTime);
                Geometry.rotation = Quaternion.Euler(new(0, input >= 0 ? 0 : 180, 0));
            }
        }
    }
}
