using System;
using System.Collections.Generic;
using Puppitor;

namespace AStarSearch
{
    // nodes are tuples of (affectVector, action, modifier, prevailingAffect)
    using Node = Tuple<AffectVector, string, string, string>;
    using Item = Tuple<double, Tuple<AffectVector, string, string, string>>;

    public static class AStarThink
    {
        // start is a node and should be a tuple of (affectVector, action, modifier, prevailingAffect)
        public static List<Tuple<string, string>> A_Star_Think(
            Affecter characterAffecter,
            ActionKeyMap<string> actionKeyMap,
            Node start,
            string goalEmotion,
            double stepMultiplier = 1,
            int maxQueueSize = 10000)
        {
            var frontier = new List<Item>(); // figure out what the hell to do about not having a priority queue in STL
            var visitedNodes = new List<Node>();
            var costSoFar = new Dictionary<Node, double>();
            var prevNode = new Dictionary<Node, Node>();

            var startItem = new Item(0, start);
            frontier.Add(startItem);
            prevNode.Add(start, null);
            costSoFar.Add(start, 0);

            //Console.WriteLine("goalEmotion: " + goalEmotion + "\nprevailingAffect: " + frontier[0].Item2.Item4 + "\ngoalEmotion value: " + frontier[0].Item2.Item1[goalEmotion] + "\nmaxValue in affectVector: " + frontier[0].Item2.Item1[Affecter.GetPossibleAffects(frontier[0].Item2.Item1)[0]]);
            while (frontier.Count > 0 && frontier.Count < maxQueueSize)
            {
                Item kvpTemp = frontier[0];
                double currCost = kvpTemp.Item1;
                Node currNode = kvpTemp.Item2;

                // remember to sort frontier
                frontier.RemoveAt(0);

                if (frontier.Count > 0)
                {
                    frontier.Sort((x, y) =>
                        x.Item1.CompareTo(y
                            .Item1)); // sort frontier in ascending order based on their priorities (Item Item1)
                }

                //Debug.Log("currCost: " + currCost + "\ncurrNode: " + currNode);
                // check if our prevailing emotion is our goal
                if (currNode.Item4.Equals(goalEmotion))
                {
                    // make the path to the goal
                    var path = new List<Tuple<string, string>>();
                    while (currNode != null)
                    {
                        var nodeStep = new Tuple<string, string>(currNode.Item2, currNode.Item3);
                        var counter = 0;
                        while (counter < stepMultiplier)
                        {
                            counter++;
                            path.Add(nodeStep);
                        }

                        currNode = prevNode[currNode];
                    }

                    Console.WriteLine("Path count: " + path.Count + "\nFrontier count: " + frontier.Count);
                    return path;
                }

                // check every adjacent node of the current node and if it is a new node or a more efficient way to get to nextNode, add it to frontier
                foreach (Tuple<double, Node> next in PuppitorAdjacencies(characterAffecter, actionKeyMap,
                             currNode.Item1, goalEmotion, stepMultiplier))
                {
                    double next_cost = next.Item1;
                    Node nextNode = next.Item2;

                    double newCost = costSoFar[currNode] + next_cost +
                                     Heuristic(nextNode.Item4, nextNode.Item1, goalEmotion);

                    if (!costSoFar.ContainsKey(nextNode) || newCost < costSoFar[nextNode])
                    {
                        costSoFar[nextNode] = newCost;
                        var newItem = new Item(newCost, nextNode);
                        frontier.Add(newItem);
                        // remember to sort frontier
                        frontier.Sort((x, y) =>
                            x.Item1.CompareTo(y
                                .Item1)); // sort frontier in ascending order based on their priorities (Item Item1)
                        prevNode[nextNode] = currNode;
                    }
                }
            }

            Console.WriteLine("NO PATH FOUND WITHIN QUEUE SIZE LIMITS");
            return new List<Tuple<string, string>>();
        }

        // gets the nodes adjacent to the current affectVector
        private static List<Tuple<double, Node>> PuppitorAdjacencies(Affecter characterAffecter,
            ActionKeyMap<string> actionKeyMap, AffectVector nodeAffectVector, string goalEmotion,
            double stepMultiplier)
        {
            var moves = new List<Tuple<double, Tuple<AffectVector, string, string, string>>>();

            List<string> affects = nodeAffectVector.GetAllAffectNames();

            // make a list of every action and modifier combination and add those nodes to the adjacency list
            foreach (Tuple<string, string> move in actionKeyMap.Moves)
            {
                var nextState = new AffectVector(nodeAffectVector);

                List<string> maxValueAffect = nextState.GetAllPrevailingAffects();
                double stepDistance = stepMultiplier;
                double goalMaxDistance = CalcDistanceBetweenAffects(nextState, maxValueAffect[0], goalEmotion);

                if (goalMaxDistance < stepMultiplier)
                {
                    stepDistance = goalMaxDistance;
                }

                characterAffecter.UpdateAffect(nextState, move.Item1, move.Item2, stepMultiplier);

                var node = new Node(nextState,
                    move.Item1,
                    move.Item2,
                    characterAffecter.GetPrevailingAffect(nextState));

                double cost = PuppitorEdgeCost(characterAffecter, nextState, move.Item1, move.Item2, affects,
                    goalEmotion, stepDistance); // calculate the edge cost of the new node

                var costNode = new Tuple<double, Node>(cost, node); // package the cost and node tuple together

                moves.Add(costNode);
            }

            return moves;
        }

        // calculates the magnitude of the change in value eof the goalEmotion in affectVector
        public static double PuppitorEdgeCost(
            Affecter characterAffecter,
            AffectVector affectVector,
            string action,
            string modifier,
            List<string> affects,
            string goalEmotion,
            double stepMultiplier)
        {
            double cost =
                Math.Abs(
                    AffecterActionModifierProduct(characterAffecter, action, modifier, goalEmotion, stepMultiplier));
            return cost;
        }

        public static double AffecterActionModifierProduct(Affecter characterAffecter, string action, string modifier,
            string affect, double stepMultiplier)
        {
            return characterAffecter.AffectRules[affect].Actions[action] *
                   characterAffecter.AffectRules[affect].Modifiers[modifier] * stepMultiplier;
        }

        public static double Heuristic(string currentAffect, AffectVector affectVector,
            string goalEmotion)
        {
            double heuristicValue = 0;

            List<string> maxValueNodes = affectVector.GetAllPrevailingAffects();

            if (maxValueNodes.Contains(goalEmotion) && !currentAffect.Equals(goalEmotion))
            {
                heuristicValue = double.MaxValue;
            }

            if (maxValueNodes.Count > 0)
            {
                heuristicValue =
                    affectVector[maxValueNodes[0]] -
                    affectVector
                        [goalEmotion]; // calculate the distance between the maximum value in the affectVector and the current value of goalEmotion
            }

            return heuristicValue;
        }

        public static double CalcDistanceBetweenAffects(AffectVector affectVector, string firstAffect,
            string secondAffect)
        {
            return affectVector[firstAffect] - affectVector[secondAffect];
        }
    }
}