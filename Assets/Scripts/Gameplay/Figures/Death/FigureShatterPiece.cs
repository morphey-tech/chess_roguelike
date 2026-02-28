using UnityEngine;

namespace Project.Gameplay.Gameplay.Figures.Death
{
    /// <summary>
    /// Компонент осколка фигуры. Добавляется runtime при разрушении.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Rigidbody))]
    public sealed class FigureShatterPiece : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lifetime = 0f;

        private Rigidbody? _rigidbody;
        private float _spawnTime;
        private bool _hasLifetime;

        public void Init(float lifetime)
        {
            _lifetime = lifetime;
            _hasLifetime = lifetime > 0f;
            _spawnTime = Time.time;

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        public void ApplyForce(Vector3 force, Vector3 torque)
        {
            if (_rigidbody != null)
            {
                _rigidbody.AddForce(force, ForceMode.Impulse);
                _rigidbody.AddTorque(torque, ForceMode.Impulse);
            }
        }

        public void SetPhysicsParams(float gravityMultiplier, float drag, float angularDrag)
        {
            if (_rigidbody != null)
            {
                _rigidbody.useGravity = true;
                _rigidbody.linearDamping = drag;
                _rigidbody.angularDamping = angularDrag;
            }
        }

        private void Update()
        {
            if (!_hasLifetime)
                return;

            if (Time.time - _spawnTime >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
