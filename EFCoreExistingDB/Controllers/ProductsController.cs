using EFCoreExistingDB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace EFCoreExistingDB.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ProductsController : ControllerBase
  {
    private readonly NorthwindContext db;

    public ProductsController(NorthwindContext db)
    {
      this.db = db;
    }

    [HttpGet]
    public IActionResult TestQueries()
    {

      // lamda expression methodlar üzerinden linq sorgusu
      var plist = this.db.Products.ToList();
      // Raw Linq sql
      var plist2 = (from p in this.db.Products select p).ToList();

      //var pJoin = (from p in this.db.Products
      //             join c in db.Categories  on p.CategoryId equals c.CategoryId
      //             into c1
      //             from c in c1.DefaultIfEmpty()
      //             select new
      //             {
      //               Name = c.CategoryName,
      //               PName = p.ProductName
      //             }).ToList();

      //var p1Join = (from c in this.db.Categories
      //             join p in db.Products on c.CategoryId equals p.CategoryId
      //             into joined
      //             from p in joined.DefaultIfEmpty()
      //             select new
      //             {
      //               Name = c.CategoryName,
      //               PName = p.ProductName
      //             }).ToList();

      // lambda expression join (Include)
      var pLamdaJoin = db.Products.Include(x => x.Category).ToList();

      //pLamdaJoin.FirstOrDefault().Category.CategoryName;

      var selectJoin = db.Products.Select(a => new
      {
        ProductName = a.ProductName,
        CategoryName = a.Category.CategoryName
      }).ToList();


      // bir entityt başka bir entity ile bağlı ise include dolayı yoldan alt entity bağlı ise thenInclude
      var oList = db.Orders.Include(x => x.OrderDetails).ThenInclude(x => x.Product).ToList();
      // order ile product joinledik ama joinlemek için orderdetails tablosonu kullandık.


      var o1Join = (from o in this.db.Orders
                    join od in db.OrderDetails on o.OrderId equals od.OrderId
                    join p in db.Products on od.ProductId equals p.ProductId 
                    select o).ToList();


      //using (var context = new NorthwindContext())
      //{

      //}

      var plist34 = (from p in this.db.Products orderby p.ProductName ascending, p.UnitPrice descending  select p  ).ToList();

      var plist36 = db.Products.OrderBy(x => x.ProductName).ToList();

      var plist37 = (from p in this.db.Products  select p).OrderBy(x=> x.ProductName).ThenByDescending(x=> x.UnitPrice).ToList();

      var pcount = (from p in this.db.Products orderby p.ProductName ascending, p.UnitPrice descending select p).Count();


      var pWhere = (from p in this.db.Products where p.ProductName.Contains("a") orderby p.ProductName ascending, p.UnitPrice descending select p).Count();

      var pGroup = (from p in this.db.Products group p by p.CategoryId into grp select new { Cname = grp.Key.Value, Adet = grp.Count() }).ToList();

      var lGroup = db.Products.GroupBy(x => x.CategoryId).Select(a => new
      {
        CId = a.Key.Value,
        Count = a.Count()
      }).ToList();


      var query = db.Products.Join(db.Categories,
                                p => p.ProductId,
                                c => c.CategoryId,
                                (p, c) => new { PName = p.ProductName, CName  = c.CategoryName }).ToList();


      // select Many

      var querySelect = db.Orders.Where(x => x.OrderId == 10248).SelectMany(x => x.OrderDetails).ToList();

      var sqlRawQuery = this.db.Products.FromSqlRaw("Select * from Products where CategoryId=1").ToList();

      int categoryId = 1;

      var sqlRaw2 = this.db.Products.FromSqlInterpolated($"select * from Products where CategoryId={categoryId}").ToList();


      // sayfalama işlemleri
      var plist234 = db.Products.OrderBy(x => x.ProductName).Take(25).Skip(5).ToList();

      // birim fiyatı ortalamadan büyük olan ürünleri
      var plll = db.Products.Where(x=> x.UnitPrice > db.Products.Average(x => x.UnitPrice).Value).ToList();

      // içinde categoryId değeri geçenler (1,2,3) 
      List<string> number = new List<string> { "1", "2", "3" };
      var sqlIn = db.Products.Where(x => number.Contains(x.CategoryId.ToString())).ToList();

      string text = "chai";
      // EF Like
      var sqlLike = db.Products.Where(x => EF.Functions.Like(x.ProductName, $"%{text}%")).ToList();

      var sqlAny = db.Products.Where(x => number.Any(y => y == x.CategoryId.ToString())).ToList();

      /***
       * 
       * 
       * SELECT (
    SELECT TOP(1) [c].[CategoryName]
    FROM [Products] AS [p0]
    LEFT JOIN [Categories] AS [c] ON [p0].[CategoryID] = [c].[CategoryID]
    WHERE ([p].[CategoryID] = [p0].[CategoryID]) OR (([p].[CategoryID] IS NULL) AND ([p0].[CategoryID] IS NULL))) AS [Kategori], COUNT(*) AS [Adet], AVG([p].[UnitPrice]) AS [OrtalamaFiyat]
FROM [Products] AS [p]
GROUP BY [p].[CategoryID]
       * 
       */

      var kategoriBazliUrunAdet = db.Products.GroupBy(x => x.CategoryId).Select(a => new
      {
        Kategori = a.FirstOrDefault().Category.CategoryName,
        Adet = a.Count(),
        OrtalamaFiyat = a.Average(x => x.UnitPrice).Value
      }).ToList();

      /**
       * SELECT [c].[CategoryName] AS [Kategori], COUNT(*) AS [Adet], AVG([p].[UnitPrice]) AS [OrtalamaFiyat]
FROM [Products] AS [p]
LEFT JOIN [Categories] AS [c] ON [p].[CategoryID] = [c].[CategoryID]
GROUP BY [c].[CategoryName]
       * **/

      var kategoriBazliUrunAdet2 = db.Products.Include(x=> x.Category).GroupBy(x => x.Category.CategoryName).Select(a => new
      {
        Kategori = a.Key,
        Adet = a.Count(),
        OrtalamaFiyat = a.Average(x => x.UnitPrice).Value
      }).ToList();


      return Ok();
    }
  }
}
