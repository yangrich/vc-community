﻿using System;
using System.Collections.Generic;

namespace VirtoCommerce.ApiClient.DataContracts.Order
{
    public class CustomerOrder : Operation
    {
        public string Id { get; set; }

        public string CustomerId { get; set; }

        public string StoreId { get; set; }

        public string ChannelId { get; set; }

        public string OrganizationId { get; set; }

        public string EmployeeId { get; set; }

        public Discount Discount { get; set; }

        public List<Address> Addresses { get; set; }

        public List<Shipment> Shipments { get; set; }

        public List<LineItem> Items { get; set; }

        public List<PaymentIn> InPayments { get; set; }
    }
}