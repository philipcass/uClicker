using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Building")]
    public class Building : UnlockableComponent
    {
        public float Cost;
        public float Amount;
        public Requirement[] Requirements;
    }
}