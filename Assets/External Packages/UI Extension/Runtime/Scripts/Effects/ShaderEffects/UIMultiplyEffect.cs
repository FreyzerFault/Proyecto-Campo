﻿/// Credit 00christian00
/// Sourced from - http://forum.unity3d.com/threads/any-way-to-show-part-of-an-image-without-using-mask.360085/#post-2332030


namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Effects/Extensions/UIMultiplyEffect")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIMultiplyEffect : MonoBehaviour
    {
        private MaskableGraphic mGraphic;

        // Use this for initialization
        private void Start()
        {
            SetMaterial();
        }

        public void OnValidate()
        {
            SetMaterial();
        }

        public void SetMaterial()
        {
            mGraphic = GetComponent<MaskableGraphic>();
            if (mGraphic != null)
            {
                if (mGraphic.material == null || mGraphic.material.name == "Default UI Material")
                    //Applying default material with UI Image Crop shader
                    mGraphic.material = new Material(ShaderLibrary.GetShaderInstance("UI Extensions/UIMultiply"));
            }
            else
            {
                Debug.LogError("Please attach component to a Graphical UI component");
            }
        }
    }
}