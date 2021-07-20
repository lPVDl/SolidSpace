using Unity.Entities;

namespace SolidSpace.Entities.Splitting
{
    public interface ISplittingCommandSystem
    {
        void ScheduleSplittingCheck(Entity entity);
    }
}