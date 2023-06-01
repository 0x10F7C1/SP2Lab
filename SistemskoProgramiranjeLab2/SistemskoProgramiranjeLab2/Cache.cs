using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SistemskoProgramiranjeLab2;
public class Cache
{
    //Tracks all the entries on the disk
    private ConcurrentDictionary<string, string> diskEntries
        = new ConcurrentDictionary<string, string>();
    //Memory cache
    private ConcurrentDictionary<string, string> memoryCache
        = new ConcurrentDictionary<string, string>();
    //Tracks all the memory cache entries
    private List<MemoryCacheEntry> memoryBook
        = new List<MemoryCacheEntry>();

    private int PATH_GEN = 0;
    private int capacity;
    private int occupiedMemory = 0;

    public Cache(int capacity)
    {
        this.capacity = capacity;
    }
    public async Task<string> GetCacheEntryOrDefault(string key) 
    {
        string? value = memoryCache.GetValueOrDefault(key);
        if (value == null)
        {
            string? path = diskEntries.GetValueOrDefault(key);
            if (path == null)
            {
                return string.Empty;
              
            }
            else
            {
                Console.WriteLine($"Reading resource {key} from disk cache");
                value = File.ReadAllText(path);        
                File.Delete(path);
                Console.WriteLine($"Attepmting to move the resource {key} to the Memory cache");
                await InsertCacheEntry(key, value);
                return value;
            }
        }
        else
        {
            Console.WriteLine($"Reading resource {key} from Memory cache");
            return value;
        }
 
    }

    //Nakon sto Deezer API vrati JSON odgovor
    public async Task InsertCacheEntry(string key, string value)
    {
        Boolean insertInMemory = true;
        int entryMemory = value.Length * 2;
        Console.WriteLine($"Entry memory {entryMemory}bytes");
        if (entryMemory <= capacity - occupiedMemory)
        {
            AddToMemoryCache(key, value);
        }
        else
        {
            int i = 0;
            while (entryMemory > capacity - occupiedMemory)
            {
                if (!(memoryBook.Count > 0))
                {
                    insertInMemory = false;
                    break;
                }
                else
                {
                    MemoryCacheEntry entry = memoryBook[i];
                    memoryBook.RemoveAt(i);
                    string? valueToMove;
                    memoryCache.Remove(entry.Key, out valueToMove);
                    occupiedMemory -= entry.Size;
                    Console.WriteLine($"Moving resource {entry.Key} to the disk cache");
                    await AddToDiskCache(entry.Key, valueToMove);
                    i++;
                }
            }
            if (insertInMemory)
            {
                AddToMemoryCache(key, value);
            }
            else
            {
                Console.WriteLine($"Adding resoure {key} to the disk cache");
                await AddToDiskCache(key, value);
            }
        }
    }
    private async Task AddToDiskCache(string key, string value)
    {
        string path = $"C:/Server/Fajl{PATH_GEN}.txt";
        await File.WriteAllTextAsync(path, value);
        diskEntries[key] = path;
        PATH_GEN += 1;
    }
    private void AddToMemoryCache(string key, string value)
    {
        Console.WriteLine($"Adding resource {key} to the memory cache");
        memoryCache[key] = value;
        memoryBook.Add(new MemoryCacheEntry(key, value.Length * 2));
        occupiedMemory += value.Length * 2;
    }
}
