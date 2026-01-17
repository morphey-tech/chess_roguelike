using Project.Gameplay.Gameplay.Save.Service;

namespace Project.Gameplay.Gameplay.Save.Adapter
{
    public sealed class PlayerLoadoutSaveAdapter:
        ISaveDataProvider,
        ISaveDataApplier
    {
        private readonly PlayerLoadoutService _service;

        public PlayerLoadoutSaveAdapter(PlayerLoadoutService service)
        {
            _service = service;
        }

        public void Populate(SaveSnapshot snapshot)
        {
            snapshot.Loadout = _service.Current;
        }

        public void Apply(SaveSnapshot snapshot)
        {
            if (snapshot.Loadout == null)
            {
                return;
            }
            _service.Configure(snapshot.Loadout);
        }
    }
}