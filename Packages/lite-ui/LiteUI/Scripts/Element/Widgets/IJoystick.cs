using System;
using UnityEngine;

namespace LiteUI.Element.Widgets
{
    public interface IJoystick
    {
        public event Action<Vector2>? OnJoystick;
    }
}
