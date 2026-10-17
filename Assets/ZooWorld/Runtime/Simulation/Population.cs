using System;
namespace ZooWorld
{
    public sealed class Population
    {
        public int AlivePrey { get; private set; }
        public int AlivePredators { get; private set; }
        public int DeadPrey { get; private set; }
        public int DeadPredators { get; private set; }
        public int Alive => AlivePrey + AlivePredators;
        public event Action Changed;
        public void Born(Diet diet)
        {
            if (diet == Diet.Prey) AlivePrey++; else AlivePredators++;
            Changed?.Invoke();
        }
        public void Died(Diet diet)
        {
            if (diet == Diet.Prey) { AlivePrey--; DeadPrey++; }
            else { AlivePredators--; DeadPredators++; }
            Changed?.Invoke();
        }
        public void Reset()
        {
            AlivePrey = AlivePredators = DeadPrey = DeadPredators = 0;
            Changed?.Invoke();
        }
    }
}
