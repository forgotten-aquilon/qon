using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace qon.Variables.Events
{
    public class ValueChangedEventArgs<TValue> : EventArgs
    {
        public TValue PreviousValue { get; }

        public TValue NewValue { get; }

        public ValueChangedEventArgs(TValue previousValue, TValue newValue)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}
