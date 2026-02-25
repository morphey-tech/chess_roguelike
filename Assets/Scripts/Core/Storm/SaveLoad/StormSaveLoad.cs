using Project.Core.Core.Storm.Core;
using UnityEngine;

namespace Project.Core.Core.Storm.SaveLoad
{
    public class StormSaveLoad
    {
        private const string SaveKey = "StormState";

        public static void SaveToPlayerPrefs(StormSaveContext save)
        {
            PlayerPrefs.SetInt($"{SaveKey}_State", (int)save.State);
            PlayerPrefs.SetInt($"{SaveKey}_CurrentLayer", save.CurrentLayer);
            PlayerPrefs.SetInt($"{SaveKey}_StepInLayer", save.StepInLayer);
            PlayerPrefs.SetInt($"{SaveKey}_ActivationTurn", save.ActivationTurn);
            
            if (save.FirstDamageTurn.HasValue)
            {
                PlayerPrefs.SetInt($"{SaveKey}_FirstDamageTurn", save.FirstDamageTurn.Value);
                PlayerPrefs.SetInt($"{SaveKey}_FirstDamageTurn_Null", 0);
            }
            else
            {
                PlayerPrefs.SetInt($"{SaveKey}_FirstDamageTurn_Null", 1);
            }
            
            PlayerPrefs.SetInt($"{SaveKey}_FirstDamageDealt", save.FirstDamageDealt ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static StormSaveContext? LoadFromPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey($"{SaveKey}_State"))
            {
                return null;
            }

            return new StormSaveContext
            {
                State = (StormState)PlayerPrefs.GetInt($"{SaveKey}_State"),
                CurrentLayer = PlayerPrefs.GetInt($"{SaveKey}_CurrentLayer"),
                StepInLayer = PlayerPrefs.GetInt($"{SaveKey}_StepInLayer"),
                ActivationTurn = PlayerPrefs.GetInt($"{SaveKey}_ActivationTurn"),
                FirstDamageTurn = PlayerPrefs.GetInt($"{SaveKey}_FirstDamageTurn_Null") == 1
                    ? null
                    : PlayerPrefs.GetInt($"{SaveKey}_FirstDamageTurn"),
                FirstDamageDealt = PlayerPrefs.GetInt($"{SaveKey}_FirstDamageDealt") == 1
            };
        }

        public static string SaveToJson(StormSaveContext save)
        {
            return JsonUtility.ToJson(new StormStateSerializable
            {
                State = (int)save.State,
                CurrentLayer = save.CurrentLayer,
                StepInLayer = save.StepInLayer,
                ActivationTurn = save.ActivationTurn,
                FirstDamageTurn = save.FirstDamageTurn,
                FirstDamageDealt = save.FirstDamageDealt
            });
        }

        public static StormSaveContext LoadFromJson(string json)
        {
            StormStateSerializable? serializable = JsonUtility.FromJson<StormStateSerializable>(json);
            
            return new StormSaveContext
            {
                State = (StormState)serializable.State,
                CurrentLayer = serializable.CurrentLayer,
                StepInLayer = serializable.StepInLayer,
                ActivationTurn = serializable.ActivationTurn,
                FirstDamageTurn = serializable.FirstDamageTurn,
                FirstDamageDealt = serializable.FirstDamageDealt
            };
        }
        
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
}
