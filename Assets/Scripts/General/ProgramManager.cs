using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gui;
using StandaloneFileBrowser;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

namespace General
{
    public class ProgramManager : MonoBehaviour
    {
        public static ProgramManager Instance;
        public char PathChar { get; private set; }
        public Light MainLight;
        private const string LastPathKey = nameof(LastPathKey);
        [HideInInspector] public string LastPath;
        private static readonly int GamaCorrectionId = Shader.PropertyToID("_GamaCorrection");
        public HDRenderPipeline RenderPipeline;

        //Nao remover, alguns shaders dependem disso
        private const float GamaCorrection = 1f;

        #region Settings

        [Header("Settings")] public int TargetFps = 30;
        public Cubemap StartCubeMap;

        #endregion

        #region Gui Objects

        public List<GameObject> SceneObjects = new List<GameObject>();

        [HideInInspector] [Header("Gui Objects")]
        public GameObject CommandListExecutorObject;

        [HideInInspector] public GameObject ControlsGuiObject;
        [HideInInspector] public GameObject MainGuiObject;
        [HideInInspector] public GameObject SettingsGuiObject;
        [HideInInspector] public GameObject MaterialGuiObject;
        public GameObject TestObject;

        #endregion

        #region Suported Formats

        public static readonly string[] LoadFormats =
        {
            "png", "jpg", "jpeg", "tga", "bmp", "exr"
        };

        public static readonly ExtensionFilter[] ImageLoadFilter =
        {
            new ExtensionFilter("Image Files", LoadFormats)
        };

        public static readonly string[] SaveFormats =
        {
            "png", "jpg", "jpeg", "tga", "exr"
        };

        public static readonly ExtensionFilter[] ImageSaveFilter =
        {
            new ExtensionFilter("Image Files", SaveFormats)
        };

        #endregion


        private ProgramManager()
        {
        }

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = TargetFps;
            Shader.SetGlobalFloat(GamaCorrectionId, GamaCorrection);
            LastPath = PlayerPrefs.HasKey(LastPathKey) ? PlayerPrefs.GetString(LastPathKey) : null;

            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
                PathChar = '\\';
            else PathChar = '/';
        }

        private IEnumerator Start()
        {
            ControlsGuiObject = FindMonoBehaviour<ControlsGui>().gameObject;
            MainGuiObject = FindMonoBehaviour<MainGui>().gameObject;
            SettingsGuiObject = FindMonoBehaviour<SettingsGui>().gameObject;
            CommandListExecutorObject = FindMonoBehaviour<CommandListExecutor>().gameObject;
            MaterialGuiObject = FindMonoBehaviour<MaterialGui>().gameObject;

            ActivateObjects();
            InvokeRepeating(nameof(SlowUpdate), 0.1f, 0.2f);
            yield return StartCoroutine(GetHdrpCoroutine());
        }

        private T FindMonoBehaviour<T>() where T : MonoBehaviour
        {
            foreach (var sceneObject in SceneObjects)
            {
                var behaviour = sceneObject.GetComponent<T>();
                if (behaviour != null) return behaviour;
            }

            return null;
        }

        private void Update()
        {
        }

        private void SlowUpdate()
        {
            if (Application.targetFrameRate != TargetFps)
            {
                Application.targetFrameRate = TargetFps;
            }

            PlayerPrefs.SetString(LastPathKey, LastPath);
        }

        private void ActivateObjects()
        {
            TestObject.SetActive(true);
            MainGuiObject.SetActive(true);
            SettingsGuiObject.SetActive(true);
            ControlsGuiObject.SetActive(true);
            MaterialGuiObject.SetActive(false);
            CommandListExecutorObject.SetActive(true);
        }

        public void OpenFullMaterial()
        {
            MainGui.Instance.CloseWindows();
            TextureManager.Instance.FixSize();
            TextureManager.Instance.SetFullMaterial();
            MaterialGuiObject.GetComponent<MaterialGui>().Initialize();
            MaterialGuiObject.SetActive(true);
        }

        private IEnumerator GetHdrpCoroutine()
        {
            RenderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            var maxTries = 10;
            while (RenderPipeline == null)
            {
                RenderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
                yield return new WaitForSeconds(0.1f);
                maxTries--;

                if (maxTries != 0) continue;
                yield break;
            }
        }
    }
}