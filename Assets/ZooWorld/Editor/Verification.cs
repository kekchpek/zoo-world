using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ZooWorld.Editor
{
    [InitializeOnLoad]
    public static class Verification
    {
        private const string Active = "ZooWorld.Verifying";
        private static int stage;
        private static double deadline;
        private static float boundaryStart;
        private static AnimalActor boundaryAnimal;
        private static int checks;
        private static EditorFrameProbe probe;
        static Verification()
        {
            if (SessionState.GetBool(Active,false)) EditorApplication.update += AttachProbe;
        }
        [MenuItem("Zoo World/Run verification")]
        public static void Run()
        {
            DomainChecks();
            SessionState.SetBool(Active,true);
            EditorSceneManager.OpenScene("Assets/ZooWorld/Scenes/ZooWorld.unity");
            EditorApplication.update -= AttachProbe; EditorApplication.update += AttachProbe;
            stage = 0; deadline = EditorApplication.timeSinceStartup + 90;
            EditorApplication.isPlaying = true;
        }
        private static void Assert(bool condition, string name)
        {
            if (!condition) throw new Exception("Verification failed: " + name);
            checks++; Debug.Log("ZOO_PASS: " + name);
        }
        public static void DomainChecks()
        {
            Assert(!FoodChain.CanEat(Diet.Prey,Diet.Prey),"prey never eat prey");
            Assert(FoodChain.FirstWins(Diet.Predator,5,Diet.Prey,1),"predator eats prey");
            Assert(!FoodChain.FirstWins(Diet.Prey,1,Diet.Predator,5),"prey loses to predator");
            Assert(FoodChain.FirstWins(Diet.Predator,1,Diet.Predator,2) &&
                !FoodChain.FirstWins(Diet.Predator,2,Diet.Predator,1),"predator duel is independent of callback order");
            var profile = ScriptableObject.CreateInstance<JumpMovement>();
            var motor = profile.CreateMotor(); float travelled = 0f;
            for (int i=0;i<70;i++) travelled += motor.Step(Vector3.right,.02f).x * .02f;
            Assert(Mathf.Abs(travelled - profile.Distance)<.001f,"jump covers configured distance in a full interval");
            var independent = profile.CreateMotor();
            Assert(independent.Step(Vector3.forward,.02f).z>0,"motors have independent state");
            UnityEngine.Object.DestroyImmediate(profile);
            var population = new Population(); population.Born(Diet.Prey); population.Born(Diet.Predator);
            population.Died(Diet.Prey); Assert(population.DeadPrey==1 && population.Alive==1,"population accounting");
            population.Reset(); Assert(population.Alive==0 && population.DeadPrey==0,"population reset");
        }
        private static void AttachProbe()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            var world = UnityEngine.Object.FindFirstObjectByType<ZooSimulation>();
            if (world == null || world.Arena == null) return;
            probe = world.gameObject.AddComponent<EditorFrameProbe>();
            probe.Tick = Poll;
            EditorApplication.update -= AttachProbe;
        }
        private static void ClickButton(string name)
        {
            Canvas.ForceUpdateCanvases();
            var button = GameObject.Find(name).GetComponent<Button>();
            var rect = button.GetComponent<RectTransform>();
            var point = rect.TransformPoint(rect.rect.center);
            var pointer = new PointerEventData(EventSystem.current) {
                position = RectTransformUtility.WorldToScreenPoint(null,point),
                button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer,hits);
            Assert(hits.Count>0 && hits[0].gameObject==button.gameObject,name+" is the first uGUI raycast hit");
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }
        private static void Poll()
        {
            try
            {
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
                var world = UnityEngine.Object.FindFirstObjectByType<ZooSimulation>();
                if (world == null || world.Arena == null) return;
                double now = EditorApplication.timeSinceStartup;
                if (stage == 0)
                {
                    // Domain reload resets static state, so run these checks inside the play domain too.
                    DomainChecks(); world.Restart();
                    world.Spawn(world.Species[0],new Vector3(0,.5f,0));
                    world.Spawn(world.Species[0],new Vector3(.6f,.5f,0));
                    stage=1; deadline=now+.4; return;
                }
                if (now<deadline) return;
                if (stage==1)
                {
                    Assert(world.Population.AlivePrey==2 && world.Population.DeadPrey==0 &&
                        Vector3.Distance(world.Animals[0].transform.position,world.Animals[1].transform.position)>.8f,
                        "prey collision survives and separates by actual physics");
                    world.Restart();
                    world.Spawn(world.Species[0],new Vector3(0,.5f,0));
                    world.Spawn(world.Species[1],new Vector3(.6f,.5f,0));
                    stage=2; deadline=now+.4;
                }
                else if (stage==2)
                {
                    Assert(world.Population.DeadPrey==1 && world.Population.AlivePredators==1,"actual predator/prey collision eats exactly once");
                    Assert(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Length>0,"uGUI labels exist");
                    world.Restart();
                    world.Spawn(world.Species[1],new Vector3(0,.5f,0));
                    world.Spawn(world.Species[1],new Vector3(.6f,.5f,0));
                    stage=3; deadline=now+.4;
                }
                else if (stage==3)
                {
                    Assert(world.Population.DeadPredators==1 && world.Population.AlivePredators==1,"predator duel kills exactly one");
                    ClickButton("PAUSE"); Assert(Time.timeScale==0f && world.Paused,"uGUI Pause click pauses");
                    ClickButton("RESTART"); Assert(Time.timeScale==1f && world.Population.Alive==0,"uGUI Restart click clears and resumes");
                    boundaryStart=world.Arena.Bounds.xMax+3;
                    boundaryAnimal=world.Spawn(world.Species[1],new Vector3(boundaryStart,.5f,0));
                    stage=4; deadline=now+1;
                }
                else if (stage==4)
                {
                    Assert(boundaryAnimal.transform.position.x<boundaryStart-1f,"offscreen animal returns inward");
                    world.Restart(); stage=5; deadline=now+2.1;
                }
                else
                {
                    Assert(world.Population.Alive>=1,"automatic spawning within two seconds");
                    Assert(world.Population.Alive<=2,"spawn cadence does not flood scene");
                    SessionState.SetBool(Active,false); EditorApplication.update-=AttachProbe;
                    if (probe != null) probe.Tick = null;
                    File.WriteAllText("Verification.json","{\"passed\":true,\"checks\":"+checks+",\"unity\":\""+Application.unityVersion+"\"}");
                    Debug.Log("ZOO_VERIFICATION_SUCCEEDED: " + checks + " checks");
                    EditorApplication.isPlaying=false;
                    if (Application.isBatchMode) EditorApplication.Exit(0);
                }
            }
            catch (Exception error)
            {
                SessionState.SetBool(Active,false); EditorApplication.update-=AttachProbe;
                    if (probe != null) probe.Tick = null;
                Debug.LogException(error); EditorApplication.isPlaying=false;
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }
}
