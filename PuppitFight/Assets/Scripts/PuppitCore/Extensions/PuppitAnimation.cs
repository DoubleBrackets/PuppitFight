using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     Simple animator that maps Puppit's prevailing affect directly to an animation state
/// </summary>
public class PuppitAnimation : MonoBehaviour
{
    [FormerlySerializedAs("_puppit")]
    [SerializeField]
    private PuppitLimb _puppitLimb;

    [SerializeField]
    private Animator _animator;

    private string _currentAffect = string.Empty;

    private void Update()
    {
        string prevailAffect = _puppitLimb.GetPrevailingAffect();
        _animator.SetBool(_currentAffect, false);
        _animator.SetBool(_puppitLimb.GetPrevailingAffect(), true);
        _currentAffect = prevailAffect;
    }
}