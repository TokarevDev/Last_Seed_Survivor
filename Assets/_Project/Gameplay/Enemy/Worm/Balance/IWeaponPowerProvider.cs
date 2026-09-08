namespace Game.Gameplay.Enemy.Worm.Balance
{
    public interface IWeaponPowerProvider
    {
        WeaponPowerSnapshot GetCurrentPower();
    }

}
