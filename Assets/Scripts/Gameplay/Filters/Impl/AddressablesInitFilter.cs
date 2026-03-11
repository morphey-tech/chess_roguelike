using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Filters;
using UnityEngine.AddressableAssets;

namespace Project.Gameplay.Gameplay.Filters.Impl
{
    public class AddressablesInitFilter : IApplicationFilter
    {
        public async UniTask RunAsync()
        {
            //TODO: обработать исключение
            try 
            {
                await Addressables.InitializeAsync().ToUniTask();
            } catch (Exception) 
            {
                throw new OperationCanceledException();
            }
        }
    }
}