using System;
using System.Collections.Generic;

namespace Project.Gameplay.Presentations
{
    // Залупа с ecs меня не отпускает. Мега костыль как заплатка для получения Id сущности
    public static class IdGetter
    {
        private static int _nextId = 0;
        private static Queue<int> _free = new();

        public static int MakeId()
        {
            return _free.Count == 0 ? _nextId++ : _free.Dequeue();
        }
        
        public static void Release(int id)
        {
            if (_free.Contains(id))
                throw new Exception($"Id: {id} already released");
            
            _free.Enqueue(id);
        }
        
        public static void Reset()
        {
            _nextId = 0;
            _free.Clear();
        }
    }
}