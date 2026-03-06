using System;

namespace Project.Core.Core.Configs.Artifacts
{
    /// <summary>
    /// Tags for artifact categorization and synergies.
    /// </summary>
    [Flags]
    public enum ArtifactTag
    {
        None = 0,

        /// <summary>Offensive artifacts (damage, attack buffs).</summary>
        Attack = 1,

        /// <summary>Defensive artifacts (shields, armor, HP).</summary>
        Defense = 2,

        /// <summary>Healing and regeneration.</summary>
        Heal = 4,

        /// <summary>Movement and positioning.</summary>
        Movement = 8,

        /// <summary>Summoning creatures.</summary>
        Summon = 16,

        /// <summary>Crowd control (stun, slow, push).</summary>
        Control = 32,

        /// <summary>Economy (crowns, rewards).</summary>
        Economy = 64,

        /// <summary>Utility (dodge, crit, armor pen).</summary>
        Utility = 128,

        /// <summary>Death-triggered effects.</summary>
        Death = 256,

        /// <summary>Battle start/end effects.</summary>
        Battle = 512
    }

    /// <summary>
    /// Extension methods for ArtifactTag.
    /// </summary>
    public static class ArtifactTagExtensions
    {
        /// <summary>
        /// Check if artifact has a specific tag.
        /// </summary>
        public static bool HasTag(this ArtifactTag tags, ArtifactTag tag)
        {
            return (tags & tag) != 0;
        }

        /// <summary>
        /// Count how many tags are set.
        /// </summary>
        public static int CountTags(this ArtifactTag tags)
        {
            int count = 0;
            int flag = (int)ArtifactTag.Attack;
            int maxFlag = (int)ArtifactTag.Battle;
            
            while (flag <= maxFlag)
            {
                if (((int)tags & flag) != 0)
                {
                    count++;
                }
                flag <<= 1;
            }
            return count;
        }
    }
}
