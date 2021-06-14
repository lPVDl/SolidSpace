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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(a => a.GetTypes()).ToList();

            foreach (var type in allTypes)
            {
                var attribute = type.GetCustomAttribute<InspectorDataValidatorAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                if (type.IsAbstract)
                {
                    var message = $"'{type.FullName}' can not be used for validation. Validator can not be abstract";
                    Debug.LogError(message);
                        
                    continue;
                }

                var methodFound = false;
                foreach (var inter in type.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var genericDefinition = inter.GetGenericTypeDefinition();
                    if (genericDefinition != typeof(IDataValidator<>))
                    {
                        continue;
                    }

                    var genericArgument0 = inter.GetGenericArguments()[0];
                    if (Validators.TryGetValue(genericArgument0, out var validationMethod))
                    {
                        var message = $"'{genericArgument0.FullName}' can be validated by more than one validator. " +
                                      $"'{validationMethod.validator.GetType().FullName}' will be used. " +
                                      $"'{type.FullName}' will be ignored.";
                        Debug.LogError(message);
                        
                        break;
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
                            
                            break;
                        }

                        Validators[genericArgument0] = method;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    methodFound = true;
                }

                if (!methodFound)
                {
                    Debug.LogError($"Failed to find any validation method in '{type.FullName}'");
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
            // TODO [T-7]: Validation ignores class inheritance.
            return Validators.TryGetValue(type, out validator);
        }
    }
}