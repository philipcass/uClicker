using UnityEngine;

namespace uClicker
{
    public abstract class Component : ScriptableObject
    {
        public string Name;
    }

    public abstract class UnlockableComponent : Component
    {
        public bool Unlocked;
    }
}