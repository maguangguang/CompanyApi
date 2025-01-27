using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CompanyApi;
using CompanyApi.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace CompanyApiTest.Controllers
{
    public class CompanyControllerTest
    {
        [Fact]
        public async void Should_add_new_company_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var company = new Company(name: "Benz");
            var stringContent = SerializeToJsonString(company);

            var response = await client.PostAsync("/api/companies", stringContent);

            response.EnsureSuccessStatusCode();
            var savedCompany = await DeserializeToType<Company>(response);
            Assert.Equal(company, savedCompany);
        }

        [Fact]
        public async void Should_return_BadRequest_when_add_new_company_with_the_same_name()
        {
            var client = await ResetContextAndGetHttpClient();

            var company = new Company(name: "Benz");
            var stringContent = SerializeToJsonString(company);

            await client.PostAsync("/api/companies", stringContent);
            var response = await client.PostAsync("/api/companies", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void Should_return_all_companies_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var companyBMW = new Company(name: "BMW");
            await client.PostAsync("/api/companies", SerializeToJsonString(companyBMW));

            var response = await client.GetAsync("/api/companies");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var gotCompanies = await DeserializeToType<List<Company>>(response);
            Assert.Equal(new List<Company>() { companyBenz, companyBMW }, gotCompanies);
        }

        [Fact]
        public async void Should_get_one_company_by_ID_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);

            var response = await client.GetAsync($"/api/companies/{createdCompany.CompanyID}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var gotCompany = await DeserializeToType<Company>(response);
            Assert.Equal(companyBenz, gotCompany);
        }

        [Fact]
        public async void Should_get_one_company_by_ID_failed_given_not_existing_company_ID()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            await DeserializeToType<Company>(createdCompanyResponse);

            var response = await client.GetAsync($"/api/companies/NOT_EXISTING_COMPANY_ID");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void Should_get_companies_by_page_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var company1 = new Company(name: "Company1");
            var company2 = new Company(name: "Company2");
            var company3 = new Company(name: "Company3");
            await client.PostAsync("/api/companies", SerializeToJsonString(company1));
            await client.PostAsync("/api/companies", SerializeToJsonString(company2));
            await client.PostAsync("/api/companies", SerializeToJsonString(company3));

            var response = await client.GetAsync($"/api/companies?pageSize=2&pageIndex=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var gotCompanies = await DeserializeToType<List<Company>>(response);
            Assert.Equal(new List<Company>() { company1, company2 }, gotCompanies);
        }

        [Fact]
        public async void Should_get_the_last_page_companies_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var company1 = new Company(name: "Company1");
            var company2 = new Company(name: "Company2");
            var company3 = new Company(name: "Company3");
            await client.PostAsync("/api/companies", SerializeToJsonString(company1));
            await client.PostAsync("/api/companies", SerializeToJsonString(company2));
            await client.PostAsync("/api/companies", SerializeToJsonString(company3));

            var response = await client.GetAsync($"/api/companies?pageSize=2&pageIndex=2");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var gotCompanies = await DeserializeToType<List<Company>>(response);
            Assert.Equal(new List<Company>() { company3 }, gotCompanies);
        }

        [Fact]
        public async void Should_update_company_name_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);
            var updatedCompany = new Company() { CompanyID = createdCompany.CompanyID, Name = "MercedesBenz" };

            var response = await client.PutAsync($"/api/companies/{createdCompany.CompanyID}",
                SerializeToJsonString(updatedCompany));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var gotCompany = await DeserializeToType<Company>(response);
            Assert.Equal(updatedCompany, gotCompany);
        }

        [Fact]
        public async void Should_add_an_employee_to_company_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);

            var employee = new Employee(name: "ZhangSan", salary: 10000);
            var response = await client.PostAsync($"/api/companies/{createdCompany.CompanyID}/employees",
                SerializeToJsonString(employee));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var addedEmployee = await DeserializeToType<Employee>(response);
            Assert.Equal(employee, addedEmployee);
        }

        [Fact]
        public async void Should_get_all_employees_from_company_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);
            var employee = new Employee(name: "ZhangSan", salary: 10000);
            await client.PostAsync($"/api/companies/{createdCompany.CompanyID}/employees",
                SerializeToJsonString(employee));

            var response = await client.GetAsync($"/api/companies/{createdCompany.CompanyID}/employees");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var employees = await DeserializeToType<List<Employee>>(response);
            Assert.Equal(new List<Employee>() { employee }, employees);
        }

        [Fact]
        public async void Should_update_employee_by_EmployeeID_in_company_successfully()
        {
            var client = await ResetContextAndGetHttpClient();

            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);
            var employee = new Employee(name: "ZhangSan", salary: 10000);
            var addEmployeeResponse = await client.PostAsync($"/api/companies/{createdCompany.CompanyID}/employees",
                SerializeToJsonString(employee));
            var createdEmployee = await DeserializeToType<Employee>(addEmployeeResponse);

            var modifiedEmployee = new Employee() { Name = "LiSi", Salary = 20000 };
            var response =
                await client.PutAsync(
                    $"/api/companies/{createdCompany.CompanyID}/employees/{createdEmployee.EmployeeID}",
                    SerializeToJsonString(modifiedEmployee));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedEmployee = await DeserializeToType<Employee>(response);
            Assert.Equal(modifiedEmployee, updatedEmployee);
        }

        [Fact]
        public async void Should_delete_employee_by_EmployeeID_in_company_successfully()
        {
            // given
            var client = await ResetContextAndGetHttpClient();
            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);
            var employee = new Employee() { Name = "LiSi", Salary = 20000 };
            var addEmployeeResponse = await client.PostAsync($"/api/companies/{createdCompany.CompanyID}/employees",
                SerializeToJsonString(employee));
            var createdEmployee = await DeserializeToType<Employee>(addEmployeeResponse);
            // when
            var deleteResponse =
                await client.DeleteAsync(
                    $"/api/companies/{createdCompany.CompanyID}/employees/{createdEmployee.EmployeeID}");
            // then
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var getEmployeesResponse =
                await client.GetAsync(
                    $"/api/companies/{createdCompany.CompanyID}/employees/{createdEmployee.EmployeeID}");
            Assert.Equal(HttpStatusCode.NotFound, getEmployeesResponse.StatusCode);
        }

        [Fact]
        public async void Should_delete_employees_when_delete_company()
        {
            // given
            var client = await ResetContextAndGetHttpClient();
            var companyBenz = new Company(name: "Benz");
            var createdCompanyResponse = await client.PostAsync("/api/companies", SerializeToJsonString(companyBenz));
            var createdCompany = await DeserializeToType<Company>(createdCompanyResponse);
            var employee = new Employee() { Name = "LiSi", Salary = 20000 };
            var addEmployeeResponse = await client.PostAsync($"/api/companies/{createdCompany.CompanyID}/employees",
                SerializeToJsonString(employee));
            var createdEmployee = await DeserializeToType<Employee>(addEmployeeResponse);
            // when
            var deleteResponse =
                await client.DeleteAsync($"/api/companies/{createdCompany.CompanyID}");
            // then
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var getEmployeesResponse =
                await client.GetAsync(
                    $"/api/companies/{createdCompany.CompanyID}/employees/{createdEmployee.EmployeeID}");
            Assert.Equal(HttpStatusCode.NotFound, getEmployeesResponse.StatusCode);
        }

        private static async Task<HttpClient> ResetContextAndGetHttpClient()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            var client = server.CreateClient();
            await ClearAllCompanies(client);
            return client;
        }

        private static async Task ClearAllCompanies(HttpClient client)
        {
            await client.DeleteAsync("/api/companies");
        }

        private static async Task<T> DeserializeToType<T>(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseBody);
        }

        private static StringContent SerializeToJsonString<T>(T obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}