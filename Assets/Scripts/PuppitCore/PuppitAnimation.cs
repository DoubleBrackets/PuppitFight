using UnityEngine;

public class PuppitAnimation : MonoBehaviour
{
    [SerializeField]
    private Puppit _puppit;

    [SerializeField]
    private Animator _animator;

    private string _currentAffect = string.Empty;

    private void Update()
    {
        string prevailAffect = _puppit.GetPrevailingAffect();
        _animator.SetBool(_currentAffect, false);
        _animator.SetBool(_puppit.GetPrevailingAffect(), true);
        _currentAffect = prevailAffect;
    }
}