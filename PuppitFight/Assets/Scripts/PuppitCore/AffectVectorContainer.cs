using Puppitor;
using UnityEngine;

/// <summary>
///     MonoBehaviour that contains an AffectVector. Used for sharing an affect vector between multiple Puppits
/// </summary>
public class AffectVectorContainer : MonoBehaviour
{
    public AffectVector AffectVector
    {
        get
        {
            if (_affectVector == null)
            {
                _affectVector = new AffectVector();
            }

            return _affectVector;
        }
    }

    private AffectVector _affectVector;
}