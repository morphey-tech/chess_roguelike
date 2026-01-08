using System;
using UniRx;
using UnityEngine;

namespace Project.Core.Player
{
    [Serializable]
    public class PlayerModel
    {
        private readonly ReactiveProperty<Vector3> _position = new(Vector3.zero);
        private readonly ReactiveProperty<float> _rotationY = new(0f);
        private readonly ReactiveProperty<float> _health = new(100f);
        private readonly ReactiveProperty<float> _stamina = new(100f);
        
        public IReactiveProperty<Vector3> Position => _position;
        public IReactiveProperty<float> RotationY => _rotationY;
        public IReactiveProperty<float> Health => _health;
        public IReactiveProperty<float> Stamina => _stamina;
        
        public void SetPosition(Vector3 position)
        {
            _position.Value = position;
        }
        
        public void SetRotation(float rotationY)
        {
            _rotationY.Value = rotationY;
        }
        
        public void SetHealth(float health)
        {
            _health.Value = Mathf.Clamp(health, 0f, 100f);
        }
        
        public void SetStamina(float stamina)
        {
            _stamina.Value = Mathf.Clamp(stamina, 0f, 100f);
        }
        
        public PlayerSaveData ToSaveData()
        {
            return new PlayerSaveData
            {
                PositionX = _position.Value.x,
                PositionY = _position.Value.y,
                PositionZ = _position.Value.z,
                RotationY = _rotationY.Value,
                Health = _health.Value,
                Stamina = _stamina.Value
            };
        }
        
        public void FromSaveData(PlayerSaveData data)
        {
            _position.Value = new Vector3(data.PositionX, data.PositionY, data.PositionZ);
            _rotationY.Value = data.RotationY;
            _health.Value = data.Health;
            _stamina.Value = data.Stamina;
        }
    }
    
    [Serializable]
    public class PlayerSaveData
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationY;
        public float Health;
        public float Stamina;
    }
}


