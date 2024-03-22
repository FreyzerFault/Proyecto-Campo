﻿///Credit judah4
///Sourced from - http://forum.unity3d.com/threads/color-picker.267043/


using TMPro;

namespace UnityEngine.UI.Extensions.ColorPicker
{
#if UNITY_2022_1_OR_NEWER
    [RequireComponent(typeof(TMP_Text))]
#else
    [RequireComponent(typeof(Text))]
#endif
    public class ColorLabel : MonoBehaviour
    {
        public ColorPickerControl picker;

        public ColorValues type;

        public string prefix = "R: ";
        public float minValue;
        public float maxValue = 255;

        public int precision;

#if UNITY_2022_1_OR_NEWER
        private TMP_Text label;
#else
        private Text label;
#endif
        private void Awake()
        {
#if UNITY_2022_1_OR_NEWER
            label = GetComponent<TMP_Text>();
#else
            label = GetComponent<Text>();
#endif
            if (!label)
                Debug.LogError(
                    $"{gameObject.name} does not have a Text component assigned for the {nameof(ColorLabel)}"
                );
        }

        private void OnEnable()
        {
            if (Application.isPlaying && picker != null)
            {
                picker.onValueChanged.AddListener(ColorChanged);
                picker.onHSVChanged.AddListener(HSVChanged);
            }
        }

        private void OnDestroy()
        {
            if (picker != null)
            {
                picker.onValueChanged.RemoveListener(ColorChanged);
                picker.onHSVChanged.RemoveListener(HSVChanged);
            }
        }

        private void ColorChanged(Color color)
        {
            UpdateValue();
        }

        private void HSVChanged(float hue, float sateration, float value)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            if (picker == null)
            {
                label.text = prefix + "-";
            }
            else
            {
                var value = minValue + picker.GetValue(type) * (maxValue - minValue);

                label.text = prefix + ConvertToDisplayString(value);
            }
        }

        private string ConvertToDisplayString(float value)
        {
            if (precision > 0) return value.ToString("f " + precision);
            return Mathf.FloorToInt(value).ToString();
        }
    }
}