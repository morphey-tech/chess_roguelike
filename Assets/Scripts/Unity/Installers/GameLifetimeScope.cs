using LiteUI.UI.Service;
using MessagePipe;
using Project.Gameplay.Assets;
using Project.Gameplay.Save;
using Project.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;
        
        [Header("Input")]
        [SerializeField] private WorldObjectCollector _worldObjectCollector;
        [SerializeField] private UiObjectCollector _uiObjectCollector;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureMessagePipe(builder);
            ConfigureStates(builder);
            ConfigureCollectors(builder);
            ConfigureConfigs(builder);
            ConfigureInput(builder);
            ConfigurePlayer(builder);
            ConfigureServices(builder);
            ConfigureQuests(builder);
            builder.RegisterBuildCallback(RunBootstrap);
        }
        
        private void ConfigureMessagePipe(IContainerBuilder builder)
        {
            MessagePipeOptions options = builder.RegisterMessagePipe();
            options.HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Throw;
            options.EnableCaptureStackTrace = true;
        }
        
        private void ConfigureStates(IContainerBuilder builder)
        {
        }

        private void ConfigureCollectors(IContainerBuilder builder)
        {
            if (_worldObjectCollector != null)
                builder.RegisterInstance(_worldObjectCollector);
            
            if (_uiObjectCollector != null)
                builder.RegisterInstance(_uiObjectCollector);
        }

        private void ConfigureConfigs(IContainerBuilder builder)
        {
        }

        private void ConfigureInput(IContainerBuilder builder)
        {
            builder.RegisterInstance(_inputActions);
        }
        
        private void ConfigurePlayer(IContainerBuilder builder)
        {
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            builder.Register<AssetService>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<SaveSystem>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void ConfigureQuests(IContainerBuilder builder)
        {
        }

        private void RunBootstrap(IObjectResolver resolver)
        { 
            resolver.Inject(_worldObjectCollector.RequireObjectByType<MonoSceneBootstrap>());
        }
    }
}
