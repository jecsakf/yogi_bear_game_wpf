using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearGame.Persistence
{
    class YogiBearFileDataAccess : IYogiBearDataAccess
    {
        /// <summary>
        /// Fájl betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktábla.</returns>
        public async Task<YogiBearTable> LoadAsync(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    string line = await reader.ReadLineAsync();
                    string[] pieces = line.Split(' ');
                    int tableSize = int.Parse(pieces[0]); // beolvassuk a tábla méretét
                    
                    List<int> baskets = new List<int>();
                    baskets.AddRange(Array.ConvertAll(pieces[1].Split(','), s => int.Parse(s)));

                    List<int> trees = new List<int>();
                    trees.AddRange(Array.ConvertAll(pieces[2].Split(','), s => int.Parse(s)));

                    List<int> rangers = new List<int>();
                    rangers.AddRange(Array.ConvertAll(pieces[3].Split(','), s => int.Parse(s)));

                    (char,int,int)[] rangersDirection = new (char, int,int)[rangers.Count];
                    string[] directions = pieces[4].Split(',');
                    for (int i = 0; i < directions.Length; i++)
                    {
                        rangersDirection[i] = (char.Parse(directions[i]), rangers[i], 0);
                    }
                        
                    YogiBearTable table = new YogiBearTable(tableSize,baskets,rangers,trees,rangersDirection); // létrehozzuk a táblát

                    for (Int32 i = 0; i < tableSize; i++)
                    {
                        for (Int32 j = 0; j < tableSize; j++)
                        {
                            if (i == 0 && j == 0) table.SetValue(i, j, 1);
                            else if (baskets.Contains(i * tableSize + j)) table.SetValue(i, j, 4);
                            else if (rangers.Contains(i * tableSize + j)) table.SetValue(i, j, 2);
                            else if (trees.Contains(i * tableSize + j)) table.SetValue(i, j, 3);
                            else table.SetValue(i, j, 0);
                        }
                    }

                    return table;
                }
            }
            catch
            {
                throw new YogiBearDataException();
            }
        }
    }
}
