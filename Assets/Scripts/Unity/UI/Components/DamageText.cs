

using System;
using DG.Tweening;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Unity.UI.Components.Game;
using TMPro;
using UnityEngine;

namespace Project.Unity.UI.Components
{
  [RequireComponent(typeof(AnchorToTarget))]
  public class DamageText : MonoBehaviour,  ICompletable
  {
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _duration;
    [SerializeField] private AnimationCurve _moveCurve;
    [SerializeField] private AnimationCurve _scaleCurve;
    
    private Action<ICompletable> _onComplete;
    private AnchorToTarget _anchorToTarget;

    Component ICompletable.Value => this;

    void ICompletable.SetOnCompleteAction(Action<ICompletable> action)
    {
      _onComplete = action;
    }

    public void Play(DamageVisualContext ctx)
    {
      if (ctx.IsDodged)
      {
        _text.text = "MISS";
      }
      else
      {
        _text.text = ctx.Amount.ToString();
      }
    }

    public void OnAnimationEnd()
    {
       _onComplete?.Invoke(this);
    }
  }
}