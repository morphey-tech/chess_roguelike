using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Components;

namespace Project.Gameplay
{
    // Сомневаюсь что это будет так
    public class Entity
    {
        public int Id { get; }

        private readonly List<IEntityComponent> _components = new();
        
        public Entity(int id) => Id = id;

        public T EnsureComponent<T>(T component) where T : IEntityComponent
        {
            _components.Add(component);
            return component;
        }

        public void Del<T>() where T : IEntityComponent
        {
            _components.RemoveAll(c => c is T);
        }

        public bool Exists<T>() where T : IEntityComponent
        {
           return _components.Any(c => c is T);
        }
    }
}