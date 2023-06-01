using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemskoProgramiranjeLab2;

public class MemoryCacheEntry
{
    private int size;
    private string key;
    public string Key { get { return key; } }
    public int Size { get { return size;} }
    public MemoryCacheEntry(string key, int size)
    {
        this.key = key;
        this.size = size;
    }
}
