﻿using System;

namespace ProxiCall.Bot.Models
{
    //Contains the informations needed during Deserialization but to be ignored during Serialization
    public class OpportunityDetailed : Opportunity
    {
        public string Id { get; set; }

        private Lead lead;

        public Lead Lead
        {
            get { return lead; }
            set
            {
                lead = value;
                LeadId = lead.Id;
            }
        }

        private Product product;

        public Product Product
        {
            get { return product; }
            set
            {
                product = value;
                if(product!=null)
                {
                    ProductId = product.Id;
                }
                else
                {
                    ProductId = string.Empty;
                }
            }
        }

        public Nullable<DateTime> CreationDate { get; set; }

        public OpportunityDetailed()
        {
            Lead = new Lead();
            Product = new Product();
        }

        public void ResetLead()
        {
            Lead = new Lead();
        }

        public void ResetProduct()
        {
            Product = new Product();
        }
    }
}