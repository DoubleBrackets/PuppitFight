using System;
using System.Collections.Generic;
using System.Linq;
using Puppitor;
using UnityEngine;
using Random = System.Random;

namespace UCTSearch
{
    // a node in the game tree
    // does not track turns
    public class Node
    {
        public List<Node> childNodes;
        public Tuple<string, string> nodeMove;
        public Node parentNode;
        public double reward;
        public List<Tuple<string, string>> untriedMoves;
        public int visits;

        public Node(Tuple<string, string> move = null, Node parent = null, ActionKeyMap<KeyCode> actionKeyMap = null)
        {
            nodeMove = move;
            parentNode = parent;
            childNodes = new List<Node>();
            reward = 0;
            visits = 0;
            untriedMoves = actionKeyMap.GetMoves();
        }

        // use the UCB1 formula to select a child node
        public Node UCTSelectChild()
        {
            return childNodes.OrderByDescending(c => c.reward / c.visits + Math.Sqrt(2 * Math.Log(visits) / c.visits))
                .ToList()[0];
        }

        // remove nodeMove from untriedMoves and add a new child node for the move and return it
        public Node AddChild(Tuple<string, string> triedMove, ActionKeyMap<KeyCode> actionKeyMap)
        {
            var node = new Node(triedMove, this, actionKeyMap);
            untriedMoves.Remove(triedMove);
            childNodes.Add(node);
            return node;
        }

        // update this node with an additional visit and add a specified result to the node's reward
        public void Update(double result)
        {
            visits++;
            reward += result;
        }
    }

    public static class UCTThink
    {
        private static readonly Random randomInstance = new();

        // conduct a UCT search for itermax iterations from the given root state
        // returns the best move from the root state
        public static Tuple<string, string> UCT_Think(AffectVector rootAffectVector,
            ActionKeyMap<KeyCode> actionKeyMap, Affecter characterAffecter, string goalEmotion, int itermax,
            int rolloutMax = 50)
        {
            var rootNode = new Node(null, null, actionKeyMap);

            for (var i = 0; i < itermax; i++)
            {
                Node currNode = rootNode;
                var affectVector = new AffectVector(rootAffectVector);
                var action = "";
                var modifier = "";

                // selection
                while (currNode.untriedMoves.Count < 1 && currNode.childNodes.Count > 0)
                {
                    currNode = currNode.UCTSelectChild();
                    Tuple<string, string> temp = UpdateAffectState(currNode.nodeMove, affectVector, characterAffecter);
                    action = temp.Item1;
                    modifier = temp.Item2;
                }

                // expansion
                if (currNode.untriedMoves.Count > 0)
                {
                    Tuple<string, string> move =
                        currNode.untriedMoves[randomInstance.Next(0, currNode.untriedMoves.Count)];
                    Tuple<string, string> temp = UpdateAffectState(move, affectVector, characterAffecter);
                    action = temp.Item1;
                    modifier = temp.Item2;

                    currNode = currNode.AddChild(move, actionKeyMap);
                }

                // rollout until we find an affectVector where the goalEmotion has a higher relative value to the other affects or we have done N simulations
                var rolloutLength = 0;
                while (affectVector.EvaluateAffectVector(characterAffecter.CurrentAffect, goalEmotion) < 0 &&
                       rolloutLength < rolloutMax)
                {
                    Tuple<string, string> temp = UpdateAffectState(
                        actionKeyMap.Moves[randomInstance.Next(0, actionKeyMap.Moves.Count)], affectVector,
                        characterAffecter);
                    action = temp.Item1;
                    modifier = temp.Item2;
                    rolloutLength++;
                }

                // backpropogate
                while (currNode != null)
                {
                    currNode.Update(affectVector.EvaluateAffectVector(characterAffecter.CurrentAffect,
                        goalEmotion));
                    currNode = currNode.parentNode;
                }
            }

            return rootNode.childNodes.OrderByDescending(c => c.reward / c.visits).ToList()[0].nodeMove;
        }

        private static Tuple<string, string> UpdateAffectState(Tuple<string, string> move,
            AffectVector affectVector, Affecter characterAffecter)
        {
            string action = move.Item1;
            string modifier = move.Item2;
            characterAffecter.UpdateAffect(affectVector, action, modifier);
            characterAffecter.GetPrevailingAffect(affectVector);

            return new Tuple<string, string>(action, modifier);
        }
    }
}