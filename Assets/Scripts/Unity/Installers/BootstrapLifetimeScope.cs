using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Installers
{
    /// <summary>
    /// Скоуп для Init сцены. Наследуется от RootLifetimeScope.
    /// </summary>
    public class BootstrapLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Init сцена простая — всё приходит из RootLifetimeScope
            // Можно добавить специфичные для init сервисы если нужно
        }
    }
}
