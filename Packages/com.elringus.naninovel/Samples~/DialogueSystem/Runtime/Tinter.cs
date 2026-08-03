using UnityEngine;

namespace Naninovel.Samples
{
    [RequireComponent(typeof(Renderer))]
    public class Tinter : MonoBehaviour
    {
        public Color Color = Color.white;

        private static readonly int colorId = Shader.PropertyToID("_Color");
        private new Renderer renderer;
        private MaterialPropertyBlock props;

        public void Tint (Color color)
        {
            renderer ??= GetComponent<Renderer>();
            props ??= new();
            renderer.GetPropertyBlock(props);
            props.SetColor(colorId, color);
            renderer.SetPropertyBlock(props);
        }

        private void Start ()
        {
            Tint(Color);
        }

        private void OnValidate ()
        {
            Tint(Color);
        }
    }
}
