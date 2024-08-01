using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NgEmu.Processor
{
    internal class CPU
    {
        int registerA;
        int registerD;
        int registerAs;

        struct EdgeRegister
        {
            public int store;
            public int output;

            public int Step(bool st, int d, bool cl)
            {
                if (cl && st)
                    store = d;
                if (!cl)
                    output = store;
                return output;
            }
        };

        EdgeRegister registerA_edge;
        EdgeRegister registerD_edge;
        EdgeRegister registerAs_edge;
        EdgeRegister pc_edge;

        class ROM
        {
            int[] data;

            public ROM(int[] bin)
            {
                data = bin;
            }

            public int GetValue(int address)
            {
                if (address < data.Length)
                    return data[address];
                return 0;
            }

            protected void SetValue(int address, int value)
            {
                if (address >= data.Length)
                    return;
                data[address] = value;
            }
        };

        class RAM : ROM
        {
            int temp_address;
            int temp_value;

            public RAM(int size) : base(new int[size])
            {

            }

            public void Assign(int address, bool st, int value, bool cl)
            {
                if (cl == false && st)
                    SetValue(temp_address, temp_value);
                else
                {
                    temp_address = address;
                    temp_value = value;
                }
            }
        }

        int[] bin;

        public CPU()
        {
            unchecked
            { 
                bin = new int[] {
                    // A = D+1
                    0b1000010100100000,
                    //  D = A
                    0b1000010010010000,
                    // *A = A
                    0b1000010010001000,
                    // A = 0
                    0b0000000000000000,
                    // JMP
                    0b1000000000000111
                };
            }

            rom = new ROM(bin);
        }

        ROM rom;

        RAM memory = new RAM(25000);

        // current instruction
        int instruction;

        struct CUALUInput
        {
            public int instruction;
            
            public int d;
            public int a;
            public int a_;
        };


        /// <summary>
        /// Control Unit and Arithmetic Logic Unit result
        /// </summary>
        struct CUALUResult
        {
            public int result; // X

            public bool a;
            public bool d;
            public bool a_;

            public bool j;
        };

        public static bool Condition(bool lt, bool eq, bool gt, int X)
        { 
            bool ci = OPCode.ci.IsSet(X);

            bool right = !(gt && !(X < 0 || X == 0));
            bool left = !(lt && X < 0) && !(eq && X == 0);
            return !(left && right);
        }

        private CUALUResult Instruction(CUALUInput input)
        {
            CUALUResult output;

            int X = input.d;
            int Y = input.a;

            bool u = OPCode.u.IsSet(input.instruction);
            bool op1 = OPCode.op1.IsSet(input.instruction);
            bool op0 = OPCode.op0.IsSet(input.instruction);
            bool zx = OPCode.zx.IsSet(input.instruction);
            bool sw = OPCode.sw.IsSet(input.instruction);

            if (OPCode.dref.IsSet(input.instruction))
                X = input.a_;
            // ALU

            // result.result = ALU()
            {
                int X1 = X; // ALU result stored
                int Y1 = Y;

                if (sw)
                { 
                    X1 = Y;
                    Y1 = X;
                }
                if (zx)
                    X1 = 0;

                /// Arithmetic
                if (u)
                {
                    int B = Y1;
                    if (op0)
                        B = 1;

                    if (op1)
                        X1 -= B;
                    else
                        X1 += B;
                }
                else // Logic
                {
                    if (op1 && op0)
                        X1 = ~X1;
                    else if (op1 && !op0)
                        X1 = X1 ^ Y1;
                    else if (!op1 && op0)
                        X1 = X1 | Y1;
                    else
                        X1 = X1 & Y1;
                }
                output.result = X1;
            }

            output.j = CPU.Condition(OPCode.lt.IsSet(input.instruction), OPCode.eq.IsSet(input.instruction), OPCode.gt.IsSet(input.instruction), output.result);
            output.a = OPCode.ta.IsSet(input.instruction);
            output.d = OPCode.td.IsSet(input.instruction);
            output.a_ = OPCode.ta_.IsSet(input.instruction);
            return output;
        }

        private CUALUResult ControlUnit(CUALUInput input)
        {
            CUALUResult output = new CUALUResult();
            bool ci = OPCode.ci.IsSet(input.instruction);
            CUALUResult aluResult = Instruction(new CUALUInput() { instruction = ci ? input.instruction : 0, a = input.a, d = input.d, a_ = input.a_ });
            output.result = ci ? aluResult.result : input.instruction;
            output.d = aluResult.d;
            output.a_ = aluResult.a_;
            output.a = ci ? aluResult.a : true;
            output.j = aluResult.j;
            return output;
        }

        private void Memory(CUALUResult cuResult, bool cl)
        {
            registerA_edge.Step(cuResult.a, cuResult.result, cl);
            registerD_edge.Step(cuResult.d, cuResult.result, cl);

            memory.Assign(registerA_edge.output, cuResult.a_, cuResult.result, cl);

            registerA = registerA_edge.output;
            registerD = registerD_edge.output;
            registerAs = memory.GetValue(registerA);

            registerAs_edge.Step(cuResult.a_, cuResult.result, cl);
        }

        public void Step()
        {
            instruction = rom.GetValue(pc_edge.output);
            CUALUResult cuResult = ControlUnit(new CUALUInput() { instruction = instruction, a = registerA, d = registerD, a_ = registerAs});

            // Counter
            {
                int value = (int)(cuResult.j ? registerA : pc_edge.output + 1);
                pc_edge.Step(true, value, true);
            }

            // Memory
            Memory(cuResult, true);
            PrintState();

            pc_edge.Step(false, 0, false);
            Memory(cuResult, false);
        }

        public void PrintState()
        {
            //Console.Clear();
            Console.SetCursorPosition(0, 0);
            //Console.WriteLine($"A: {registerA}");
            Console.WriteLine($"D: {registerD}");
            //Console.WriteLine($"A*: {registerAs}");
            return;
            Console.WriteLine($"I: {instruction}");
            Console.WriteLine($"PC: {pc_edge.output}");

            Console.WriteLine($"Stack 0-10:");

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{i} \t: {memory.GetValue(i)}");
            }

            Console.WriteLine($"Stack A*:");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{registerA + i} \t: {memory.GetValue(registerA + i)}");
            }

            Console.WriteLine($"ROM :");
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine($"{i} \t: {rom.GetValue(i)} {((i == pc_edge.output) ? '*' : ' ')}");
            }

            string data = "";
            int address = 0x4000;
            for (int j = 0; j < 256; j++)
            {
                for (int i = 0; i < 32; i++)
                {
                    int value = memory.GetValue(address);
                    for (int u = 0; u < 16; u++)
                    {
                        data.Append(((value << u) & 1) == 1 ? '#' : ' ');
                    }
                    address++;
                }
                data.Append('\n');
            }
            File.WriteAllText("out.txt", data);
        }
    }
}
