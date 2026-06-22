using System.Linq;
using Microsoft.EntityFrameworkCore;
using plm_api; 

namespace plm_api.Services
{
    public class BomCostService
    {
        private readonly AppDbContext _context;  

        public BomCostService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verilen bir üst montajın (Parent) fiyatını, alt parçalarının adet ve fiyatlarına göre günceller.
        /// </summary>
        public void RecalculateParentCost(int parentProductId)
        {
            
            var components = _context.ProductItems
                .Include(pi => pi.ChildProduct)
                .Where(pi => pi.ParentProductId == parentProductId)
                .ToList();

            
            if (!components.Any()) return;

            // 2. Her bileşenin (Fiyat * Adet) değerini toplayıp kümülatif maliyeti buluyoruz
            decimal totalCost = 0;
            foreach (var item in components)
            {
                if (item.ChildProduct != null)
                {
                    totalCost += item.ChildProduct.Price * (decimal)item.Quantity;
                }
            }

            // 
            var parentProduct = _context.Products.FirstOrDefault(p => p.Id == parentProductId);
            if (parentProduct != null)
            {
                parentProduct.Price = totalCost;
                _context.SaveChanges();

                //  KRİTİK NOKTA (Zincirleme Etki): 
                // Eğer bu güncellediğimiz Parent ürün de başka bir üst montajın alt parçasıysa, 
                // bir üst seviyeyi de tetikliyoruz! (Örn: Cıvata değişti -> Salıncak değişti -> Aks Komple değişti)
                var upperRecept = _context.ProductItems.FirstOrDefault(pi => pi.ChildProductId == parentProductId);
                if (upperRecept != null)
                {
                    RecalculateParentCost(upperRecept.ParentProductId);
                }
            }
        }
    }
}   