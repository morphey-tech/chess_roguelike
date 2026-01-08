using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteUI.Notification.Model
{
    public class NotificationModel
    {
        private const float DEFAULT_DURATION = 5;
        
        public string Id { get; }

        public Type Type { get; }
        
        public object?[]? Parameters { get; }

        public List<string> Tags { get; } = new();

        public float Duration { get; private set; } = DEFAULT_DURATION;

        private NotificationModel(Type type, object?[]? parameters)
        {
            Id = $"{type.Name}_{Guid.NewGuid().ToString()}";
            Type = type;
            Parameters = parameters;
        }

        public static NotificationModel Create<T>(params object?[]? parameters) where T : MonoBehaviour
        {
            return new NotificationModel(typeof(T), parameters);
        }

        public NotificationModel SetDuration(float duration)
        {
            Duration = duration;
            return this;
        }
        
        public NotificationModel SetTag(string tag)
        {
            Tags.Add(tag);
            return this;
        }
        
        public NotificationModel SetTags(List<string> tags)
        {
            Tags.AddRange(tags);
            return this;
        }
        
        public NotificationModel SetTags(params string[] tags)
        {
            Tags.AddRange(tags);
            return this;
        }

        public bool HasTag(string tag)
        {
            return Tags.Contains(tag);
        }
        
        public bool HasAnyTag(List<string> tags)
        {
            return Tags.Any(tags.Contains);
        }
    }
}
