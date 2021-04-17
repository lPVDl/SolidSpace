using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpaceSimulator.Runtime;

using Debug = UnityEngine.Debug;

namespace SpaceSimulator.Editor.Validation
{
    public static class AssemblyValidatorFactory
    {
        private static readonly Dictionary<Type, ValidationMethod> _validators;
        private static readonly Type[] _argumentTypes;

        static AssemblyValidatorFactory()
        {
            try
            {
                _validators = new Dictionary<Type, ValidationMethod>();
                _argumentTypes = new[] { typeof(object), typeof(ValidationResult) };
                
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
                    if (genericDefinition != typeof(IValidator<>))
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
                    if (_validators.TryGetValue(genericArgument0, out var validationMethod))
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

                        _validators[genericArgument0] = method;
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
            IValidator<int> _dummy;

            _argumentTypes[0] = genericArgumentType;

            return objectType.GetMethod(nameof(_dummy.Validate), _argumentTypes);
        }

        public static bool TryGetValidatorFor(Type type, out ValidationMethod validator)
        {
            return _validators.TryGetValue(type, out validator);
        }
    }
}