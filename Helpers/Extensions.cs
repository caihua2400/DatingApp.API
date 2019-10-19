using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-error",message);
            response.Headers.Add("Access-Control-Expose-Headers","Application-error");
            response.Headers.Add("Access-Control-Allow-Origin","*");
        }

        public static int CalculateAge(this DateTime DateOfBirth)
        {
            var year = DateTime.Now.Year - DateOfBirth.Year;
            return DateTime.Now.Year - DateOfBirth.Year;
        }

        public static void AddPagination(this HttpResponse response, 
            int currentPage, int itemsPerpage,int totalItems,int totalPages)
        {
            var pagiNationHeader= new PaginationHeader(currentPage, itemsPerpage,totalItems,totalPages);
            var camelCaseFormetter= new JsonSerializerSettings();
            camelCaseFormetter.ContractResolver=new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination",JsonConvert.SerializeObject(pagiNationHeader,camelCaseFormetter));
            response.Headers.Add("Access-Control-Expose-Headers","Pagination");
        }
    }
}