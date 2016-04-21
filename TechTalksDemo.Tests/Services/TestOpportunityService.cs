using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using TechTalksDemo.Helpers;
using TechTalksDemo.Models;
using TechTalksDemo.Services;

namespace TechTalksDemo.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class TestOpportunityService
    {

        [Fact]
        public void TestAdd()
        {
            ConfigurationHelper.Ensure();

            var service = new OpportunityService();

            var countBefore = service.GetAll().Count();

            var newItem = new Opportunity() {
                Id = 0,
                Name = string.Format("Opportunity {0}", countBefore)
            };
            
            service.Add(newItem);

            var countAfter = service.GetAll().Count();
            Assert.Equal(countBefore, countAfter - 1);
        }

        [Fact]
        public void TestUpdate()
        {
            ConfigurationHelper.Ensure();

            var service = new OpportunityService();

            var item = service.GetAll().FirstOrDefault();

            if(item == null)
            {
                this.TestAdd();
                item = service.GetAll().FirstOrDefault();
            }

            var itemId = item.Id;
            string unique = (Guid.NewGuid()).ToString();

            item.Name = unique;
            service.Update(item);

            item = service.GetById(itemId);
            Assert.True(item.Name.Equals(unique, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestDelete()
        {
			this.TestAdd();

            ConfigurationHelper.Ensure();
            var service = new OpportunityService();

            var countBefore = service.GetAll().Count();

            var maxId = service.GetAll().Max(i => i.Id);

            service.Delete(maxId);

            var countAfter = service.GetAll().Count();
            Assert.Equal(countBefore, countAfter + 1);
        }
    }
}
