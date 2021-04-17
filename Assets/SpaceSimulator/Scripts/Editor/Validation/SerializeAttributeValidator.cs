using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using SpaceSimulator.Editor.Validation;
using SpaceSimulator.Runtime;
using UnityEngine;

[assembly: RegisterValidator(typeof(SerializeAttributeValidator))]

namespace SpaceSimulator.Editor.Validation
{
    public class SerializeAttributeValidator : AttributeValidator<SerializeAttribute>
    {
        private readonly Runtime.ValidationResult _result;
        private readonly HashSet<MethodInfo> _trashValidators;
        private readonly object[] _invocationParameters;

        public SerializeAttributeValidator()
        {
            _trashValidators = new HashSet<MethodInfo>();
            _result = new Runtime.ValidationResult();
            _invocationParameters = new object[] { null, _result };
        }
        
        protected override void Initialize()
        {
            _trashValidators.Clear();
        }

        protected override void Validate(Sirenix.OdinInspector.Editor.Validation.ValidationResult result)
        {
            if (Property.BaseValueEntry.ValueState == PropertyValueState.NullReference)
            {
                result.ResultType = ValidationResultType.Error;

                if (int.TryParse(Property.NiceName, out _))
                {
                    result.Message = $"Element at index ({Property.NiceName}) is null";
                    return;
                }

                result.Message = $"Property '{Property.NiceName}' is null";
                
                return;
            }

            var itemType = Property.BaseValueEntry.TypeOfValue;
            if (!AssemblyValidatorFactory.TryGetValidatorFor(itemType, out var validationMethod))
            {
                return;
            }

            _result.IsError = false;
            _result.Message = string.Empty;

            try
            {
                _invocationParameters[0] = Property.BaseValueEntry.WeakSmartValue;
                validationMethod.method.Invoke(validationMethod.validator, _invocationParameters);

                if (_result.IsError)
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = _result.Message;
                }
            }
            catch (Exception e)
            {
                if (_trashValidators.Contains(validationMethod.method))
                {
                    return;
                }

                _trashValidators.Add(validationMethod.method);
                
                Debug.LogException(e);
            }
        }
    }
}