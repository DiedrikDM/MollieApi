﻿using Mollie.Api.Client;
using Mollie.Api.Models;
using Mollie.Api.Models.Order;
using Mollie.Api.Models.Payment;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mollie.Api.Models.Payment.Response;
using RichardSzalay.MockHttp;

namespace Mollie.Tests.Unit.Client {
    [TestFixture]
    public class OrderClientTests : BaseClientTests {
        private const string defaultOrderJsonResponse = @"{
            ""resource"": ""order"",
            ""id"": ""ord_kEn1PlbGa"",
            ""profileId"": ""pfl_URR55HPMGx"",
            ""method"": ""ideal"",
            ""amount"": {
                ""value"": ""1027.99"",
                ""currency"": ""EUR""
            },
        }";
        
        private const string defaultPaymentJsonResponse = @"{
            ""amount"":{
                ""currency"":""EUR"",
                ""value"":""100.00""
            },
            ""description"":""Description"",
            ""redirectUrl"":""http://www.mollie.com""}";

        [Test]
        public async Task GetOrderAsync_NoEmbedParameters_QueryStringIsEmpty() {
            // Given: We make a request to retrieve a order without wanting any extra data
            const string orderId = "abcde";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Get, $"{BaseMollieClient.ApiEndPoint}orders/{orderId}", defaultOrderJsonResponse);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.GetOrderAsync(orderId);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetOrderAsync_SingleEmbedParameters_QueryStringContainsEmbedParameter() {
            // Given: We make a request to retrieve a order with a single embed parameter
            const string orderId = "abcde";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Get, $"{BaseMollieClient.ApiEndPoint}orders/{orderId}?embed=payments", defaultOrderJsonResponse);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.GetOrderAsync(orderId, embedPayments: true);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetOrderAsync_MultipleEmbedParameters_QueryStringContainsMultipleParameters() {
            // Given: We make a request to retrieve a order with a single embed parameter
            const string orderId = "abcde";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Get, $"{BaseMollieClient.ApiEndPoint}orders/{orderId}?embed=payments,refunds,shipments", defaultOrderJsonResponse);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.GetOrderAsync(orderId, embedPayments: true, embedRefunds: true, embedShipments: true);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
        }
        
        [Test]
        public async Task GetOrderAsync_WithTestModeParameter_QueryStringContainsTestModeParameter() {
            // Given: We make a request to retrieve a order with a single embed parameter
            const string orderId = "abcde";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Get, $"{BaseMollieClient.ApiEndPoint}orders/{orderId}?testmode=true", defaultOrderJsonResponse);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.GetOrderAsync(orderId, testmode: true);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
        }
        
        [TestCase(null, null, null, false, "")]
        [TestCase("from", null, null, false, "?from=from")]
        [TestCase("from", 50, null, false, "?from=from&limit=50")]
        [TestCase(null, null, "profile-id", false, "?profileId=profile-id")]
        [TestCase(null, null, "profile-id", true, "?profileId=profile-id&testmode=true")]
        public async Task GetOrderListAsync_QueryParameterOptions_CorrectParametersAreAdded(string from, int? limit, string profileId, bool testmode, string expectedQueryString) {
            // Given: We make a request to retrieve the list of orders
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Get, $"{BaseMollieClient.ApiEndPoint}orders{expectedQueryString}", defaultOrderJsonResponse);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.GetOrderListAsync(from, limit, profileId, testmode);

            // Then
            mockHttp.VerifyNoOutstandingRequest();
        }
        
        [TestCase(null, null, false, "")]
        [TestCase("from", null, false, "?from=from")]
        [TestCase("from", 50, false, "?from=from&limit=50")]
        [TestCase(null, null, true, "?testmode=true")]
        public async Task GetOrderRefundListAsync_QueryParameterOptions_CorrectParametersAreAdded(string from, int? limit, bool testmode, string expectedQueryString) {
            // Given: We make a request to retrieve the list of orders
            const string orderId = "abcde";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Get, $"{BaseMollieClient.ApiEndPoint}orders/{orderId}/refunds{expectedQueryString}", defaultOrderJsonResponse);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.GetOrderRefundListAsync(orderId, from, limit, testmode);

            // Then
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public async Task CreateOrderAsync_SinglePaymentMethod_RequestIsSerializedInExpectedFormat() {
            // Given: we create a order with a single payment method
            OrderRequest orderRequest = this.CreateOrderRequestWithOnlyRequiredFields();
            orderRequest.Method = PaymentMethod.Ideal;
            string expectedPaymentMethodJson = $"\"method\":[\"{PaymentMethod.Ideal}";
            const string jsonResponse = defaultOrderJsonResponse;
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Post, $"{BaseMollieClient.ApiEndPoint}orders", jsonResponse, expectedPaymentMethodJson);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            OrderResponse orderResponse = await orderClient.CreateOrderAsync(orderRequest);

            // Then            
            mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(orderRequest.Method, orderResponse.Method);
        }

        [Test]
        public async Task CreateOrderAsync_MultiplePaymentMethods_RequestIsSerializedInExpectedFormat() {
            // Given: we create a order with a single payment method
            OrderRequest orderRequest = this.CreateOrderRequestWithOnlyRequiredFields();
            orderRequest.Methods = new List<string>() {
                PaymentMethod.Ideal,
                PaymentMethod.CreditCard,
                PaymentMethod.DirectDebit
            };
            string expectedPaymentMethodJson = $"\"method\":[\"{PaymentMethod.Ideal}\",\"{PaymentMethod.CreditCard}\",\"{PaymentMethod.DirectDebit}\"]";
            const string jsonResponse = defaultOrderJsonResponse;
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Post, $"{BaseMollieClient.ApiEndPoint}orders", jsonResponse, expectedPaymentMethodJson);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.CreateOrderAsync(orderRequest);

            // Then            
            mockHttp.VerifyNoOutstandingExpectation();
        }
        
        [Test]
        public async Task CreateOrderPaymentAsync_PaymentWithSinglePaymentMethod_RequestIsSerializedInExpectedFormat() {
            // Given: We create a payment request with multiple payment methods
            OrderPaymentRequest orderPaymentRequest = new OrderPaymentRequest() {
                CustomerId = "customer-id",
                Testmode = true,
                MandateId = "mandate-id",
                Methods = new List<string>() {
                    PaymentMethod.Ideal
                }
            };
            const string orderId = "order-id";
            string url = $"{BaseMollieClient.ApiEndPoint}orders/{orderId}/payments";
            string expectedPaymentMethodJson = $"\"method\":[\"{PaymentMethod.Ideal}\"]";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Post, url, defaultPaymentJsonResponse, expectedPaymentMethodJson);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.CreateOrderPaymentAsync(orderId, orderPaymentRequest);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
        }
        
        [Test]
        public async Task CreateOrderPaymentAsync_PaymentWithMultiplePaymentMethods_RequestIsSerializedInExpectedFormat() {
            // Given: We create a payment request with multiple payment methods
            OrderPaymentRequest orderPaymentRequest = new OrderPaymentRequest() {
                CustomerId = "customer-id",
                Testmode = true,
                MandateId = "mandate-id",
                Methods = new List<string>() {
                    PaymentMethod.Ideal,
                    PaymentMethod.CreditCard,
                    PaymentMethod.DirectDebit
                }
            };
            const string orderId = "order-id";
            string url = $"{BaseMollieClient.ApiEndPoint}orders/{orderId}/payments";
            string expectedPaymentMethodJson = $"\"method\":[\"{PaymentMethod.Ideal}\",\"{PaymentMethod.CreditCard}\",\"{PaymentMethod.DirectDebit}\"]";
            var mockHttp = this.CreateMockHttpMessageHandler(HttpMethod.Post, url, defaultPaymentJsonResponse, expectedPaymentMethodJson);
            HttpClient httpClient = mockHttp.ToHttpClient();
            OrderClient orderClient = new OrderClient("abcde", httpClient);

            // When: We send the request
            await orderClient.CreateOrderPaymentAsync(orderId, orderPaymentRequest);

            // Then
            mockHttp.VerifyNoOutstandingExpectation();
        }

        private OrderRequest CreateOrderRequestWithOnlyRequiredFields() {
            return new OrderRequest() {
                Amount = new Amount(Currency.EUR, "100.00"),
                OrderNumber = "16738",
                Lines = new List<OrderLineRequest>() {
                    new OrderLineRequest() {
                        Name = "A box of chocolates",
                        Quantity = 1,
                        UnitPrice = new Amount(Currency.EUR, "100.00"),
                        TotalAmount = new Amount(Currency.EUR, "100.00"),
                        VatRate = "21.00",
                        VatAmount = new Amount(Currency.EUR, "17.36")
                    }
                },
                BillingAddress = new OrderAddressDetails() {
                    GivenName = "John",
                    FamilyName = "Smit",
                    Email = "johnsmit@gmail.com",
                    City = "Rotterdam",
                    Country = "NL",
                    PostalCode = "0000AA",
                    Region = "Zuid-Holland",
                    StreetAndNumber = "Coolsingel 1"
                },
                RedirectUrl = "http://www.google.nl",
                Locale = Locale.nl_NL
            };
        }
    }
}
