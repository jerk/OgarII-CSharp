using Ogar_CSharp.primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.protocols
{
    public abstract class Protocol
    {
        public abstract string Type { get; }

        public abstract string SubType { get; }
        public abstract bool Distinguishes(Reader reader);

        public abstract Func<Reader, Protocol> decider { get; }
    }
}
