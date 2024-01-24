using UnityEngine;
using UnityEngine.InputSystem;

namespace Map
{
    public class MapInputController : MonoBehaviour
    {
        [SerializeField] private GameObject minimapParent;
        [SerializeField] private GameObject fullScreenMapParent;

        [SerializeField] private MapUIRenderer minimapUI;
        [SerializeField] private MapUIRenderer fullscreenMapUI;

        private MapUIRenderer Minimap =>
            minimapUI != null ? minimapUI : GameObject.FindWithTag("Minimap").GetComponent<MapUIRenderer>();

        private MapUIRenderer FullScreenMap => fullscreenMapUI != null
            ? fullscreenMapUI
            : GameObject.FindWithTag("Map Fullscreen").GetComponent<MapUIRenderer>();


        private void Awake()
        {
            minimapUI = GameObject.FindWithTag("Minimap")?.GetComponent<MapUIRenderer>();
            fullscreenMapUI = GameObject.FindWithTag("Map Fullscreen")?.GetComponent<MapUIRenderer>();
        }


        private void CloseMap()
        {
            fullScreenMapParent.SetActive(false);
            minimapParent.SetActive(true);
            GameManager.Instance.State = GameManager.GameState.Playing;
        }

        private void OpenMap()
        {
            minimapParent.SetActive(false);
            fullScreenMapParent.SetActive(true);
            GameManager.Instance.State = GameManager.GameState.Paused;
        }

        // INPUTS
        private void OnToggleMap()
        {
            if (GameManager.Instance.IsPlaying)
            {
                OpenMap();
            }
            else if (GameManager.Instance.IsPaused)
            {
                if (FullScreenMap.MarkerManager.numSelectedMarkers > 0)
                    FullScreenMap.MarkerManager.DeselectAllMarkers();
                else
                    CloseMap();
            }
        }

        private void OnZoomInOut(InputValue value)
        {
            if (minimapParent.activeSelf)
                Minimap.ZoomScale += Mathf.Clamp(value.Get<float>(), -1, 1) / 10f;
        }

        private void OnDeselectAll()
        {
            if (GameManager.Instance.State == GameManager.GameState.Paused)
                FullScreenMap.MarkerManager.DeselectAllMarkers();
        }
    }
}