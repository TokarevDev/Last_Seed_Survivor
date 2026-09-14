namespace Game.Gameplay.Enemy.Worm.Balance
{
    public interface IWeaponPowerSource
    {
        WeaponPowerSnapshot GetCurrentPower();
    }
}
