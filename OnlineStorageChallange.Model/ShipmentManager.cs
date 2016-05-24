using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStorageChallange.Model
{
    public class CombinationResult
    {
        public bool Possible { get; set; }
        public int OrderCount { get; set; } = 0;
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; } 

    }

    public enum MaximizationPossibilities
    {
        OrderCount,
        TotalRevenue,
        TotalProfit
    }
    public class ShipmentManager
    {
        private readonly OnlineStoreChallangeDatabaseEntities _entities = new OnlineStoreChallangeDatabaseEntities();
        List<CombinationResult> _combinationResults = new List<CombinationResult>();

        /// <summary>
        /// Chooses the most suitable Combination
        /// </summary>
        /// <param name="filter">The filter by which the best Combination is to be used</param>
        public async void GetBestCombinationAsync(MaximizationPossibilities filter)
        {
            await Task.Run(() =>
            {
                switch (filter)
                {
                    case MaximizationPossibilities.OrderCount:
                        _combinationResults = _combinationResults.OrderByDescending(result => result.OrderCount).AsParallel().ToList();
                        break;
                    case MaximizationPossibilities.TotalRevenue:
                        _combinationResults =
                            _combinationResults.OrderByDescending(result => result.TotalRevenue).AsParallel().ToList();
                        break;
                    case MaximizationPossibilities.TotalProfit:
                        _combinationResults.OrderByDescending(result => result.TotalProfit).AsParallel().ToList();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
                }
            });
            
            //string PropertyName = "asd";
            //_combinationResults = _combinationResults.OrderBy(o => { return o.GetType().GetProperties().ToList().Find(m => m.Name == PropertyName); }).ToList();
        }
        /// <summary>
        /// Calculates the result of all posible combinations
        /// </summary>
        public async void CalculateAsync()
        {

            await Task.Run(() =>
            {
                List<List<int>> combinatiosList =
                    GenerateCombinationsParallel(_entities.OrderHeaders.Where(o => !o.Is_Completed).ToList().Count);
                Parallel.ForEach(combinatiosList, ints => { _combinationResults.Add(Search(ints)); });

                // Only Possible combinations should be considered
                _combinationResults.RemoveAll(combinationResult => !combinationResult.Possible);
            });


        }

        public CombinationResult Search(List<int> indexSeries)
        {
            // TODO: Optimize from 3n to 1n
            CombinationResult result = new CombinationResult();
            List<OrderHeader> orders = indexSeries.Select(series => _entities.OrderHeaders.ToList()[series]).ToList();
            //List<Product> products = (from orderHeader in orders from orderLine in orderHeader.OrderLines select orderLine.Product).Distinct().ToList();
            result.OrderCount = indexSeries.Count;
            List<Tuple<Product, int>> tuplesList = new List<Tuple<Product, int>>();
            foreach (OrderHeader orderHeader in orders)
            {
                foreach (OrderLine orderLine in orderHeader.OrderLines)
                {
                    Tuple<Product, int> currentProduct = tuplesList.Find(o => o.Item1.ProductID == orderLine.Product.ProductID);
                    int index = tuplesList.FindIndex(o => o.Item1.ProductID == orderLine.Product.ProductID);
                    if (currentProduct != null)
                    {
                        tuplesList[index] = new Tuple<Product, int>(orderLine.Product, currentProduct.Item2 + orderLine.Quantity);
                    }
                    else
                    {
                        tuplesList.Add(new Tuple<Product, int>(orderLine.Product, orderLine.Quantity));
                    }
                }
            }

            // Decide, if the combination is possible
            if (!tuplesList.All(tuple => tuple.Item2 <= _entities.Products.ToList().Find(prod => prod.ProductID == tuple.Item1.ProductID).Quantity_Available))
            {
                // If not possible, break execution
                result.Possible = false;
                return result;
            }
            foreach (OrderHeader orderHeader in orders)
            {
                foreach (OrderLine orderLine in orderHeader.OrderLines)
                {
                    result.TotalRevenue += orderLine.LineCost;
                    result.TotalProfit += orderLine.Product.Margin*orderLine.Quantity ?? 0;
                }
            }
            return result;
        }

        // Inspired by: http://stackoverflow.com/a/7802892
        // Get index sequences
        public List<List<int>> GenerateCombinationsParallel(int n)
        {
            List<List<int>> temp = new List<List<int>>();
            int count = (int)Math.Pow(2, n);
            Parallel.For(1, count - 1, (i) =>
            {
                List<int> temp1 = new List<int>();
                string str = Convert.ToString(i, 2).PadLeft(n, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        temp1.Add(j);
                    }
                }
                lock (temp)
                {
                    temp.Add(temp1);
                }
            });

            return temp;
        }
        public List<List<int>> GenerateCombinationsSequential(int n)
        {
            List<List<int>> temp = new List<List<int>>();
            int count = (int)Math.Pow(2, n);
            
            for (int i = 1; i <= count - 1; i++)
            {
                List<int> temp1 = new List<int>();
                string str = Convert.ToString(i, 2).PadLeft(n, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        temp1.Add(j);
                    }
                }
                temp.Add(temp1);
            }
            return temp;
        }
    }
}
