﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mollie.Api.Client;
using Mollie.Api.Models;
using Mollie.Api.Models.Shipment;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Mollie.Tests.Unit.Client {
    [TestFixture]
    public class ShipmentClientTests : BaseClientTests {
        [Test]
        public async Task CreateShipmentAsync_ValidShipment_ResponseIsDeserializedInExpectedFormat() {
            // Given: We create a shipment
            var shipmentRequest = new ShipmentRequest {
                Tracking = new TrackingObject {
                    Carrier = "tracking-carrier",
                    Code = "tracking-code",
                    Url = "tracking-url"
                },
                Testmode = true,
                Lines = new List<ShipmentLineRequest> {
                    new ShipmentLineRequest {
                        Id = "shipment-line-id",
                        Amount = new Amount(Currency.EUR, 50),
                        Quantity = 1
                    }
                }
            };
            const string orderId = "order-id";
            const string expectedPartialRequest = @"{""tracking"":{""carrier"":""tracking-carrier"",""code"":""tracking-code"",""url"":""tracking-url""},""lines"":[{""id"":""shipment-line-id"",""quantity"":1,""amount"":{""currency"":""EUR"",""value"":""50.00""}}],""testmode"":true}";
            var mockHttp = this.CreateMockHttpMessageHandler(
                HttpMethod.Post, 
                $"{BaseMollieClient.ApiEndPoint}orders/{orderId}/shipments", 
                DefaultShipmentJsonToReturn, 
                expectedPartialRequest);
            HttpClient httpClient = mockHttp.ToHttpClient();
            ShipmentClient shipmentClient = new ShipmentClient("abcde", httpClient);
            
            // When: We send the request
            ShipmentResponse shipmentResponse = await shipmentClient.CreateShipmentAsync(orderId, shipmentRequest);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
            Assert.IsNotNull(shipmentResponse);
            Assert.AreEqual(orderId, shipmentResponse.OrderId);
            Assert.AreEqual(shipmentRequest.Tracking.Carrier, shipmentResponse.Tracking.Carrier);
            Assert.AreEqual(shipmentRequest.Tracking.Code, shipmentResponse.Tracking.Code);
            Assert.AreEqual(shipmentRequest.Tracking.Url, shipmentResponse.Tracking.Url);
        }
        
        [TestCase("orders/order-id/shipments/shipment-id", false)]
        [TestCase("orders/order-id/shipments/shipment-id?testmode=true", true)]
        public async Task GetShipmentAsync_TestModeParameterCase_QueryStringOnlyContainsTestModeParameterIfTrue(string expectedUrl, bool testModeParameter) {
            // Given: We retrieve a shipment
            const string orderId = "order-id";
            const string shipmentId = "shipment-id";
            
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{BaseMollieClient.ApiEndPoint}{expectedUrl}")
                .Respond("application/json", DefaultShipmentJsonToReturn);
            HttpClient httpClient = mockHttp.ToHttpClient();
            ShipmentClient shipmentClient = new ShipmentClient("abcde", httpClient);

            // When: We send the request
            ShipmentResponse shipmentResponse = await shipmentClient.GetShipmentAsync(orderId, shipmentId, testModeParameter);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
            Assert.IsNotNull(shipmentResponse);
        }
        
        [TestCase("orders/order-id/shipments", false)]
        [TestCase("orders/order-id/shipments?testmode=true", true)]
        public async Task GetShipmentsListAsync_TestModeParameterCase_QueryStringOnlyContainsTestModeParameterIfTrue(string expectedUrl, bool testModeParameter) {
            // Given: We retrieve the list of shipments
            const string orderId = "order-id";
            
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{BaseMollieClient.ApiEndPoint}{expectedUrl}")
                .Respond("application/json", DefaultShipmentJsonToReturn);
            HttpClient httpClient = mockHttp.ToHttpClient();
            ShipmentClient shipmentClient = new ShipmentClient("abcde", httpClient);

            // When: We send the request
            var shipmentListResponse = await shipmentClient.GetShipmentsListAsync(orderId, testModeParameter);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
            Assert.IsNotNull(shipmentListResponse);
        }
        
        [Test]
        public async Task UpdateShipmentAsync_ValidUpdateShipmentRequest_ResponseIsDeserializedInExpectedFormat() {
            // Given: We create a shipment
            var updateShipmentRequest = new ShipmentUpdateRequest {
                Tracking = new TrackingObject {
                    Carrier = "tracking-carrier",
                    Code = "tracking-code",
                    Url = "tracking-url"
                },
                Testmode = true
            };
            const string orderId = "order-id";
            const string shipmentId = "shipment-id";
            const string expectedPartialRequest = @"{""tracking"":{""carrier"":""tracking-carrier"",""code"":""tracking-code"",""url"":""tracking-url""},""testmode"":true}";
            var mockHttp = this.CreateMockHttpMessageHandler(
                HttpMethod.Patch, 
                $"{BaseMollieClient.ApiEndPoint}orders/{orderId}/shipments/{shipmentId}", 
                DefaultShipmentJsonToReturn, 
                expectedPartialRequest);
            HttpClient httpClient = mockHttp.ToHttpClient();
            ShipmentClient shipmentClient = new ShipmentClient("abcde", httpClient);
            
            // When: We send the request
            ShipmentResponse shipmentResponse = await shipmentClient.UpdateShipmentAsync(orderId, shipmentId, updateShipmentRequest);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
            Assert.IsNotNull(shipmentResponse);
            Assert.AreEqual(orderId, shipmentResponse.OrderId);
            Assert.AreEqual(updateShipmentRequest.Tracking.Carrier, shipmentResponse.Tracking.Carrier);
            Assert.AreEqual(updateShipmentRequest.Tracking.Code, shipmentResponse.Tracking.Code);
            Assert.AreEqual(updateShipmentRequest.Tracking.Url, shipmentResponse.Tracking.Url);
        }

        private const string DefaultShipmentJsonToReturn = @"{
     ""resource"": ""shipment"",
     ""id"": ""shipment-id"",
     ""orderId"": ""order-id"",
     ""createdAt"": ""2018-08-09T14:33:54+00:00"",
     ""tracking"": {
         ""carrier"": ""tracking-carrier"",
         ""code"": ""tracking-code"",
         ""url"": ""tracking-url""
     },
     ""lines"": [
         {
             ""resource"": ""orderline"",
             ""id"": ""odl_dgtxyl"",
             ""orderId"": ""ord_pbjz8x"",
             ""name"": ""LEGO 42083 Bugatti Chiron"",
             ""sku"": ""5702016116977"",
             ""type"": ""physical"",
             ""status"": ""shipping"",
             ""metadata"": null,
             ""isCancelable"": true,
             ""quantity"": 1,
             ""unitPrice"": {
                 ""value"": ""399.00"",
                 ""currency"": ""EUR""
             },
             ""vatRate"": ""21.00"",
             ""vatAmount"": {
                 ""value"": ""51.89"",
                 ""currency"": ""EUR""
             },
             ""discountAmount"": {
                 ""value"": ""100.00"",
                 ""currency"": ""EUR""
             },
             ""totalAmount"": {
                 ""value"": ""299.00"",
                 ""currency"": ""EUR""
             },
             ""createdAt"": ""2018-08-02T09:29:56+00:00"",
             ""_links"": {
                 ""productUrl"": {
                     ""href"": ""https://shop.lego.com/nl-NL/Bugatti-Chiron-42083"",
                     ""type"": ""text/html""
                 },
                 ""imageUrl"": {
                     ""href"": ""https://sh-s7-live-s.legocdn.com/is/image//LEGO/42083_alt1?$main$"",
                     ""type"": ""text/html""
                 }
             }
         },
         {
             ""resource"": ""orderline"",
             ""id"": ""odl_jp31jz"",
             ""orderId"": ""ord_pbjz8x"",
             ""name"": ""LEGO 42056 Porsche 911 GT3 RS"",
             ""sku"": ""5702015594028"",
             ""type"": ""physical"",
             ""status"": ""completed"",
             ""metadata"": null,
             ""isCancelable"": false,
             ""quantity"": 1,
             ""unitPrice"": {
                 ""value"": ""329.99"",
                 ""currency"": ""EUR""
             },
             ""vatRate"": ""21.00"",
             ""vatAmount"": {
                 ""value"": ""57.27"",
                 ""currency"": ""EUR""
             },
             ""totalAmount"": {
                 ""value"": ""329.99"",
                 ""currency"": ""EUR""
             },
             ""createdAt"": ""2018-08-02T09:29:56+00:00"",
             ""_links"": {
                 ""productUrl"": {
                     ""href"": ""https://shop.lego.com/nl-NL/Porsche-911-GT3-RS-42056"",
                     ""type"": ""text/html""
                 },
                 ""imageUrl"": {
                     ""href"": ""https://sh-s7-live-s.legocdn.com/is/image/LEGO/42056?$PDPDefault$"",
                     ""type"": ""text/html""
                 }
             }
         }
     ],
     ""_links"": {
         ""self"": {
             ""href"": ""https://api.mollie.com/v2/order/ord_kEn1PlbGa/shipments/shp_3wmsgCJN4U"",
             ""type"": ""application/hal+json""
         },
         ""order"": {
             ""href"": ""https://api.mollie.com/v2/orders/ord_kEn1PlbGa"",
             ""type"": ""application/hal+json""
         },
         ""documentation"": {
             ""href"": ""https://docs.mollie.com/reference/v2/shipments-api/get-shipment"",
             ""type"": ""text/html""
         }
     }
 }";
    }
}