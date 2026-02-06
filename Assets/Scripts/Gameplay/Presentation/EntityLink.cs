using UnityEngine;

namespace Project.Gameplay.Presentations
{
  public interface IPresenter 
  {
    void Init(EntityLink link);
  }
  
  public class EntityLink : MonoBehaviour
  {
    public int EntityId => _entity.Id;
    
    private Entity _entity;
    public EntityInstances Map { get; set; }

    public void Init(Entity entity, EntityInstances map)
    {
      _entity = entity;
      Map = map;
    }

    public Entity GetEntity()
    {
      return _entity;
    }
  }
}
