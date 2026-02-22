using System;
using Project.Core.Core.ShrinkingZone.Core;
using UnityEngine;

namespace Project.Core.Core.ShrinkingZone.SaveLoad
{
    /// <summary>
    /// Менеджер сохранения/загрузки состояния shrinking zone
    /// </summary>
    public class ZoneShrinkSaveLoad
    {
        private const string SaveKey = "ZoneShrinkState";

        /// <summary>
        /// Сохранить состояние в PlayerPrefs (для простых случаев)
        /// </summary>
        public static void SaveToPlayerPrefs(ZoneShrinkState state)
        {
            PlayerPrefs.SetInt($"{SaveKey}_State", (int)state.State);
            PlayerPrefs.SetInt($"{SaveKey}_CurrentLayer", state.CurrentLayer);
            PlayerPrefs.SetInt($"{SaveKey}_StepInLayer", state.StepInLayer);
            PlayerPrefs.SetInt($"{SaveKey}_ActivationTurn", state.ActivationTurn);
            
            if (state.FirstDamageTurn.HasValue)
            {
                PlayerPrefs.SetInt($"{SaveKey}_FirstDamageTurn", state.FirstDamageTurn.Value);
                PlayerPrefs.SetInt($"{SaveKey}_FirstDamageTurn_Null", 0);
            }
            else
            {
                PlayerPrefs.SetInt($"{SaveKey}_FirstDamageTurn_Null", 1);
            }
            
            PlayerPrefs.SetInt($"{SaveKey}_FirstDamageDealt", state.FirstDamageDealt ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Загрузить состояние из PlayerPrefs
        /// </summary>
        public static ZoneShrinkState LoadFromPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey($"{SaveKey}_State"))
                return null;

            return new ZoneShrinkState
            {
                State = (ZoneState)PlayerPrefs.GetInt($"{SaveKey}_State"),
                CurrentLayer = PlayerPrefs.GetInt($"{SaveKey}_CurrentLayer"),
                StepInLayer = PlayerPrefs.GetInt($"{SaveKey}_StepInLayer"),
                ActivationTurn = PlayerPrefs.GetInt($"{SaveKey}_ActivationTurn"),
                FirstDamageTurn = PlayerPrefs.GetInt($"{SaveKey}_FirstDamageTurn_Null") == 1
                    ? (int?)null
                    : PlayerPrefs.GetInt($"{SaveKey}_FirstDamageTurn"),
                FirstDamageDealt = PlayerPrefs.GetInt($"{SaveKey}_FirstDamageDealt") == 1
            };
        }

        /// <summary>
        /// Сохранить состояние в JSON
        /// </summary>
        public static string SaveToJson(ZoneShrinkState state)
        {
            return JsonUtility.ToJson(new ZoneShrinkStateSerializable
            {
                State = (int)state.State,
                CurrentLayer = state.CurrentLayer,
                StepInLayer = state.StepInLayer,
                ActivationTurn = state.ActivationTurn,
                FirstDamageTurn = state.FirstDamageTurn,
                FirstDamageDealt = state.FirstDamageDealt
            });
        }

        /// <summary>
        /// Загрузить состояние из JSON
        /// </summary>
        public static ZoneShrinkState LoadFromJson(string json)
        {
            var serializable = JsonUtility.FromJson<ZoneShrinkStateSerializable>(json);
            
            return new ZoneShrinkState
            {
                State = (ZoneState)serializable.State,
                CurrentLayer = serializable.CurrentLayer,
                StepInLayer = serializable.StepInLayer,
                ActivationTurn = serializable.ActivationTurn,
                FirstDamageTurn = serializable.FirstDamageTurn,
                FirstDamageDealt = serializable.FirstDamageDealt
            };
        }

        /// <summary>
        /// Очистить сохранённые данные
        /// </summary>
        public static void Clear()
        {
            PlayerPrefs.DeleteKey($"{SaveKey}_State");
            PlayerPrefs.DeleteKey($"{SaveKey}_CurrentLayer");
            PlayerPrefs.DeleteKey($"{SaveKey}_StepInLayer");
            PlayerPrefs.DeleteKey($"{SaveKey}_ActivationTurn");
            PlayerPrefs.DeleteKey($"{SaveKey}_FirstDamageTurn");
            PlayerPrefs.DeleteKey($"{SaveKey}_FirstDamageTurn_Null");
            PlayerPrefs.DeleteKey($"{SaveKey}_FirstDamageDealt");
        }
    }

    /// <summary>
    /// Сериализуемая версия состояния для JSON
    /// </summary>
    [Serializable]
    public class ZoneShrinkStateSerializable
    {
        public int State;
        public int CurrentLayer;
        public int StepInLayer;
        public int ActivationTurn;
        public int? FirstDamageTurn;
        public bool FirstDamageDealt;
    }
}
