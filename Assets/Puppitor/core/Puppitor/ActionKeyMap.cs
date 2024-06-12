using System;
using System.Collections.Generic;
using System.Linq;

namespace Puppitor
{
    /// <summary>
    ///     <para />
    ///     Action_key_map contains the interface for storing keybindings and performing actions.
    ///     <para />
    ///     The dictionary is modeled on Ren'Py's keymap.
    ///     <para />
    ///     The class also wraps the flags for detecting if an action is being done.
    ///     <para />
    ///     The possible_action_states dict is used to keep track of all interpreted actions being done and is updated based on
    ///     keys and buttons pressed.
    ///     <para />
    ///     The actual_action_states is broken into modifier states and actions.
    ///     <para />
    ///     Only one action can be happening at a time.
    ///     <para />
    ///     Only one modifier can be active at a time.
    ///     <para />
    ///     Modifiers update independently from actions.
    ///     <para />
    ///     Actions update independently from modifiers.
    /// </summary>
    /// <typeparam name="TInputT"></typeparam>
    public class ActionKeyMap<TInputT>
    {
        public readonly Dictionary<string, Dictionary<string, bool>> ActualActionStates;
        public readonly List<Tuple<string, string>> Moves;

        private readonly Dictionary<string, string> _currentStates;

        private readonly Dictionary<string, string> _defaultStates;
        private readonly Dictionary<string, Dictionary<string, List<TInputT>>> _keyMap;

        private readonly Dictionary<string, bool> _possibleActionStates;

        private readonly Dictionary<string, List<string>> _updatableStates;

        public ActionKeyMap(Dictionary<string, Dictionary<string, List<TInputT>>> keyMap,
            string defaultAction = "resting", string defaultModifier = "neutral")
        {
            // this dictionary and values should not be modified ever and are generally for internal use only
            _defaultStates = new Dictionary<string, string>();
            _defaultStates.Add("actions", defaultAction);
            _defaultStates.Add("modifiers", defaultModifier);

            // this dictionary and values should only be modified internally and are used to access the current state of the actions being performed 
            _currentStates = new Dictionary<string, string>();
            _currentStates.Add("actions", _defaultStates["actions"]);
            _currentStates.Add("modifiers", _defaultStates["modifiers"]);

            Moves = new List<Tuple<string, string>>();

            // expected format of keyMap
            /*
            {
                "actions": {
                            "open_flow": [InputT n],
                            'closed_flow': [InputT m],
                            'projected_energy': [InputT b]
                },
                "modifiers": {
                            "tempo_up": [InputT c],
                            "tempo_down": [InputT z]
                }
            }
            */

            _keyMap = keyMap;

            // flags corresponding to actions being specified by the user input
            // FOR INPUT DETECTION USE ONLY
            // MULTIPLE ACTIONS AND MODIFIER STATES CAN BE TRUE
            // structure of possibleActionStates
            /*
                possibleActionStates = {
                            "open_flow" : false,
                            "closed_flow" : false,
                            "projected_energy" : false,
                            "tempo_up" : false,
                            "tempo_down" : false
            */

            _possibleActionStates = new Dictionary<string, bool>();

            foreach (string action in keyMap["actions"].Keys)
            {
                _possibleActionStates.Add(action, false);
            }

            foreach (string modifier in keyMap["modifiers"].Keys)
            {
                _possibleActionStates.Add(modifier, false);
            }

            // flags used for specifying the current state of actions for use in updating a character's physical affect
            // FOR SEMANTIC USE
            // ONLY ONE ACTION AND MODIFIER STATE CAN BE TRUE

            /*actualActionStates = {
                            "actions" : {"resting" : true, "open_flow" : false, "closed_flow" : false, "projected_energy" : false},
                            "modifiers" : {"tempo_up" : false, "tempo_down" : false, "neutral" : true}
            */

            var tempActualActionDict = new Dictionary<string, bool>();
            tempActualActionDict[defaultAction] = true;
            foreach (string action in keyMap["actions"].Keys)
            {
                tempActualActionDict[action] = false;
            }

            var tempActualModifierDict = new Dictionary<string, bool>();
            tempActualModifierDict[defaultModifier] = true;
            foreach (string modifier in keyMap["modifiers"].Keys)
            {
                tempActualModifierDict[modifier] = false;
            }

            ActualActionStates = new Dictionary<string, Dictionary<string, bool>>();
            ActualActionStates.Add("actions", tempActualActionDict);
            ActualActionStates.Add("modifiers", tempActualModifierDict);

            tempActualActionDict = null;
            tempActualModifierDict = null;

            _updatableStates = new Dictionary<string, List<string>>();
            _updatableStates.Add("actions", ActualActionStates["actions"].Keys.ToList());
            _updatableStates.Add("modifiers", ActualActionStates["modifiers"].Keys.ToList());

            foreach (string action in ActualActionStates["actions"].Keys)
            {
                foreach (string modifier in ActualActionStates["modifiers"].Keys)
                {
                    Moves.Add(new Tuple<string, string>(action, modifier));
                }
            }

            Console.Write(this);
        }

