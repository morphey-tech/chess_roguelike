using UnityEngine;

namespace Project.Gameplay.Presentations
{
  public interface IPresentationsMap
  {
    public bool Has(int id);
    public GameObject Find(int id);
    public GameObject Get(int id);
  }
}
