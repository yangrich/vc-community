﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Foundation.Catalogs.Repositories;
using VirtoCommerce.Foundation.Frameworks;
using foundation = VirtoCommerce.Foundation.Catalogs.Model;

namespace VirtoCommerce.CatalogModule.Repositories
{
	public interface IFoundationCatalogRepository : ICatalogRepository
	{
		
		foundation.CatalogBase GetCatalogById(string catalogId);
		foundation.Category GetCategoryById(string categoryId);
		foundation.Item[] GetItemByIds(string[] itemIds);
		foundation.Item[] GetAllItemVariations(foundation.Item item);
		foundation.Property[] GetPropertiesByIds(string[] propIds);
		foundation.Category GetPropertyCategory(string propId);
		foundation.Property[] GetAllCategoryProperties(foundation.Category category);
		void SetItemCategoryRelation(foundation.Item item, foundation.Category category);
		void SetVariationRelation(foundation.Item item, string mainProductId);
		void SetCategoryProperty(foundation.Category category, foundation.Property property);
		void RemoveItems(string[] itemIds);
		

	}
}