using System;
using NUnit.Framework;
using uClicker;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class Tests
    {
        private ClickerManager _testManager;
        private Currency _testCurrency;
        private Clickable _testClickable;

        [SetUp]
        public void CreateObjects()
        {
            _testManager = ScriptableObject.CreateInstance<ClickerManager>();
            _testCurrency = ScriptableObject.CreateInstance<Currency>();
            _testClickable = ScriptableObject.CreateInstance<Clickable>();
            _testManager.SaveSettings.SaveName = "TEST_SAVE";
            _testManager.SaveSettings.SavePath = Application.dataPath;
        }

        [TearDown]
        public void TearDown()
        {
            _testManager.ClearProgress();
        }

        [TestCase(3, ExpectedResult = 3)]
        public int TestClicking(int clickCount)
        {
            _testClickable.Currency = _testCurrency;
            _testClickable.Amount = 1;
            ArrayUtility.Add(ref _testManager.Config.Currencies, _testCurrency);
            _testManager.OnEnable();

            Assert.AreEqual(0, _testManager.State.CurrencyCurrentTotals[_testCurrency]);
            for (int i = 0; i < clickCount; i++)
            {
                _testManager.Click(_testClickable);
            }

            return (int) _testManager.State.CurrencyCurrentTotals[_testCurrency];
        }

        [Test]
        public void TestGenerators()
        {
            _testClickable.Currency = _testCurrency;
            _testClickable.Amount = 1;
            ArrayUtility.Add(ref _testManager.Config.Currencies, _testCurrency);

            var building = ScriptableObject.CreateInstance<Building>();
            building.YieldAmount = new CurrencyTuple()
            {
                Amount = 5,
                Currency = _testCurrency
            };
            _testManager.State.EarnedBuildings[building] = 1;
            _testManager.OnEnable();
            _testManager.Tick();
            Assert.AreEqual(5, _testManager.State.CurrencyCurrentTotals[_testCurrency]);
        }

        [TestCase(ManagerSaveSettings.SaveTypeEnum.SaveToPlayerPrefs)]
        [TestCase(ManagerSaveSettings.SaveTypeEnum.SaveToFile)]
        public void TestSave(ManagerSaveSettings.SaveTypeEnum saveType)
        {
            _testClickable.Currency = _testCurrency;
            _testClickable.Amount = 1;
            ArrayUtility.Add(ref _testManager.Config.Currencies, _testCurrency);

            _testManager.SaveSettings.SaveType = saveType;
            Assert.Throws<ArgumentException>(() => { _testManager.LoadProgress(); });
            _testManager.SaveProgress();
            _testManager.LoadProgress();
        }
    }
}