using System.Collections.Generic;
using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.World;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Appear;
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
        
        [Header("Collectors")]
        [SerializeField] private WorldObjectCollector _worldObjectCollector;
        [SerializeField] private UiObjectCollector _uiObjectCollector;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureMessagePipe(builder);
            ConfigureInput(builder);
            ConfigureCollectors(builder);
            ConfigureServices(builder);
            builder.RegisterBuildCallback(RunBootstrap);
        }
        
        private void ConfigureMessagePipe(IContainerBuilder builder)
        {
            MessagePipeOptions options = builder.RegisterMessagePipe();
            options.HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Throw;
            options.EnableCaptureStackTrace = true;
        }
        
        private void ConfigureInput(IContainerBuilder builder)
        {
            builder.RegisterInstance(_inputActions);
        }

        private void ConfigureCollectors(IContainerBuilder builder)
        {
            if (_worldObjectCollector != null)
            {
                builder.RegisterInstance(_worldObjectCollector)
                    .AsSelf()
                    .As<IWorldRoot>();
            }
            if (_uiObjectCollector != null)
            {
                builder.RegisterInstance(_uiObjectCollector);
            }
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            builder.Register<BoardAppearAnimationFactory>(Lifetime.Singleton)
                .WithParameter(new List<IBoardAppearAnimationStrategy>
                {
                    new BoardNoneAppearStrategy(),
                    new BoardWaveAppearStrategy()
                });
            builder.Register<BoardSpawnService>(Lifetime.Singleton);
            builder.Register<CellsSpawnService>(Lifetime.Singleton);
        }

        private void RunBootstrap(IObjectResolver resolver)
        { 
            resolver.Inject(_worldObjectCollector.RequireObjectByType<MonoSceneBootstrap>());
        }
    }
}
