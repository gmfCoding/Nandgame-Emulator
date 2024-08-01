using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NgEmu.Processor
{
    internal class Testing
    {
        public static void Assert(bool expected, bool result, string message)
        {
            if (result != expected)
                Console.WriteLine(message);
        }

        public void TestConditions()
        {
            Assert(true, CPU.Condition(false, false, true, 1), "001 1 > 0");
            Assert(false, CPU.Condition(false, false, true, 0), "001 0 > 0 ");
            Assert(false, CPU.Condition(false, false, true, -1), "001 -1 > 0");

            Assert(false, CPU.Condition(false, true, false, 1), "010 1 == 0");
            Assert(true, CPU.Condition(false, true, false, 0), "010 0 == 0");
            Assert(false, CPU.Condition(false, true, false, -1), "010 -1 == 0");

            Assert(true, CPU.Condition(false, true, true, 1), "010 1 >= 0");
            Assert(true, CPU.Condition(false, true, true, 0), "010 0 >= 0");
            Assert(false, CPU.Condition(false, true, true, -1), "010 -1 >= 0");

            Assert(false, CPU.Condition(true, false, false, 1), "001 1 < 0");
            Assert(false, CPU.Condition(true, false, false, 0), "001 0 < 0");
            Assert(true, CPU.Condition(true, false, false, -1), "001 -1 < 0");

            Assert(true, CPU.Condition(true, false, true, 1), "010 1 != 0");
            Assert(false, CPU.Condition(true, false, true, 0), "010 0 != 0");
            Assert(true, CPU.Condition(true, false, true, -1), "010 -1 != 0");

            Assert(false, CPU.Condition(true, true, false, 1), "010 1 <= 0");
            Assert(true, CPU.Condition(true, true, false, 0), "010 0 <= 0");
            Assert(true, CPU.Condition(true, true, false, -1), "010 -1 <= 0");

            Assert(true, CPU.Condition(true, true, true, 1), "010 1 Always");
            Assert(true, CPU.Condition(true, true, true, -0), "111 0 Always");
            Assert(true, CPU.Condition(true, true, true, -1), "010 -1 Alwaysd");
        }
    }
}
