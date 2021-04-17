using Sirenix.OdinInspector.Editor.Validation;
using SpaceSimulator.Editor.Validation;

[assembly: RegisterValidator(typeof(NullReferenceValidator))]
[assembly: RegisterValidator(typeof(EntityCycleConfigValidator))]
[assembly: RegisterValidator(typeof(GameCycleConfigValidator))]