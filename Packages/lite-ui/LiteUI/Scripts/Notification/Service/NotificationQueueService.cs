using System.Collections.Generic;
using System.Linq;
using LiteUI.Notification.Model;

namespace LiteUI.Notification.Service
{
    public class NotificationQueueService
    {
        private readonly List<NotificationModel> _showQueue = new();

        public void AddFirst(NotificationModel model)
        {
            if (ContainsById(model.Id)) {
                return;
            }
            _showQueue.Insert(0, model);
        }
        
        public void AddLast(NotificationModel model)
        {
            if (ContainsById(model.Id)) {
                return;
            }
            _showQueue.Add(model);
        }

        public void RemoveById(string id)
        {
            NotificationModel? model = GetById(id);
            if (model != null) {
                _showQueue.Remove(model);
            }
        }

        public void RemoveByTag(string tag)
        {
            _showQueue.RemoveAll(m => m.HasTag(tag));
        }

        public NotificationModel? Pull(List<string> excludedTags)
        {
            NotificationModel? result = null;
            foreach (NotificationModel model in _showQueue) {
                if (model.Tags.Any(excludedTags.Contains)) {
                    continue;
                }
                result = model;
            }
            if (result != null) {
                _showQueue.Remove(result);
            }
            return result;
        }

        public bool ContainsById(string id)
        {
            return GetById(id) != null;
        }
        
        public bool ContainsByTag(string tag)
        {
            return GetByTag(tag).Count > 0;
        }

        private NotificationModel? GetById(string id)
        {
            return _showQueue.FirstOrDefault(m => m.Id == id);
        }
        
        private List<NotificationModel> GetByTag(string tag)
        {
            return _showQueue.Where(m => m.HasTag(tag)).ToList();
        }
    }
}
