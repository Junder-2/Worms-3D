using UnityEngine;

namespace UI
{
    public class AimLine : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material mat;

        [SerializeField] private float dotScale = 1;

        public void DrawLine(Matrix4x4[] pos)
        {
            Graphics.DrawMeshInstanced(mesh, 0, mat, pos, pos.Length);
        }

        public float GetScale() => dotScale;
    }
}
