namespace WeaponSystem
{
    public interface IReloadable
    {
        void Reload();
        bool CanReload();
        bool IsReloading { get; }
    }
}
