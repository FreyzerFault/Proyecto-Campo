using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using ExtensionMethods;
using JetBrains.Annotations;
using Map.Markers;
using PathFinding;
using UnityEngine;
#if UNITY_EDITOR
using MyBox;
#endif

namespace Map.Path
{
    public class PathGenerator : Singleton<MonoBehaviour>
    {
        [SerializeField] private PathFindingConfigSO pathFindingConfig;

        // Player -> 1º Marker Path
        [SerializeField] private PathRenderer playerPathRenderer;

        // Direct Path
        [SerializeField] private PathRenderer directPathRenderer;

        // Paths between markers
        [SerializeField] private PathRenderer markerPathRendererPrefab;
        [SerializeField] private PathRenderer[] markersPathRenderers = Array.Empty<PathRenderer>();
        [SerializeField] private Transform markersPathParent;

        // WORLD Markers
        [SerializeField] private GameObject marker3DPrefab;
        public MarkerObject[] markerObjects;
        [SerializeField] private CinemachineTargetGroup camTargetGroup;

        [SerializeField] private bool showDirectPath = true;


        private Transform playerTransform;
        private PathFindingAlgorithm PathFinding => pathFindingConfig.Algorithm;
        private MarkerManager MarkerManager => MarkerManager.Instance;

        private void Awake()
        {
            playerTransform = GameObject.FindWithTag("Player").transform;
            markerObjects ??= Array.Empty<MarkerObject>();
        }

        private void Start()
        {
            // Min Height depends on water height
            pathFindingConfig.minHeight = MapManager.Instance.WaterHeight;

            PathFinding.CleanCache();

            // PATH
            InitializePath();

            // First Marker Objects Spawning
            InitializeMarkerObjects();

            // Aerial Camera Targets
            InitializeCamTargets();
        }

        private void Update()
        {
            UpdatePlayerPath();

            // Update Direct Line Renderer
            UpdateDirectPath();
        }

        public event Action<PathFinding.Path[]> OnPathRenderersChange;


        // ================== LINE RENDERER ==================
        private void InitializePath()
        {
            MarkerManager.OnMarkerAdded += (_, index) =>
            {
                // Si es el 1º marcador se actualiza el camino del jugador
                if (index == 0)
                    UpdatePlayerPath();

                UpdateMarkersPath();
                OnPathRenderersChange?.Invoke();
            };
            MarkerManager.OnMarkerRemoved += (_, index) =>
            {
                // Si es el 1º marcador se actualiza el camino del jugador
                if (index == 0)
                    UpdatePlayerPath();

                UpdateMarkersPath();
            };
            MarkerManager.OnMarkerMoved += (marker, index) =>
            {
                // Si es el 1º marcador se actualiza el camino del jugador
                if (index == 0)
                    UpdatePlayerPath();

                UpdateMarkersPath();
            };
            MarkerManager.OnMarkersClear += ClearPathLines;

            UpdatePlayerPath();
            UpdateMarkersPath();
            UpdateDirectPath();
        }

        private void UpdateMarkersPath()
        {
            // Delete previous markers path
            foreach (var obj in markersPathRenderers)
                if (Application.isPlaying)
                    Destroy(obj.gameObject);
                else
                    DestroyImmediate(obj.gameObject);
            markersPathRenderers = Array.Empty<PathRenderer>();

            if (MarkerManager.MarkerCount < 2)
                return;

            var randomColor = ColorExtensions.RandomColorSaturated();

            for (var i = 0; i < MarkerManager.MarkerCount - 1; i++)
            {
                var marker = MarkerManager.Markers[i];
                var nextMarker = MarkerManager.Markers[i + 1];

                // Create PathRenderer
                var pathRenderer = Instantiate(markerPathRendererPrefab, markersPathParent);
                markersPathRenderers = markersPathRenderers.Append(pathRenderer).ToArray();

                // PATH
                pathRenderer.Path = BuildPath(
                    new[] { marker.WorldPosition, nextMarker.WorldPosition },
                    out var exploredNodes,
                    out var openNodes
                );

                // All NODES
                pathRenderer.exploredNodes = exploredNodes;
                pathRenderer.openNodes = openNodes;

                // COLOR
                pathRenderer.color = randomColor.RotateHue(i * 0.1f);
            }
        }

        private void UpdatePlayerPath()
        {
            if (MarkerManager.MarkerCount == 0) return;

            var terrainData = Terrain.activeTerrain.terrainData;
            var initialPos = terrainData.GetWorldPosition(terrainData.GetNormalizedPosition(playerTransform.position));

            // Player -> 1º Marker
            var playerDirection = playerTransform.forward;
            playerPathRenderer.Path = BuildPath(
                new[]
                {
                    initialPos,
                    MarkerManager.NextMarker.WorldPosition
                },
                out var exploredNodes,
                out var openNodes,
                new Vector2(playerDirection.x, playerDirection.z)
            );

            playerPathRenderer.exploredNodes = exploredNodes;
            playerPathRenderer.openNodes = openNodes;
        }


