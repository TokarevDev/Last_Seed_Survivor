namespace Game.Core.Combat
{
    public interface IDamageable<TDamage>
    {
        void TakeDamage(in TDamage damage);
    }

}
