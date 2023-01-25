using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.ValueTypes
{
    public class Sql
    {
        public string Value { get; }

        public Sql(string value)
        {
            Value = value;
        }

        public static explicit operator Sql(string value) => new Sql(value);

        public override string ToString()
        {
            return this.Value;
        }
    }
}
