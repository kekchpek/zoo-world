namespace ZooWorld
{
    public static class FoodChain
    {
        // Stable birth IDs make predator duels independent of collision callback order.
        public static bool FirstWins(Diet first, int firstId, Diet second, int secondId)
        {
            if (first == Diet.Prey) return false;
            return second == Diet.Prey || firstId < secondId;
        }
        public static bool CanEat(Diet first, Diet second) => first == Diet.Predator || second == Diet.Predator;
    }
}
