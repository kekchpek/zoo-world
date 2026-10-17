using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace ZooWorld.Editor
{
    [InitializeOnLoad]
    public static class ProjectBuilder
    {
        static ProjectBuilder()
        {
            if (!System.IO.File.Exists("Assets/ZooWorld/Scenes/ZooWorld.unity"))
                EditorApplication.delayCall += Create;
        }
        [MenuItem("Zoo World/Create demo scene")]
        public static void Create()
        {
            if (System.IO.File.Exists("Assets/ZooWorld/Scenes/ZooWorld.unity")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Top-down camera", typeof(Camera), typeof(AudioListener));
            var camera = cameraObject.GetComponent<Camera>(); cameraObject.tag = "MainCamera";
            camera.transform.position = new Vector3(0,25,0); camera.transform.rotation = Quaternion.Euler(90,0,0);
            camera.orthographic = true; camera.orthographicSize = 12f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.10f,.19f,.15f);
            camera.nearClipPlane = .1f; camera.farClipPlane = 100f;
            var lightObject = new GameObject("Sun", typeof(Light)); var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.1f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(65,-30,0);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.58f,.68f,.6f);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "Habitat";
            floor.transform.position = new Vector3(0,-.12f,0); floor.transform.localScale = new Vector3(150,.2f,150);
            UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());
            var ground = new Material(Shader.Find("Standard")) { color = new Color(.14f,.27f,.21f) };
            AssetDatabase.CreateAsset(ground,"Assets/ZooWorld/Settings/Ground.mat"); floor.GetComponent<Renderer>().sharedMaterial = ground;
            var hop = ScriptableObject.CreateInstance<JumpMovement>();
            AssetDatabase.CreateAsset(hop,"Assets/ZooWorld/Settings/FrogJump.asset");
            var crawl = ScriptableObject.CreateInstance<LinearMovement>();
            AssetDatabase.CreateAsset(crawl,"Assets/ZooWorld/Settings/SnakeLinear.asset");
            var frog = ScriptableObject.CreateInstance<AnimalDefinition>(); frog.DisplayName = "Frog"; frog.Diet = Diet.Prey;
            frog.Shape = BodyShape.Frog; frog.Color = new Color(.5f,.85f,.25f); frog.SpawnWeight = 3f; frog.Movement = hop;
            AssetDatabase.CreateAsset(frog,"Assets/ZooWorld/Settings/Frog.asset");
            var snake = ScriptableObject.CreateInstance<AnimalDefinition>(); snake.DisplayName = "Snake"; snake.Diet = Diet.Predator;
            snake.Shape = BodyShape.Snake; snake.Color = new Color(1f,.49f,.18f); snake.SpawnWeight = 1f; snake.Movement = crawl;
            AssetDatabase.CreateAsset(snake,"Assets/ZooWorld/Settings/Snake.asset");
            var world = new GameObject("Zoo World • Composition root").AddComponent<ZooSimulation>();
            world.WorldCamera = camera; world.Species = new[] { frog,snake };
            EditorSceneManager.SaveScene(scene,"Assets/ZooWorld/Scenes/ZooWorld.unity");
            EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene("Assets/ZooWorld/Scenes/ZooWorld.unity",true)};
            PlayerSettings.companyName = "Zoo World"; PlayerSettings.productName = "Zoo World";
            PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
            Debug.Log("ZOO_SCENE_CREATED");
        }
        public static void Build()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] {"Assets/ZooWorld/Scenes/ZooWorld.unity"},
                locationPathName = "Builds/ZooWorld.app", target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Build failed: " + report.summary.result);
            Debug.Log("ZOO_BUILD_SUCCEEDED");
        }
    }
}
