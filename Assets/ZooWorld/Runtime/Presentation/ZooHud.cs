using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ZooWorld
{
    public sealed class ZooHud : MonoBehaviour
    {
        private ZooSimulation simulation;
        private Canvas canvas;
        private Font font;
        private Text prey, predators, live, status, pause;
        private readonly List<Meal> meals = new List<Meal>();
        private sealed class Meal { public Transform Target; public Text Label; public float Until; }
        private static readonly Color Ink = new Color(.87f,.94f,.88f);
        private static readonly Color Muted = new Color(.57f,.69f,.62f);
        public void Initialize(ZooSimulation simulation)
        {
            this.simulation = simulation;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Zoo World • uGUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440,900); scaler.matchWidthOrHeight = .5f;
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            events.transform.SetParent(transform, false);
            Panel("Header", new Vector2(0,1), new Vector2(1,1), new Vector2(0,-170), Vector2.zero, new Color(.035f,.09f,.075f,.97f));
            Label("Z O O  W O R L D", new Vector2(0,1), new Vector2(38,-30), new Vector2(550,50), 34, Ink);
            Label("A SMALL WORLD. A WILD FOOD CHAIN.", new Vector2(0,1), new Vector2(40,-86), new Vector2(650,25), 14, Muted);
            live = Label("", new Vector2(0,1), new Vector2(40,-125), new Vector2(580,26), 17, Ink);
            Label("EATEN / PREY", new Vector2(1,1), new Vector2(-360,-36), new Vector2(170,24), 14, Muted);
            prey = Label("00", new Vector2(1,1), new Vector2(-360,-68), new Vector2(160,65), 42, new Color(.6f,.9f,.37f));
            Label("EATEN / PREDATORS", new Vector2(1,1), new Vector2(-180,-36), new Vector2(170,24), 14, Muted);
            predators = Label("00", new Vector2(1,1), new Vector2(-180,-68), new Vector2(160,65), 42, new Color(1f,.63f,.3f));
            Panel("Footer", Vector2.zero, new Vector2(1,0), Vector2.zero, new Vector2(0,94), new Color(.035f,.09f,.075f,.97f));
            Label("● FROG  /  PREY       ● SNAKE  /  PREDATOR", Vector2.zero, new Vector2(40,68), new Vector2(630,25), 15, Ink);
            status = Label("", Vector2.zero, new Vector2(40,38), new Vector2(740,25), 14, Muted);
            pause = Button("PAUSE", new Vector2(1,0), new Vector2(-320,68), simulation.TogglePause);
            Button("RESTART", new Vector2(1,0), new Vector2(-164,68), simulation.Restart);
            simulation.Population.Changed += Refresh;
            Refresh();
        }
        private RectTransform Rect(GameObject go, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(canvas.transform, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high; return rect;
        }
        private void Panel(string name, Vector2 min, Vector2 max, Vector2 low, Vector2 high, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            Rect(go,min,max,low,high); go.GetComponent<Image>().color = color;
        }
        private Text Label(string text, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject(text.Length > 0 ? text : "Counter", typeof(RectTransform), typeof(Text));
            var rect = Rect(go,anchor,anchor,Vector2.zero,Vector2.zero); rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var label = go.GetComponent<Text>(); label.text = text; label.font = font; label.fontSize = fontSize;
            label.color = color; label.raycastTarget = false; label.horizontalOverflow = HorizontalWrapMode.Overflow;
            return label;
        }
        private Text Button(string title, Vector2 anchor, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = Rect(go,anchor,anchor,Vector2.zero,Vector2.zero); rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(138,46);
            go.GetComponent<Image>().color = new Color(.16f,.28f,.22f);
            go.GetComponent<Button>().onClick.AddListener(action);
            var label = Label(title,anchor,position,new Vector2(138,46),15,Ink);
            label.alignment = TextAnchor.MiddleCenter;
            // Parent the title to its button so hierarchy and navigation remain clear.
            label.rectTransform.SetParent(go.transform, true);
            return label;
        }
        public void Refresh()
        {
            var p = simulation.Population;
            prey.text = p.DeadPrey.ToString("00"); predators.text = p.DeadPredators.ToString("00");
            live.text = $"LIVING   {p.AlivePrey} prey   /   {p.AlivePredators} predators";
            status.text = simulation.Paused ? "PAUSED • Take a closer look at the ecosystem." : p.Alive >= simulation.Capacity ?
                "Habitat full • Spawning resumes when space becomes available." : "LIVE • A new animal every 1–2 seconds. Watch nature take its course.";
            pause.text = simulation.Paused ? "RESUME" : "PAUSE";
        }
        public void ShowMeal(Transform target)
        {
            var label = Label("Tasty!",Vector2.zero,Vector2.zero,new Vector2(130,42),23,new Color(1f,.88f,.48f));
            label.alignment = TextAnchor.MiddleCenter;
            meals.Add(new Meal { Target = target, Label = label, Until = Time.time + 1.1f });
        }
        private void LateUpdate()
        {
            for (int i = meals.Count - 1; i >= 0; i--)
            {
                var meal = meals[i];
                if (meal.Target == null || !meal.Target.gameObject.activeInHierarchy || Time.time >= meal.Until)
                { Destroy(meal.Label.gameObject); meals.RemoveAt(i); continue; }
                Vector3 screen = simulation.WorldCamera.WorldToScreenPoint(meal.Target.position);
                meal.Label.rectTransform.position = new Vector3(screen.x, screen.y - 28f, 0f);
                meal.Label.rectTransform.pivot = new Vector2(.5f,1f);
            }
        }
        public void ClearMeals() { foreach (var meal in meals) Destroy(meal.Label.gameObject); meals.Clear(); }
        private void OnDestroy() { if (simulation != null) simulation.Population.Changed -= Refresh; }
    }
}
