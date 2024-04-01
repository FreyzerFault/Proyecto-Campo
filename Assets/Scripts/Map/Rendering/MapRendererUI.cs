using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using MapGeneration.TextureGeneration;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using Gradient = UnityEngine.Gradient;

namespace Map.Rendering
{
    public class MapRendererUI : MonoBehaviour
    {
        public Gradient heightGradient = new();

        // Icono del Player
        [SerializeField] private RectTransform playerSprite;

        // MARKERS
        [SerializeField] private Transform markersUIParent;

        [SerializeField] private MarkerUI markerUIPrefab;

        // Image
        [SerializeField] private Image image;

        [SerializeField] protected RectTransform frameRectTransform;

        [SerializeField] protected RectTransform imageRectTransform;

        [SerializeField] private float zoom = 1;

        private readonly List<MarkerUI> _markersUIObjects = new();

        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = value;
                UpdateZoom();
            }
        }

        private float ImageWidth =>
            image.rectTransform.rect.width * image.rectTransform.localScale.x;

        private float ImageHeight =>
            image.rectTransform.rect.height * image.rectTransform.localScale.y;

        private Vector2 ImageSize => new(ImageWidth, ImageHeight);

        // private float ZoomScale => MapManager.Instance.Zoom;

        protected MarkerManager MarkerManager => MarkerManager.Instance;

        private Vector2 OriginPoint
        {
            get
            {
                var corners = new Vector3[4];
                imageRectTransform.GetWorldCorners(corners);
                return corners[0];
            }
        }

        // ================================== UNITY ==================================


        private void Start()
        {
            imageRectTransform = image.GetComponent<RectTransform>();
            frameRectTransform = imageRectTransform.parent.GetComponent<RectTransform>();

            // SUBSCRIBERS:
            MarkerManager.OnMarkerAdded += HandleAdded;
            MarkerManager.OnMarkerRemoved += HandleRemoved;
            MarkerManager.OnMarkersClear += HandleClear;
            MapManager.Instance.OnZoomChanged += HandleZoomIn;

            // RENDER
            RenderTerrain();
            UpdateZoom();

            // MARKERS
            UpdateMarkers();
        }

        private void Update()
        {
            UpdatePlayerPoint();
        }

        private void OnDestroy()
        {
            // SUBSCRIBERS:
            MarkerManager.OnMarkerAdded -= HandleAdded;
            MarkerManager.OnMarkerRemoved -= HandleRemoved;
            MarkerManager.OnMarkersClear -= HandleClear;
            MapManager.Instance.OnZoomChanged -= HandleZoomIn;
        }

        // ============================= DEBUG =============================

        private void OnDrawGizmos()
        {
            var imagePos = imageRectTransform.PivotGlobal();
            var imageMinCorner = imageRectTransform.MinCorner();
            var sizeScaled = imageRectTransform.SizeScaled();

            // MIN y MAX
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(imageMinCorner, 20);
            // Gizmos.DrawSphere(imageMinCorner + sizeScaled, 20);
            Gizmos.DrawSphere(imageRectTransform.PivotGlobal(), 20);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                imageRectTransform.PivotGlobal(),
                imageRectTransform.PivotGlobal() + imageRectTransform.rect.position
            );
        }

        // ================================== EVENT SUSCRIBERS ==================================
        private void HandleAdded(Marker marker, int index) => InstantiateMarker(marker, index);

        private void HandleRemoved(Marker marker, int index) => DestroyMarkerUI(index);

        private void HandleClear() => ClearMarkersUI();

        // ================================== TERRAIN VISUALIZATION ==================================
        private void RenderTerrain()
        {
            var heightMap = MapManager.Instance.heightMap;
            // Create Texture of Map
            var texture = TextureGenerator.BuildTexture2D(heightMap, heightGradient);

            texture.Apply();
            image.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, heightMap.Size, heightMap.Size),
                Vector2.one / 2
            );
        }

        // ================================== PLAYER POINT ==================================
        private void UpdatePlayerPoint()
        {
            playerSprite.anchoredPosition =
                MapManager.Instance.PlayerNormalizedPosition * frameRectTransform.rect.size;
            playerSprite.rotation = MapManager.Instance.PlayerRotationForUI;

            UpdateZoom();
        }

        private void HandleZoomIn(float zoomAmount) => Zoom = Mathf.Max(0, zoom + zoomAmount);

        private void UpdateZoom()
        {
            // Asignar el pivot a la posicion del jugador normalizada para que cada movimiento sea relativo a él
            image.rectTransform.pivot = MapManager.Instance.PlayerNormalizedPosition;

            // Posicionar el mapa en el centro del frame
            var frameCenter = frameRectTransform.TransformPoint(frameRectTransform.rect.size / 2);
            imageRectTransform.position = frameCenter;

            // Escalar el mapa relativo al centro donde está el Player
            image.rectTransform.localScale = new Vector3(zoom, zoom, 1);

            // La flecha del player se escala al revés para que no se vea afectada por el zoom
            playerSprite.localScale = new Vector3(1 / zoom, 1 / zoom, 1);

            // Reajustar el offset hacia los bordes para que no se vea el fondo
            var imgCorners = new Vector3[4];
            var frameCorners = new Vector3[4];
            imageRectTransform.GetWorldCorners(imgCorners);
            frameRectTransform.GetWorldCorners(frameCorners);

            var distanceToLowerCorner = Vector3.Max(imgCorners[0] - frameCorners[0], Vector3.zero);
            var distanceToUpperCorner = Vector3.Max(frameCorners[2] - imgCorners[2], Vector3.zero);
            image.rectTransform.position += distanceToUpperCorner - distanceToLowerCorner;
        }

        // ================================== MARKERS ==================================
        private void ClearMarkersUI()
        {
            GetComponentsInChildren<MarkerUI>()
                .ToList()
                .ForEach(
                    marker =>
                    {
                        if (Application.isPlaying)
                            Destroy(marker.gameObject);
                        else
                            DestroyImmediate(marker.gameObject);
                    }
                );
        }

        private void UpdateMarkers()
        {
            ClearMarkersUI();

            // Se instancian de nuevo todos los markers
            foreach (var marker in MarkerManager.Markers) InstantiateMarker(marker);
        }

        private void InstantiateMarker(Marker marker, int index = -1)
        {
            var markerUI = Instantiate(markerUIPrefab, markersUIParent).GetComponent<MarkerUI>();
            markerUI.Marker = marker;
            _markersUIObjects.Insert(index == -1 ? _markersUIObjects.Count : index, markerUI);
        }

        private void DestroyMarkerUI(int index)
        {
            var markerUI = _markersUIObjects[index];

            if (Application.isPlaying)
                Destroy(markerUI.gameObject);
            else
                DestroyImmediate(markerUI.gameObject);

            _markersUIObjects.RemoveAt(index);
        }

        public void ToggleMarkers(bool value)
        {
            markersUIParent.gameObject.SetActive(value);
        }

#if UNITY_EDITOR
        // ================================== BUTTONS on INSPECTOR ==================================
        [ButtonMethod]
        protected void UpdateMap()
        {
            RenderTerrain();
            UpdateZoom();
            UpdatePlayerPoint();
        }

        // BUTTONS
        [ButtonMethod]
        protected void UpdatePlayerPointInMap()
        {
            UpdatePlayerPoint();
        }

        [ButtonMethod]
        protected void ZoomMapToPlayerPosition()
        {
            UpdateZoom();
        }

        [ButtonMethod]
        protected void ReRenderTerrain()
        {
            RenderTerrain();
        }
#endif
    }
}