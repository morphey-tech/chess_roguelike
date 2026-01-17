using Project.Gameplay.Gameplay.Save.Service;

namespace Project.Gameplay.Gameplay.Save.Adapter
{
    public class PlayerMetaProgressSaveAdapter:
        ISaveDataProvider,
        ISaveDataApplier
    {
        private readonly PlayerMetaProgressService _metaService;

        public PlayerMetaProgressSaveAdapter(PlayerMetaProgressService metaService)
        {
            _metaService = metaService;
        }

        public void Populate(SaveSnapshot snapshot)
        {
            snapshot.MetaProgress = _metaService.Model;
        }

        public void Apply(SaveSnapshot snapshot)
        {
            if (snapshot.MetaProgress == null)
            {
                return;
            }

            _metaService.Configure(snapshot.MetaProgress);
        }
    }
}