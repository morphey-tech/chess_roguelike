using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Save.Service;

namespace Project.Gameplay.Gameplay.Save.Adapter
{
    public class PlayerRunStateSaveAdapter:
        ISaveDataProvider,
        ISaveDataApplier
    {
        private readonly PlayerRunStateService _service;
        private readonly RandomService _random;

        public PlayerRunStateSaveAdapter(PlayerRunStateService service, RandomService random)
        {
            _service = service;
            _random = random;
        }

        public void Populate(SaveSnapshot snapshot)
        {
            snapshot.Run = _service.HasRun
                ? _service.Current
                : null;
        }

        public void Apply(SaveSnapshot snapshot)
        {
            if (snapshot.Run == null)
            {
                _service.Clear();
                return;
            }
            _service.Configure(snapshot.Run);
            _random.SetSeed(snapshot.Run.Seed);
        }
    }
}