using System;
using System.Collections.Generic;
using Project.Core.Core.Configs.Stats;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Configs;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FigureStatsFactory : IFigureStatsFactory
    {
        private readonly ConfigProvider _configProvider;
        private StatsConfigRepository? _statsRepo;

        [Inject]
        private FigureStatsFactory(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public FigureStats Create(string statsId)
        {
            _statsRepo ??= _configProvider.GetSync<StatsConfigRepository>("stats_conf");
            StatsConfig? cfg = _statsRepo.Get(statsId);
            if (cfg == null)
            {
                throw new Exception($"Stats config not found: {statsId}");
            }
            return Build(cfg);
        }

        private static FigureStats Build(StatsConfig cfg)
        {
            List<AttackProfile> attacks = new(cfg.Attacks?.Length ?? 0);
            if (cfg.Attacks != null)
            {
                foreach (AttackConfig ac in cfg.Attacks)
                {
                    attacks.Add(new AttackProfile(
                        ac.Type,
                        ac.Damage,
                        ac.Range,
                        ac.Targeting,
                        ac.Delivery,
                        ac.Pattern,
                        ac.ProjectileId));
                }
            }

            float baseAttack = cfg.Attack > 0 ? 
                cfg.Attack : (attacks.Count > 0 ? attacks[0].Damage : 0);
            float baseDefence = cfg.Defence;
            float baseEvasion = cfg.Evasion;
            return new FigureStats(cfg.MaxHp, attacks, baseAttack,
                baseDefence, baseEvasion);
        }

        public void ClearCache()
        {
            _statsRepo = null;
        }
    }
}
