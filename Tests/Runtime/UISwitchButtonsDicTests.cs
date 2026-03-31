using System;
using NUnit.Framework;
using UnityEngine;
using AnkleBreaker.Utils.UIBasics;

namespace AnkleBreaker.Utils.UIBasics.Tests
{
    public class UISwitchButtonsDicTests
    {
        // ── Concrete types for testing ───────────────────────────────────

        private enum TestTab { TabA, TabB, TabC }

        [Serializable]
        private class TestSwitchDic : UISwitchButtonsDic<TestTab, UISwitchButton> { }

        // ── Helpers ──────────────────────────────────────────────────────

        private UISwitchButton CreateSwitchButton(string name)
        {
            var go = new GameObject(name);
            // UISwitchButton inherits from Button which requires a Canvas for raycasting,
            // but for pure logic tests we only need the component reference.
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

        // ── Init ─────────────────────────────────────────────────────────

        [Test]
        public void Init_DefaultTab_IsOn_OthersOff()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);

                Assert.IsTrue(swA.IsOn, "Default tab A should be on");
                Assert.IsFalse(swB.IsOn, "Tab B should be off");
                Assert.IsFalse(swC.IsOn, "Tab C should be off");
                Assert.AreEqual(TestTab.TabA, dic.DefaultTab);
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

        // ── SelectTab ────────────────────────────────────────────────────

        [Test]
        public void SelectTab_SwitchesToNewTab()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                bool result = dic.SelectTab(TestTab.TabB);

                Assert.IsTrue(result);
                Assert.IsFalse(swA.IsOn, "Previous tab A should be off");
                Assert.IsTrue(swB.IsOn, "New tab B should be on");
                Assert.IsFalse(swC.IsOn, "Tab C should remain off");
                Assert.AreEqual(TestTab.TabB, dic.CurrentTab);
                Assert.IsFalse(dic.AllTabClosed);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void SelectTab_AlreadySelected_ReturnsFalse()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                bool result = dic.SelectTab(TestTab.TabA);

                Assert.IsFalse(result, "Selecting the already-active tab should return false");
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void SelectTab_FiresOnSelectionChanged()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);

                TestTab received = default;
                bool fired = false;
                dic.OnSelectionChanged = tab => { received = tab; fired = true; };

                dic.SelectTab(TestTab.TabC);

                Assert.IsTrue(fired, "OnSelectionChanged should have been invoked");
                Assert.AreEqual(TestTab.TabC, received);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        // ── CloseAllTab ──────────────────────────────────────────────────

        [Test]
        public void CloseAllTab_AllButtonsOff()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                dic.CloseAllTab();

                Assert.IsFalse(swA.IsOn);
                Assert.IsFalse(swB.IsOn);
                Assert.IsFalse(swC.IsOn);
                Assert.IsTrue(dic.AllTabClosed);
            }
            finally { DestroyAll(swA, swB, swC); }
        }

        [Test]
        public void SelectTab_AfterCloseAll_Works()
        {
            var dic = CreateDic(out var swA, out var swB, out var swC);
            try
            {
                dic.Init(TestTab.TabA);
                dic.CloseAllTab();

                bool result = dic.SelectTab(TestTab.TabB);

                Assert.IsTrue(result);
                Assert.IsTrue(swB.IsOn);
                Assert.IsFalse(dic.AllTabClosed);
            }
            finally { DestroyAll(swA, swB, swC); }
        }
    }
}
