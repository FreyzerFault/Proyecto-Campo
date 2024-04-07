using System;
using System.Collections.Generic;
using Map.PathFinding;
using UnityEngine;
using DavidUtils.ExtensionMethods;
#if UNITY_EDITOR
using MyBox;
#endif

namespace Map
{
    public class PathFindingGenerator : PathGenerator
    {
        [SerializeField] private PathFindingConfigSo pathFindingConfig;

        private PathFindingAlgorithm PathFinding => pathFindingConfig.Algorithm;

        private new void Start()
        {
            // Min Height depends on water height
            pathFindingConfig.minHeight = MapManager.Instance.WaterHeight;
            
            // Redo PathFinding from zero
            PathFinding.CleanCache();
            
            // Subscribe to PathFindingConfig changes
            pathFindingConfig.OnFineTune += RedoPathFinding;
            
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            pathFindingConfig.OnFineTune -= RedoPathFinding;
        }

        // ================== PATH FINDING ==================

        #region BUILD PATH

        protected override Path BuildPath(
            Vector3 start,
            Vector3 end,
            Vector2? initialDirection = null,
            Vector2? endDirection = null
        )
        {
            if (start == end) return Path.EmptyPath;

            var startNode = new Node(
                start,
                size: pathFindingConfig.cellSize,
                direction: initialDirection
            );
            var endNode = new Node(end, size: pathFindingConfig.cellSize, direction: endDirection);
            return PathFinding.FindPath(startNode, endNode, MapManager.Terrain, pathFindingConfig);
        }

        protected override List<Path> BuildPath(
            Vector3[] checkPoints,
            Vector2[] initialDirections = null
        )
        {
            var pathsBuilt = new List<Path>();
            var haveDirections = initialDirections is { Length: > 0 };

            for (var i = 1; i < checkPoints.Length; i++)
            {
                var start = checkPoints[i - 1];
                var end = checkPoints[i];

                var startDirection =
                    haveDirections && initialDirections.Length > i - 1
                        ? initialDirections[i - 1]
                        : Vector2.zero;
                var endDirection =
                    haveDirections && initialDirections.Length > i
                        ? initialDirections[i]
                        : Vector2.zero;

                pathsBuilt.Add(BuildPath(start, end, startDirection, endDirection));
            }

            return pathsBuilt;
        }

        #endregion

        #region LEGAL POS
        
        public bool IsLegalPos(Vector2 normPos) => IsLegalPos(MapManager.Terrain.GetWorldPosition(normPos));

        public bool IsLegalPos(Vector3 pos) =>
            PathFinding.IsLegal(new Node(pos, size: pathFindingConfig.cellSize), pathFindingConfig);

        #endregion


#if UNITY_EDITOR
        [ButtonMethod]
#endif
        private void RedoPathFinding()
        {
            PathFinding.CleanCache();

            RedoPath();
        }
    }
}