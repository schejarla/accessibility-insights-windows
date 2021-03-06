// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using EvaluationCode = AccessibilityInsights.Rules.EvaluationCode;
using static AccessibilityInsights.RulesTest.ControlType;

namespace AccessibilityInsights.RulesTest.Library
{
    [TestClass]
    public class NameIsNullButElementIsNotKeyboardFocusableTest
    {
        private static AccessibilityInsights.Rules.IRule Rule = new AccessibilityInsights.Rules.Library.NameIsNullButElementIsNotKeyboardFocusable();

        [TestMethod]
        public void TestNameIsNullButElementIsNotKeyboardFocusablePass()
        {
            using (var e = new MockA11yElement())
            {
                e.Name = "";
                Assert.AreEqual(EvaluationCode.Pass, Rule.Evaluate(e));
            } // using
        }

        [TestMethod]
        public void TestNameIsNullButElementIsNotKeyboardFocusableOpen()
        {
            using (var e = new MockA11yElement())
            {
                Assert.AreEqual(EvaluationCode.Open, Rule.Evaluate(e));
            } // using
        }

        [TestMethod]
        public void TestNameIsNullButElementIsNotKeyboardFocusableNullArgument()
        {
            Assert.AreEqual(EvaluationCode.RuleExecutionError, Rule.Evaluate(null));
        }

        [TestMethod]
        public void TestNameIsNullButElementIsNotKeyboardFocusableProgressBar()
        {
            // progress bars are processed by NameIsNotNull

            var e = new MockA11yElement();
            e.ControlTypeId = CheckBox;
            e.BoundingRectangle = new Rectangle(0, 0, 25, 25);
            Assert.IsTrue(Rule.Condition.Matches(e));

            e.ControlTypeId = ProgressBar;
            Assert.IsFalse(Rule.Condition.Matches(e));
        }
    } // class
} // namespace
