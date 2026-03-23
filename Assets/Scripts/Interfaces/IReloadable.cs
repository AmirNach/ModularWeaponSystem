namespace WeaponSystem
{
    /// <summary>
    /// Contract for anything that supports reloading.
    /// </summary>
    public interface IReloadable
    {
        void Reload();
        bool CanReload();
        bool IsReloading { get; }
    }
}
