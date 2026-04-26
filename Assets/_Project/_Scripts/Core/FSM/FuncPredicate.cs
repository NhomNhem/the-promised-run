using System;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Core.FSM {
    public class FuncPredicate : IPredicate{
        private readonly Func<bool> _predicateFunc;
        
        public FuncPredicate(Func<bool> predicateFunc) => _predicateFunc = predicateFunc;
        
        public bool Evaluate() => _predicateFunc.Invoke();
    }
}