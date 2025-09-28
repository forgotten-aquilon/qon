using qon.Domains;
using qon.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables
{
    public class EuclideanVariable<T> : SuperpositionVariable<T>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public WFCMachine<T> Machine { get; protected set; }
        protected EuclideanVariable(WFCMachine<T> machine) : base() { Machine = machine; }
        public EuclideanVariable(WFCMachine<T> machine, T value, string name = "") : base(value, name) { Machine = machine; }
        public EuclideanVariable(WFCMachine<T> machine, IDomain<T> domain, string name = "") : base(domain, name) { Machine = machine; }
        public EuclideanVariable(WFCMachine<T> machine, IDomain<T> domain, T value, string name = "") : base(domain, value, name) { Machine = machine; }

        public override object this[string propertyName]
        {
            get => propertyName switch
            {
                "x" or "X" => X,
                "y" or "Y" => Y,
                "z" or "Z" => Z,
                _ => base[propertyName]
            };

            set => _ = propertyName switch
            {
                "x" or "X" => ExceptionHelper.ThrowIfTypesMismatch<int>(value),
                "y" or "Y" => ExceptionHelper.ThrowIfTypesMismatch<int>(value),
                "z" or "Z" => ExceptionHelper.ThrowIfTypesMismatch<int>(value),
                _ => base[propertyName] = value
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? GetNullOrValueProperty(string propertyName) => propertyName switch
        {
            "x" or "X" => X,
            "y" or "Y" => Y,
            "z" or "Z" => Z,
            _ => base.GetNullOrValueProperty(propertyName)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TryGetProperty(string propertyName, out object? property)
        {   
            var (ok, value) = propertyName switch
            {
                "x" or "X" => (true, X),
                "y" or "Y" => (true, Y),
                "z" or "Z" => (true, Z),
                _ => (base.TryGetProperty(propertyName, out var p), p)
            };

            property = value;

            return ok;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ContainsProperty(string propertyName) => propertyName switch
        {
            "x" or "X" => true,
            "y" or "Y" => true,
            "z" or "Z" => true,
            _ => base.ContainsProperty(propertyName)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone()
        {
            EuclideanVariable<T> clone = new(Machine)
            {
                Domain = Domain.Copy(),
                Properties = new Dictionary<string, object>(Properties),
                Value = Value.Copy(),
                State = State,
                Name = Name,
                X = X,
                Y = Y,
                Z = Z,
            };

            return clone;
        }
    }
}
