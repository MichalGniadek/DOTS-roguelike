using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class GridPositionToTranslationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (ref Translation translation, in GridPosition position) =>
            {
                translation.Value.x = position.Value.x * 1;
                translation.Value.y = position.Value.y * 1;
            })
        .ScheduleParallel();
    }
}