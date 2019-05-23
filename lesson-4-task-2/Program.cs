using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lesson_4_task_2
{
    //2. Дана коллекция List<T>.Требуется подсчитать, сколько раз каждый элемент встречается в данной коллекции:
    //   a. для целых чисел;
    //   b. * для обобщенной коллекции;
    //   c. ** используя Linq.
    // Дмитрий Волков

    class MyList<T>: List<T>
    {
        public Dictionary<T, int> DoIt()
        {
            Dictionary<T, int> result = new Dictionary<T, int>();

            foreach (var item in this)
                if (result.ContainsKey(item))
                    result[item]++;
                else
                    result.Add(item, 1);

            return result;
        }
    }

    class Program
    {
        // Крутилки
        static Random rnd = new Random();
        private static int amountOfElements = 40;
        private static int minElem = 4;
        private static int maxElem = 30;

        static MyList<int> list = new MyList<int>();

        static void Main(string[] args)
        {
            for (int i = 0; i < amountOfElements; i++)
                list.Add( rnd.Next(minElem, maxElem) );

            foreach (KeyValuePair<int, int> item in list.DoIt())
                Console.WriteLine($"{item.Key} - {item.Value}");

            Console.WriteLine();

            var query = from data in list
                        group data by data into g
                        select new { num = g.Key, count = g.Count()};

            foreach (var group in query)
                Console.WriteLine("{0} : {1}", group.num, group.count);
        }
    }
}
