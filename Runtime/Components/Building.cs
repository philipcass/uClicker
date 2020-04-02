#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Building")]
    public class Building : UnlockableComponent
    {
        public Currency Currency;
        public float Cost;
        public float Amount;
        public Requirement[] Requirements;
    }
}