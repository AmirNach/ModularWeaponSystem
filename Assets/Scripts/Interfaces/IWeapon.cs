namespace WeaponSystem
{
    public interface IWeapon
    {
        void Shoot();
        void StopShoot();
        bool CanShoot();
        int GetCurrentAmmo();
    }
}
