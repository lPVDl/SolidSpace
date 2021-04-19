using System;
using System.Collections.Generic;

namespace SpaceSimulator
{
    public class FinalizationController : IDisposable
    {
        private readonly List<IFinalazable> _finalazables;

        public FinalizationController(List<IFinalazable> finalazables)
        {
            _finalazables = finalazables;
        }
        
        public void Dispose()
        {
            foreach (var item in _finalazables)
            {
                item.FinalizeObject();
            }
        }
    }
}