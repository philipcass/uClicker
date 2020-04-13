using System;
using UnityEngine;

namespace uClicker
{
    [CreateAssetMenu(menuName = "uClicker/Clickable")]
    public class Clickable : ClickerComponent
    {
        public Currency Currency;
        public float Amount;
    }
}