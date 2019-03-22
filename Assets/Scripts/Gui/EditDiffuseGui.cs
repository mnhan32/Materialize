#region

using System.Collections;
using General;
using Settings;
using UnityEngine;
using Logger = General.Logger;

#endregion

namespace Gui
{
    public class EditDiffuseGui : TexturePanelGui
    {
        private RenderTexture _avgMap;
        private Material _blitMaterial;
        private RenderTexture _blurMap;

        private Texture2D _diffuseMap;
        private Texture2D _diffuseMapOriginal;

        private EditDiffuseSettings _eds;

        private Material _material;
        private bool _settingsInitialized;

        private float _slider = 0.5f;

        private RenderTexture _tempMap;
        private int _windowId;

        private Rect _windowRect;


        protected override IEnumerator Process()
        {
            Logger.Log("Processing Diffuse");

            _blitMaterial.SetVector(ImageSizeId, new Vector4(ImageSize.x, ImageSize.y, 0, 0));

            _blitMaterial.SetTexture(MainTex, _diffuseMapOriginal);

            _blitMaterial.SetTexture(BlurTex, _blurMap);
            _blitMaterial.SetFloat(BlurContrast, _eds.BlurContrast);

            _blitMaterial.SetTexture(AvgTex, _avgMap);

            _blitMaterial.SetFloat(LightMaskPow, _eds.LightMaskPow);
            _blitMaterial.SetFloat(LightPow, _eds.LightPow);

            _blitMaterial.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
            _blitMaterial.SetFloat(DarkPow, _eds.DarkPow);

            _blitMaterial.SetFloat(HotSpot, _eds.HotSpot);
            _blitMaterial.SetFloat(DarkSpot, _eds.DarkSpot);

            _blitMaterial.SetFloat(FinalContrast, _eds.FinalContrast);

            _blitMaterial.SetFloat(FinalBias, _eds.FinalBias);

            _blitMaterial.SetFloat(ColorLerp, _eds.ColorLerp);

            _blitMaterial.SetFloat(Saturation, _eds.Saturation);

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);

            Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 11);

            TextureManager.Instance.GetTextureFromRender(_tempMap, ProgramEnums.MapType.Diffuse);

            RenderTexture.ReleaseTemporary(_tempMap);

