using UnityEngine;

namespace uClicker
{
    public abstract class ClickerComponent : ScriptableObject, ISerializationCallbackReceiver
    {
        System.Guid _guid = System.Guid.Empty;
        [HideInInspector]
        [SerializeField]
        private byte[] _serializedGuid;

        public void OnBeforeSerialize()
        {
            if (_guid != System.Guid.Empty)
            {
                _serializedGuid = _guid.ToByteArray();
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

    public abstract class UnlockableComponent : ClickerComponent
    {
        public bool Unlocked;
    }
}