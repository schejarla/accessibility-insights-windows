// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AccessibilityInsights.Core.Bases;
using AccessibilityInsights.Core.Enums;
using AccessibilityInsights.Rules.PropertyConditions;
using EvaluationCode = AccessibilityInsights.Rules.EvaluationCode;
using static AccessibilityInsights.RulesTest.ControlType;


namespace AccessibilityInsights.RulesTest.Library
{
    [TestClass]
    public class BoundingRectangleContainedInParentTest
    {
        private static AccessibilityInsights.Rules.IRule Rule = new AccessibilityInsights.Rules.Library.BoundingRectangleContainedInParent();
        const int Margin = BoundingRectangle.OverlapMargin;

        [TestMethod]
        public void TestBoundingRectangleContainedInParentPass()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                parent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2));

                e.Parent = parent;

                Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentTabItemPass()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            using (var grandParent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                e.ControlTypeId = TabItem;

                grandParent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2));
                grandParent.ControlTypeId = Tab;

                e.Parent = parent;
                parent.Parent = grandParent;

                Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentSkipTabItemPass()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            using (var grandParent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                e.ControlTypeId = Edit;

                // parent is smaller than grandparent, but is TabItem
                parent.BoundingRectangle = new Rectangle(400, 400, 10, 10);
                parent.ControlTypeId = TabItem;

                grandParent.BoundingRectangle = e.BoundingRectangle;
                grandParent.ControlTypeId = Tab;
                grandParent.Framework = Framework.WPF;

                e.Parent = parent;
                parent.Parent = grandParent;

                Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentSkipTabItemFail()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            using (var grandparent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                e.ControlTypeId = Button;

                parent.BoundingRectangle = new Rectangle(400, 400, 10, 10);
                parent.ControlTypeId = TabItem;

                // e and grandparent should match. So if they were compared, the rule would pass
                // But grandparent should not have framework WPF.
                grandparent.BoundingRectangle = e.BoundingRectangle;
                grandparent.ControlTypeId = Tab;

                e.Parent = parent;
                parent.Parent = grandparent;

                Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Error);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentTreeItemPass()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            using (var grandParent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                e.ControlTypeId = TreeItem;

                grandParent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2));
                grandParent.ControlTypeId = Tree;

                e.Parent = parent;
                parent.Parent = grandParent;

                Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentContainerMismatchPass()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                e.ControlTypeId = TabItem;

                parent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2));
                parent.ControlTypeId = Tree;

                e.Parent = parent;

                Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentLeftFail()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                parent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin + 1,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2));

                e.Parent = parent;

                Assert.AreNotEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentTopFail()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                parent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin + 1,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2));

                e.Parent = parent;

                Assert.AreNotEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentRightFail()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                parent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2) - 1,
                    e.BoundingRectangle.Height - (Margin * 2));

                e.Parent = parent;

                Assert.AreNotEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentBottomFail()
        {
            using (var e = new MockA11yElement())
            using (var parent = new MockA11yElement())
            {
                e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
                parent.BoundingRectangle = new Rectangle(
                    e.BoundingRectangle.Left + Margin,
                    e.BoundingRectangle.Top + Margin,
                    e.BoundingRectangle.Width - (Margin * 2),
                    e.BoundingRectangle.Height - (Margin * 2) - 1);

                e.Parent = parent;

                Assert.AreNotEqual(Rule.Evaluate(e), EvaluationCode.Pass);
            } // using
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentElementNullFail()
        {
            Action action = () => Rule.Evaluate(null);
            Assert.ThrowsException<ArgumentException>(action);
        }

        [TestMethod]
        public void TestBoundingRectangleContainedInParentNoParentFail()
        {
            using (var e = new MockA11yElement())
            {
                Action action = () => Rule.Evaluate(e);
                Assert.ThrowsException<Exception>(action);
            } // using
        }

        [TestMethod]
        public void BoundingRectangleContainedInParent_VerticallyScrollablePass()
        {
            var e = new MockA11yElement();
            var parent = new MockA11yElement();

            // parent should be smaller vertically than child
            e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
            parent.BoundingRectangle = new Rectangle(300, 300, 500, 200);

            var pattern = new A11yPattern();
            pattern.Id = PatternIDs.Scroll;
            pattern.Properties = new List<A11yPatternProperty>();
            pattern.Properties.Add(new A11yPatternProperty { Name = "VerticallyScrollable", Value = true });
            pattern.Properties.Add(new A11yPatternProperty { Name = "VerticalScrollPercent", Value = 0 });

            parent.Patterns.Add(pattern);
            e.Parent = parent;

            Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
        }

        [TestMethod]
        public void BoundingRectangleContainedInParent_HorizontallyScrollablePass()
        {
            var e = new MockA11yElement();
            var parent = new MockA11yElement();

            // parent should be smaller Horizontally than child
            e.BoundingRectangle = new Rectangle(300, 300, 500, 500);
            parent.BoundingRectangle = new Rectangle(300, 300, 200, 500);

            var pattern = new A11yPattern();
            pattern.Id = PatternIDs.Scroll;
            pattern.Properties = new List<A11yPatternProperty>();
            pattern.Properties.Add(new A11yPatternProperty { Name = "HorizontallyScrollable", Value = true });
            pattern.Properties.Add(new A11yPatternProperty { Name = "HorizontalScrollPercent", Value = 0 });

            parent.Patterns.Add(pattern);
            e.Parent = parent;

            Assert.AreEqual(Rule.Evaluate(e), EvaluationCode.Pass);
        }
    } // class
} // namespace
