using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using uClicker;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

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
        }
        
        [Test]
        public void TestClicking()
        {
            _testClickable.Currency = _testCurrency;
            _testClickable.Amount = 1;
            ArrayUtility.Add(ref _testManager.Config.Currencies, _testCurrency);
            _testManager.OnEnable();
            
            Assert.AreEqual(0, _testManager.State.CurrencyCurrentTotals[_testCurrency]);
            _testManager.Click(_testClickable);
            _testManager.Click(_testClickable);
            _testManager.Click(_testClickable);
            Assert.AreEqual(3, _testManager.State.CurrencyCurrentTotals[_testCurrency]);

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
    }
}
