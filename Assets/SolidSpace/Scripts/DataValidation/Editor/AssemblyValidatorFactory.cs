using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace SolidSpace.DataValidation.Editor
{
    internal static class AssemblyValidatorFactory
    {
        private static readonly Dictionary<Type, ValidationMethod> Validators;
        private static readonly Type[] ArgumentTypes;

        static AssemblyValidatorFactory()
        {
            try
            {
                Validators = new Dictionary<Type, ValidationMethod>();
                ArgumentTypes = new[] { typeof(object) };
                
                Initialize();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void Initialize()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("Assembly-CSharp"));
            
            var allTypes = assemblies.SelectMany(a => a.GetTypes()).ToList();

            foreach (var type in allTypes)
            {
                foreach (var inter in type.GetInterfaces())
                {
                    if (!inter.IsGenericType)
                    {
                        continue;
                    }

                    var genericDefinition = inter.GetGenericTypeDefinition();
                    if (genericDefinition != typeof(IDataValidator<>))
                    {
                        continue;
                    }

                    if (!type.IsClass || type.IsAbstract)
                    {
                        var message = $"'{type.FullName}' can not be used for validation. Validator must be non-abstract class.";
                        Debug.LogError(message);
                        continue;
                    }

                    var genericArgument0 = inter.GetGenericArguments()[0];
                    if (Validators.TryGetValue(genericArgument0, out var validationMethod))
                    {
                        var message = $"'{genericArgument0.FullName}' can be validated by more than one validator. " +
                                      $"'{validationMethod.validator.GetType().FullName}' will be used. " +
                                      $"'{type.FullName}' will be ignored.";
                        Debug.LogError(message);
                        continue;
                    }
                    
                    try
                    {
                        var method = new ValidationMethod
                        {
                            method = GetValidationMethod(type, genericArgument0),
                            validator = Activator.CreateInstance(type)
                        };

                        if (method.method == null)
                        {
                            Debug.LogError($"Failed to find validation method in '{type.FullName}'");
                            continue;
                        }

                        Validators[genericArgument0] = method;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        private static MethodInfo GetValidationMethod(Type objectType, Type genericArgumentType)
        {
            IDataValidator<object> dummy;

            ArgumentTypes[0] = genericArgumentType;

            return objectType.GetMethod(nameof(dummy.Validate), ArgumentTypes);
        }

        public static bool TryGetValidatorFor(Type type, out ValidationMethod validator)
        {
            return Validators.TryGetValue(type, out validator);
        }
    }
}