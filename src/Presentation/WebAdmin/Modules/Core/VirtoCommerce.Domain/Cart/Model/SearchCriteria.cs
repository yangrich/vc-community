﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Domain.Cart.Model
{
	public class SearchCriteria
	{
		public SearchCriteria()
		{
			Count = 20;
		}

		public string Keyword { get; set; }
		public string CustomerId { get; set; }
		public string SiteId { get; set; }

		public int Start { get; set; }

		public int Count { get; set; }
	}
}
