using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using Random = System.Random;
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
using System.Text.Json;
#endif

namespace Puppitor
{
    /// <summary>
    ///     Interior class for use as part of parsing a Puppitor rule file into a useable format by C#.
    ///     AffectName should correspond to the key where the AffectEntry instance is stored.
    ///     Adjacent_affects may be empty.
    ///     Actions, modifiers, and equilibrium_point are the primary elements that should be accessed by an Affecter.
    ///     NOTE: AffectEntry uses snake_case for portability with JSON across languages, if you have a problem with that it's
    ///     on you.
    /// </summary>
    public class AffectEntry
    {
        //public string affectName { get; set; }
        public Dictionary<string, double> Actions { get; set; }
        public Dictionary<string, double> Modifiers { get; set; }
        public Dictionary<string, int> AdjacentAffects { get; set; }
        public double EquilibriumPoint { get; set; }

        public override string ToString()
        {
            var result = "";

            //result += "\naffect: " + affectName;

            result += "\n\tactions:";
            foreach (KeyValuePair<string, double> kvp in Actions)
            {
                result += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            }

            result += "\n\tmodifiers:";
            foreach (KeyValuePair<string, double> kvp in Modifiers)
            {
                result += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            }

            result += "\n\tadjacent affects:";
            foreach (KeyValuePair<string, int> affect in AdjacentAffects)
            {
                result += "\n\t\t" + affect.Key + ": " + affect.Value;
            }

            result += "\n\tequilibrium point: " + EquilibriumPoint + "\n";

            return result;
        }
    }

    /// <summary>
    ///     Affecter is a wrapper around a JSON object based dictionary of affects (see contents of the affect_rules directory
    ///     for formatting details)
    ///     By default Affecter clamps the values of an affect vector (dictionaries built using MakeAffectVector) in the
    ///     range of 0.0 to 1.0 and uses theatrical terminology, consistent with
    ///     the default keys in action_key_map.py inside of the actual_action_states dictionary in the Action_Key_Map class
    /// </summary>
    public class Affecter
    {
        public string CurrentAffect { get; private set; }

        private readonly string _equilibriumClassAction;
        private readonly double _ceilValue;
        private readonly double _floorValue;

        private readonly Random _randomInstance;

        private readonly List<string> _affectNameList;
        private readonly List<string> _connectedAffects;

        private readonly bool _log;

        public Dictionary<string, AffectEntry> AffectRules;

        public Affecter(
            string affectRulesJson,
            double affectFloor = 0.0,
            double affectCeiling = 1.0,
            string equilibriumAction = "resting",
            bool log = false)
        {
            _log = log;

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            Log(affectRulesJSON);
            affectRules = JsonSerializer.Deserialize<Dictionary<string, AffectEntry>>(affectRulesJSON);
#else
            Log("Falling Back to SimpleJSON");
            JsonClass affectRulesTemp = Json.Parse(affectRulesJson).AsObject;

            Log(affectRulesTemp.ToString());

            AffectRules = new Dictionary<string, AffectEntry>();

            ConvertRules(affectRulesTemp);
#endif

            _floorValue = affectFloor;
            _ceilValue = affectCeiling;
            _equilibriumClassAction = equilibriumAction;
            CurrentAffect = null;

            // set the initial affect to the one with the highest equilibrium point
            foreach (KeyValuePair<string, AffectEntry> kvp in AffectRules)
            {
                double entryEquilibrium = kvp.Value.EquilibriumPoint;
                if (CurrentAffect == null)
                {
                    CurrentAffect = kvp.Key;
                }
                else if (entryEquilibrium > AffectRules[CurrentAffect].EquilibriumPoint)
                {
                    CurrentAffect = kvp.Key;
                }
            }

            _randomInstance = new Random();

            foreach (KeyValuePair<string, AffectEntry> affectEntry in AffectRules)
            {
                Log(affectEntry.Value.ToString());
            }

            // list of affects to be updated to avoid using elementAt()
            _affectNameList = new List<string>();

            foreach (KeyValuePair<string, AffectEntry> entry in AffectRules)
            {
                _affectNameList.Add(entry.Key);
            }

            // choice functions lists
            _connectedAffects = new List<string>();
        }

