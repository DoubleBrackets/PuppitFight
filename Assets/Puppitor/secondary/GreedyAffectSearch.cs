using System;
using System.Collections.Generic;
using Puppitor;
using UnityEngine;

public class GreedyAffectSearch
{
    private readonly List<string> affectNames;
    private readonly AffectVector[] copiedAffectVector;

    // main state tracking elements
    private readonly List<Tuple<double, string, string>> futureStatesForEval;

    private Tuple<double, string, string> futureStateEntry;
    private double goalEmotionValue;
    private int simulationIndex;

    public GreedyAffectSearch(int numActions, int numModifiers, List<string> affects)
    {
        futureStatesForEval = new List<Tuple<double, string, string>>();
        for (var i = 0; i < numActions * numModifiers; i++)
        {
            futureStatesForEval.Add(new Tuple<double, string, string>(0.0, "", ""));
        }

        affectNames = new List<string>();

        copiedAffectVector = new AffectVector[numActions * numModifiers];
        for (var i = 0; i < copiedAffectVector.Length; i++)
        {
            copiedAffectVector[i] = new AffectVector();
            foreach (string affect in affects)
            {
                copiedAffectVector[i].Add(affect, 0.0);
                affectNames.Add(affect);
            }
        }

        goalEmotionValue = 0;
        //futureStateEntry = new Tuple<double, string, string>();
        simulationIndex = 0;
    }

    public Tuple<string, string> Think(ActionKeyMap<KeyCode> actionsToTry, Affecter characterAffecter,
        AffectVector currAffectVector, string goalEmotion)
    {
        goalEmotionValue = 0;
        simulationIndex = 0;

        // perform every possible action and modifier combination to find the one with the highest value of the goal emotion
        foreach (string action in actionsToTry.ActualActionStates["actions"].Keys)
        {
            foreach (string modifier in actionsToTry.ActualActionStates["modifiers"].Keys)
            {
                foreach (string affect in affectNames)
                {
                    copiedAffectVector[simulationIndex][affect] = currAffectVector[affect];
                }

                characterAffecter.UpdateAffect(copiedAffectVector[simulationIndex], action, modifier);
                goalEmotionValue = copiedAffectVector[simulationIndex][goalEmotion];

                futureStateEntry = new Tuple<double, string, string>(
                    copiedAffectVector[simulationIndex].EvaluateAffectVector(characterAffecter.CurrentAffect,
                        goalEmotion), action, modifier);
                futureStatesForEval[simulationIndex] = futureStateEntry;

                simulationIndex++;
            }
        }

        // sort states into ascending order by the goalEmotionValue
        futureStatesForEval.Sort((x, y) => x.Item1.CompareTo(y.Item1));

        // choose the state with the highest goalEmotionValue and return the action and modifier that was performed to get there
        Tuple<double, string, string> bestActionModifier = futureStatesForEval[futureStatesForEval.Count - 1];
        var finalActionModifier = new Tuple<string, string>(bestActionModifier.Item2, bestActionModifier.Item3);
        return finalActionModifier;
    }
}