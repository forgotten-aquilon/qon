using qon.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables
{
    public class QVariable<T>
    {
        public string Name
        {
            get => Properties["@Name"] as string ?? "";
            protected set => Properties["@Name"] = value;
        }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public T? Value { get; private set; }


        private QVariable()
        {
            Name = string.Empty;
        }

        private QVariable(string name)
        {
            Name = name;
        }

        public QVariable<T> AddProperty(string name, object value)
        {
            if (Properties.ContainsKey(name))
            {
                throw new InternalLogicException("Property with this name already exists");
            }

            Properties[name] = value;
            return this;
        }
    }
}
