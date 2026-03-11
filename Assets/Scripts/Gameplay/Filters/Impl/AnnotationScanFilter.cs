using Cysharp.Threading.Tasks;
 using Project.Core.Core.Annotation;
using Project.Core.Core.Filters;
using VContainer;

namespace Project.Gameplay.Gameplay.Filters.Impl
{
    public sealed class AnnotationScanFilter : IApplicationFilter
    {
        private readonly AnnotationScanService _scanAnnotationService;

        [Inject]
        private AnnotationScanFilter(AnnotationScanService scanAnnotationService)
        {
            _scanAnnotationService = scanAnnotationService;
        }

        public async UniTask RunAsync()
        {
            _scanAnnotationService.ScanAssembly("Gameplay");
            _scanAnnotationService.ScanAssembly("LiteUI");
            await UniTask.Yield();
        }
    }
}