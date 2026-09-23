namespace DefenderOfDreams.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int amount, UnityEngine.Vector2 hitDirection);
    }
}
