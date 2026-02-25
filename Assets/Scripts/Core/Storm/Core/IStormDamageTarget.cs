namespace Project.Core.Core.Storm.Core
{
    public interface IStormDamageTarget
    {
        int MaxHP { get; }

        void TakeDamage(int damage);
    }
}