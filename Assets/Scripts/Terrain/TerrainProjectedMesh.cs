using ExtensionMethods;
using UnityEngine;

namespace Terrain
{
    public class TerrainProjectedMesh : MonoBehaviour
    {
        public Vector2 size = Vector2.one * 10;
        public float resolution = 1;
        public float offset = 0.2f;

        public bool realTimeUpdate;
        private Mesh _mesh;
        private UnityEngine.Terrain _terrain;

        private void Awake()
        {
            _terrain = UnityEngine.Terrain.activeTerrain;
            _mesh = GetComponent<MeshFilter>().mesh;

            _mesh.GenerateMeshPlane(resolution, size);
            ProjectOnTerrain();
        }

        private void Update()
        {
            if (realTimeUpdate) ProjectOnTerrain();

            transform.LockRotationVertical();
        }

        private void ProjectOnTerrain()
        {
            _terrain.ProjectMeshInTerrain(_mesh, transform, offset);
        }
    }
}