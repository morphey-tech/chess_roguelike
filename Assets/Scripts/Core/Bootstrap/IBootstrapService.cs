using System;
using Cysharp.Threading.Tasks;

namespace Project.Core.Bootstrap
{
    public interface IBootstrapService
    {
        GameState CurrentState { get; }
        IObservable<GameState> OnStateChanged { get; }
        IObservable<float> OnInitProgress { get; }

        UniTask InitializeAsync();
        UniTask StartGameAsync();
        UniTask ReturnToMainMenuAsync();
        UniTask QuitGameAsync();
    }
}

