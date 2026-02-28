using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    /// <summary>
    /// Компонент для удаления осколка через заданное время.
    /// </summary>
    public sealed class FragmentLifetime : MonoBehaviour
    {
        public float lifetime = 30f;
        private float _spawnTime;

        private void Start()
        {
            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - _spawnTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}