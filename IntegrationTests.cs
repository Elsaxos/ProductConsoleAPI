using Microsoft.EntityFrameworkCore;
using ProductConsoleAPI.Business;
using ProductConsoleAPI.Business.Contracts;
using ProductConsoleAPI.Data.Models;
using ProductConsoleAPI.DataAccess;
using System.ComponentModel.DataAnnotations;

namespace ProductConsoleAPI.IntegrationTests.NUnit
{
    public  class IntegrationTests
    {
        private TestProductsDbContext dbContext;
        private IProductsManager productsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestProductsDbContext();
            this.productsManager = new ProductsManager(new ProductsRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddProductAsync_ShouldAddNewProduct()
        {
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(newProduct);

            var dbProduct = await this.dbContext.Products.FirstOrDefaultAsync(p => p.ProductCode == newProduct.ProductCode);

            Assert.NotNull(dbProduct);
            Assert.That(dbProduct.ProductName, Is.EqualTo(newProduct.ProductName));
            Assert.That(dbProduct.Description, Is.EqualTo(newProduct.Description));
            Assert.That(dbProduct.Price, Is.EqualTo(newProduct.Price));
            Assert.That(dbProduct.Quantity, Is.EqualTo(newProduct.Quantity));
            Assert.That(dbProduct.OriginCountry, Is.EqualTo(newProduct.OriginCountry));
            Assert.That(dbProduct.ProductCode, Is.EqualTo(newProduct.ProductCode));
            
        }

        //Negative test
        [Test]
        public async Task AddProductAsync_TryToAddProductWithInvalidCredentials_ShouldThrowException()
        {
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = -1m,
                Quantity = 100,
                Description = "Anything for description"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await productsManager.AddAsync(newProduct));
            var actual = await dbContext.Products.FirstOrDefaultAsync(c => c.ProductCode == newProduct.ProductCode);

            Assert.IsNull(actual);
            Assert.That(ex?.Message, Is.EqualTo("Invalid product!"));

        }

        [Test]
        public async Task DeleteProductAsync_WithValidProductCode_ShouldRemoveProductFromDb()
        {
            // Arrange
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(newProduct);

            // Act
            await productsManager.DeleteAsync(newProduct.ProductCode);
            // Assert
            var productinTheDb = await dbContext.Products.FirstOrDefaultAsync(x => x.ProductCode == newProduct.ProductCode);
            Assert.IsNull(productinTheDb);
        }

        [Test]
        public async Task DeleteProductAsync_TryToDeleteWithNullOrWhiteSpaceProductCode_ShouldThrowException()
        {
            

            // Act
            var exception = Assert.Throws<ArgumentException> (() => productsManager.DeleteAsync(null));

            // Assert
            Assert.That (exception.Message, Is.EqualTo("Product code cannot be empty."));
        }

        [Test]
        public async Task GetAllAsync_WhenProductsExist_ShouldReturnAllProducts()
        {
            // Arrange
            var firstProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProducts",
                ProductCode = "AB12A",
                Price = 1.25m,
                Quantity = 101,
                Description = "Anything for descriptions"
            };
            var sekondProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(firstProduct);
            await productsManager.AddAsync(sekondProduct);


            // Act
            var result = await productsManager.GetAllAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_WhenNoProductsExist_ShouldThrowKeyNotFoundException()
        {
            var expection = Assert.ThrowsAsync<KeyNotFoundException>(() => productsManager.GetAllAsync());
            // Assert
            Assert.That(expection.Message, Is.EqualTo("No product found."));
        }

        [Test]
        public async Task SearchByOriginCountry_WithExistingOriginCountry_ShouldReturnMatchingProducts()
        {
            // Arrange
            var firstProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProducts",
                ProductCode = "AB12A",
                Price = 1.25m,
                Quantity = 101,
                Description = "Anything for descriptions"
            };
            var sekondProduct = new Product()
            {
                OriginCountry = "USA",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(firstProduct);
            await productsManager.AddAsync(sekondProduct);

            var result = await productsManager.SearchByOriginCountry(sekondProduct.OriginCountry);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task SearchByOriginCountryAsync_WithNonExistingOriginCountry_ShouldThrowKeyNotFoundException()
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(() => productsManager.SearchByOriginCountry(" "));
            // Assert
            Assert.That(exception.Message, Is.EqualTo("Country name cannot be empty."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidProductCode_ShouldReturnProduct()
        {
            // Arrange
            var firstProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProducts",
                ProductCode = "AB12A",
                Price = 1.25m,
                Quantity = 101,
                Description = "Anything for descriptions"
            };
            var sekondProduct = new Product()
            {
                OriginCountry = "USA",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(firstProduct);
            await productsManager.AddAsync(sekondProduct);

            // Act
            var result = await productsManager.GetSpecificAsync("AB12C");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("USA", result.OriginCountry);

        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidProductCode_ShouldThrowKeyNotFoundException()
        {
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => productsManager.GetSpecificAsync(" "));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Product code cannot be empty."));
        }

        [Test]
        public async Task UpdateAsync_WithValidProduct_ShouldUpdateProduct()
        {
            // Arrange
            var firstProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProducts",
                ProductCode = "AB12A",
                Price = 1.25m,
                Quantity = 101,
                Description = "Anything for descriptions"
            };
            var sekondProduct = new Product()
            {
                OriginCountry = "USA",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(firstProduct);
            await productsManager.AddAsync(sekondProduct);

            // Act
            firstProduct.ProductName = "UPDATED NAME!";
          
            await productsManager.UpdateAsync(firstProduct);
            // Assert
            var itemInDb = await dbContext.Products.FirstOrDefaultAsync(x => x.ProductCode == firstProduct.ProductCode);
            Assert.NotNull(itemInDb);
            Assert.AreEqual(firstProduct.ProductName, itemInDb.ProductName);
            Assert.AreEqual(firstProduct.Description, itemInDb.Description);
            Assert.AreEqual(firstProduct.Price, itemInDb.Price);
            Assert.AreEqual(firstProduct.Quantity, itemInDb.Quantity);
            Assert.AreEqual(firstProduct.OriginCountry, itemInDb.OriginCountry);

        }

        [Test]
        public async Task UpdateAsync_WithInvalidProduct_ShouldThrowValidationException()
        {
            var exeption = Assert.ThrowsAsync<ValidationException>(() => productsManager.UpdateAsync(new Product()));
            Assert.That(exeption.Message, Is.EqualTo("Invalid product!"));

        }
    }
}
