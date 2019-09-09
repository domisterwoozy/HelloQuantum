using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static HelloQuantum.QuantumStateExt;

namespace HelloQuantum
{
    public class QuantumSim
    {
        private readonly Random randomSource = new Random();
        private readonly IUnitaryTransform transform;
        private readonly Register[] regs;

        public int GatesProcessed { get; private set; } = 0;

        public QuantumSim(IUnitaryTransform transform, params Register[] regs)
        {
            this.transform = transform;
            this.regs = regs;
        }

        public IDictionary<Register, long> Simulate(IQuantumState input)
        {
            var res = transform.Transform(input);

            var ret = new Dictionary<Register, long>();
            foreach (var reg in regs)
            {
                double[] probs = res.GetDistribution(reg);
                double randomDouble = randomSource.NextDouble();
                double accum = 0;
                for(long regValue = 0; regValue < probs.LongLength; regValue++)
                {
                    accum += probs[regValue];
                    if (accum > randomDouble)
                    {
                        ret[reg] = regValue;
                        break;
                    }
                }
            }

            GatesProcessed += transform.NumGates;

            return ret;
        }
    }
}