        // USED FOR UPDATING BASED ON KEYBOARD INPUTS    
        public void UpdatePossibleStates(string stateToUpdate, bool newValue)
        {
            if (_possibleActionStates.ContainsKey(stateToUpdate))
            {
                _possibleActionStates[stateToUpdate] = newValue;
            }
        }

        /* USED FOR UPDATING THE INTERPRETABLE STATE BASED ON WHICH ACTION IS DISPLAYED
         * updates a specified action or modifier to a new boolean value
         * UPDATING AN ACTION WILL SET ALL OTHER ACTIONS TO FALSE
         * UPDATING A MODIFIER WILL SET ALL OTHER MODIFIERS TO FALSE

         * MODIFERS AND ACTIONS ARE ASSUMED TO BE MUTUALLY EXCLUSIVE WHEN UPDATING
         */
        public void UpdateActualStates(string stateToUpdate, string classOfAction, bool newValue)
        {
            List<string> states = _updatableStates[classOfAction];

            if (states.Contains(stateToUpdate))
            {
                // go through each of the possible actions or modifiers
                // and set all but the one being explicitly changed to false
                // and use the given value (newValue) to update the value of the
                // specified action/modifier
                foreach (string state in states)
                {
                    if (stateToUpdate.Equals(state))
                    {
                        ActualActionStates[classOfAction][state] = newValue;
                        _currentStates[classOfAction] = state;
                    }
                    else
                    {
                        ActualActionStates[classOfAction][state] = false;
                    }
                }

                if (newValue == false)
                {
                    // return to doing the default behavior
                    _currentStates[classOfAction] = _defaultStates[classOfAction];
                    ActualActionStates[classOfAction][_defaultStates[classOfAction]] = true;
                }
            }
        }

        // makes a copy of moves to allow search algorithms like MCTS to easily store lists of available moves
        public List<Tuple<string, string>> GetMoves()
        {
            var moveList = new List<Tuple<string, string>>();

            foreach (Tuple<string, string> move in Moves)
            {
                moveList.Add(new Tuple<string, string>(move.Item1, move.Item2));
            }

            return moveList;
        }

        // switches the default action or modifier to the specified new default
        // newDefault must be an action or modifier in the existing set of actions and modifiers contained in ActionKeyMap
        // classOfAction must be either "actions" or "modifiers"
        public void ChangeDefault(string newDefault, string classOfAction)
        {
            if (!_defaultStates.ContainsKey(classOfAction))
            {
                Console.WriteLine(classOfAction + " is not an \"action\" or \"modifier\"");
                return;
            }

            if (!_keyMap[classOfAction].ContainsKey(newDefault))
            {
                Console.WriteLine(newDefault + " is not in " + classOfAction);
                return;
            }

            string oldDefault = _defaultStates[classOfAction];
            List<TInputT> oldNonDefaultKeys = _keyMap[classOfAction][newDefault];

            Console.WriteLine("original default: " + oldDefault + ", newDefault original keys: ");

            foreach (TInputT key in oldNonDefaultKeys)
            {
                Console.WriteLine(key);
            }

            _defaultStates[classOfAction] = newDefault;
            _keyMap[classOfAction].Remove(newDefault);
            _possibleActionStates.Remove(newDefault);
            _keyMap[classOfAction][oldDefault] = oldNonDefaultKeys;
            _possibleActionStates[oldDefault] = false;

            Console.WriteLine("keyMap: " + this);
        }

        public override string ToString()
        {
            var result = "\n";

            result += "defaultAction = " + _defaultStates["actions"] + "\ndefaultModifier = " +
                      _defaultStates["modifiers"] + "\n\n";

            result += "currentAction = " + _currentStates["actions"] + "\ncurrentModifier = " +
                      _currentStates["modifiers"] + "\n";

            result += "\nKeyMap:\n";

            foreach (KeyValuePair<string, Dictionary<string, List<TInputT>>> kvp in _keyMap)
            {
                result += "\t" + kvp.Key + "\n";

                foreach (KeyValuePair<string, List<TInputT>> kvpInner in kvp.Value)
                {
                    result += "\t\t" + kvpInner.Key + " = ";

                    foreach (TInputT key in kvpInner.Value)
                    {
                        result += key + "\n";
                    }
                }
            }

            result += "\nPossibleActionStates:\n";

            foreach (KeyValuePair<string, bool> kvp in _possibleActionStates)
            {
                result += "\t" + kvp.Key + " = " + kvp.Value + "\n";
            }

            result += "\nActualActionStates:\n";

            foreach (KeyValuePair<string, Dictionary<string, bool>> kvp in ActualActionStates)
            {
                result += "\t" + kvp.Key + "\n";

                foreach (KeyValuePair<string, bool> kvpInner in kvp.Value)
                {
                    result += "\t\t" + kvpInner.Key + " = " + kvpInner.Value + "\n";
                }
            }

            result += "\nMoves:";

            foreach (Tuple<string, string> move in Moves)
            {
                result += "(" + move.Item1 + ", " + move.Item2 + ") ";
            }

            result += "\n";

            return result;
        }
    }
}