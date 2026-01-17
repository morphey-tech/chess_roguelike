using Project.Gameplay.Gameplay.Save.Service;

namespace Project.Gameplay.Gameplay.Save.Adapter
{
    public class PlayerRunStateSaveAdapter:
        ISaveDataProvider,
        ISaveDataApplier
    {
        private readonly PlayerRunStateService _service;

        public PlayerRunStateSaveAdapter(PlayerRunStateService service)
        {
            _service = service;
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
        }
    }
}