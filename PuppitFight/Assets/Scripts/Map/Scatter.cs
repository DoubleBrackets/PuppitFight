using System.Collections.Generic;
using UnityEngine;

// Based off https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
public class Scatter : MonoBehaviour
{
    [SerializeField]
    private GameObject _scatterPrefab;

    [SerializeField]
    private float _scatterDistance;

    [SerializeField]
    private int _sampleLimit;

    [SerializeField]
    private int _scatterCount;

    [SerializeField]
    private Vector2 _range;

    [SerializeField]
    private List<GameObject> _scatteredObjects;

    [ContextMenu("Scatter")]
    public void ScatterObjects()
    {
        if (Application.isPlaying)
        {
            return;
        }

        foreach (GameObject scatteredObject in _scatteredObjects)
        {
            DestroyImmediate(scatteredObject);
        }

        _scatteredObjects.Clear();

        for (var i = 0; i < _scatterCount; i++)
        {
            GameObject scatter = Instantiate(_scatterPrefab,
                new Vector3(Random.Range(-_range.x, _range.x), Random.Range(-_range.y, _range.y), 0),
                Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward));
            scatter.transform.SetParent(transform);
            _scatteredObjects.Add(scatter);
        }
    }
}