using System;
using System.Collections.Generic;
using System.IO;
using Puppitor;
using UnityEngine;

public class PuppitorTest : MonoBehaviour
{
    [SerializeField]
    private string _ruleFilePath1;

    [SerializeField]
    private string _ruleFilePath2;

    [SerializeField]
    private string _ruleFilePath3;

    private void Start()
    {
        Test();
    }

    private void Test()
    {
        Test1();
        Test2();
    }

    private void Test1()
    {
        /* Test 1 */
        string fileName = _ruleFilePath1;
        string jsonString = File.ReadAllText(fileName);

        var testAffecter = new Affecter(jsonString);

        AffectVector affectVector = testAffecter.MakeAffectVector();

        Debug.Log("affectVector");
        affectVector.LogAffectVector();

        testAffecter.UpdateAffect(affectVector, "open_flow", "neutral");

        Debug.Log("affectVector updated");
        affectVector.LogAffectVector();

        Debug.Log("affectVector Prevailing Affects");
        affectVector.LogPrevailingAffect();

        // test the user multiplier and addition values
        testAffecter.UpdateAffect(affectVector, "open_flow", "neutral", 1.5, 0.2);

        Debug.Log("affectVector updated");
        affectVector.LogAffectVector();

        Debug.Log("affectVector Prevailing Affects");
        affectVector.LogPrevailingAffect();

        Debug.Log("affectVector Current Affect");
        List<string> prevailingAffects = affectVector.GetAllPrevailingAffects();
        string currAffect = testAffecter.ChoosePrevailingAffect(prevailingAffects);
        Debug.Log(currAffect);

        Debug.Log($"wrapper function result: {testAffecter.GetPrevailingAffect(affectVector)}");

        Debug.Log("Affect Vector Evaluated Using joy: " +
                  affectVector.EvaluateAffectVector(testAffecter.CurrentAffect, "joy"));

        string fileString = JsonUtility.ToJson(testAffecter.AffectRules);

        Debug.Log(fileString);
    }

    private void Test2()
    {
        /* Test 2 */
        string fileName = _ruleFilePath2;
        string jsonString = File.ReadAllText(fileName);
        var affecterTest = new Affecter(jsonString);

        AffectVector affectVector = affecterTest.MakeAffectVector();

        Debug.Log("differentAffectVector");
        affectVector.LogAffectVector();

        affecterTest.UpdateAffect(affectVector, "cross_arms", "casually");

        Debug.Log("differentAffectVector updated");
        affectVector.LogAffectVector();

        Debug.Log("differentAffectVector Prevailing Affects");
        List<string> diffPrevailingAffects = affectVector.GetAllPrevailingAffects();
        foreach (string affect in diffPrevailingAffects)
        {
            Debug.Log(affect);
        }

        Debug.Log("differentAffectVector Current Affect");
        string currAffect = affecterTest.ChoosePrevailingAffect(diffPrevailingAffects);
        Debug.Log(currAffect);

        var modifierDict = new Dictionary<string, List<string>>
        {
            { "tempo up", new List<string> { "c" } },
            { "tempo down", new List<string> { "z" } }
        };

        var actionDict = new Dictionary<string, List<string>>
        {
            { "projected energy", new List<string> { "n" } },
            { "open flow", new List<string> { "m" } },
            { "closed flow", new List<string> { "b" } }
        };

        var keyMap = new Dictionary<string, Dictionary<string, List<string>>>
        {
            { "actions", actionDict },
            { "modifiers", modifierDict }
        };

        var test = new ActionKeyMap<string>(keyMap);

        test.UpdatePossibleStates("projected energy", true);

        Debug.Log(test);

        test.UpdateActualStates("projected energy", "actions", true);

        Debug.Log(test);

        Debug.Log("Now Testing GetMoves():");
        List<Tuple<string, string>> testGetMoves = test.GetMoves();

        foreach (Tuple<string, string> move in testGetMoves)
        {
            Console.Write("(" + move.Item1 + ", " + move.Item2 + ") ");
        }

        /* Test 3 */
        fileName = _ruleFilePath3;
        jsonString = File.ReadAllText(fileName);
        affecterTest.LoadOpenRuleFile(jsonString);

        foreach (KeyValuePair<string, AffectEntry> entry in affecterTest.AffectRules)
        {
            Debug.Log($"{entry.Key}:\n\t {entry.Value}");
        }

        Debug.Log("ChangeDefaults");
        test.ChangeDefault("projected energy", "actions");
    }
}