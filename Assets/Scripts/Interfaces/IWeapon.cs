namespace WeaponSystem
{
    /// <summary>
    /// Contract for anything that can fire a weapon.
    /// </summary>
    public interface IWeapon
    {
        void Fire();
        void StopFire();
        bool CanFire();
        int GetCurrentAmmo();
    }
}
