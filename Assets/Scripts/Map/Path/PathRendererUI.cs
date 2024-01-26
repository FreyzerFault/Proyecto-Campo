using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using MyBox;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Map.Path
{
    public class PathRendererUI : MonoBehaviour, IPathRenderer<UILineRenderer>
    {
        // RENDERER
        [SerializeField] protected UILineRenderer linePrefab;
        [SerializeField] protected List<UILineRenderer> lineRenderers = new();

        // PATH
        [SerializeField] protected List<PathFinding.Path> paths = new();

        [SerializeField] private float lineThickness = 1f;

        private MapUIRenderer _mapUIRenderer;
        private Terrain _terrain;

        private RectTransform MapRectTransform => _mapUIRenderer.GetComponent<RectTransform>();

        public float LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                lineRenderers.ForEach(line => { line.LineThickness = value; });
            }
        }

        // ============================= INITIALIZATION =============================
        private void Awake()
        {
            _terrain = Terrain.activeTerrain;
            _mapUIRenderer = GetComponentInParent<MapUIRenderer>();
        }

        private void Start()
        {
            if (!IsEmpty) UpdateAllLines();
        }


        public List<PathFinding.Path> Paths
        {
            get => paths;
            set
            {
                paths = value;
                UpdateAllLines();
            }
        }

        public PathFinding.Path Path
        {
            get => PathCount == 0 ? PathFinding.Path.EmptyPath : paths[0];
            set
            {
                if (PathCount == 0)
                {
                    AddPath(value);
                }
                else
                {
                    paths[0] = value;
                    UpdateLine(0);
                }
            }
        }

        public int PathCount => Paths.Count;

        public bool IsEmpty => PathCount == 0 || Path.IsEmpty;

        // ============================= MODIFY LIST =============================

        public void AddPath(PathFinding.Path path, int index = -1)
        {
            if (index == -1) index = lineRenderers.Count;

            var lineRenderer = Instantiate(linePrefab, transform);
            lineRenderers.Insert(index, lineRenderer);
            paths.Insert(index, path);

            lineRenderer.color = lineRenderers.Count > 1
                ? lineRenderers[index - 1].color.RotateHue(0.1f)
                : Color.yellow;

            lineRenderer.LineThickness = LineThickness;

            // Asigna el Path al LineRenderer
            UpdateLine(index);
        }

        public void RemovePath(int index = -1)
        {
            if (index == -1) index = lineRenderers.Count - 1;

            if (Application.isPlaying)
                Destroy(lineRenderers[index].gameObject);
            else
                DestroyImmediate(lineRenderers[index].gameObject);

            lineRenderers.RemoveAt(index);
            paths.RemoveAt(index);
        }

        // ============================= UPDATE LINE RENDERERS =============================

        public void UpdateAllLines()
        {
            for (var i = 0; i < PathCount; i++)
                if (i >= lineRenderers.Count) AddPath(paths[i]);
                else UpdateLine(i);
        }


        // Asigna un Path a un LineRenderer

        public void UpdateLine(int index = -1)
        {
            if (index == -1)
            {
                UpdateAllLines();
                return;
            }

            var path = Paths[index];
            var lineRenderer = lineRenderers[index];

            if (path.NodeCount < 2)
            {
                lineRenderer.Points = Array.Empty<Vector2>();
                return;
            }

            _terrain ??= Terrain.activeTerrain;

            var normPoints = path.GetPathNormalizedPoints(_terrain);
            var localpoints = normPoints.Select(normPoint => MapRectTransform.NormalizedToLocalPoint(normPoint));

            // Update Line Renderer
            lineRenderer.Points = localpoints.ToArray();
        }


#if UNITY_EDITOR
        [ButtonMethod]
#endif
        public void ClearPaths()
        {
            for (var i = 0; i < PathCount; i++) RemovePath(i);
        }


        public void SetPath(PathFinding.Path path, int index = -1)
        {
            if (index == -1) index = lineRenderers.Count - 1;

            paths[index] = path;
            UpdateLine(index);
        }
    }
}