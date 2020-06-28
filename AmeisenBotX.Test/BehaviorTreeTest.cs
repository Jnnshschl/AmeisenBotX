using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class BehaviorTreeTest
    {
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
                            return BehaviorTree.Enums.BehaviorTreeStatus.Success;
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
                                return BehaviorTree.Enums.BehaviorTreeStatus.Success;
                            }
                        ),
                        new Leaf<TestBlackboard>
                        (
                            (blackboard) =>
                            {
                                ++treeResult1;
                                return BehaviorTree.Enums.BehaviorTreeStatus.Success;
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
                            return BehaviorTree.Enums.BehaviorTreeStatus.Success;
                        }
                    ),
                    new Leaf<TestBlackboard>
                    (
                        (blackboard) =>
                        {
                            ++treeResult1;
                            return BehaviorTree.Enums.BehaviorTreeStatus.Success;
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

    public class TestBlackboard
    {
        public bool FirstNode;
        public bool SecondFirstNode;
    }
}