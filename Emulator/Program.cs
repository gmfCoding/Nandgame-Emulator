using NgEmu.Processor;

Console.WriteLine("Hello, World!");
CPU cpu = new CPU();

while (true)
{
    cpu.Step();
}