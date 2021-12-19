using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    public interface IEntityDestructionBuffer
    {
        void ScheduleDestroy(NativeSlice<Entity> entities);

        void ScheduleDestroy(Entity entity);
    }
}