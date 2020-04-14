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

        public GUIDContainer(Guid guid)
        {
            _guid = guid;
            _serializedGuid = _guid.ToString();
        }

        public System.Guid Guid
        {
            get { return _guid; }
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
            if (_serializedGuid != null && !string.IsNullOrEmpty(_serializedGuid))
            {
                _guid = new System.Guid(_serializedGuid);
            }
        }
    }

    public abstract class ClickerComponent : ScriptableObject, ISerializationCallbackReceiver
    {
        public static Dictionary<Guid, ClickerComponent> RuntimeLookup = new Dictionary<Guid, ClickerComponent>();
        [HideInInspector] public GUIDContainer GUIDContainer;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            RuntimeLookup[GUIDContainer.Guid] = this;
        }
    }

    [Serializable]
    public enum RequirementOperand
    {
        And,
        Or
    }

    [Serializable]
    public enum RequirementType
    {
        Currency,
        Building,
        Upgrade
    }

    [Serializable]
    public class Requirement
    {
        public RequirementType RequirementType;
        public CurrencyTuple UnlockAmount;
        public BuildingTuple UnlockBuilding;
        public Upgrade UnlockUpgrade;
    }

    [Serializable]
    public struct RequirementGroup
    {
        public RequirementOperand GroupOperand;
        public Requirement[] Requirements;
    }

    public abstract class UnlockableComponent : ClickerComponent
    {
        public bool Unlocked;
        public RequirementGroup[] RequirementGroups;
    }
}