        /// <summary>
        ///     Helper function for use when loading a Puppitor rule file.
        ///     Converts a raw JSONClass into a dictionary of (string, AffectEntry)
        ///     Pairs to sandbox the usage of SimpleJSON to this file.
        ///     Also to convert data to its proper format
        /// </summary>
        /// <param name="affectRulesTemp"></param>
        private void ConvertRules(JsonClass affectRulesTemp)
        {
            foreach (KeyValuePair<string, JsonNode> nodeEntry in affectRulesTemp)
            {
                // make the new affect entry and setup containers
                var affectEntry = new AffectEntry();
                //affectEntry.affectName = nodeEntry.Key;
                affectEntry.EquilibriumPoint = Convert.ToDouble(nodeEntry.Value["equilibrium_point"]);
                affectEntry.AdjacentAffects = new Dictionary<string, int>();
                affectEntry.Actions = new Dictionary<string, double>();
                affectEntry.Modifiers = new Dictionary<string, double>();

                // populate each container with their corresponding data from the JSON file stored in affectRulesTemp
                foreach (KeyValuePair<string, JsonNode> adjacencyEntry in nodeEntry.Value["adjacent_affects"].AsObject)
                {
                    var tempIntValue = Convert.ToInt32(adjacencyEntry.Value);
                    affectEntry.AdjacentAffects.Add(adjacencyEntry.Key, tempIntValue);
                    //Log("{0}: {1}",adjacencyEntry.Key, affectEntry.adjacentAffects[adjacencyEntry.Key]);
                }

                foreach (KeyValuePair<string, JsonNode> actionEntry in nodeEntry.Value["actions"].AsObject)
                {
                    var tempDoubleVal = Convert.ToDouble(actionEntry.Value);
                    affectEntry.Actions.Add(actionEntry.Key, tempDoubleVal);
                    //Log("{0}: {1}", actionEntry.Key, affectEntry.actions[actionEntry.Key]);
                }

                foreach (KeyValuePair<string, JsonNode> modEntry in nodeEntry.Value["modifiers"].AsObject)
                {
                    var tempDoubleVal = Convert.ToDouble(modEntry.Value);
                    affectEntry.Modifiers.Add(modEntry.Key, tempDoubleVal);
                    //Log("{0}: {1}", modEntry.Key, affectEntry.modifiers[modEntry.Key]);
                }

                AffectRules.Add(nodeEntry.Key, affectEntry);
            }
        }

        /// <summary>
        ///     Discards the stored affect_rules and replaces it with a new rule file
        /// </summary>
        /// <param name="affectRuleFile"></param>
        public void LoadOpenRuleFile(string affectRuleFile)
        {
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            affectRules = JsonSerializer.Deserialize<Dictionary<string, AffectEntry>>(affectRuleFile);
#else
            Log("Falling Back to SimpleJSON");
            JsonClass affectRulesTemp = Json.Parse(affectRuleFile).AsObject;

            AffectRules = new Dictionary<string, AffectEntry>();

            ConvertRules(affectRulesTemp);
#endif

            // update the affect list with the new rule file domain
            _affectNameList.Clear();

            foreach (KeyValuePair<string, AffectEntry> entry in AffectRules)
            {
                _affectNameList.Add(entry.Key);
            }
        }

        private void Log(string message)
        {
            if (_log)
            {
                Log(message);
            }
        }

        /// <summary>
        ///     Apply an action w/ modifier to an affect vector, using this Affector's rules.
        ///     To make sure affectVectors are in the correct format, use <see cref="MakeAffectVector" />.
        ///     NOTE: Performing equilibriumAction will move affect values towards the equilibriumValue of the Affecter.
        ///     NOTE: Clamps affect values between floorValue and ceilValue.
        /// </summary>
        /// <param name="affectVector">The affect vector to update. Float is the strength of the expressed effect</param>
        /// <param name="currentAction">Standard action expressed by an ActionKeyMap instance in its actual_action_states</param>
        /// <param name="currentModifier"></param>
        /// <param name="valueMultiplier">Static multiplier to the affect change</param>
        /// <param name="valueAdd">Static offset to the affect change (applies after valueMultiplier)</param>
        public void UpdateAffect(
            AffectVector affectVector,
            string currentAction,
            string currentModifier,
            double valueMultiplier = 1.0, double valueAdd = 0.0)
        {
            // using a raw for loop here because the values within the affectVector can be changed
            //for (int i = 0; i < affectVector.Count; i++)
            foreach (string affectName in _affectNameList)
            {
                double currentActionUpdateValue = AffectRules[affectName].Actions[currentAction];
                double currentModifierUpdateValue = AffectRules[affectName].Modifiers[currentModifier];
                double currentEquilibriumValue = AffectRules[affectName].EquilibriumPoint;
                double currentAffectValue = affectVector[affectName];

                double valueToAdd = valueMultiplier * (currentModifierUpdateValue * currentActionUpdateValue) +
                                    valueAdd;

                // while performing the resting action, move values towards the given equilibrium point
                if (currentAction.Equals(_equilibriumClassAction))
                {
                    int equilibriumDirection = currentAffectValue < currentEquilibriumValue ? 1 : -1;
                    double floor = currentAffectValue < currentEquilibriumValue ? _floorValue : currentEquilibriumValue;
                    double ceil = currentAffectValue < currentEquilibriumValue ? currentEquilibriumValue : _ceilValue;

                    affectVector.UpdateAndClampValues(affectName, equilibriumDirection * Math.Abs(valueToAdd), floor,
                        ceil);
                }
                else
                {
                    affectVector.UpdateAndClampValues(affectName, valueToAdd, _floorValue, _ceilValue);
                }
            }
        }

