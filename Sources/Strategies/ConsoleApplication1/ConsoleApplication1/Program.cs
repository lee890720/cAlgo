using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<double> _wei = new List<double>(50);
            _wei.Add(1.0);
            _wei.Add(2.0);
            _wei.Add(3.0);
            _wei.Add(2.0);
            var result = from item in _wei   //每一项                        
                         group item by item into gro   //按项分组，没组就是gro                        
                         orderby gro.Count() descending   //按照每组的数量进行排序                        
                         select new { num = gro.Key, nums = gro.Count() };   //返回匿名类型对象，输出这个组的值和这个值出现的次数            
            foreach (var item in result.Take(1))
            {
                Response.Write(string.Format("数字{0}出现了{1}次", item.num, item.nums));
            }
        }
    }
}
