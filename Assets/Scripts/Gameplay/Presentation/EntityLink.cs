using UnityEngine;

namespace Project.Gameplay.Presentations
{
  public interface IPresenter 
  {
    void Init(EntityLink link);
  }
  
  public class EntityLink : MonoBehaviour
  {
    public int EntityId { get; private set; }
    public PresentationManagerInstances Map { get; set; }

    public void Init(int id, PresentationManagerInstances map)
    {
      EntityId = id;
      Map = map;
    }
  }
}
