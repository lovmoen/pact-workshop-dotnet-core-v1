using System;
using System.Collections.Generic;
using System.IO;
using Consumer;
using Newtonsoft.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace tests
{
   public class ConsumerPactTests : IClassFixture<ConsumerPactClassFixture>
   {
      private IMockProviderService _mockProviderService;
      private string _mockProviderServiceBaseUri;

      public ConsumerPactTests(ConsumerPactClassFixture fixture)
      {
         _mockProviderService = fixture.MockProviderService;
         _mockProviderService.ClearInteractions(); //NOTE: Clears any previously registered interactions before the test is run
         _mockProviderServiceBaseUri = fixture.MockProviderServiceBaseUri;
      }

      [Fact]
      public void ItHandlesInvalidDateParam()
      {
         // Arange
         var invalidRequestMessage = "validDateTime is not a date or time";
         _mockProviderService.Given("There is data")
            .UponReceiving("A valid GET request for Date Validation with invalid date parameter")
            .With(new ProviderServiceRequest
            {
               Method = HttpVerb.Get,
               Path = "/api/provider",
               Query = "validDateTime=lolz"
            })
            .WillRespondWith(new ProviderServiceResponse
            {
               Status = 400,
               Headers = new Dictionary<string, object>
               {
                  { "Content-Type", "application/json; charset=utf-8" }
               },
               Body = new
               {
                  message = invalidRequestMessage
               }
            });

         // Act
         var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi("lolz", _mockProviderServiceBaseUri).GetAwaiter().GetResult();
         var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

         // Assert
         Assert.Contains(invalidRequestMessage, resultBodyText);
      }

      [Fact]
      public void ItHandlesAnEmptyDateParam()
      {
         // Arange
         var invalidRequestMessage = "validDateTime is required";
         _mockProviderService.Given("There is data")
            .UponReceiving("A valid GET request for Date Validation with empty date parameter")
            .With(new ProviderServiceRequest
            {
               Method = HttpVerb.Get,
               Path = "/api/provider",
               Query = "validDateTime="
            })
            .WillRespondWith(new ProviderServiceResponse
            {
               Status = 400,
               Headers = new Dictionary<string, object>
               {
                  { "Content-Type", "application/json; charset=utf-8" }
               },
               Body = new
               {
                  message = invalidRequestMessage
               }
            });

         // Act
         var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi(string.Empty, _mockProviderServiceBaseUri).GetAwaiter().GetResult();
         var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

         // Assert
         Assert.Contains(invalidRequestMessage, resultBodyText);
      }

      [Fact]
      public void ItHandlesHavingNoData()
      {
         // Arange
         var inputString = "07-06-2018";
         var invalidRequestMessage = string.Empty;
         _mockProviderService.Given("There is no data")
            .UponReceiving("A valid GET request for Date Validation with valid date parameter")
            .With(new ProviderServiceRequest
            {
               Method = HttpVerb.Get,
               Path = "/api/provider",
               Query = $"validDateTime={inputString}"
            })
            .WillRespondWith(new ProviderServiceResponse
            {
               Status = 404
            });

         // Act
         var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi(inputString, _mockProviderServiceBaseUri).GetAwaiter().GetResult();
         var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

         // Assert
         Assert.Contains(invalidRequestMessage, resultBodyText);
      }

      [Fact]
      public void ItHandlesValidDateParam()
      {
         // Arange
         var inputString = "07-06-2018";
         _mockProviderService.Given("There is data")
            .UponReceiving("A valid GET request for Date Validation with valid date parameter")
            .With(new ProviderServiceRequest
            {
               Method = HttpVerb.Get,
               Path = "/api/provider",
               Query = $"validDateTime={inputString}"
            })
            .WillRespondWith(new ProviderServiceResponse
            {
               Status = 200,
               Headers = new Dictionary<string, object>
               {
                  { "Content-Type", "application/json; charset=utf-8" }
               },
               Body = new
               {
                  test = "NO",
                  validDateTime = $"{inputString} 00:00:00"
               }
            });

         // Act
         var result = ConsumerApiClient.ValidateDateTimeUsingProviderApi(inputString, _mockProviderServiceBaseUri).GetAwaiter().GetResult();
         var resultBodyText = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
         
         // Assert
         Assert.Contains($"{inputString} 00:00:00", resultBodyText);
      }
   }
}
