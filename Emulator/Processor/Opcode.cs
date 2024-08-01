using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NgEmu.Processor
{

    public enum OPCode
    {
        /// <summary>
        /// The following instruction is an operation instruction.
        /// When not set: A = instruction ie (A = value)
        /// </summary>
        ci = 0b1000000000000000,

        /// <summary>
        /// When sourcing from the A register, source instead from the value that A points to. (dereference)
        /// </summary>
        dref = 0b0001000000000000,

        /// <summary>
        /// When set use arithmetic instead of logic
        /// </summary>
        u = 0b0000010000000000,

        ///---------------------
        ///     |     |  u |!u |
        /// OP1 | OP0 |----|---|
        ///  0  |  0  | &  | + |
        ///  0  |  1  | |  | +1|
        ///  1  |  0  | ^  | - |
        ///  1  |  1  | ~  | -1|
        /// ----------|----|---|

        /// <summary>
        /// When set:
        /// use subtraction
        /// narrows logic operator to INV & XOR
        /// 
        /// When not:
        /// use addition
        /// narrows logic opeator to OR & AND
        /// </summary>
        op1 = 0b0000001000000000,

        /// <summary>
        /// Multiple:
        /// Second arg is one (1)
        /// OR
        /// When set narrows logic operator to INV & OR
        /// When not narrows logic opeator to XOR & AND
        /// </summary>
        op0 = 0b0000000100000000,

        /// <summary>
        /// First arg is zero (0)
        /// </summary>
        zx = 0b0000000010000000,

        /// <summary>
        /// Swap operands (A+D vs D+A / A-D vs D-A / 0 - A vs 0 - D)
        /// </summary>
        sw = 0b0000000001000000,

        /// <summary>
        /// Stores result in A (A = result)
        /// </summary>
        ta = 0b000000000100000,

        /// <summary>
        /// Stores result in D (D = result)
        /// </summary>
        td = 0b000000000010000,

        /// <summary>
        /// Stores result in address stored in A (A* = result)
        /// </summary>
        ta_ = 0b000000000001000,

        lt = 0b000000000000100,

        eq = 0b000000000000010,

        gt = 0b000000000000001
    }

    public static class Opcode
    {
        public static bool IsSet(this OPCode code, int instruction)
        { 
            return (instruction & (int)code) == (int)code;
        }
    }
}
