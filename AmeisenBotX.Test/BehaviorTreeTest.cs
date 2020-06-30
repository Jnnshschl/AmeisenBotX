using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Interfaces;
using AmeisenBotX.BehaviorTree.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class BehaviorTreeTest
    {
        [TestMethod]
        public void DualSelectorTreeTest()
        {
            int treeResult00 = 0;
            int treeResult10 = 0;
            int treeResult01 = 0;
            int treeResult11 = 0;

            TestBlackboard testBlackboard = new TestBlackboard();

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new DualSelector<TestBlackboard>
                (
                    (blackboard) => blackboard.FirstNode,
                    (blackboard) => blackboard.SecondFirstNode,
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult00;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult10;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult01;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult11;
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),
                testBlackboard
            );

            testBlackboard.FirstNode = false;
            testBlackboard.SecondFirstNode = false;

            tree.Tick();

            Assert.AreEqual(1, treeResult00);
            Assert.AreEqual(0, treeResult10);
            Assert.AreEqual(0, treeResult01);
            Assert.AreEqual(0, treeResult11);

            testBlackboard.FirstNode = true;

            tree.Tick();

            Assert.AreEqual(1, treeResult00);
            Assert.AreEqual(1, treeResult10);
            Assert.AreEqual(0, treeResult01);
            Assert.AreEqual(0, treeResult11);

            testBlackboard.FirstNode = false;
            testBlackboard.SecondFirstNode = true;

            tree.Tick();

            Assert.AreEqual(1, treeResult00);
            Assert.AreEqual(1, treeResult10);
            Assert.AreEqual(1, treeResult01);
            Assert.AreEqual(0, treeResult11);

            testBlackboard.FirstNode = true;
            testBlackboard.SecondFirstNode = true;

            tree.Tick();

            Assert.AreEqual(1, treeResult00);
            Assert.AreEqual(1, treeResult10);
            Assert.AreEqual(1, treeResult01);
            Assert.AreEqual(1, treeResult11);
        }

        [TestMethod]
        public void InverterTreeTest()
        {
            int treeResult0 = 0;
            int treeResult1 = 0;

            TestBlackboard testBlackboard = new TestBlackboard()
            {
                FirstNode = true
            };

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new Sequence<TestBlackboard>
                (
                    new Inverter<TestBlackboard>
                    (
                        new Leaf<TestBlackboard>
                        (
                            (blackboard) =>
                            {
                                ++treeResult0;
                                return BehaviorTreeStatus.Success;
                            }
                        )
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult1;
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),
                testBlackboard
            );

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(0, treeResult1);

            tree.Tick();
            tree.Tick();

            Assert.AreEqual(3, treeResult0);
            Assert.AreEqual(0, treeResult1);
        }

        [TestMethod]
        public void NestedTreeTest()
        {
            int treeResult0 = 0;
            int treeResult1 = 0;
            int treeResult01 = 0;

            TestBlackboard testBlackboard = new TestBlackboard()
            {
                FirstNode = true,
                SecondFirstNode = true
            };

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new Selector<TestBlackboard>
                (
                    (blackboard) => blackboard.FirstNode,
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Selector<TestBlackboard>
                    (
                        (blackboard) => blackboard.SecondFirstNode,
                        new Leaf<TestBlackboard>
                        (
                            (blackboard) =>
                            {
                                ++treeResult01;
                                return BehaviorTreeStatus.Success;
                            }
                        ),
                        new Leaf<TestBlackboard>
                        (
                            (blackboard) =>
                            {
                                ++treeResult1;
                                return BehaviorTreeStatus.Success;
                            }
                        )
                    )
                ),
                testBlackboard
            );

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(0, treeResult1);
            Assert.AreEqual(0, treeResult01);

            testBlackboard.FirstNode = false;

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(0, treeResult1);
            Assert.AreEqual(1, treeResult01);

            testBlackboard.SecondFirstNode = false;

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(1, treeResult01);
        }

        [TestMethod]
        public void OngoingTreeTest()
        {
            int treeResult0 = 0;
            int treeResult1 = 0;
            int treeResult2 = 0;

            TestBlackboard testBlackboard = new TestBlackboard();

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new Sequence<TestBlackboard>
                (
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult1;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult2;
                            return treeResult2 < 5 ? BehaviorTreeStatus.Ongoing : BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),
                testBlackboard
            );

            tree.Tick();

            Assert.AreEqual(0, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(0, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(0, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(1, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(2, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(3, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(4, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(5, treeResult2);

            tree.Tick();

            Assert.AreEqual(2, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(5, treeResult2);
        }

        [TestMethod]
        public void OngoingTreeTestResume()
        {
            int treeResult0 = 0;
            int treeResult1 = 0;
            int treeResult2 = 0;

            TestBlackboard testBlackboard = new TestBlackboard();

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new Sequence<TestBlackboard>
                (
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult1;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult2;
                            return treeResult2 < 5 ? BehaviorTreeStatus.Ongoing : BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),
                testBlackboard,
                true
            );

            Assert.AreEqual(true, tree.ResumeOngoingNodes);
            Assert.AreEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(0, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(0, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(0, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(1, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(2, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(3, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(4, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(5, treeResult2);

            Assert.AreNotEqual(null, tree.OngoingNode);

            tree.Tick();

            Assert.AreEqual(2, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(5, treeResult2);

            Assert.AreEqual(null, tree.OngoingNode);
        }

        [TestMethod]
        public void SequenceTreeTest()
        {
            int treeResult0 = 0;
            int treeResult1 = 0;
            int treeResult2 = 0;

            TestBlackboard testBlackboard = new TestBlackboard();

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new Sequence<TestBlackboard>
                (
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult1;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult2;
                            return BehaviorTreeStatus.Failed;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),
                testBlackboard
            );

            tree.Tick();

            Assert.AreEqual(0, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(0, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(0, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(1, treeResult1);
            Assert.AreEqual(1, treeResult2);

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(2, treeResult1);
            Assert.AreEqual(1, treeResult2);
        }

        [TestMethod]
        public void SimpleTreeTest()
        {
            int treeResult0 = 0;
            int treeResult1 = 0;

            TestBlackboard testBlackboard = new TestBlackboard()
            {
                FirstNode = true
            };

            AmeisenBotBehaviorTree<TestBlackboard> tree = new AmeisenBotBehaviorTree<TestBlackboard>
            (
                new Selector<TestBlackboard>
                (
                    (blackboard) => blackboard.FirstNode,
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult0;
                            return BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult1;
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),
                testBlackboard
            );

            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(0, treeResult1);

            testBlackboard.FirstNode = false;

            tree.Tick();
            tree.Tick();

            Assert.AreEqual(1, treeResult0);
            Assert.AreEqual(2, treeResult1);
        }
    }

    public class TestBlackboard : IBlackboard
    {
        public bool FirstNode;
        public bool SecondFirstNode;

        public void Update()
        {
        }
    }
}