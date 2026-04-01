using System;
using NUnit.Framework;
using UnityEngine;
using AnkleBreaker.Utils.UIBasics;

namespace AnkleBreaker.Utils.UIBasics.Tests
{
    public class UISwitchButtonsDicTests
    {
        // ── Concrete types for testing ───────────────────────────────────────

        private enum TestTab { TabA, TabB, TabC }

        [Serializable]
        private class TestSwitchDic : UISwitchButtonsDic<TestTab, UISwitchButton> { }

        // ── Helpers ──────────────────────────────────────────────────────────

        private UISwitchButton CreateSwitchButton(string name)
        {
            var go = new GameObject(name);
            return go.AddComponent<UISwitchButton>();
        }

        private TestSwitchDic CreateDic(out UISwitchButton swA, out UISwitchButton swB, out UISwitchButton swC)
        {
            swA = CreateSwitchButton("A");
            swB = CreateSwitchButton("B");
            swC = CreateSwitchButton("C");

            var dic = new TestSwitchDic();
            dic.Add(TestTab.TabA, swA);
            dic.Add(TestTab.TabB, swB);
            dic.Add(TestTab.TabC, swC);
            return dic;
        }

        private void DestroyAll(params UISwitchButton[] buttons)
        {
            foreach (var btn in buttons)
                if (btn != null)
                    UnityEngine.Object.DestroyImmediate(btn.gameObject);
        }

        /// <summary>Simulates a UI click on the button (invokes onClick).</summary>
        private void SimulateClick(UISwitchButton btn)
        {
            btn.onClick.Invoke();
        }

        // ── Init ─────────────────────────────────────────────────────────────

        [Test]
        public void Init_DefaultKey_IsOn_OthersOff()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);

                Assert.IsTrue(swA.IsOn, "Default key A should be on");
                Assert.IsFalse(swB.IsOn, "Key B should be off");
                Assert.IsFalse(swC.IsOn, "Key C should be off");
                Assert.AreEqual(TestTab.TabA, dic.DefaultKey);
                Assert.AreEqual(TestTab.TabA, dic.SelectedKey);
                Assert.IsFalse(dic.NoneSelected);
                Assert.IsTrue(dic.IsInitialized);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Init_SwitchOnClick_SetToFalse()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);

                Assert.IsFalse(swA.SwitchOnClick);
                Assert.IsFalse(swB.SwitchOnClick);
                Assert.IsFalse(swC.SwitchOnClick);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Init_UnselectOnClick_DefaultFalse()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                Assert.IsFalse(dic.UnselectOnClick);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Init_UnselectOnClick_SetToTrue()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA, unselectOnClick: true);
                Assert.IsTrue(dic.UnselectOnClick);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        // ── Select ───────────────────────────────────────────────────────────

        [Test]
        public void Select_SwitchesToNewKey()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                bool result = dic.Select(TestTab.TabB);

                Assert.IsTrue(result);
                Assert.IsFalse(swA.IsOn, "Previous key A should be off");
                Assert.IsTrue(swB.IsOn, "New key B should be on");
                Assert.IsFalse(swC.IsOn, "Key C should remain off");
                Assert.AreEqual(TestTab.TabB, dic.SelectedKey);
                Assert.IsFalse(dic.NoneSelected);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Select_AlreadySelected_ReturnsFalse()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                bool result = dic.Select(TestTab.TabA);

                Assert.IsFalse(result, "Selecting the already-active key should return false");
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Select_FiresOnSelectionChanged()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);

                TestTab received = default;
                bool fired = false;
                dic.OnSelectionChanged = tab => { received = tab; fired = true; };

                dic.Select(TestTab.TabC);

                Assert.IsTrue(fired, "OnSelectionChanged should have been invoked");
                Assert.AreEqual(TestTab.TabC, received);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        // ── HandleClick (auto-selection on click) ────────────────────────────

        [Test]
        public void Click_UnselectedButton_SelectsIt()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                SimulateClick(swB);

                Assert.IsTrue(swB.IsOn, "Clicked button B should be on");
                Assert.IsFalse(swA.IsOn, "Previous key A should be off");
                Assert.AreEqual(TestTab.TabB, dic.SelectedKey);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Click_SelectedButton_UnselectOnClickFalse_StaysOn()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA, unselectOnClick: false);
                SimulateClick(swA);

                Assert.IsTrue(swA.IsOn, "Selected button should stay on when UnselectOnClick is false");
                Assert.IsFalse(dic.NoneSelected);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Click_SelectedButton_UnselectOnClickTrue_ClearsSelection()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA, unselectOnClick: true);
                SimulateClick(swA);

                Assert.IsFalse(swA.IsOn, "Selected button should turn off when UnselectOnClick is true");
                Assert.IsTrue(dic.NoneSelected);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Click_FiresOnSwitchClick()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);

                TestTab received = default;
                bool fired = false;
                dic.OnSwitchClick = key => { received = key; fired = true; };

                SimulateClick(swB);

                Assert.IsTrue(fired, "OnSwitchClick should have been invoked");
                Assert.AreEqual(TestTab.TabB, received);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        // ── ClearSelection ──────────────────────────────────────────────────

        [Test]
        public void ClearSelection_AllButtonsOff()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                dic.ClearSelection();

                Assert.IsFalse(swA.IsOn);
                Assert.IsFalse(swB.IsOn);
                Assert.IsFalse(swC.IsOn);
                Assert.IsTrue(dic.NoneSelected);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void Select_AfterClearSelection_Works()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                dic.ClearSelection();

                bool result = dic.Select(TestTab.TabB);

                Assert.IsTrue(result);
                Assert.IsTrue(swB.IsOn);
                Assert.IsFalse(dic.NoneSelected);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        // ── Reset ────────────────────────────────────────────────────────────

        [Test]
        public void Reset_ClearsState()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                dic.Reset();

                Assert.IsFalse(dic.IsInitialized);
                Assert.IsTrue(dic.NoneSelected);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void DoubleInit_DoesNotAccumulateListeners()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                int clickCount = 0;
                dic.OnSwitchClick = _ => clickCount++;

                dic.Init(TestTab.TabA);
                dic.Init(TestTab.TabB); // re-init should reset listeners

                SimulateClick(swA);

                Assert.AreEqual(1, clickCount, "OnSwitchClick should fire exactly once after double Init");
            }
            finally { DestroyAll(swA, swB, swC); }
        }
    }
}