        // Direct Path to every marker
        private void UpdateDirectPath()
        {
            if (!showDirectPath)
            {
                directPathRenderer.Path = global::PathFinding.Path.EmptyPath;
                return;
            }

            if (MarkerManager.MarkerCount < 1) return;

            // Ignora los Markers Checked
            // Player -> Next -> Unchecked
            directPathRenderer.Path = new PathFinding.Path(MarkerManager.Markers
                .Where(marker => marker.State != MarkerState.Checked)
                .Select(marker => new Node(marker.WorldPosition))
                .Prepend(new Node(playerTransform.position))
                .ToArray()
            );
        }

        private void ClearPathLines()
        {
            foreach (Transform obj in markersPathParent)
                obj.GetComponent<PathRenderer>().ClearLine();
            playerPathRenderer.ClearLine();
            directPathRenderer.ClearLine();
        }


        // ================== PATH FINDING ==================
        private PathFinding.Path BuildPath(Vector3[] checkPoints, [CanBeNull] out List<Node> exploredNodes,
            [CanBeNull] out List<Node> openNodes, Vector2? initialDirection = null)
        {
            exploredNodes = new List<Node>();
            openNodes = new List<Node>();

            if (checkPoints.Length == 0) return global::PathFinding.Path.EmptyPath;

            return PathFinding.FindPathByCheckpoints(
                checkPoints.Select(point => new Node(
                    point,
                    size: pathFindingConfig.cellSize,
                    direction: initialDirection
                )).ToArray(),
                MapManager.Instance.terrain,
                pathFindingConfig,
                out exploredNodes,
                out openNodes
            );
        }

        // ================== WORLD MARKERS ==================

        private void InitializeMarkerObjects()
        {
            foreach (var marker in MarkerManager.Markers) SpawnMarkerInWorld(marker);

            MarkerManager.OnMarkerAdded += SpawnMarkerInWorld;
            MarkerManager.OnMarkerRemoved += DestroyMarkerInWorld;
            MarkerManager.OnMarkersClear += ClearMarkersInWorld;
        }

        private void SpawnMarkerInWorld(Marker marker, int index = -1)
        {
            var pos = marker.WorldPosition;

            var parent = GameObject.FindWithTag("Map Path");
            var markerObj = Instantiate(marker3DPrefab, pos, Quaternion.identity, parent.transform)
                .GetComponent<MarkerObject>();
            markerObj.Data = marker;

            markerObjects = index == -1
                ? markerObjects.Append(markerObj).ToArray()
                : markerObjects.InsertBetween(markerObj, index).ToArray();
        }

        private void DestroyMarkerInWorld(Marker marker, int index)
        {
            var markerObj = markerObjects[index];
            if (Application.isPlaying)
                Destroy(markerObj.gameObject);
            else
                DestroyImmediate(markerObj.gameObject);

            var list = markerObjects.ToList();
            list.RemoveAt(index);
            markerObjects = list.ToArray();
        }

        private void ClearMarkersInWorld()
        {
            foreach (var markerObj in markerObjects)
                if (Application.isPlaying)
                    Destroy(markerObj.gameObject);
                else
                    DestroyImmediate(markerObj.gameObject);

            markerObjects = Array.Empty<MarkerObject>();
        }

        // ================== CAM TARGETS ==================
        private void InitializeCamTargets()
        {
            ClearCamTargets();
            UpdateCamTargets();

            MarkerManager.OnMarkerAdded += (_, _) => UpdateCamTargets();
            MarkerManager.OnMarkerRemoved += (_, _) => UpdateCamTargets();
            MarkerManager.OnMarkersClear += ClearCamTargets;
        }

        private void UpdateCamTargets()
        {
            ClearCamTargets();
            foreach (var obj in markerObjects) camTargetGroup.AddMember(obj.transform, 1, 1);

            camTargetGroup.AddMember(playerTransform, 1, 1);
        }

        private void ClearCamTargets()
        {
            camTargetGroup.m_Targets = Array.Empty<CinemachineTargetGroup.Target>();
        }

        // =================================== UI ===================================

#if UNITY_EDITOR
        [ButtonMethod]
        private void RedoPathFinding()
        {
            PathFinding.CleanCache();

            UpdateMarkersPath();
            UpdatePlayerPath();
        }
#endif
    }
}