using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Components;
using UniRx;

namespace Project.Gameplay
{
    public class Entity
    {
        public int Id { get; }
        public ReactiveCollection<IEntityComponent> Components { get; } = new();

        public Entity(int id)
        {
            Id = id;
        }

        public T EnsureComponent<T>(T component) where T : IEntityComponent
        {
            Components.Add(component);
            return component;
        }

        public void Del<T>() where T : IEntityComponent
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                if (Components[i] is T)
                {
                    Components.RemoveAt(i);
                }
            }
        }

        public bool Exists<T>() where T : IEntityComponent
        {
           return Components.Any(c => c is T);
        }
    }
}