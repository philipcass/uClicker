using System;
using System.Collections.Generic;
using UnityEngine;

namespace uClicker
{
    [Serializable]
    public struct GUIDContainer : ISerializationCallbackReceiver
    {
        private Guid _guid;
        [SerializeField] private string _serializedGuid;

        public System.Guid Guid
        {
            get
            {
                _guid = _guid != default(Guid) ? _guid : new System.Guid(_serializedGuid);
                return _guid;
            }
        }

        public void OnBeforeSerialize()
        {
            if (Guid != System.Guid.Empty)
            {
                _serializedGuid = Guid.ToString();
            }
        }

        public void OnAfterDeserialize()
        {
            if (_serializedGuid != null && _serializedGuid.Length == 16)
            {
                _guid = new System.Guid(_serializedGuid);
            }
        }
    }

    public abstract class ClickerComponent : ScriptableObject, ISerializationCallbackReceiver
    {
        public static Dictionary<Guid, ClickerComponent> Lookup = new Dictionary<Guid, ClickerComponent>();
        [HideInInspector] public GUIDContainer GUIDContainer;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            Lookup[GUIDContainer.Guid] = this;
        }
    }

    public abstract class UnlockableComponent : ClickerComponent
    {
        public bool Unlocked;
    }
}