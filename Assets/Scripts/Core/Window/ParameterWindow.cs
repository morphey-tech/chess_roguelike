using System;

namespace Project.Core.Window
{

  public abstract class ParameterlessWindow : Window
  {
    public new void Show(bool immediate = false)
    {
      base.Show(immediate);
    }
  }

  public abstract class ParameterWindow<A1> : Window
  {
    public void Show(A1 a, bool immediate = false)
    {
      base.Show(immediate);

      OnShow(a);
    }

    public override void ShowWithDeps(object[] deps)
    {
      if (deps == null || deps.Length != 1)
        throw new ArgumentException(
          $"ParameterWindow.ShowWithDeps {this.GetType().Name} should be called with single argument");

      Show((A1)deps[0]);
    }

    protected new abstract void OnShow(A1 value);
  }

  public abstract class ParameterWindow<A1, A2> : Window
  {
    public void Show(A1 a1, A2 a2, bool immediate = false)
    {
      base.Show(immediate);

      OnShow(a1, a2);
    }

    public override void ShowWithDeps(object[] deps)
    {
      if (deps.Length != 2)
        throw new ArgumentException("ParameterWindow.ShowWithDeps should be called with two arguments");

      Show((A1)deps[0], (A2)deps[1]);
    }

    protected new abstract void OnShow(A1 a1, A2 a2);
  }

  public abstract class ParameterWindow<A1, A2, A3> : Window
  {
    public void Show(A1 a1, A2 a2, A3 a3, bool immediate = false)
    {
      base.Show(immediate);
      OnShow(a1, a2, a3);
    }

    public override void ShowWithDeps(object[] deps)
    {
      if (deps.Length != 3)
        throw new ArgumentException("ParameterWindow.ShowWithDeps should be called with three arguments");

      Show((A1)deps[0], (A2)deps[1], (A3)deps[2]);
    }

    protected new abstract void OnShow(A1 a1, A2 originalUpgradeView, A3 a3);
  }
}
