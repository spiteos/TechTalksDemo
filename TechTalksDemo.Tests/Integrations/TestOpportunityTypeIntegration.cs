using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using TechTalksDemo.Controllers;
using TechTalksDemo.Helpers;
using TechTalksDemo.Models;
using TechTalksDemo.Services;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting.Internal;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http;

namespace TechTalksDemo.Tests
{
    public class TestOpportunityTypeIntegration
    {
        private readonly TestServer server;

        public TestOpportunityTypeIntegration()
        {
            ConfigurationHelper.Ensure();
            server = new TestServer(TestServer.CreateBuilder().UseStartup<Startup>());
        }

        [Fact]
        public async void TestGetAll()
        {
            using (var client = server.CreateClient().AcceptJson())
            {
                var response = await client.GetAsync("/api/OpportunityTypes");
                var result = await response.Content.ReadAsJsonAsync<List<OpportunityType>>();

                var service = new OpportunityTypeService();
                var count = service.GetAll().Count();

                Assert.Equal(result.Count, count);
            }
        }

        [Fact]
        public async void TestGetById()
        {
            using (var client = server.CreateClient().AcceptJson())
            {
                var service = new OpportunityTypeService();
                var maxId = service.GetAll().Max(i => i.Id);

                var response = await client.GetAsync(string.Format("/api/OpportunityTypes/{0}", maxId));
                var result = await response.Content.ReadAsJsonAsync<OpportunityType>();                

                Assert.Equal(result.Id, maxId);
                Assert.Equal((int)response.StatusCode, 200);
            }
        }

        [Fact]
        public async void TestPost()
        {
            using (var client = server.CreateClient().AcceptJson())
            {
                var step = 1;
                var service = new OpportunityTypeService();
                var countBefore = service.GetAll().Count();
                var item = service.GetAll().FirstOrDefault();
                if(item == null)
                {
                    var newItem = new OpportunityType() {
                        Id = 0,
                        Name = string.Format("OpportunityType {0}", countBefore)
                    };

                    service.Add(item);
                    item = service.GetAll().FirstOrDefault();

                    step = 2;
                }

                var response = await client.PostAsJsonAsync("/api/OpportunityTypes", item);
                var result = await response.Content.ReadAsJsonAsync<OpportunityType>();

                var countAfter = service.GetAll().Count();

                Assert.Equal(countBefore, countAfter - step);
                Assert.Equal((int)response.StatusCode, 201);
            }
        }

        [Fact]
        public async void TestPut()
        {
            using (var client = server.CreateClient().AcceptJson())
            {
                var service = new OpportunityTypeService();
                var item = service.GetAll().FirstOrDefault();

                if (item == null)
                {
                    this.TestPost();
                    item = service.GetAll().FirstOrDefault();
                }

                var itemId = item.Id;
                string unique = (Guid.NewGuid()).ToString();

                item.Name = unique;

                var response = await client.PutAsJsonAsync(string.Format("/api/OpportunityTypes/{0}", itemId), item);
                var result = await response.Content.ReadAsJsonAsync<OpportunityType>();

                item = service.GetById(itemId);

                Assert.True(item.Name.Equals(unique, StringComparison.OrdinalIgnoreCase));
                Assert.Equal((int)response.StatusCode, 200);
            }
        }

        [Fact]
        public async void TestDelete()
        {
            using (var client = server.CreateClient().AcceptJson())
            {
                var service = new OpportunityTypeService();
                var maxId = service.GetAll().Max(i => i.Id);
                var countBefore = service.GetAll().Count();

                var response = await client.DeleteAsync(string.Format("/api/OpportunityTypes/{0}", maxId));
                var result = await response.Content.ReadAsJsonAsync<OpportunityType>();

                var countAfter = service.GetAll().Count();

                Assert.Equal(countBefore, countAfter + 1);
                Assert.Equal((int)response.StatusCode, 200);
            }
        }
    }
}
