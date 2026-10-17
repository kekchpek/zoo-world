using System.Collections.Generic;
using UnityEngine;
namespace ZooWorld
{
    public sealed class ZooSimulation : MonoBehaviour
    {
        public AnimalDefinition[] Species;
        public Camera WorldCamera;
        [Min(1)] public int Capacity = 300;
        public readonly Population Population = new Population();
        private readonly List<AnimalActor> animals = new List<AnimalActor>();
        private AnimalFactory factory;
        private Arena arena;
        private ZooHud hud;
        private float untilSpawn;
        private int nextId;
        public bool Paused { get; private set; }
        public Arena Arena => arena;
        public IReadOnlyList<AnimalActor> Animals => animals;

        private void Start()
        {
            foreach (var species in Species)
                if (species == null || species.Movement == null) { Debug.LogError("Every species needs a movement profile."); enabled = false; return; }
            arena = new Arena(WorldCamera); factory = new AnimalFactory(Species);
            hud = gameObject.AddComponent<ZooHud>(); hud.Initialize(this);
            Restart();
        }
        private void Update()
        {
            if (Paused) return;
            untilSpawn -= Time.deltaTime;
            if (untilSpawn > 0f) return;
            if (Population.Alive < Capacity) Spawn(SelectSpecies(), arena.RandomPoint(.7f));
            untilSpawn = Random.Range(1f, 2f);
        }
        private AnimalDefinition SelectSpecies()
        {
            float total = 0f;
            foreach (var species in Species) total += species.SpawnWeight;
            float choice = Random.value * total;
            foreach (var species in Species) { choice -= species.SpawnWeight; if (choice <= 0f) return species; }
            return Species[Species.Length - 1];
        }
        public AnimalActor Spawn(AnimalDefinition definition, Vector3 position)
        {
            var actor = factory.Create(definition, position, ++nextId, arena, this, transform);
            animals.Add(actor); Population.Born(definition.Diet); return actor;
        }
        public void Resolve(AnimalActor first, AnimalActor second)
        {
            if (first == second || !first.IsAlive || !second.IsAlive) return;
            if (!FoodChain.CanEat(first.Definition.Diet, second.Definition.Diet)) return;
            bool firstWins = FoodChain.FirstWins(first.Definition.Diet, first.BirthId, second.Definition.Diet, second.BirthId);
            var winner = firstWins ? first : second; var loser = firstWins ? second : first;
            if (!loser.TryDie()) return;
            animals.Remove(loser); Population.Died(loser.Definition.Diet);
            hud.ShowMeal(winner.transform);
        }
        public void TogglePause()
        {
            Paused = !Paused; Time.timeScale = Paused ? 0f : 1f;
            hud.Refresh();
        }
        public void Restart()
        {
            foreach (var animal in animals) if (animal != null) animal.TryDie();
            animals.Clear(); Population.Reset(); hud.ClearMeals();
            Paused = false; Time.timeScale = 1f; untilSpawn = Random.Range(1f, 2f); nextId = 0;
            hud.Refresh();
        }
        private void OnDestroy() { Time.timeScale = 1f; factory?.Dispose(); }
    }
}