        /// <summary>
        ///     Chooses the next Current Affect.
        /// </summary>
        /// <param name="possibleAffects">
        ///     A list of strings of affects defined in the .json file loaded into the Affecter
        ///     instance. Can be generated using the GetPossibleAffects() function
        /// </param>
        /// <param name="randomFloor"></param>
        /// <param name="randomCeil"></param>
        /// <returns></returns>
        public string ChoosePrevailingAffect(List<string> possibleAffects, int randomFloor = 0, int randomCeil = 101)
        {
            _connectedAffects.Clear();

            // The choice logic is as follows
            // 1. Pick the only available affect
            if (possibleAffects.Count == 1)
            {
                CurrentAffect = possibleAffects[0];
                return CurrentAffect;
            }

            // 2. if there is more than one and the currentAffect is in the set of possible affects pick it
            if (possibleAffects.Contains(CurrentAffect))
            {
                return CurrentAffect;
            }

            // 3. if the currentAffect is not in the set but there is at least one affect connected to the current affect
            // pick from that subset, with weights if any are specified
            Dictionary<string, int> currAdjacencyWeights = AffectRules[CurrentAffect].AdjacentAffects;

            foreach (string affect in possibleAffects)
            {
                if (currAdjacencyWeights.ContainsKey(affect))
                {
                    _connectedAffects.Add(affect);
                }
            }

            if (_connectedAffects.Count > 0)
            {
                int randomNum = _randomInstance.Next(randomFloor, randomCeil);
                // weighted random choice of the connected affects to the current affect
                // a weight of 0 is ignored
                foreach (string affect in _connectedAffects)
                {
                    int currAffectWeight = currAdjacencyWeights[affect];
                    if (currAffectWeight > 0 && randomNum <= currAffectWeight)
                    {
                        CurrentAffect = affect;
                        return CurrentAffect;
                    }

                    randomNum -= currAffectWeight;
                }

                // if all weights are 0, just pick randombly
                randomNum = _randomInstance.Next(_connectedAffects.Count);

                CurrentAffect = _connectedAffects[randomNum];
                return CurrentAffect;
            }

            int randomIndex = _randomInstance.Next(possibleAffects.Count);
            CurrentAffect = possibleAffects[randomIndex];
            return CurrentAffect;
        }

        /// <summary>
        ///     Utility Wrapper function around the GetPossibleAffects() to ChoosePrevailingAffect() pipeline
        /// </summary>
        /// <returns>Prevailing effect in the AffectVector</returns>
        public string GetPrevailingAffect(AffectVector affectVector)
        {
            List<string> possibleAffects = affectVector.GetAllPrevailingAffects();
            string prevailingAffect = ChoosePrevailingAffect(possibleAffects);
            return prevailingAffect;
        }

        /// <summary>
        ///     Creates a matching affect vector dictionary based on provided affect rules.
        ///     NOTE: it is recommended you make an Affecter instance THEN make the corresponding AffectVector to make sure the
        ///     keys match
        /// </summary>
        public AffectVector MakeAffectVector()
        {
            List<string> affectNames = AffectRules.Keys.ToList();
            var affectVector = new AffectVector();

            foreach (string affect in affectNames)
            {
                affectVector.Add(affect, AffectRules[affect].EquilibriumPoint);
            }

            return affectVector;
        }

        /// <returns>List including the names of all actions included in the affect rules</returns>
        public List<string> GetAllActionNames()
        {
            return AffectRules.Values.SelectMany(affectEntry => affectEntry.Actions.Keys).Distinct().ToList();
        }

        public List<string> GetAllModifierNames()
        {
            return AffectRules.Values.SelectMany(affectEntry => affectEntry.Modifiers.Keys).Distinct().ToList();
        }
    }
}