using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAM.PTO
{
    public class MalformedFileException : Exception
    {
        public MalformedFileException() : base() {}
        public MalformedFileException(string msg) : base(msg) { }
    }
}
