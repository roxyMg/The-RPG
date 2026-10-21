using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HarborV2.Editor
{
    [InitializeOnLoad]
    public static class HarborTestRunner
    {
        private const string Key="HarborV2.TestRunning";
        static HarborTestRunner()
        {
            EditorApplication.update+=()=>
            {
                if(!SessionState.GetBool(Key,false))return;
                if(EditorApplication.isPaused)EditorApplication.isPaused=false;
                if(!EditorApplication.isPlaying)return;
                if(Object.FindAnyObjectByType<HarborPlaytest>()==null)
                {
                    // Limit simulated frame advance during batch shader/import stalls; gameplay physics settings stay unchanged.
                    Time.captureDeltaTime=1f/60;
                    Debug.Log("HARBOR_V2_TEST_ATTACH: deterministic 60 Hz test clock");
                    new GameObject("V2 automated route and original player test").AddComponent<HarborPlaytest>();
                }
            };
        }
        public static void Run()
        {
            EditorSceneManager.OpenScene(HarborBuilder.ScenePath);
            HarborBuilder.Validate();Application.runInBackground=true;Time.timeScale=1;
            SessionState.SetBool(Key,true);EditorApplication.isPaused=false;
            Debug.Log("HARBOR_V2_TEST_REQUESTED");
            EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
        }
        public static void BuildAndRun(){HarborBuilder.Create();Run();}
    }
}
