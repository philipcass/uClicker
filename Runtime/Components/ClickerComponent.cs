using UnityEngine;

namespace uClicker
{
    public abstract class ClickerComponent : ScriptableObject
    {
        public string Name;
    }

    public abstract class UnlockableComponent : ClickerComponent
    {
        public bool Unlocked;
    }
}