            yield break;
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.EditDiffuseSettings = _eds;
        }

        private void Awake()
        {
            _windowRect = new Rect(10.0f, 265.0f, 300f, 540f);
        }

        public void SetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            if (projectObject.EditDiffuseSettings != null)
            {
                _eds = projectObject.EditDiffuseSettings;
            }
            else
            {
                _settingsInitialized = false;
                InitializeSettings();
            }

            StuffToBeDone = true;
        }

        private void InitializeSettings()
        {
            if (_settingsInitialized) return;
            Logger.Log("Initializing Edit Diffuse Settings");
            _eds = new EditDiffuseSettings();
            _settingsInitialized = true;
        }

        // Use this for initialization
        private void Start()
        {
            _material = new Material(ThisMaterial);
            TestObject.GetComponent<Renderer>().material = _material;

            _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));

            InitializeSettings();

            _windowId = ProgramManager.Instance.GetWindowId;
            Logger.Log($"Window ID de {name} : {_windowId}");
        }

        // Update is called once per frame
        private void Update()
        {
            if (ProgramManager.IsLocked) return;

            if (IsNewTexture)
            {
                InitializeTextures();
                IsNewTexture = false;
            }

            if (StuffToBeDone)
            {
                StartCoroutine(ProcessBlur());
                StuffToBeDone = false;
            }


            _material.SetFloat(Slider, _slider);

            _material.SetFloat(BlurContrast, _eds.BlurContrast);

            _material.SetFloat(LightMaskPow, _eds.LightMaskPow);
            _material.SetFloat(LightPow, _eds.LightPow);

            _material.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
            _material.SetFloat(DarkPow, _eds.DarkPow);

            _material.SetFloat(HotSpot, _eds.HotSpot);
            _material.SetFloat(DarkSpot, _eds.DarkSpot);

            _material.SetFloat(FinalContrast, _eds.FinalContrast);
            _material.SetFloat(FinalBias, _eds.FinalBias);

            _material.SetFloat(ColorLerp, _eds.ColorLerp);

            _material.SetFloat(Saturation, _eds.Saturation);
        }

        private void DoMyWindow(int windowId)
        {
            var offsetX = 10;
            var offsetY = 15;

            GUI.enabled = true;
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Diffuse Reveal Slider");
            _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
            offsetY += 37;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Average Color Blur Size", _eds.AvgColorBlurSize,
                out _eds.AvgColorBlurSize, 5, 100))
                StuffToBeDone = true;
            offsetY += 37;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Size", _eds.BlurSize,
                out _eds.BlurSize, 5, 100)) StuffToBeDone = true;
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Contrast", _eds.BlurContrast,
                out _eds.BlurContrast, -1.0f, 1.0f);
            offsetY += 37;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Light Mask Power", _eds.LightMaskPow,
                out _eds.LightMaskPow, 0.0f, 1.0f);
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Light", _eds.LightPow,
                out _eds.LightPow, 0.0f, 1.0f);
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Shadow Mask Power", _eds.DarkMaskPow,
                out _eds.DarkMaskPow, 0.0f, 1.0f);
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Shadow", _eds.DarkPow,
                out _eds.DarkPow, 0.0f, 1.0f);
            offsetY += 37;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Hot Spot Removal", _eds.HotSpot,
                out _eds.HotSpot, 0.0f, 1.0f);
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Dark Spot Removal", _eds.DarkSpot,
                out _eds.DarkSpot, 0.0f, 1.0f);
            offsetY += 37;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _eds.FinalContrast,
                out _eds.FinalContrast, -2.0f, 2.0f);
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _eds.FinalBias,
                out _eds.FinalBias, -0.5f, 0.5f);
            offsetY += 37;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Keep Original Color", _eds.ColorLerp,
                out _eds.ColorLerp, 0.0f, 1.0f);
            offsetY += 37;
            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Saturation", _eds.Saturation,
                out _eds.Saturation, 0.0f, 1.0f);

            GUI.enabled = true;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void OnGUI()
        {
            if (Hide) return;
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Edit Diffuse");
        }

        protected override void CleanupTextures()
        {
            RenderTexture.ReleaseTemporary(_blurMap);
            RenderTexture.ReleaseTemporary(_tempMap);
            RenderTexture.ReleaseTemporary(_avgMap);
        }

        private void InitializeTextures()
        {
            TestObject.GetComponent<Renderer>().sharedMaterial = _material;

            CleanupTextures();

            _diffuseMapOriginal = TextureManager.Instance.DiffuseMapOriginal;

            _material.SetTexture(MainTex, _diffuseMapOriginal);

            ImageSize.x = _diffuseMapOriginal.width;
            ImageSize.y = _diffuseMapOriginal.height;

            Logger.Log("Initializing Textures of size: " + ImageSize.x + "x" + ImageSize.y);

            _blurMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
            _avgMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);
        }

        private IEnumerator ProcessBlur()
        {
            while (!ProgramManager.Lock()) yield return null;

            Logger.Log("Processing Blur");

            RenderTexture.ReleaseTemporary(_tempMap);
            _tempMap = TextureManager.Instance.GetTempRenderTexture(ImageSize.x, ImageSize.y);

            _blitMaterial.SetVector(ImageSizeId, new Vector4(ImageSize.x, ImageSize.y, 0, 0));
            _blitMaterial.SetFloat(BlurContrast, 1.0f);
            _blitMaterial.SetFloat(BlurSpread, 1.0f);

            // Blur the image 1
            _blitMaterial.SetInt(BlurSamples, _eds.BlurSize);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempMap, _blurMap, _blitMaterial, 1);
            _material.SetTexture(BlurTex, _blurMap);


            _blitMaterial.SetTexture(MainTex, _diffuseMapOriginal);
            _blitMaterial.SetInt(BlurSamples, _eds.AvgColorBlurSize);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempMap, _avgMap, _blitMaterial, 1);

            _blitMaterial.SetFloat(BlurSpread, _eds.AvgColorBlurSize / 5.0f);
            _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
            Graphics.Blit(_avgMap, _tempMap, _blitMaterial, 1);
            _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
            Graphics.Blit(_tempMap, _avgMap, _blitMaterial, 1);

            _material.SetTexture(AvgTex, _avgMap);

            RenderTexture.ReleaseTemporary(_tempMap);

            yield return new WaitForSeconds(0.5f);

            ProgramManager.Unlock();
        }
    }
}